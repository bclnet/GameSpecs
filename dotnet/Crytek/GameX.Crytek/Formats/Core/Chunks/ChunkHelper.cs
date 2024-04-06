using System.Numerics;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    /// <summary>
    /// Helper chunk. This is the top level, then nodes, then mesh, then mesh subsets. CCCC0001
    /// </summary>
    public abstract class ChunkHelper : Chunk // CCCC0001
    {
        public string Name;
        public HelperType HelperType;
        public Vector3 Pos;
        public Matrix4x4 Transform;

        public override string ToString()
            => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}";

        #region Log
#if LOG
        public override void LogChunk()
        {
            Log($"*** START Helper Chunk ***");
            Log($"    ChunkType:   {ChunkType}");
            Log($"    Version:     {Version:X}");
            Log($"    ID:          {ID:X}");
            Log($"    HelperType:  {HelperType}");
            Log($"    Position:    {Pos.X}, {Pos.Y}, {Pos.Z}");
            Log($"*** END Helper Chunk ***");
        }
#endif
        #endregion
    }
}