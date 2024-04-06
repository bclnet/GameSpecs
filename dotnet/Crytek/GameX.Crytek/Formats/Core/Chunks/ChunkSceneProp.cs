using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public abstract class ChunkSceneProp : Chunk     // cccc0008 
    {
        // This chunk isn't really used, but contains some data probably necessary for the game.
        // Size for 0x744 type is always 0xBB4 (test this)
        public int NumProps; // number of elements in the props array  (31 for type 0x744)
        public string[] PropKey;
        public string[] PropValue;

        public override string ToString()
            => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}";

        #region Log
#if LOG
        public override void LogChunk()
        {
            Log($"*** START SceneProp Chunk ***");
            Log($"    ChunkType:   {ChunkType}");
            Log($"    Version:     {Version:X}");
            Log($"    ID:          {ID:X}");
            for (var i = 0; i < NumProps; i++) Log($"{PropKey[i],30}{PropValue[i],20}");
            Log("*** END SceneProp Chunk ***");
        }
#endif
        #endregion
    }
}