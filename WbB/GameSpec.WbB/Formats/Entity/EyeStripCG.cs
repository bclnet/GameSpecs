using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class EyeStripCG : IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                IconImage != 0 ? new MetadataInfo($"Icon: {IconImage:X8}", clickable: true) : null,
                IconImageBald != 0 ? new MetadataInfo($"Bald Icon: {IconImageBald:X8}", clickable: true) : null,
                new MetadataInfo("ObjDesc", items: (ObjDesc as IGetMetadataInfo).GetInfoNodes()),
                new MetadataInfo("ObjDescBald", items: (ObjDescBald as IGetMetadataInfo).GetInfoNodes()),
            };
            return nodes;
        }
    }
}
