using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public abstract class ChunkSourceInfo : Chunk  // cccc0013:  Source Info chunk.  Pretty useless overall
    {
        public string SourceFile;
        public string Date;
        public string Author;

        public override string ToString()
            => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Sourcefile: {SourceFile}";

        #region Log
#if LOG
        public override void LogChunk()
        {
            Log($"*** SOURCE INFO CHUNK ***");
            Log($"    ID: {ID:X}");
            Log($"    Sourcefile: {SourceFile}.");
            Log($"    Date:       {Date}.");
            Log($"    Author:     {Author}.");
            Log($"*** END SOURCE INFO CHUNK ***");
        }
#endif
        #endregion
    }
}