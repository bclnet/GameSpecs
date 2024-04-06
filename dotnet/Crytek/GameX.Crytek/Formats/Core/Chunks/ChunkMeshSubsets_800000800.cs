using System;
using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkMeshSubsets_800000800 : ChunkMeshSubsets
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            Flags = MathX.SwapEndian(r.ReadUInt32());   // Might be a ref to this chunk
            NumMeshSubset = (int)MathX.SwapEndian(r.ReadUInt32());  // number of mesh subsets
            SkipBytes(r, 8);
            MeshSubsets = new MeshSubset[NumMeshSubset];
            for (var i = 0; i < NumMeshSubset; i++)
            {
                MeshSubsets[i].FirstIndex = (int)MathX.SwapEndian(r.ReadUInt32());
                MeshSubsets[i].NumIndices = (int)MathX.SwapEndian(r.ReadUInt32());
                MeshSubsets[i].FirstVertex = (int)MathX.SwapEndian(r.ReadUInt32());
                MeshSubsets[i].NumVertices = (int)MathX.SwapEndian(r.ReadUInt32());
                MeshSubsets[i].MatID = MathX.SwapEndian(r.ReadUInt32());
                MeshSubsets[i].Radius = MathX.SwapEndian(r.ReadSingle());
                MeshSubsets[i].Center.X = MathX.SwapEndian(r.ReadSingle());
                MeshSubsets[i].Center.Y = MathX.SwapEndian(r.ReadSingle());
                MeshSubsets[i].Center.Z = MathX.SwapEndian(r.ReadSingle());
            }
        }
    }
}