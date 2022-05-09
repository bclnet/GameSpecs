using GameSpec.Metadata;
using GameSpec.Formats;
using K4os.Compression.LZ4;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.Valve.Formats.Blocks
{
    public class DATABinaryKV3 : DATA, IGetMetadataInfo
    {
        public enum KVFlag
        {
            None,
            Resource,
            DeferredResource
        }

        public enum KVType : byte
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

        static readonly Guid KV3_ENCODING_BINARY_BLOCK_COMPRESSED = new Guid(new byte[] { 0x46, 0x1A, 0x79, 0x95, 0xBC, 0x95, 0x6C, 0x4F, 0xA7, 0x0B, 0x05, 0xBC, 0xA1, 0xB7, 0xDF, 0xD2 });
        static readonly Guid KV3_ENCODING_BINARY_UNCOMPRESSED = new Guid(new byte[] { 0x00, 0x05, 0x86, 0x1B, 0xD8, 0xF7, 0xC1, 0x40, 0xAD, 0x82, 0x75, 0xA4, 0x82, 0x67, 0xE7, 0x14 });
        static readonly Guid KV3_ENCODING_BINARY_BLOCK_LZ4 = new Guid(new byte[] { 0x8A, 0x34, 0x47, 0x68, 0xA1, 0x63, 0x5C, 0x4F, 0xA1, 0x97, 0x53, 0x80, 0x6F, 0xD9, 0xB1, 0x19 });
        //static readonly Guid KV3_FORMAT_GENERIC = new Guid(new byte[] { 0x7C, 0x16, 0x12, 0x74, 0xE9, 0x06, 0x98, 0x46, 0xAF, 0xF2, 0xE6, 0x3E, 0xB5, 0x90, 0x37, 0xE7 });
        public const int MAGIC = 0x03564B56; // VKV3 (3 isn't ascii, its 0x03)
        public const int MAGIC2 = 0x4B563301; // KV3\x01

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

        string[] _strings;
        byte[] _types;
        long _typeIndex;
        long _eightBytesOffset;
        long _binaryBytesOffset = -1;

        public override void Read(BinaryPak parent, BinaryReader r)
        {
            r.Position(Offset);
            var s = new MemoryStream();
            var w = new BinaryWriter(s);
            var r2 = new BinaryReader(s);
            var magic = r.ReadUInt32();
            if (magic == MAGIC2) ReadVersion2(r, w, r2);
            else if (magic == MAGIC) ReadVersion1(r, w, r2);
            else throw new InvalidDataException($"Invalid KV3 signature {magic}");
        }

        void ReadVersion1(BinaryReader reader, BinaryWriter w, BinaryReader r)
        {
            Encoding = reader.ReadGuid();
            Format = reader.ReadGuid();

            if (Encoding.CompareTo(KV3_ENCODING_BINARY_BLOCK_COMPRESSED) == 0) BlockDecompress(reader, w, r);
            else if (Encoding.CompareTo(KV3_ENCODING_BINARY_BLOCK_LZ4) == 0) DecompressLZ4(reader, w);
            else if (Encoding.CompareTo(KV3_ENCODING_BINARY_UNCOMPRESSED) == 0) reader.CopyTo(w.BaseStream);
            else throw new InvalidDataException($"Unrecognised KV3 Encoding: {Encoding}");
            r.Position(0);

            _strings = new string[r.ReadUInt32()];
            for (var i = 0; i < _strings.Length; i++) _strings[i] = r.ReadZUTF8();

            Data = (IDictionary<string, object>)ParseBinaryKV3(r, null, true);
        }

        void ReadVersion2(BinaryReader r, BinaryWriter w, BinaryReader r2)
        {
            Format = r.ReadGuid();
            var compressionMethod = r.ReadInt32();
            var binaryBytes = r.ReadInt32(); // how many bytes (binary blobs)
            var integers = r.ReadInt32(); // how many 4 byte values (ints)
            var eightByteValues = r.ReadInt32(); // how many 8 byte values (doubles)

            if (compressionMethod == 0)
            {
                var length = r.ReadInt32();
                var buffer = new byte[length];
                r.Read(buffer, 0, length);
                w.Write(buffer);
            }
            else if (compressionMethod == 1) DecompressLZ4(r, w);
            else throw new Exception($"Unknown KV3 compression method: {compressionMethod}");

            _binaryBytesOffset = 0;
            r2.Position(binaryBytes, align: 4); // Align to % 4 after binary blobs

            _strings = new string[r2.ReadInt32()];
            var kv3Offset = r2.Position();

            // Subtract one integer since we already read it (_strings)
            // Align to % 8 for the start of doubles
            _eightBytesOffset = r2.Position(r2.Position() + (integers - 1) * 4, 8);

            r2.Skip(eightByteValues * 8);

            for (var i = 0; i < _strings.Length; i++) _strings[i] = r2.ReadZUTF8();

            // bytes after the string table is kv types, minus 4 static bytes at the end
            _types = r2.ReadBytes((int)(r2.BaseStream.Length - 4 - r2.Position()));

            // Move back to the start of the KV data for reading.
            r2.Position(kv3Offset);
            Data = (IDictionary<string, object>)ParseBinaryKV3(r2, null, true);
        }

        static void BlockDecompress(BinaryReader r, BinaryWriter w, BinaryReader r2)
        {
            var flags = r.ReadBytes(4);
            if ((flags[3] & 0x80) > 0) { w.Write(r.ReadBytes((int)(r.BaseStream.Length - r.BaseStream.Position))); return; }
            var length = (flags[2] << 16) + (flags[1] << 8) + flags[0];
            while (r.BaseStream.Position != r.BaseStream.Length)
                try
                {
                    var blockMask = r.ReadUInt16();
                    for (var i = 0; i < 16; i++)
                    {
                        if ((blockMask & (1 << i)) > 0)
                        {
                            var offsetSize = r.ReadUInt16();
                            var offset = ((offsetSize & 0xFFF0) >> 4) + 1;
                            var size = (offsetSize & 0x000F) + 3;
                            var lookupSize = Math.Min(offset, size);
                            var position = r2.Position();
                            r2.Skip(-offset);
                            var data = r2.ReadBytes(lookupSize);
                            w.BaseStream.Position = position;
                            while (size > 0) { w.Write(data, 0, Math.Min(lookupSize, size)); size -= lookupSize; }
                        }
                        else w.Write(r.ReadByte());
                        if (w.BaseStream.Length == length) return;
                    }
                }
                catch (EndOfStreamException) { return; }
        }

        void DecompressLZ4(BinaryReader r, BinaryWriter w)
        {
            var uncompressedSize = r.ReadUInt32();
            var compressedSize = (int)(Size - (r.BaseStream.Position - Offset));

            var input = r.ReadBytes(compressedSize);
            var output = new Span<byte>(new byte[uncompressedSize]);

            LZ4Codec.Decode(input, output);

            w.Write(output.ToArray()); // TODO: Write as span
            w.BaseStream.Position = 0;
        }

        (KVType Type, KVFlag Flag) ReadType(BinaryReader r)
        {
            var databyte = _types != null ? _types[_typeIndex++] : r.ReadByte();
            var flag = KVFlag.None;
            if ((databyte & 0x80) > 0)
            {
                databyte &= 0x7F; // Remove the flag bit
                flag = _types != null ? (KVFlag)_types[_typeIndex++] : (KVFlag)r.ReadByte();
            }
            return ((KVType)databyte, flag);
        }

        object ParseBinaryKV3(BinaryReader r, IDictionary<string, object> parent, bool inArray)
        {
            string name;
            if (!inArray) { var stringId = r.ReadInt32(); name = stringId == -1 ? string.Empty : _strings[stringId]; }
            else name = null;
            var (type, flag) = ReadType(r);
            var value = ReadBinaryValue(type, flag, r);
            if (name != null) parent?.Add(name, value);
            return value;
        }

        object ReadBinaryValue(KVType type, KVFlag flag, BinaryReader r)
        {
            var position = r.BaseStream.Position;
            switch (type)
            {
                case KVType.NULL: return MakeValue(type, null, flag);
                case KVType.BOOLEAN:
                    {
                        if (_binaryBytesOffset > -1) r.BaseStream.Position = _binaryBytesOffset;
                        var value = MakeValue(type, r.ReadBoolean(), flag);
                        if (_binaryBytesOffset > -1) { _binaryBytesOffset = r.BaseStream.Position; r.BaseStream.Position = position; }
                        return value;
                    }
                case KVType.BOOLEAN_TRUE: return MakeValue(type, true, flag);
                case KVType.BOOLEAN_FALSE: return MakeValue(type, false, flag);
                case KVType.INT64_ZERO: return MakeValue(type, 0L, flag);
                case KVType.INT64_ONE: return MakeValue(type, 1L, flag);
                case KVType.INT64:
                    {
                        if (_eightBytesOffset > 0) r.BaseStream.Position = _eightBytesOffset;
                        var value = MakeValue(type, r.ReadInt64(), flag);
                        if (_eightBytesOffset > 0) { _eightBytesOffset = r.BaseStream.Position; r.BaseStream.Position = position; }
                        return value;
                    }
                case KVType.UINT64:
                    {
                        if (_eightBytesOffset > 0) r.BaseStream.Position = _eightBytesOffset;
                        var value = MakeValue(type, r.ReadUInt64(), flag);
                        if (_eightBytesOffset > 0) { _eightBytesOffset = r.BaseStream.Position; r.BaseStream.Position = position; }
                        return value;
                    }
                case KVType.INT32: return MakeValue(type, r.ReadInt32(), flag);
                case KVType.UINT32: return MakeValue(type, r.ReadUInt32(), flag);
                case KVType.DOUBLE:
                    {
                        if (_eightBytesOffset > 0) r.BaseStream.Position = _eightBytesOffset;
                        var value = MakeValue(type, r.ReadDouble(), flag);
                        if (_eightBytesOffset > 0) { _eightBytesOffset = r.BaseStream.Position; r.BaseStream.Position = position; }
                        return value;
                    }
                case KVType.DOUBLE_ZERO: return MakeValue(type, 0.0D, flag);
                case KVType.DOUBLE_ONE: return MakeValue(type, 1.0D, flag);
                case KVType.STRING:
                    {
                        var id = r.ReadInt32();
                        return MakeValue(type, id == -1 ? string.Empty : _strings[id], flag);
                    }
                case KVType.BINARY_BLOB:
                    {
                        var length = r.ReadInt32();
                        if (_binaryBytesOffset > -1) r.BaseStream.Position = _binaryBytesOffset;
                        var value = MakeValue(type, r.ReadBytes(length), flag);
                        if (_binaryBytesOffset > -1) { _binaryBytesOffset = r.BaseStream.Position; r.BaseStream.Position = position + 4; }
                        return value;
                    }
                case KVType.ARRAY:
                    {
                        var count = r.ReadInt32();
                        var values = new object[count];
                        for (var i = 0; i < count; i++) values[i] = ParseBinaryKV3(r, null, true);
                        return MakeValue(type, values, flag);
                    }
                case KVType.ARRAY_TYPED:
                    {
                        var count = r.ReadInt32();
                        var (subType, subFlag) = ReadType(r);
                        var values = new object[count];
                        for (var i = 0; i < count; i++) values[i] = ReadBinaryValue(subType, subFlag, r);
                        return MakeValue(type, values, flag);
                    }
                case KVType.OBJECT:
                    {
                        var objectLength = r.ReadInt32();
                        var newObject = new Dictionary<string, object>();
                        for (var i = 0; i < objectLength; i++) ParseBinaryKV3(r, newObject, false);
                        return MakeValue(type, newObject, flag);
                    }
                default: throw new InvalidDataException($"Unknown KVType {type} on byte {r.BaseStream.Position - 1}");
            }
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

        public override void WriteText(IndentedTextWriter w) => w.Write(KVExtensions.Print(Data));
    }
}
