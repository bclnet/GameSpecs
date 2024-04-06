using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public abstract class ChunkCompiledIntSkinVertices : Chunk
    {
        public int Reserved;
        public int NumIntVertices; // Calculate by size of data div by size of IntSkinVertex structure.
        public IntSkinVertex[] IntSkinVertices;

        #region Log
#if LOG
        public override void LogChunk()
        {
            Log($"*** START MorphTargets Chunk ***");
            Log($"    ChunkType:           {ChunkType}");
            Log($"    Node ID:             {ID:X}");
        }
#endif
        #endregion
    }
}