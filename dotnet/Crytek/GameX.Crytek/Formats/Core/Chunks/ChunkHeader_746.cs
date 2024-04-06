using System;
using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkHeader_746 : ChunkHeader
    {
        public override void Read(BinaryReader r)
        {
            ChunkType = (ChunkType)r.ReadUInt16() + 0xCCCBF000;
            Version = r.ReadUInt16();
            ID = r.ReadInt32();
            Size = r.ReadUInt32();
            Offset = r.ReadUInt32();
        }
    }
}