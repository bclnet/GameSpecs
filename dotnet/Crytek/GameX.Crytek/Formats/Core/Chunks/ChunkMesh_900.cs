using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkMesh_900 : ChunkMesh
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            Flags1 = 0;
            Flags2 = r.ReadInt32();
            NumVertices = r.ReadInt32();
            NumIndices = r.ReadInt32();
            NumVertSubsets = (int)r.ReadUInt32();
            SkipBytes(r, 4);
            MinBound = r.ReadVector3();
            MaxBound = r.ReadVector3();
            ID = 2; // Node chunk ID = 1
            IndicesData = 4;
            VertsUVsData = 5;
            NormalsData = 6;
            TangentsData = 7;
            BoneMapData = 8;
            ColorsData = 9;
        }
    }
}