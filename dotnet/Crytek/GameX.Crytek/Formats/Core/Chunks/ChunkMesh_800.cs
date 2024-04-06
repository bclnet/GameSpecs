using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkMesh_800 : ChunkMesh
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            NumVertSubsets = 1;
            SkipBytes(r, 8);
            NumVertices = r.ReadInt32();
            NumIndices = r.ReadInt32();             //  Number of indices
            SkipBytes(r, 4);
            MeshSubsetsData = r.ReadInt32();        // refers to ID in mesh subsets  1d for candle.  Just 1 for 0x800 type
            SkipBytes(r, 4);
            VerticesData = r.ReadInt32();           // ID of the datastream for the vertices for this mesh
            NormalsData = r.ReadInt32();            // ID of the datastream for the normals for this mesh
            UVsData = r.ReadInt32();                // refers to the ID in the Normals datastream?
            ColorsData = r.ReadInt32();
            Colors2Data = r.ReadInt32();
            IndicesData = r.ReadInt32();
            TangentsData = r.ReadInt32();
            ShCoeffsData = r.ReadInt32();
            ShapeDeformationData = r.ReadInt32();
            BoneMapData = r.ReadInt32();
            FaceMapData = r.ReadInt32();
            VertMatsData = r.ReadInt32();
            SkipBytes(r, 16);
            for (var i = 0; i < 4; i++)
            {
                PhysicsData[i] = r.ReadInt32();
                if (PhysicsData[i] != 0) MeshPhysicsData = PhysicsData[i];
            }
            MinBound = r.ReadVector3();
            MaxBound = r.ReadVector3();
        }
    }
}