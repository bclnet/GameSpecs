using System;
using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkMtlName_900 : ChunkMtlName
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);
            
            Name = r.ReadFYString(128);
            NumChildren = 0;
        }
    }
}