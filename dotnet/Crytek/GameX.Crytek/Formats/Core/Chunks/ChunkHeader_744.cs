using System;
using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkHeader_744 : ChunkHeader
    {
        public override void Read(BinaryReader r)
        {
            ChunkType = (ChunkType)r.ReadUInt32();
            Version = r.ReadUInt32();
            Offset = r.ReadUInt32();
            ID = r.ReadInt32();
            Size = 0; // TODO: Figure out how to return a size - postprocess header table maybe?
        }
    }
}