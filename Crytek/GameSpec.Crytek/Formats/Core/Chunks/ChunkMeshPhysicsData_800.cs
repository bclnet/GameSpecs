﻿using System.IO;

namespace GameSpec.Crytek.Formats.Core.Chunks
{
    class ChunkMeshPhysicsData_800 : ChunkMeshPhysicsData
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            // TODO: Implement ChunkMeshPhysicsData ver 0x800.
        }
    }
}