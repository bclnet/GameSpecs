using System;
using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkMtlName_800 : ChunkMtlName
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            MatType = (MtlNameType)r.ReadUInt32();
            // if 0x01, then material lib. If 0x12, mat name. This is actually a bitstruct.
            NFlags2 = r.ReadUInt32(); // NFlags2
            Name = r.ReadFYString(128);
            PhysicsType = new[] { (MtlNamePhysicsType)r.ReadUInt32() };
            NumChildren = (int)r.ReadUInt32();
            // Now we need to read the Children references. 2 parts; the number of children, and then 66 - numchildren padding
            ChildIDs = r.ReadTArray<uint>(sizeof(uint), NumChildren);
            SkipBytes(r, 32);
        }
    }
}