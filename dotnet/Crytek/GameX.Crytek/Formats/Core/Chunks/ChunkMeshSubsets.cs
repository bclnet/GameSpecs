using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public abstract class ChunkMeshSubsets : Chunk // cccc0017:  The different parts of a mesh.  Needed for obj exporting
    {
        public uint Flags; // probably the offset
        public int NumMeshSubset; // number of mesh subsets
        public MeshSubset[] MeshSubsets;

        // For bone ID meshes? Not sure where this is used yet.
        public int NumberOfBoneIDs;
        public ushort[] BoneIDs;

        public override string ToString()
            => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}, Number of Mesh Subsets: {NumMeshSubset}";

        #region Log
#if LOG
        public override void LogChunk()
        {
            Log("*** START MESH SUBSET CHUNK ***");
            Log("    ChunkType:       {ChunkType}");
            Log("    Mesh SubSet ID:  {ID:X}");
            Log("    Number of Mesh Subsets: {NumMeshSubset}");
            for (var i = 0; i < NumMeshSubset; i++)
            {
                Log($"        ** Mesh Subset:          {i}");
                Log($"           First Index:          {MeshSubsets[i].FirstIndex}");
                Log($"           Number of Indices:    {MeshSubsets[i].NumIndices}");
                Log($"           First Vertex:         {MeshSubsets[i].FirstVertex}");
                Log($"           Number of Vertices:   {MeshSubsets[i].NumVertices}  (next will be {MeshSubsets[i].NumVertices + MeshSubsets[i].FirstVertex})");
                Log($"           Material ID:          {MeshSubsets[i].MatID}");
                Log($"           Radius:               {MeshSubsets[i].Radius}");
                Log($"           Center:   {MeshSubsets[i].Center.X},{MeshSubsets[i].Center.Y},{MeshSubsets[i].Center.Z}");
                Log($"        ** Mesh Subset {i} End");
            }
            Log("*** END MESH SUBSET CHUNK ***");
        }
#endif
        #endregion
    }
}