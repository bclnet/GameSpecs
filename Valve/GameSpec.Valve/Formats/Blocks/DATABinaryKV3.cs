using GameSpec.Metadata;
using GameSpec.Formats;
using K4os.Compression.LZ4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Buffers;

namespace GameSpec.Valve.Formats.Blocks
{
    public class DATABinaryKV3 : DATA, IGetMetadataInfo
    {
        public enum KVFlag //was:Serialization/KeyValues/KVFlaggedValue
        {
            None,
            Resource,
            DeferredResource
        }

        public enum KVType : byte //was:Serialization/KeyValues/KVValue
        {
            STRING_MULTI = 0, // STRING_MULTI doesn't have an ID
            NULL = 1,
            BOOLEAN = 2,
            INT64 = 3,
            UINT64 = 4,
            DOUBLE = 5,
            STRING = 6,
            BINARY_BLOB = 7,
            ARRAY = 8,
            OBJECT = 9,
            ARRAY_TYPED = 10,
            INT32 = 11,
            UINT32 = 12,
            BOOLEAN_TRUE = 13,
            BOOLEAN_FALSE = 14,
            INT64_ZERO = 15,
            INT64_ONE = 16,
            DOUBLE_ZERO = 17,
            DOUBLE_ONE = 18,
        }

        static readonly Guid KV3_ENCODING_BINARY_BLOCK_COMPRESSED = new(new byte[] { 0x46, 0x1A, 0x79, 0x95, 0xBC, 0x95, 0x6C, 0x4F, 0xA7, 0x0B, 0x05, 0xBC, 0xA1, 0xB7, 0xDF, 0xD2 });
        static readonly Guid KV3_ENCODING_BINARY_UNCOMPRESSED = new(new byte[] { 0x00, 0x05, 0x86, 0x1B, 0xD8, 0xF7, 0xC1, 0x40, 0xAD, 0x82, 0x75, 0xA4, 0x82, 0x67, 0xE7, 0x14 });
        static readonly Guid KV3_ENCODING_BINARY_BLOCK_LZ4 = new(new byte[] { 0x8A, 0x34, 0x47, 0x68, 0xA1, 0x63, 0x5C, 0x4F, 0xA1, 0x97, 0x53, 0x80, 0x6F, 0xD9, 0xB1, 0x19 });
        static readonly Guid KV3_FORMAT_GENERIC = new(new byte[] { 0x7C, 0x16, 0x12, 0x74, 0xE9, 0x06, 0x98, 0x46, 0xAF, 0xF2, 0xE6, 0x3E, 0xB5, 0x90, 0x37, 0xE7 });
        public const int MAGIC = 0x03564B56; // VKV3 (3 isn't ascii, its 0x03)
        public const int MAGIC2 = 0x4B563301; // KV3\x01
        public const int MAGIC3 = 0x4B563302; // KV3\x02

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag) => new List<MetadataInfo> {
            new MetadataInfo(null, new MetadataContent { Type = "Text", Name = "BinaryKV3", Value = ToString() }),
            new MetadataInfo("BinaryKV3", items: new List<MetadataInfo> {
                new MetadataInfo($"Data: {Data.Count}"),
                new MetadataInfo($"Encoding: {Encoding}"),
                new MetadataInfo($"Format: {Format}"),
            }),
        };

        public IDictionary<string, object> Data { get; private set; }
        public Guid Encoding { get; private set; }
        public Guid Format { get; private set; }

        string[] strings;
        byte[] types;
        BinaryReader uncompressedBlockDataReader;
        int[] uncompressedBlockLengthArray;
        long currentCompressedBlockIndex;
        long currentTypeIndex;
        long currentEightBytesOffset = -1;
        long currentBinaryBytesOffset = -1;

        public override void Read(BinaryPak parent, BinaryReader r)
        {
            r.Seek(Offset);
            var magic = r.ReadUInt32();
            switch (magic)
            {
                case MAGIC: ReadVersion1(r); break;
                case MAGIC2: ReadVersion2(r, w, r2); break;
                //case MAGIC3: ReadVersion3(r, w, r2); break;
                default: throw new ArgumentOutOfRangeException(nameof(magic), $"Invalid KV3 signature {magic}");
            }
        }

        void DecompressLZ4(BinaryReader r, MemoryStream s)
        {
            var uncompressedSize = r.ReadUInt32();
            var compressedSize = (int)(Size - (r.BaseStream.Position - Offset));

            var output = new Span<byte>(new byte[uncompressedSize]);
            var buf = ArrayPool<byte>.Shared.Rent(compressedSize);
            try
            {
                var input = buf.AsSpan(0, compressedSize);
                r.Read(input);

                var written = LZ4Codec.Decode(input, output);
                if (written != output.Length) throw new InvalidDataException($"Failed to decompress LZ4 (expected {output.Length} bytes, got {written}).");
            }
            finally { ArrayPool<byte>.Shared.Return(buf); }

            s.Write(output);
        }

        void ReadVersion1(BinaryReader r)
        {
            Encoding = r.ReadGuid();
            Format = r.ReadGuid();

            using var s = new MemoryStream();
            using var r2 = new BinaryReader(s, System.Text.Encoding.UTF8, true);

            if (Encoding.CompareTo(KV3_ENCODING_BINARY_BLOCK_COMPRESSED) == 0) s.Write(BlockCompress.FastDecompress(r));
            else if (Encoding.CompareTo(KV3_ENCODING_BINARY_BLOCK_LZ4) == 0) DecompressLZ4(r, s);
            else if (Encoding.CompareTo(KV3_ENCODING_BINARY_UNCOMPRESSED) == 0) r.CopyTo(s);
            else throw new ArgumentOutOfRangeException(nameof(Encoding), $"Unrecognised KV3 Encoding: {Encoding}");
            s.Seek(0, SeekOrigin.Begin);

            strings = new string[r2.ReadUInt32()];
            for (var i = 0; i < strings.Length; i++) strings[i] = r2.ReadZUTF8();

            Data = (IDictionary<string, object>)ParseBinaryKV3(r2, null, true);

            var trailer = r2.ReadUInt32();
            if (trailer != 0xFFFFFFFF) throw new ArgumentOutOfRangeException(nameof(trailer), $"Invalid trailer {trailer}");
        }

        void ReadVersion2(BinaryReader r)
        {
            Format = r.ReadGuid();
            var compressionMethod = r.ReadInt32();
            var countOfBinaryBytes = r.ReadInt32(); // how many bytes (binary blobs)
            var countOfIntegers = r.ReadInt32(); // how many 4 byte values (ints)
            var countOfEightByteValues = r.ReadInt32(); // how many 8 byte values (doubles)

            using var s = new MemoryStream();

            if (compressionMethod == 0)
            {
                var length = r.ReadInt32();
                var output = new Span<byte>(new byte[length]);
                r.Read(output);
                s.Write(output);
                s.Seek(0, SeekOrigin.Begin);
            }
            else if (compressionMethod == 1) DecompressLZ4(r, s);
            else throw new ArgumentOutOfRangeException(nameof(compressionMethod), $"Unknown KV3 compression method: {compressionMethod}");

            using var r2 = new BinaryReader(s, System.Text.Encoding.UTF8, true);

            currentBinaryBytesOffset = 0;
            r2.Seek(countOfBinaryBytes, 4);

            var countOfStrings = r2.ReadInt32();
            var kvDataOffset = r2.BaseStream.Position;

            // Subtract one integer since we already read it (countOfStrings)
            r2.Skip((countOfIntegers - 1) * 4, 8);

            currentEightBytesOffset = r2.BaseStream.Position;
            r2.BaseStream.Position += countOfEightByteValues * 8;

            strings = new string[countOfStrings];
            for (var i = 0; i < countOfStrings; i++) strings[i] = r2.ReadZUTF8();

            // bytes after the string table is kv types, minus 4 static bytes at the end
            var typesLength = r2.BaseStream.Length - 4 - r2.BaseStream.Position;
            types = new byte[typesLength];
            for (var i = 0; i < typesLength; i++) types[i] = r2.ReadByte();

            // Move back to the start of the KV data for reading.
            r2.Seek(kvDataOffset);
            Data = (IDictionary<string, object>)ParseBinaryKV3(r2, null, true);
        }

        (KVType Type, KVFlag Flag) ReadType(BinaryReader r)
        {
            var databyte = types != null ? types[currentTypeIndex++] : r.ReadByte();
            var flag = KVFlag.None;
            if ((databyte & 0x80) > 0)
            {
                databyte &= 0x7F; // Remove the flag bit
                flag = types != null ? (KVFlag)types[currentTypeIndex++] : (KVFlag)r.ReadByte();
            }
            return ((KVType)databyte, flag);
        }

        IDictionary<string, object> ParseBinaryKV3(BinaryReader r, IDictionary<string, object> parent, bool inArray = false)
        {
            string name;
            if (!inArray)
            {
                var stringId = r.ReadInt32();
                name = stringId == -1 ? string.Empty : strings[stringId];
            }
            else name = null;
            var (type, flag) = ReadType(r);
            return ReadBinaryValue(name, type, flag, r, parent);
        }

        IDictionary<string, object> ReadBinaryValue(string name, KVType type, KVFlag flag, BinaryReader r, IDictionary<string, object> parent)
        {
            var currentOffset = r.BaseStream.Position;
            switch (type)
            {
                case KVType.NULL: parent.Add(name, MakeValue(type, null, flag)); break;
                case KVType.BOOLEAN:
                    {
                        if (currentBinaryBytesOffset > -1) r.BaseStream.Position = currentBinaryBytesOffset;
                        parent.Add(name, MakeValue(type, r.ReadBoolean(), flag));
                        if (currentBinaryBytesOffset > -1) { currentBinaryBytesOffset++; r.BaseStream.Position = currentOffset; }
                        break;
                    }
                case KVType.BOOLEAN_TRUE: parent.Add(name, MakeValue(type, true, flag)); break;
                case KVType.BOOLEAN_FALSE: parent.Add(name, MakeValue(type, false, flag)); break;
                case KVType.INT64_ZERO: parent.Add(name, MakeValue(type, 0L, flag)); break;
                case KVType.INT64_ONE: parent.Add(name, MakeValue(type, 1L, flag); break;
                case KVType.INT64:
                    {
                        if (currentEightBytesOffset > 0) r.BaseStream.Position = currentEightBytesOffset;
                        parent.Add(name, MakeValue(type, r.ReadInt64(), flag));
                        if (currentEightBytesOffset > 0) { currentEightBytesOffset = r.BaseStream.Position; r.BaseStream.Position = currentOffset; }
                        break;
                    }
                case KVType.UINT64:
                    {
                        if (currentEightBytesOffset > 0) r.BaseStream.Position = currentEightBytesOffset;
                        parent.Add(name, MakeValue(type, r.ReadUInt64(), flag));
                        if (currentEightBytesOffset > 0) { currentEightBytesOffset = r.BaseStream.Position; r.BaseStream.Position = currentOffset; }
                        break;
                    }
                case KVType.INT32: parent.Add(name, MakeValue(type, r.ReadInt32(), flag)); break;
                case KVType.UINT32: parent.Add(name, MakeValue(type, r.ReadUInt32(), flag)); break;
                case KVType.DOUBLE:
                    {
                        if (currentEightBytesOffset > 0) r.BaseStream.Position = currentEightBytesOffset;
                        parent.Add(name, MakeValue(type, r.ReadDouble(), flag));
                        if (currentEightBytesOffset > 0) { currentEightBytesOffset = r.BaseStream.Position; r.BaseStream.Position = currentOffset; }
                        break;
                    }
                case KVType.DOUBLE_ZERO: parent.Add(name, MakeValue(type, 0.0D, flag)); break;
                case KVType.DOUBLE_ONE: parent.Add(name, MakeValue(type, 1.0D, flag)); break;
                case KVType.STRING:
                    {
                        var id = r.ReadInt32();
                        parent.Add(name, MakeValue(type, id == -1 ? string.Empty : strings[id], flag));
                        break;
                    }
                case KVType.BINARY_BLOB:
                    {
                        if (uncompressedBlockDataReader != null)
                        {
                            var output = uncompressedBlockDataReader.ReadBytes(uncompressedBlockLengthArray[currentCompressedBlockIndex++]);
                            parent.Add(name, MakeValue(type, output, flag));
                            break;
                        }
                        var length = r.ReadInt32();
                        if (currentBinaryBytesOffset > -1) r.BaseStream.Position = currentBinaryBytesOffset;
                        parent.Add(name, MakeValue(type, r.ReadBytes(length), flag));
                        if (currentBinaryBytesOffset > -1) { currentBinaryBytesOffset = r.BaseStream.Position; r.BaseStream.Position = currentOffset + 4; }
                        break;
                    }
                case KVType.ARRAY:
                    {
                        var arrayLength = r.ReadInt32();
                        var array = new object[arrayLength];
                        for (var i = 0; i < arrayLength; i++) array[i] = ParseBinaryKV3(r, null, true);
                        parent.Add(name, MakeValue(type, array, flag));
                        break;
                    }
                case KVType.ARRAY_TYPED:
                    {
                        var typeArrayLength = r.ReadInt32();
                        var (subType, subFlag) = ReadType(r);
                        var typedArray = new object[typeArrayLength];
                        for (var i = 0; i < typeArrayLength; i++) typedArray[i] = ReadBinaryValue(subType, subFlag, r, null); //: typedArray
                        parent.Add(name, MakeValue(type, typedArray, flag));
                        break;
                    }
                case KVType.OBJECT:
                    {
                        var objectLength = r.ReadInt32();
                        var newObject = new Dictionary<string, object>();
                        for (var i = 0; i < objectLength; i++) ParseBinaryKV3(r, newObject, false);
                        if (parent == null) parent = newObject;
                        else parent.Add(name, MakeValue(type, newObject, flag));
                        break;
                    }
                default: throw new InvalidDataException($"Unknown KVType {type} on byte {r.BaseStream.Position - 1}");
            }
            return parent;
        }

        //static KVType ConvertBinaryOnlyKVType(KVType type)
        //{
        //    switch (type)
        //    {
        //        case KVType.BOOLEAN:
        //        case KVType.BOOLEAN_TRUE:
        //        case KVType.BOOLEAN_FALSE: return KVType.BOOLEAN;
        //        case KVType.INT64:
        //        case KVType.INT32:
        //        case KVType.INT64_ZERO:
        //        case KVType.INT64_ONE: return KVType.INT64;
        //        case KVType.UINT64:
        //        case KVType.UINT32: return KVType.UINT64;
        //        case KVType.DOUBLE:
        //        case KVType.DOUBLE_ZERO:
        //        case KVType.DOUBLE_ONE: return KVType.DOUBLE;
        //        case KVType.ARRAY_TYPED: return KVType.ARRAY;
        //        default: return type;
        //    }
        //}

        static object MakeValue(KVType type, object data, KVFlag flag) => data;
        //{
        //    var realType = ConvertBinaryOnlyKVType(type);
        //    flag != KVFlag.None ? (object)(realType, flag, data) : (realType, data);
        //}


#pragma warning disable CA1024 // Use properties where appropriate
        public DATABinaryKV3File GetKV3File()
#pragma warning restore CA1024 // Use properties where appropriate
        {
            // TODO: Other format guids are not "generic" but strings like "vpc19"
            var formatType = Format != KV3_FORMAT_GENERIC ? "vrfunknown" : "generic";
            return new DATABinaryKV3File(Data, format: $"{formatType}:version{{{Format}}}");
        }

        public override void WriteText(IndentedTextWriter w) => w.Write(KVExtensions.Print(Data));
    }
}
