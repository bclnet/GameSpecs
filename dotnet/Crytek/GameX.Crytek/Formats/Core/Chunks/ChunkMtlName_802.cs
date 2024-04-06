using System;
using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkMtlName_802 : ChunkMtlName
    {
        // Appears to have 4 more Bytes than ChunkMtlName_744
        public override void Read(BinaryReader r)
        {
            base.Read(r);
            
            Name = r.ReadFYString(128);
            NumChildren = (int)r.ReadUInt32();
            MatType = NumChildren == 0 ? MtlNameType.Single : MtlNameType.Library;
            PhysicsType = new MtlNamePhysicsType[NumChildren];
            for (var i = 0; i < NumChildren; i++) PhysicsType[i] = (MtlNamePhysicsType)r.ReadUInt32();
        }
    }
}