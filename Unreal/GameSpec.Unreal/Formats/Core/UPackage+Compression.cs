using System.IO;
using System.Runtime.InteropServices;

namespace GameSpec.Unreal.Formats.Core
{
    partial class UPackage
    {
        public struct CompressedChunk
        {
            public int UncompressedOffset;
            public int UncompressedSize;
            public int CompressedOffset;
            public int CompressedSize;
            public CompressedChunk(BinaryReader r, BuildName build, int licenseeVersion)
            {
                if (build == BuildName.RocketLeague && licenseeVersion >= 22)
                {
                    UncompressedOffset = (int)r.ReadInt64();
                    CompressedOffset = (int)r.ReadInt64();
                    goto streamStandardSize;
                }
                UncompressedOffset = r.ReadInt32();
                CompressedOffset = r.ReadInt32();
            streamStandardSize:
                UncompressedSize = r.ReadInt32();
                CompressedSize = r.ReadInt32();
            }
        }

        public unsafe struct CompressedChunkHeader
        {
            public uint Tag;
            public int ChunkSize;
            public CompressedChunkBlock Summary;
            public CompressedChunkBlock[] Chunks;
            public CompressedChunkHeader(BinaryReader r)
            {
                Tag = r.ReadUInt32();
                ChunkSize = r.ReadInt32();
                if ((uint)ChunkSize == UPackage.MAGIC) ChunkSize = 0x20000;
                Summary = r.ReadT<CompressedChunkBlock>(sizeof(CompressedChunkBlock));
                var chunksCount = (Summary.UncompressedSize + ChunkSize - 1) / ChunkSize;
                Chunks = r.ReadTArray<CompressedChunkBlock>(sizeof(CompressedChunkBlock), chunksCount);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        public struct CompressedChunkBlock
        {
            public int CompressedSize;
            public int UncompressedSize;
        }
    }
}