using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public abstract partial class ChunkCompiledPhysicalProxies : Chunk        // 0xACDC0003:  Hit boxes?
    {
        // Properties. VERY similar to datastream, since it's essential vertex info.
        public uint Flags2;
        public int NumPhysicalProxies;     // Number of data entries
        public int BytesPerElement;        // Bytes per data entry
        //public uint Reserved1;
        //public uint Reserved2;
        public PhysicalProxy[] PhysicalProxies;

        #region Log
#if LOG
        public override void LogChunk()
        {
            Log($"*** START CompiledPhysicalProxies Chunk ***");
            Log($"    ChunkType:           {ChunkType}");
            Log($"    Node ID:             {ID:X}");
            Log($"    Number of Targets:   {NumPhysicalProxies:X}");
        }
#endif
        #endregion
    }
}