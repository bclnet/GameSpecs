using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkMesh_801 : ChunkMesh
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
            VertsAnimID = r.ReadInt32();
            VerticesData = r.ReadInt32();
            NormalsData = r.ReadInt32();            // Chunk ID of the datastream for the normals for this mesh
            UVsData = r.ReadInt32();                // Chunk ID of the Normals datastream
            ColorsData = r.ReadInt32();
            Colors2Data = r.ReadInt32();
            IndicesData = r.ReadInt32();
            TangentsData = r.ReadInt32();
            SkipBytes(r, 16);
            for (var i = 0; i < 4; i++) PhysicsData[i] = r.ReadInt32();
            VertsUVsData = r.ReadInt32();           // This should be a vertsUV Chunk ID.
            ShCoeffsData = r.ReadInt32();
            ShapeDeformationData = r.ReadInt32();
            BoneMapData = r.ReadInt32();
            FaceMapData = r.ReadInt32();
            MinBound = r.ReadVector3();
            MaxBound = r.ReadVector3();
        }
    }
}