using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkMeshSubsets_800 : ChunkMeshSubsets
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            Flags = r.ReadUInt32();   // Might be a ref to this chunk
            NumMeshSubset = (int)r.ReadUInt32();  // number of mesh subsets
            SkipBytes(r, 8);
            MeshSubsets = new MeshSubset[NumMeshSubset];
            for (var i = 0; i < NumMeshSubset; i++)
            {
                MeshSubsets[i].FirstIndex = (int)r.ReadUInt32();
                MeshSubsets[i].NumIndices = (int)r.ReadUInt32();
                MeshSubsets[i].FirstVertex = (int)r.ReadUInt32();
                MeshSubsets[i].NumVertices = (int)r.ReadUInt32();
                MeshSubsets[i].MatID = r.ReadUInt32();
                MeshSubsets[i].Radius = r.ReadSingle();
                MeshSubsets[i].Center.X = r.ReadSingle();
                MeshSubsets[i].Center.Y = r.ReadSingle();
                MeshSubsets[i].Center.Z = r.ReadSingle();
            }
        }
    }
}