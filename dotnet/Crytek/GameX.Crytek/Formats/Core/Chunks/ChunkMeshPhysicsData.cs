using GameX.Crytek.Formats.Models;
using static OpenStack.Debug;

namespace GameX.Crytek.Formats.Core.Chunks
{
    /// <summary>
    /// Collision mesh or something like that. TODO
    /// </summary>
    /// <seealso cref="GameX.Crytek.Formats.Core.Chunks.Chunk" />
    class ChunkMeshPhysicsData : Chunk
    {
        public int PhysicsDataSize;             // Size of the physical data at the end of the chunk.
        public int Flags;
        public int TetrahedraDataSize;          // Bytes per data entry
        public int TetrahedraID;                // Chunk ID of the data stream
        public ChunkDataStream Tetrahedra;
        public uint Reserved1;
        public uint Reserved2;

        public PhysicsData physicsData;  // if physicsdatasize != 0
        public byte[] TetrahedraData; // Array length TetrahedraDataSize.  

        #region Log
#if LOG
        public override void LogChunk()
        {
            Log($"*** START CompiledBone Chunk ***");
            Log($"    ChunkType:           {ChunkType}");
            Log($"    Node ID:             {ID:X}");
            Log($"    Node ID:             {PhysicsDataSize:X}");
            Log($"    Node ID:             {TetrahedraDataSize:X}");
            Log($"    Node ID:             {TetrahedraID:X}");
            Log($"    Node ID:             {ID:X}");
        }
#endif
        #endregion
    }
}