using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class HairStyleCG : IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                IconImage != 0 ? new MetadataInfo($"Icon: {IconImage:X8}", clickable: true) : null,
                Bald ? new MetadataInfo($"Bald: True") : null,
                AlternateSetup != 0 ? new MetadataInfo($"Alternate Setup: {AlternateSetup:X8}", clickable: true) : null,
                new MetadataInfo("ObjDesc", items: (ObjDesc as IGetMetadataInfo).GetInfoNodes()),
            };
            return nodes;
        }
    }
}
