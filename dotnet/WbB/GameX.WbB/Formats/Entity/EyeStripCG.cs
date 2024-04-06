using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class EyeStripCG : IHaveMetaInfo
    {
        public readonly uint IconImage;
        public readonly uint IconImageBald;
        public readonly ObjDesc ObjDesc;
        public readonly ObjDesc ObjDescBald;

        public EyeStripCG(BinaryReader r)
        {
            IconImage = r.ReadUInt32();
            IconImageBald = r.ReadUInt32();
            ObjDesc = new ObjDesc(r);
            ObjDescBald = new ObjDesc(r);
        }

        //: Entity.EyeStripCG
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                IconImage != 0 ? new MetaInfo($"Icon: {IconImage:X8}", clickable: true) : null,
                IconImageBald != 0 ? new MetaInfo($"Bald Icon: {IconImageBald:X8}", clickable: true) : null,
                new MetaInfo("ObjDesc", items: (ObjDesc as IHaveMetaInfo).GetInfoNodes()),
                new MetaInfo("ObjDescBald", items: (ObjDescBald as IHaveMetaInfo).GetInfoNodes()),
            };
            return nodes;
        }
    }
}
