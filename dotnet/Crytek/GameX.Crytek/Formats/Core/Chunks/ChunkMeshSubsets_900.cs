using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkMeshSubsets_900 : ChunkMeshSubsets
    {
        public ChunkMeshSubsets_900(int numMeshSubset)
            => NumMeshSubset = numMeshSubset;

        public override void Read(BinaryReader r)
        {
            base.Read(r);

            MeshSubsets = new MeshSubset[NumMeshSubset];
            for (var i = 0; i < NumMeshSubset; i++)
            {
                MeshSubsets[i].MatID = (uint)r.ReadInt32();
                MeshSubsets[i].FirstIndex = r.ReadInt32();
                MeshSubsets[i].NumIndices = r.ReadInt32();
                MeshSubsets[i].FirstVertex = r.ReadInt32();
                MeshSubsets[i].NumVertices = r.ReadInt32();
                MeshSubsets[i].Radius = r.ReadSingle();
                MeshSubsets[i].Center = r.ReadVector3();
                SkipBytes(r, 12); // 3 unknowns; possibly floats;
            }
        }
    }
}