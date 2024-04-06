using System;
using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkMesh_80000800 : ChunkMesh
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            NumVertSubsets = 1;
            SkipBytes(r, 8);
            NumVertices = MathX.SwapEndian(r.ReadInt32());
            NumIndices = MathX.SwapEndian(r.ReadInt32());           //  Number of indices
            SkipBytes(r, 4);
            MeshSubsetsData = MathX.SwapEndian(r.ReadInt32());      // refers to ID in mesh subsets  1d for candle.  Just 1 for 0x800 type
            SkipBytes(r, 4);
            VerticesData = MathX.SwapEndian(r.ReadInt32());         // ID of the datastream for the vertices for this mesh
            NormalsData = MathX.SwapEndian(r.ReadInt32());          // ID of the datastream for the normals for this mesh
            UVsData = MathX.SwapEndian(r.ReadInt32());              // refers to the ID in the Normals datastream?
            ColorsData = MathX.SwapEndian(r.ReadInt32());
            Colors2Data = MathX.SwapEndian(r.ReadInt32());
            IndicesData = MathX.SwapEndian(r.ReadInt32());
            TangentsData = MathX.SwapEndian(r.ReadInt32());
            ShCoeffsData = MathX.SwapEndian(r.ReadInt32());
            ShapeDeformationData = MathX.SwapEndian(r.ReadInt32());
            BoneMapData = MathX.SwapEndian(r.ReadInt32());
            FaceMapData = MathX.SwapEndian(r.ReadInt32());
            VertMatsData = MathX.SwapEndian(r.ReadInt32());
            SkipBytes(r, 16);
            for (var i = 0; i < 4; i++)
            {
                PhysicsData[i] = MathX.SwapEndian(r.ReadInt32());
                if (PhysicsData[i] != 0) MeshPhysicsData = PhysicsData[i];
            }
            MinBound.X = MathX.SwapEndian(r.ReadSingle());
            MinBound.Y = MathX.SwapEndian(r.ReadSingle());
            MinBound.Z = MathX.SwapEndian(r.ReadSingle());
            MaxBound.X = MathX.SwapEndian(r.ReadSingle());
            MaxBound.Y = MathX.SwapEndian(r.ReadSingle());
            MaxBound.Z = MathX.SwapEndian(r.ReadSingle());
        }
    }
}