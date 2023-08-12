using System;
using System.IO;
using System.Linq;

namespace GameSpec.Cry.Formats.Core.Chunks
{
    public class ChunkBoneNameList_745 : ChunkBoneNameList
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);

            BoneNames = r.ReadCString().Split(' ').ToList();
        }
    }
}