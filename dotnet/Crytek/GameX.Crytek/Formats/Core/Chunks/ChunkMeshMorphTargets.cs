using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    /// <summary>
    /// Legacy class.  No longer used.
    /// </summary>
    public abstract class ChunkMeshMorphTargets : Chunk
    {
        public uint ChunkIDMesh;
        public int NumMorphVertices;

        public override string ToString()
            => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}, Chunk ID Mesh: {ChunkIDMesh}";

        #region Log
#if LOG
        public override void LogChunk()
        {
            Log($"*** START MorphTargets Chunk ***");
            Log($"    ChunkType:           {ChunkType}");
            Log($"    Node ID:             {ID:X}");
            Log($"    Chunk ID Mesh:       {ChunkIDMesh:X}");
        }
#endif
        #endregion
    }
}