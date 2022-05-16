using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameSpec.AC.Formats.Entity
{
    public class Season : IGetMetadataInfo
    {
        public readonly uint StartDate;
        public readonly string Name;

        public Season(BinaryReader r)
        {
            StartDate = r.ReadUInt32();
            Name = r.ReadL16String(Encoding.Default); r.Align();
        }

        //: Entity.Season
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"StartDate: {StartDate}"),
                new MetadataInfo($"Name: {Name}"),
            };
            return nodes;
        }
    }
}
