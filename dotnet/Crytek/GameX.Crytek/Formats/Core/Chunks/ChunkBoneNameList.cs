using System.Collections.Generic;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    /// <summary>
    /// Legacy class. Not used
    /// </summary>
    public abstract class ChunkBoneNameList : Chunk
    {
        public int NumEntities;
        public List<string> BoneNames;

        public override string ToString()
            => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Number of Targets: {NumEntities}";

        #region Log
#if LOG
        public override void LogChunk()
        {
            Log($"*** START MorphTargets Chunk ***");
            Log($"    ChunkType:           {ChunkType}");
            Log($"    Node ID:             {ID:X}");
            Log($"    Number of Targets:   {NumEntities:X}");
            foreach (var name in BoneNames) Log($"    Bone Name:       {name}");
        }
#endif
        #endregion
    }
}