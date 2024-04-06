using System;
using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkMtlName_80000800 : ChunkMtlName
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            MatType = (MtlNameType)MathX.SwapEndian(r.ReadUInt32());
            // if 0x01, then material lib. If 0x12, mat name. This is actually a bitstruct.
            NFlags2 = MathX.SwapEndian(r.ReadUInt32()); // NFlags2
            Name = r.ReadFYString(128);
            PhysicsType = new[] { (MtlNamePhysicsType)MathX.SwapEndian(r.ReadUInt32()) };
            NumChildren = (int)MathX.SwapEndian(r.ReadUInt32());
            // Now we need to read the Children references. 2 parts; the number of children, and then 66 - numchildren padding
            ChildIDs = new uint[NumChildren];
            for (var i = 0; i < NumChildren; i++) ChildIDs[i] = MathX.SwapEndian(r.ReadUInt32());
            SkipBytes(r, 32);
        }
    }
}