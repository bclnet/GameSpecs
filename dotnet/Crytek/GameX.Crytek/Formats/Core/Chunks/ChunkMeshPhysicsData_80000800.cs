using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    class ChunkMeshPhysicsData_80000800 : ChunkMeshPhysicsData
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            // TODO: Implement ChunkMeshPhysicsData ver 0x800.
        }
    }
}