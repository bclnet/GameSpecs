using System;
using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkTimingFormat_918 : ChunkTimingFormat
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            SecsPerTick = r.ReadSingle();
            TicksPerFrame = r.ReadInt32();
            GlobalRange.Name = r.ReadFYString(32); // Name is technically a String32, but F those structs
            GlobalRange.Start = r.ReadInt32();
            GlobalRange.End = r.ReadInt32();
        }
    }
}