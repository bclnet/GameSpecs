using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class HairStyleCG : IHaveMetaInfo
    {
        public readonly uint IconImage;
        public readonly bool Bald;
        public readonly uint AlternateSetup;
        public readonly ObjDesc ObjDesc;

        public HairStyleCG(BinaryReader r)
        {
            IconImage = r.ReadUInt32();
            Bald = r.ReadByte() == 1;
            AlternateSetup = r.ReadUInt32();
            ObjDesc = new ObjDesc(r);
        }

        //: Entity.HairStyleCG
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                IconImage != 0 ? new MetaInfo($"Icon: {IconImage:X8}", clickable: true) : null,
                Bald ? new MetaInfo($"Bald: True") : null,
                AlternateSetup != 0 ? new MetaInfo($"Alternate Setup: {AlternateSetup:X8}", clickable: true) : null,
                new MetaInfo("ObjDesc", items: (ObjDesc as IHaveMetaInfo).GetInfoNodes()),
            };
            return nodes;
        }
    }
}
