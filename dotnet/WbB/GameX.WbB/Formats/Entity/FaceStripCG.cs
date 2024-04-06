using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class FaceStripCG : IHaveMetaInfo
    {
        public readonly uint IconImage;
        public readonly ObjDesc ObjDesc;

        public FaceStripCG(BinaryReader r)
        {
            IconImage = r.ReadUInt32();
            ObjDesc = new ObjDesc(r);
        }

        //: Entity.FaceStripCG
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                IconImage != 0 ? new MetaInfo($"Icon: {IconImage:X8}", clickable: true) : null,
                new MetaInfo("ObjDesc", items: (ObjDesc as IHaveMetaInfo).GetInfoNodes()),
            };
            return nodes;
        }
    }
}
