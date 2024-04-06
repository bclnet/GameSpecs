using GameX.Meta;
using GameX.Formats;
using K4os.Compression.LZ4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Buffers;
using System.Linq;
using K4os.Compression.LZ4.Encoders;

namespace GameX.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/BinaryKV3
    public class DATABinaryKV3 : DATA, IHaveMetaInfo
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

        static readonly Guid KV3_ENCODING_BINARY_BLOCK_COMPRESSED = new Guid(new byte[] { 0x46, 0x1A, 0x79, 0x95, 0xBC, 0x95, 0x6C, 0x4F, 0xA7, 0x0B, 0x05, 0xBC, 0xA1, 0xB7, 0xDF, 0xD2 });
        static readonly Guid KV3_ENCODING_BINARY_UNCOMPRESSED = new Guid(new byte[] { 0x00, 0x05, 0x86, 0x1B, 0xD8, 0xF7, 0xC1, 0x40, 0xAD, 0x82, 0x75, 0xA4, 0x82, 0x67, 0xE7, 0x14 });
        static readonly Guid KV3_ENCODING_BINARY_BLOCK_LZ4 = new Guid(new byte[] { 0x8A, 0x34, 0x47, 0x68, 0xA1, 0x63, 0x5C, 0x4F, 0xA1, 0x97, 0x53, 0x80, 0x6F, 0xD9, 0xB1, 0x19 });
        static readonly Guid KV3_FORMAT_GENERIC = new Guid(new byte[] { 0x7C, 0x16, 0x12, 0x74, 0xE9, 0x06, 0x98, 0x46, 0xAF, 0xF2, 0xE6, 0x3E, 0xB5, 0x90, 0x37, 0xE7 });
        public const int MAGIC = 0x03564B56; // VKV3 (3 isn't ascii, its 0x03)
        public const int MAGIC2 = 0x4B563301; // KV3\x01
        public const int MAGIC3 = 0x4B563302; // KV3\x02

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Text", Name = "BinaryKV3", Value = ToString() }),
            new MetaInfo("BinaryKV3", items: new List<MetaInfo> {
                new MetaInfo($"Data: {Data.Count}"),
                new MetaInfo($"Encoding: {Encoding}"),
                new MetaInfo($"Format: {Format}"),
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

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            r.Seek(Offset);
            var magic = r.ReadUInt32();
            switch (magic)
            {
                case MAGIC: ReadVersion1(r); break;
                case MAGIC2: ReadVersion2(r); break;
                case MAGIC3: ReadVersion3(r); break;
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
            r2.SeekAndAlign(countOfBinaryBytes, 4);

            var countOfStrings = r2.ReadInt32();
            var kvDataOffset = r2.BaseStream.Position;

            // Subtract one integer since we already read it (countOfStrings)
            r2.SkipAndAlign((countOfIntegers - 1) * 4, 8);

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

        void ReadVersion3(BinaryReader r)
        {
            Format = r.ReadGuid();

            var compressionMethod = r.ReadUInt32();
            var compressionDictionaryId = r.ReadUInt16();
            var compressionFrameSize = r.ReadUInt16();
            var countOfBinaryBytes = r.ReadUInt32(); // how many bytes (binary blobs)
            var countOfIntegers = r.ReadUInt32(); // how many 4 byte values (ints)
            var countOfEightByteValues = r.ReadUInt32(); // how many 8 byte values (doubles)

            // 8 bytes that help valve preallocate, useless for us
            var stringAndTypesBufferSize = r.ReadUInt32();
            var b = r.ReadUInt16();
            var c = r.ReadUInt16();

            var uncompressedSize = r.ReadUInt32();
            var compressedSize = r.ReadUInt32();
            var blockCount = r.ReadUInt32();
            var blockTotalSize = r.ReadUInt32();

            if (compressedSize > int.MaxValue) throw new NotImplementedException("KV3 compressedSize is higher than 32-bit integer, which we currently don't handle.");
            else if (blockTotalSize > int.MaxValue) throw new NotImplementedException("KV3 compressedSize is higher than 32-bit integer, which we currently don't handle.");

            using var s = new MemoryStream();

            if (compressionMethod == 0)
            {
                if (compressionDictionaryId != 0) throw new ArgumentOutOfRangeException(nameof(compressionDictionaryId), $"Unhandled: {compressionDictionaryId}");
                else if (compressionFrameSize != 0) throw new ArgumentOutOfRangeException(nameof(compressionFrameSize), $"Unhandled: {compressionFrameSize}");

                var output = new Span<byte>(new byte[compressedSize]);
                r.Read(output);
                s.Write(output);
            }
            else if (compressionMethod == 1)
            {
                if (compressionDictionaryId != 0) throw new ArgumentOutOfRangeException(nameof(compressionDictionaryId), $"Unhandled: {compressionDictionaryId}");
                else if (compressionFrameSize != 16384) throw new ArgumentOutOfRangeException(nameof(compressionFrameSize), $"Unhandled: {compressionFrameSize}");

                var output = new Span<byte>(new byte[uncompressedSize]);
                var buf = ArrayPool<byte>.Shared.Rent((int)compressedSize);
                try
                {
                    var input = buf.AsSpan(0, (int)compressedSize);
                    r.Read(input);
                    var written = LZ4Codec.Decode(input, output);
                    if (written != output.Length) throw new InvalidDataException($"Failed to decompress LZ4 (expected {output.Length} bytes, got {written}).");
                }
                finally { ArrayPool<byte>.Shared.Return(buf); }
                s.Write(output);
            }
            else if (compressionMethod == 2)
            {
                if (compressionDictionaryId != 0) throw new ArgumentOutOfRangeException(nameof(compressionDictionaryId), $"Unhandled {compressionDictionaryId}");
                else if (compressionFrameSize != 0) throw new ArgumentOutOfRangeException(nameof(compressionFrameSize), $"Unhandled {compressionFrameSize}");

                using var zstd = new ZstdSharp.Decompressor();
                var totalSize = uncompressedSize + blockTotalSize;
                var output = new Span<byte>(new byte[totalSize]);
                var buf = ArrayPool<byte>.Shared.Rent((int)compressedSize);
                try
                {
                    var input = buf.AsSpan(0, (int)compressedSize);
                    r.Read(input);
                    if (!zstd.TryUnwrap(input, output, out var written) || totalSize != written) throw new InvalidDataException($"Failed to decompress zstd correctly (written {written} bytes, expected {totalSize} bytes)");
                }
                finally { ArrayPool<byte>.Shared.Return(buf); }
                s.Write(output);
            }
            else throw new ArgumentOutOfRangeException(nameof(compressionMethod), $"Unknown compression method {compressionMethod}");

            s.Seek(0, SeekOrigin.Begin);
            using var r2 = new BinaryReader(s, System.Text.Encoding.UTF8, true);

            currentBinaryBytesOffset = 0;
            r2.BaseStream.Position = countOfBinaryBytes;
            r2.SeekAndAlign(countOfBinaryBytes, 4); // Align to % 4 after binary blobs

            var countOfStrings = r2.ReadUInt32();
            var kvDataOffset = r2.BaseStream.Position;

            // Subtract one integer since we already read it (countOfStrings)
            r2.SkipAndAlign((countOfIntegers - 1) * 4, 8); // Align to % 8 for the start of doubles

            currentEightBytesOffset = r2.BaseStream.Position;

            r2.BaseStream.Position += countOfEightByteValues * 8;
            var stringArrayStartPosition = r2.BaseStream.Position;

            strings = new string[countOfStrings];
            for (var i = 0; i < countOfStrings; i++) strings[i] = r2.ReadZUTF8();

            var typesLength = stringAndTypesBufferSize - (r2.BaseStream.Position - stringArrayStartPosition);
            types = new byte[typesLength];
            for (var i = 0; i < typesLength; i++) types[i] = r2.ReadByte();

            if (blockCount == 0)
            {
                var noBlocksTrailer = r2.ReadUInt32();
                if (noBlocksTrailer != 0xFFEEDD00) throw new ArgumentOutOfRangeException(nameof(noBlocksTrailer), $"Invalid trailer {noBlocksTrailer}");

                // Move back to the start of the KV data for reading.
                r2.BaseStream.Position = kvDataOffset;

                Data = (IDictionary<string, object>)ParseBinaryKV3(r2, null, true);
                return;
            }

            uncompressedBlockLengthArray = new int[blockCount];
            for (var i = 0; i < blockCount; i++) uncompressedBlockLengthArray[i] = r2.ReadInt32();

            var trailer = r2.ReadUInt32();
            if (trailer != 0xFFEEDD00) throw new ArgumentOutOfRangeException(nameof(trailer), $"Invalid trailer {trailer}");

            try
            {
                using var uncompressedBlocks = new MemoryStream((int)blockTotalSize);
                uncompressedBlockDataReader = new BinaryReader(uncompressedBlocks);

                if (compressionMethod == 0)
                {
                    for (var i = 0; i < blockCount; i++) r.BaseStream.CopyTo(uncompressedBlocks, uncompressedBlockLengthArray[i]);
                }
                else if (compressionMethod == 1)
                {
                    using var lz4decoder = new LZ4ChainDecoder(compressionFrameSize, 0);
                    while (r2.BaseStream.Position < r2.BaseStream.Length)
                    {
                        var compressedBlockLength = r2.ReadUInt16();
                        var output = new Span<byte>(new byte[compressionFrameSize]);
                        var buf = ArrayPool<byte>.Shared.Rent(compressedBlockLength);
                        try
                        {
                            var input = buf.AsSpan(0, compressedBlockLength);
                            r.Read(input);
                            if (lz4decoder.DecodeAndDrain(input, output, out var decoded) && decoded > 0) uncompressedBlocks.Write(decoded < output.Length ? output[..decoded] : output);
                            else throw new InvalidOperationException("LZ4 decode drain failed, this is likely a bug.");
                        }
                        finally { ArrayPool<byte>.Shared.Return(buf); }
                    }
                }
                else if (compressionMethod == 2)
                {
                    // This is supposed to be a streaming decompress using ZSTD_decompressStream,
                    // but as it turns out, zstd unwrap above already decompressed all of the blocks for us,
                    // so all we need to do is just copy the buffer.
                    // It's possible that Valve's code needs extra decompress because they set ZSTD_d_stableOutBuffer parameter.
                    r2.BaseStream.CopyTo(uncompressedBlocks);
                }
                else throw new ArgumentOutOfRangeException(nameof(compressionMethod), $"Unimplemented compression method in block decoder {compressionMethod}");

                uncompressedBlocks.Position = 0;

                // Move back to the start of the KV data for reading.
                r2.BaseStream.Position = kvDataOffset;

                Data = (IDictionary<string, object>)ParseBinaryKV3(r2, null, true);
            }
            finally { uncompressedBlockDataReader.Dispose(); }
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

        object ParseBinaryKV3(BinaryReader r, IDictionary<string, object> parent, bool inArray = false)
        {
            string name;
            if (!inArray)
            {
                var stringId = r.ReadInt32();
                name = stringId == -1 ? string.Empty : strings[stringId];
            }
            else name = null;
            var (type, flag) = ReadType(r);
            var value = ReadBinaryValue(name, type, flag, r);
            if (name != null) parent?.Add(name, value);
            return value;
        }

        object ReadBinaryValue(string name, KVType type, KVFlag flag, BinaryReader r)
        {
            var currentOffset = r.BaseStream.Position;
            object value;
            switch (type)
            {
                case KVType.NULL: value = MakeValue(type, null, flag); break;
                case KVType.BOOLEAN:
                    {
                        if (currentBinaryBytesOffset > -1) r.BaseStream.Position = currentBinaryBytesOffset;
                        value = MakeValue(type, r.ReadBoolean(), flag);
                        if (currentBinaryBytesOffset > -1) { currentBinaryBytesOffset++; r.BaseStream.Position = currentOffset; }
                        break;
                    }
                case KVType.BOOLEAN_TRUE: value = MakeValue(type, true, flag); break;
                case KVType.BOOLEAN_FALSE: value = MakeValue(type, false, flag); break;
                case KVType.INT64_ZERO: value = MakeValue(type, 0L, flag); break;
                case KVType.INT64_ONE: value = MakeValue(type, 1L, flag); break;
                case KVType.INT64:
                    {
                        if (currentEightBytesOffset > 0) r.BaseStream.Position = currentEightBytesOffset;
                        value = MakeValue(type, r.ReadInt64(), flag);
                        if (currentEightBytesOffset > 0) { currentEightBytesOffset = r.BaseStream.Position; r.BaseStream.Position = currentOffset; }
                        break;
                    }
                case KVType.UINT64:
                    {
                        if (currentEightBytesOffset > 0) r.BaseStream.Position = currentEightBytesOffset;
                        value = MakeValue(type, r.ReadUInt64(), flag);
                        if (currentEightBytesOffset > 0) { currentEightBytesOffset = r.BaseStream.Position; r.BaseStream.Position = currentOffset; }
                        break;
                    }
                case KVType.INT32: value = MakeValue(type, r.ReadInt32(), flag); break;
                case KVType.UINT32: value = MakeValue(type, r.ReadUInt32(), flag); break;
                case KVType.DOUBLE:
                    {
                        if (currentEightBytesOffset > 0) r.BaseStream.Position = currentEightBytesOffset;
                        value = MakeValue(type, r.ReadDouble(), flag);
                        if (currentEightBytesOffset > 0) { currentEightBytesOffset = r.BaseStream.Position; r.BaseStream.Position = currentOffset; }
                        break;
                    }
                case KVType.DOUBLE_ZERO: value = MakeValue(type, 0.0D, flag); break;
                case KVType.DOUBLE_ONE: value = MakeValue(type, 1.0D, flag); break;
                case KVType.STRING:
                    {
                        var id = r.ReadInt32();
                        value = MakeValue(type, id == -1 ? string.Empty : strings[id], flag);
                        break;
                    }
                case KVType.BINARY_BLOB:
                    {
                        if (uncompressedBlockDataReader != null)
                        {
                            var output = uncompressedBlockDataReader.ReadBytes(uncompressedBlockLengthArray[currentCompressedBlockIndex++]);
                            value = MakeValue(type, output, flag);
                            break;
                        }
                        var length = r.ReadInt32();
                        if (currentBinaryBytesOffset > -1) r.BaseStream.Position = currentBinaryBytesOffset;
                        value = MakeValue(type, r.ReadBytes(length), flag);
                        if (currentBinaryBytesOffset > -1) { currentBinaryBytesOffset = r.BaseStream.Position; r.BaseStream.Position = currentOffset + 4; }
                        break;
                    }
                case KVType.ARRAY:
                    {
                        var arrayLength = r.ReadInt32();
                        var array = new object[arrayLength];
                        for (var i = 0; i < arrayLength; i++) array[i] = ParseBinaryKV3(r, null, true);
                        value = MakeValue(type, array, flag);
                        break;
                    }
                case KVType.ARRAY_TYPED:
                    {
                        var typeArrayLength = r.ReadInt32();
                        var (subType, subFlag) = ReadType(r);
                        var typedArray = new object[typeArrayLength];
                        for (var i = 0; i < typeArrayLength; i++) typedArray[i] = ReadBinaryValue(null, subType, subFlag, r);
                        value = MakeValue(type, typedArray, flag);
                        break;
                    }
                case KVType.OBJECT:
                    {
                        var objectLength = r.ReadInt32();
                        var newObject = new Dictionary<string, object>();
                        if (name != null) newObject.Add("_key", name);
                        for (var i = 0; i < objectLength; i++) ParseBinaryKV3(r, newObject, false);
                        value = MakeValue(type, newObject, flag);
                        break;
                    }
                default: throw new InvalidDataException($"Unknown KVType {type} on byte {r.BaseStream.Position - 1}");
            }
            return value;
        }

        static object MakeValue(KVType type, object data, KVFlag flag) => data;

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
