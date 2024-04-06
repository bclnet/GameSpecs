using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public abstract class ChunkTimingFormat : Chunk  // cccc000e:  Timing format chunk
    {
        // This chunk doesn't have an ID, although one may be assigned in the chunk table.
        public float SecsPerTick;
        public int TicksPerFrame;
        public RangeEntity GlobalRange;
        public int NumSubRanges;

        public override string ToString()
            => $@"Chunk Type: {ChunkType}, ID: {ID:X}, Version: {Version}, Ticks per Frame: {TicksPerFrame}, Seconds per Tick: {SecsPerTick}";

        #region Log
#if LOG
        public override void LogChunk()
        {
            Log($"*** TIMING CHUNK ***");
            Log($"    ID: {ID:X}");
            Log($"    Version: {Version:X}");
            Log($"    Secs Per Tick: {SecsPerTick}");
            Log($"    Ticks Per Frame: {TicksPerFrame}");
            Log($"    Global Range:  Name: {GlobalRange.Name}");
            Log($"    Global Range:  Start: {GlobalRange.Start}");
            Log($"    Global Range:  End:  {GlobalRange.End}");
            Log($"*** END TIMING CHUNK ***");
        }
#endif
        #endregion
    }
}