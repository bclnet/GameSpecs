using System;
using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkExportFlags_1 : ChunkExportFlags
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            ChunkType = (ChunkType)r.ReadUInt32();
            Version = r.ReadUInt32();
            ChunkOffset = r.ReadUInt32();
            ID = r.ReadInt32();
            SkipBytes(r, 4);
            RCVersion = r.ReadTArray<uint>(sizeof(uint), 4);
            RCVersionString = r.ReadFYString(16);
            SkipBytesRemaining(r);
        }
    }
}