using System.IO;
using System.Linq;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkCompiledExtToIntMap_800 : ChunkCompiledExtToIntMap
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            NumExtVertices = (int)(DataSize / sizeof(ushort));
            Source = r.ReadTArray<ushort>(sizeof(ushort), NumExtVertices);

            // Add to SkinningInfo
            var skin = GetSkinningInfo();
            skin.Ext2IntMap = Source.ToList();
            skin.HasIntToExtMapping = true;
        }
    }
}