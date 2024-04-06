using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkHeader_900 : ChunkHeader
    {
        public override void Read(BinaryReader r)
        {
            ChunkType = (ChunkType)r.ReadUInt32();
            Version = r.ReadUInt32();
            Offset = (uint)r.ReadUInt64(); // All other versions use uint. No idea why uint64 is needed.
            // 0x900 version chunks no longer have chunk IDs. Use a randon mumber for now.
            ID = GetNextRandom();
        }
    }
}