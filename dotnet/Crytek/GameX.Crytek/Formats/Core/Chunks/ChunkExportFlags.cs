using System.Text;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public abstract class ChunkExportFlags : Chunk  // cccc0015:  Export Flags
    {
        public uint ChunkOffset;                    // for some reason the offset of Export Flag chunk is stored here.
        public uint Flags;                          // ExportFlags type technically, but it's just 1 value
        public uint[] RCVersion;                    // 4 uints
        public string RCVersionString;              // Technically String16

        public override string ToString()
            => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}";

        #region Log
#if LOG
        public override void LogChunk()
        {
            Log($"*** START EXPORT FLAGS ***");
            Log($"    Export Chunk ID: {ID:X}");
            Log($"    ChunkType: {ChunkType}");
            Log($"    Version: {Version}");
            Log($"    Flags: {Flags}");
            var b = new StringBuilder("    RC Version: ");
            for (var i = 0; i < 4; i++) b.Append(RCVersion[i]);
            Log(b.ToString());
            Log();
            Log("    RCVersion String: {RCVersionString}");
            Log("*** END EXPORT FLAGS ***");
        }
#endif
        #endregion
    }
}