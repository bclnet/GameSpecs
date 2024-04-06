using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkMesh_802 : ChunkMesh
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            Flags1 = r.ReadInt32();
            Flags2 = r.ReadInt32();
            NumVertices = r.ReadInt32();
            NumIndices = r.ReadInt32();
            NumVertSubsets = r.ReadInt32();
            MeshSubsetsData = r.ReadInt32();        // Chunk ID of mesh subsets 
            SkipBytes(r, 4);
            VerticesData = r.ReadInt32();
            SkipBytes(r, 28);
            NormalsData = r.ReadInt32();            // Chunk ID of the datastream for the normals for this mesh
            SkipBytes(r, 28);
            UVsData = r.ReadInt32();               // Chunk ID of the Normals datastream
            SkipBytes(r, 28);
            ColorsData = r.ReadInt32();
            SkipBytes(r, 28);
            Colors2Data = r.ReadInt32();
            SkipBytes(r, 28);
            IndicesData = r.ReadInt32();
            SkipBytes(r, 28);
            TangentsData = r.ReadInt32();
            SkipBytes(r, 28);
            SkipBytes(r, 16);
            for (var i = 0; i < 4; i++) PhysicsData[i] = r.ReadInt32();
            VertsUVsData = r.ReadInt32();          // This should be a vertsUV Chunk ID.
            SkipBytes(r, 28);
            ShCoeffsData = r.ReadInt32();
            SkipBytes(r, 28);
            ShapeDeformationData = r.ReadInt32();
            SkipBytes(r, 28);
            BoneMapData = r.ReadInt32();
            SkipBytes(r, 28);
            FaceMapData = r.ReadInt32();
            SkipBytes(r, 28);
            SkipBytes(r, 16);
            SkipBytes(r, 96);                      // Lots of unknown data here.
            MinBound = r.ReadVector3();
            MaxBound = r.ReadVector3();
        }
    }
}