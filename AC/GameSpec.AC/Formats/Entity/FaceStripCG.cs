using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class FaceStripCG : IGetMetadataInfo
    {
        public readonly uint IconImage;
        public readonly ObjDesc ObjDesc;

        public FaceStripCG(BinaryReader r)
        {
            IconImage = r.ReadUInt32();
            ObjDesc = new ObjDesc(r);
        }

        //: Entity.FaceStripCG
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                IconImage != 0 ? new MetadataInfo($"Icon: {IconImage:X8}", clickable: true) : null,
                new MetadataInfo("ObjDesc", items: (ObjDesc as IGetMetadataInfo).GetInfoNodes()),
            };
            return nodes;
        }
    }
}
