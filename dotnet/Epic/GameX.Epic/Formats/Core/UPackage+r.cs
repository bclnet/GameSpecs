using System;
using System.Diagnostics;
using System.IO;
using static GameX.Epic.Formats.Core.Game;
using static GameX.Epic.Formats.Core.ReaderExtensions;
using static GameX.Epic.Formats.Core.UDecrypt;
using static GameX.Epic.Formats.Core.UPackage;
using static GameX.Formats.Compression;

namespace GameX.Epic.Formats.Core
{
    class LineageStream : Stream
    {
        public const int LINEAGE_HEADER_SIZE = 28;
        Stream B;
        byte XorKey;
        public LineageStream(BinaryReader r, byte xorKey) { B = r.BaseStream; XorKey = xorKey; }

        public override bool CanRead => B.CanRead;
        public override bool CanSeek => B.CanSeek;
        public override bool CanWrite => B.CanWrite;
        public override long Length => B.Length;
        public override long Position
        {
            get => B.Position;
            set => B.Position = value;
        }
        public override void Flush() => B.Flush();
        public unsafe override int Read(byte[] buffer, int offset, int count)
        {
            var r = B.Read(buffer, offset, count);
            int i; byte* p;
            if (XorKey != 0)
                fixed (byte* data = &buffer[offset])
                {
                    for (i = 0, p = data; i < count; i++, p++) *p ^= XorKey;
                }
            return r;
        }
        public override long Seek(long offset, SeekOrigin origin) => B.Seek(offset, origin);
        public override void SetLength(long value) => B.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    class BattleTerrStream : Stream
    {
        Stream B;
        public BattleTerrStream(BinaryReader r) => B = r.BaseStream;

        public override bool CanRead => B.CanRead;
        public override bool CanSeek => B.CanSeek;
        public override bool CanWrite => B.CanWrite;
        public override long Length => B.Length;
        public override long Position
        {
            get => B.Position;
            set => B.Position = value;
        }
        public override void Flush() => B.Flush();
        public unsafe override int Read(byte[] buffer, int offset, int count)
        {
            var r = B.Read(buffer, offset, count);
            int i; byte* p;
            fixed (byte* data = &buffer[offset])
                for (i = 0, p = data; i < count; i++, p++)
                {
                    byte b = *p;
                    int shift;
                    byte v;
                    for (shift = 1, v = (byte)(b & (b - 1)); v != 0; v &= (byte)(v - 1)) shift++;    // shift = number of identity bits in 'v' (but b=0 -> shift=1)
                    b = ROL8(b, shift);
                    *p = b;
                }
            return r;
        }
        public override long Seek(long offset, SeekOrigin origin) => B.Seek(offset, origin);
        public override void SetLength(long value) => B.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    class AA2Stream : Stream
    {
        Stream B;
        public AA2Stream(BinaryReader r) => B = r.BaseStream;

        public override bool CanRead => B.CanRead;
        public override bool CanSeek => B.CanSeek;
        public override bool CanWrite => B.CanWrite;
        public override long Length => B.Length;
        public override long Position
        {
            get => B.Position;
            set => B.Position = value;
        }
        public override void Flush() => B.Flush();
        public unsafe override int Read(byte[] buffer, int offset, int count)
        {
            var StartPos = (int)B.Position;
            var r = B.Read(buffer, offset, count);
            int i; byte* p;
            fixed (byte* data = &buffer[offset])
                for (i = 0, p = data; i < count; i++, p++)
                {
                    byte b = *p;
                    var PosXor = StartPos + i;
                    PosXor = (PosXor >> 8) ^ PosXor;
                    b ^= (byte)(PosXor & 0xFF);
                    if ((PosXor & 2) != 0) b = ROL8(b, 1);
                    *p = b;
                }
            return r;
        }
        public override long Seek(long offset, SeekOrigin origin) => B.Seek(offset, origin);
        public override void SetLength(long value) => B.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    class BnSStream : Stream
    {
        const string key = "qiffjdlerdoqymvketdcl0er2subioxq";
        Stream B;
        public BnSStream(BinaryReader r) => B = r.BaseStream;

        public override bool CanRead => B.CanRead;
        public override bool CanSeek => B.CanSeek;
        public override bool CanWrite => B.CanWrite;
        public override long Length => B.Length;
        public override long Position
        {
            get => B.Position;
            set => B.Position = value;
        }
        public override void Flush() => B.Flush();
        public unsafe override int Read(byte[] buffer, int offset, int count)
        {
            var Pos = (int)B.Position;
            var r = B.Read(buffer, offset, count);
            // Note: similar code exists in DecryptBladeAndSoul()
            int i; byte* p;
            fixed (byte* data = &buffer[offset])
                for (i = 0, p = data; i < count; i++, p++, Pos++) *p ^= (byte)key[Pos % 32];
            return r;
        }
        public override long Seek(long offset, SeekOrigin origin) => B.Seek(offset, origin);
        public override void SetLength(long value) => B.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        static void DecodeBnSPointer(ref int Value, uint Code1, uint Code2, int Index)
            => Value = (int)(ROR32((uint)Value, (int)((Index + Code2) & 0x1F)) ^ ROR32(Code1, Index % 32));

        void PatchBnSExports(FObjectExport[] Exps, FPackageFileSummary Summary)
        {
            var Code1 = (uint)((Summary.HeadersSize & 0xFF) << 24) | ((Summary.NameCount & 0xFF) << 16) | ((Summary.NameOffset & 0xFF) << 8) | ((Summary.ExportCount & 0xFF));
            var Code2 = (Summary.ExportOffset + Summary.ImportCount + Summary.ImportOffset) & 0x1F;
            for (var i = 0; i < Summary.ExportCount; i++)
            {
                var Exp = Exps[i];
                DecodeBnSPointer(ref Exp.SerialSize, Code1, Code2, i);
                DecodeBnSPointer(ref Exp.SerialOffset, Code1, Code2, i);
            }
        }
    }

    class DunDefStream
    {
        static void PatchDunDefExports(FObjectExport[] Exps, FPackageFileSummary Summary)
        {
            // Dungeon Defenders has nullified ExportOffset entries starting from some version.
            // Let's recover them.
            var CurrentOffset = Summary.HeadersSize;
            for (var i = 0; i < Summary.ExportCount; i++)
            {
                var Exp = Exps[i];
                if (Exp.SerialOffset == 0) Exp.SerialOffset = CurrentOffset;
                CurrentOffset = Exp.SerialOffset + Exp.SerialSize;
            }
        }
    }

    class NurienStream : Stream
    {
        static readonly byte[] key = {
            0xFE, 0xF2, 0x35, 0x2E, 0x12, 0xFF, 0x47, 0x8A,
            0xE1, 0x2D, 0x53, 0xE2, 0x21, 0xA3, 0x74, 0xA8
        };
        Stream B;
        public int Threshold = 0x7FFFFFFF;
        public NurienStream(BinaryReader r) => B = r.BaseStream;

        public override bool CanRead => B.CanRead;
        public override bool CanSeek => B.CanSeek;
        public override bool CanWrite => B.CanWrite;
        public override long Length => B.Length;
        public override long Position
        {
            get => B.Position;
            set => B.Position = value;
        }
        public override void Flush() => B.Flush();
        public unsafe override int Read(byte[] buffer, int offset, int count)
        {
            var Pos = (int)B.Position;
            var r = B.Read(buffer, offset, count);
            if (Pos >= Threshold) return r; // only first Threshold bytes are compressed (package headers)
            int i; byte* p;
            fixed (byte* data = &buffer[offset])
                for (i = 0, p = data; i < count; i++, p++, Pos++)
                {
                    if (Pos >= Threshold) return r;
                    *p ^= key[Pos & 0xF];
                }
            return r;
        }
        public override long Seek(long offset, SeekOrigin origin) => B.Seek(offset, origin);
        public override void SetLength(long value) => B.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    class RocketLeagueStream : Stream
    {
        static readonly byte[] key = {
            0xC7, 0xDF, 0x6B, 0x13, 0x25, 0x2A, 0xCC, 0x71,
            0x47, 0xBB, 0x51, 0xC9, 0x8A, 0xD7, 0xE3, 0x4B,
            0x7F, 0xE5, 0x00, 0xB7, 0x7F, 0xA5, 0xFA, 0xB2,
            0x93, 0xE2, 0xF2, 0x4E, 0x6B, 0x17, 0xE7, 0x79
        };
        Stream B;
        int EncryptionStart;
        int EncryptionEnd;
        public RocketLeagueStream(BinaryReader r) => B = r.BaseStream;

        public override bool CanRead => B.CanRead;
        public override bool CanSeek => B.CanSeek;
        public override bool CanWrite => B.CanWrite;
        public override long Length => B.Length;
        public override long Position
        {
            get => B.Position;
            set => B.Position = value;
        }
        public override void Flush() => B.Flush();
        public unsafe override int Read(byte[] buffer, int offset, int count)
        {
            var Pos = (int)B.Position;
            var r = B.Read(buffer, offset, count);

            // Check if any of the data read was encrypted
            if (Pos + count <= EncryptionStart || Pos >= EncryptionEnd) return r;

            // Determine what needs to be decrypted
            var StartOffset = Math.Max(0, Pos - EncryptionStart);
            var EndOffset = Math.Min(EncryptionEnd, Pos + count) - EncryptionStart;
            var CopySize = EndOffset - StartOffset;
            var CopyOffset = Math.Max(0, EncryptionStart - Pos);

            // Round to 16-byte AES blocks
            int BlockStartOffset = StartOffset & ~15;
            int BlockEndOffset = MathX.Align(EndOffset, 16);
            int EncryptedSize = BlockEndOffset - BlockStartOffset;
            int EncryptedOffset = StartOffset - BlockStartOffset;

            // Decrypt and copy
            throw new NotImplementedException();
            //byte* EncryptedBuffer = (byte*)(appMalloc(EncryptedSize));
            //Reader->Seek(EncryptionStart + BlockStartOffset);
            //Reader->Serialize(EncryptedBuffer, EncryptedSize);
            //appDecryptAES(EncryptedBuffer, EncryptedSize, (char*)(key), ARRAY_COUNT(key));
            //memcpy(OffsetPointer(data, CopyOffset), &EncryptedBuffer[EncryptedOffset], CopySize);
            //appFree(EncryptedBuffer);

            // Note: this code is absolutely not optimal, because it will read 16 bytes block and fully decrypt
            // it many times for every small piece of serialized daya (for example if serializing array of bytes,
            // we'll have full code above executed for each byte, instead of once per block). However, it is assumed
            // that this reader is used only for decryption of package's header, so it is not so important to
            // optimize it.

            // Restore position
            B.Position = Pos + count;
            return r;
        }
        public override long Seek(long offset, SeekOrigin origin) => B.Seek(offset, origin);
        public override void SetLength(long value) => B.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    unsafe class UE3Stream : Stream
    {
        BinaryReader R;
        Stream B;
        UPackage Ar;
        // compression data
        COMPRESS CompressionFlags;
        FCompressedChunk[] CompressedChunks;
        // own file positions, overriding FArchive's one (because parent class is used for compressed data)
        int Stopper;
        long Position_;
        // decompression buffer
        byte[] Buffer_;
        int BufferSize;
        int BufferStart;
        int BufferEnd;
        // chunk
        FCompressedChunk CurrentChunk;
        FCompressedChunkHeader ChunkHeader;
        int ChunkDataPos;
        int PositionOffset;

        public UE3Stream(BinaryReader r, UPackage ar, COMPRESS compressionFlags, FCompressedChunk[] chunks)
        {
            R = r;
            B = r.BaseStream;
            Ar = ar;
            CompressionFlags = compressionFlags;
            CompressedChunks = chunks;
        }

        public override bool CanRead => B.CanRead;
        public override bool CanSeek => B.CanSeek;
        public override bool CanWrite => B.CanWrite;
        public override long Length => B.Length;
        public override long Position
        {
            get => Position_ + PositionOffset;
            set => Position_ = value - PositionOffset;
        }
        public override void Flush() => B.Flush();
        public unsafe override int Read(byte[] buffer, int offset, int count)
        {
            if (Stopper > 0 && Position_ + offset + count > Stopper) throw new Exception($"Serializing behind stopper ({Position_:X}+{offset}+{count:X} > {Stopper:X})");
            var bufferOffset = 0;
            while (true)
            {
                // check for valid buffer
                if (Position_ >= BufferStart && Position_ < BufferEnd)
                {
                    var ToCopy = (int)(BufferEnd - Position_ - offset);     // available size
                    if (ToCopy > count) ToCopy = count;                     // shrink by required size
                    Buffer.BlockCopy(Buffer_, (int)(Position_ - offset - BufferStart), buffer, bufferOffset, ToCopy);  // copy data
                    // advance pointers/counters
                    Position_ += ToCopy;
                    count -= ToCopy;
                    bufferOffset += ToCopy;
                    if (count == 0) return bufferOffset; // copied enough
                }
                // here: data/size points outside of loaded Buffer
                PrepareBuffer((int)Position_);
                Debug.Assert(Position_ >= BufferStart && Position_ < BufferEnd);    // validate PrepareBuffer()
            }
        }

        void PrepareBuffer(int Pos)
        {
            // find compressed chunk
            FCompressedChunk Chunk = null;
            for (var ChunkIndex = 0; ChunkIndex < CompressedChunks.Length; ChunkIndex++)
            {
                Chunk = CompressedChunks[ChunkIndex];
                if (Pos < Chunk.UncompressedOffset + Chunk.UncompressedSize) break;
            }
            Debug.Assert(Chunk != null); // should be at least 1 chunk in CompressedChunks

            // DC Universe has uncompressed package headers but compressed remaining package part
            if (Pos < Chunk.UncompressedOffset)
            {
                var Size = Chunk.CompressedOffset;
                Buffer_ = new byte[Size];
                BufferSize = Size;
                BufferStart = 0;
                BufferEnd = Size;
                B.Position = 0;
                B.Read(Buffer_, 0, Size);
                return;
            }
            else if (Chunk != CurrentChunk)
            {
                // serialize compressed chunk header
                B.Position = Chunk.CompressedOffset;
                if (Ar.Game == Bioshock)
                {
                    var CompressedSize = R.ReadInt32();
                    ChunkHeader = new FCompressedChunkHeader
                    {
                        Blocks = new[] {
                            new FCompressedChunkBlock
                            {
                                UncompressedSize = Ar.ArLicenseeVer >= 57 ? R.ReadInt32() : 32768, //?? Bioshock 2; no version code found
                                CompressedSize = CompressedSize,
                            }
                        }
                    };
                }
                else if (Chunk.CompressedSize != Chunk.UncompressedSize) ChunkHeader = new FCompressedChunkHeader(R, Ar);
                else
                {
                    // have seen such block in Borderlands: chunk has CompressedSize==UncompressedSize and has no compression; no such code in original engine
                    ChunkHeader = new FCompressedChunkHeader
                    {
                        BlockSize = -1, // mark as uncompressed (checked below)
                        Blocks = new[] {
                            new FCompressedChunkBlock
                            {
                                UncompressedSize = Chunk.UncompressedSize,
                                CompressedSize = Chunk.UncompressedSize,
                            }
                        }
                    };
                    ChunkHeader.Sum.CompressedSize = ChunkHeader.Sum.UncompressedSize = Chunk.UncompressedSize;
                }
                ChunkDataPos = (int)R.BaseStream.Position;
                CurrentChunk = Chunk;
            }

            // find block in ChunkHeader.Blocks
            var ChunkPosition = Chunk.UncompressedOffset;
            var ChunkData = ChunkDataPos;
            Debug.Assert(ChunkPosition <= Pos);
            FCompressedChunkBlock Block = null;
            for (var BlockIndex = 0; BlockIndex < ChunkHeader.Blocks.Length; BlockIndex++)
            {
                Block = ChunkHeader.Blocks[BlockIndex];
                if (ChunkPosition + Block.UncompressedSize > Pos) break;
                ChunkPosition += Block.UncompressedSize;
                ChunkData += Block.CompressedSize;
            }
            Debug.Assert(Block != null);

            // read compressed data
            //?? optimize? can share compressed buffer and decompressed buffer between packages
            var CompressedBlock = new byte[Block.CompressedSize];
            R.Seek(ChunkData);
            R.Read(CompressedBlock, 0, Block.CompressedSize);
            // prepare buffer for decompression
            if (Block.UncompressedSize > BufferSize)
            {
                Buffer_ = new byte[Block.UncompressedSize];
                BufferSize = Block.UncompressedSize;
            }
            // decompress data
            if (ChunkHeader.BlockSize != -1)    // my own mark
            {
                // Decompress block
                var UsedCompressionFlags = CompressionFlags;
                if (Ar.Game == Batman4 && CompressionFlags == COMPRESS.LZO_ENC_BNS) UsedCompressionFlags = COMPRESS.LZ4;
                appDecompress(CompressedBlock, Block.CompressedSize, Buffer_, Block.UncompressedSize, UsedCompressionFlags);
            }
            else
            {
                // No compression
                Debug.Assert(Block.CompressedSize == Block.UncompressedSize);
                Buffer.BlockCopy(CompressedBlock, 0, Buffer_, 0, Block.CompressedSize);
            }
            // setup BufferStart/BufferEnd
            BufferStart = ChunkPosition;
            BufferEnd = ChunkPosition + Block.UncompressedSize;
        }

        COMPRESS FoundCompression = COMPRESS.None;

        static COMPRESS DetectCompressionMethod(byte[] CompressedBuffer)
        {
            byte b1 = CompressedBuffer[0], b2 = CompressedBuffer[1];
            return b1 == 0x78 && (b2 == 0x9C || b2 == 0xDA) ? COMPRESS.ZLIB // b1=CMF: 7=32k buffer (CINFO), 8=deflate (CM), b2=FLG
                : (b1 == 0x8C || b1 == 0xCC) && (b2 == 5 || b2 == 6 || b2 == 10 || b2 == 11 || b2 == 12) ? COMPRESS.OODLE
                : GForceGame >= UE4_BASE ? COMPRESS.LZ4 // in most cases UE4 games are using either oodle or lz4 - the first one is explicitly recognizable
                : COMPRESS.LZO; // LZO was used only with UE3 games as standard compression method
        }

        int appDecompress(byte[] CompressedBuffer, int CompressedSize, byte[] UncompressedBuffer, int UncompressedSize, COMPRESS Flags)
        {
            var OldFlags = Flags;
            if (GForceGame == GoWU)
            {
                // It is strange, but this game has 2 Flags both used for LZ4 - probably they were used for different compression settings of the same algorithm.
                if (Flags == COMPRESS.LZX || Flags == (COMPRESS)32) Flags = COMPRESS.LZ4;
            }
            else if (GForceGame == BladeNSoul && Flags == COMPRESS.LZO_ENC_BNS) // note: GForceGame is required (to not pass 'Game' here)
            {
                DecryptBladeAndSoul(CompressedBuffer, CompressedSize);
                Flags = COMPRESS.LZO; // overide compression
            }
            else if (GForceGame == Smite)
            {
                if ((Flags & (COMPRESS)512) != 0)
                {
                    for (var i = 0; i < CompressedSize; i++) CompressedBuffer[i] ^= 0x2A; // Simple encryption
                    Flags &= ~(COMPRESS)512; // Remove encryption flag
                }
                if (Flags == COMPRESS.XXX) Flags = COMPRESS.OODLE; // Overide compression, appeared in late 2019 builds
            }
            else if (GForceGame == MassEffectLE)
            {
                if (Flags == (COMPRESS)0x400) Flags = COMPRESS.OODLE;
            }
            else if (GForceGame == TaoYuan) // note: GForceGame is required (to not pass 'Game' here);
            {
                DecryptTaoYuan(CompressedBuffer, CompressedSize);
            }
            else if ((GForceGame == DevilsThird) && (Flags & COMPRESS.XXX) != 0)
            {
                DecryptDevlsThird(CompressedBuffer, CompressedSize);
                // override compression
                Flags &= ~COMPRESS.XXX;
            }

            if (Flags == COMPRESS.FIND && FoundCompression >= 0)
            {
                // Do not detect compression multiple times: there were cases (Sea of Thieves) when
                // game is using LZ4 compression, however its first 2 bytes occasionally matched oodle,
                // so one of blocks were mistakenly used oodle.
                Flags = FoundCompression;
            }
            else if (Flags == COMPRESS.FIND && CompressedSize >= 2)
            {
                Flags = DetectCompressionMethod(CompressedBuffer);
                FoundCompression = Flags; // Cache detected compression method
            }

        restart_decompress:
            if (CompressedBuffer.Length != CompressedSize || UncompressedBuffer.Length != UncompressedSize) throw new Exception("Internal error");

            int newLen;
            switch (Flags)
            {
                case COMPRESS.LZO:
                    newLen = DecompressLzo(CompressedBuffer, UncompressedBuffer);
                    if (newLen != UncompressedSize) throw new Exception($"len mismatch: {newLen} != {UncompressedSize}");
                    return newLen;
                case COMPRESS.ZLIB:
                    newLen = DecompressZlib(CompressedBuffer, UncompressedBuffer);
                    if (newLen <= 0) throw new Exception($"zlib uncompress({CompressedSize},{UncompressedSize}) returned {newLen}");
                    return newLen;
                case COMPRESS.LZX:
#if SUPPORT_XBOX360
                    //appDecompressLZX(CompressedBuffer, UncompressedBuffer);
                    return UncompressedSize;
#else
                    throw new Exception("appDecompress: Lzx compression is not supported");
#endif
                case COMPRESS.LZ4:
                    newLen = DecompressLz4(CompressedBuffer, UncompressedBuffer);
                    if (newLen <= 0) throw new Exception($"LZ4_decompress_safe returned {newLen}");
                    else if (newLen != UncompressedSize) throw new Exception($"lz4 len mismatch: {newLen} != {UncompressedSize}");
                    return newLen;
                // defined for supported engine versions, it means - some games may need Oodle decompression
                case COMPRESS.OODLE:
                    newLen = DecompressOodle(CompressedBuffer, UncompressedBuffer);
                    return UncompressedSize;
                // Unknown compression flags
                default:
                    // Try to use compression detection
                    if (FoundCompression >= 0) { } // Already detected a working decompressor (if it wouldn't be working, we'd already crash)
                    else
                    {
                        Debug.Assert(CompressedSize >= 2);
                        FoundCompression = DetectCompressionMethod(CompressedBuffer);
                        Debug.WriteLine($"appDecompress: unknown compression flags {Flags:X}, detected {FoundCompression:X}, retrying ...");
                        Flags = FoundCompression;
                    }
                    Flags = FoundCompression;
                    break;
            }
            goto restart_decompress;
        }

        public override long Seek(long offset, SeekOrigin origin) => origin == SeekOrigin.Begin ? B.Seek(offset - PositionOffset, origin) : B.Seek(offset, origin);
        public override void SetLength(long value) => B.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
