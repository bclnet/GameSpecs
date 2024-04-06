using System;
using System.IO;

namespace GameX.Crytek.Formats.Core.Chunks
{
    public class ChunkHelper_744 : ChunkHelper
    {
        public override void Read(BinaryReader r)
        {
            base.Read(r);
            HelperType = (HelperType)Enum.ToObject(typeof(HelperType), r.ReadUInt32());
            if (Version == 0x744) Pos = r.ReadVector3(); // only has the Position.
            else if (Version == 0x362)   // will probably never see these.
            {
                var name = r.ReadChars(64);
                var stringLength = 0;
                for (int i = 0, j = name.Length; i < j; i++) if (name[i] == 0) { stringLength = i; break; }
                Name = new string(name, 0, stringLength);
                HelperType = (HelperType)r.ReadUInt32();
                Pos = r.ReadVector3();
            }
        }
    }
}