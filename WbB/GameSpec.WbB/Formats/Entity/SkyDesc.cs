using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class SkyDesc : IGetMetadataInfo
    {
        public readonly double TickSize;
        public readonly double LightTickSize;
        public readonly DayGroup[] DayGroups;

        public SkyDesc(BinaryReader r)
        {
            TickSize = r.ReadDouble();
            LightTickSize = r.ReadDouble(); r.Align();
            DayGroups = r.ReadL32Array(x => new DayGroup(x));
        }

        //: Entity.SkyDesc
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"TickSize: {TickSize}"),
                new MetadataInfo($"LightTickSize: {LightTickSize}"),
                new MetadataInfo("DayGroups", items: DayGroups.Select((x, i) => new MetadataInfo($"{i:D2}", items: (x as IGetMetadataInfo).GetInfoNodes()))),
            };
            return nodes;
        }
    }
}
