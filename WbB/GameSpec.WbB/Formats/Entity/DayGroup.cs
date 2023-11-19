using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameSpec.WbB.Formats.Entity
{
    public class DayGroup : IGetMetadataInfo
    {
        public readonly float ChanceOfOccur;
        public readonly string DayName;
        public readonly SkyObject[] SkyObjects;
        public readonly SkyTimeOfDay[] SkyTime;

        public DayGroup(BinaryReader r)
        {
            ChanceOfOccur = r.ReadSingle();
            DayName = r.ReadL16Encoding(Encoding.Default); r.Align();
            SkyObjects = r.ReadL32Array(x => new SkyObject(x));
            SkyTime = r.ReadL32Array(x => new SkyTimeOfDay(x));
        }

        //: Entity.DayGroup
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"ChanceOfOccur: {ChanceOfOccur}"),
                new MetadataInfo($"Weather: {DayName}"),
                new MetadataInfo("SkyObjects", items: SkyObjects.Select((x, i) => new MetadataInfo($"{i}", items: (x as IGetMetadataInfo).GetInfoNodes()))),
                new MetadataInfo("SkyTimesOfDay", items: SkyTime.Select((x, i) => new MetadataInfo($"{i}", items: (x as IGetMetadataInfo).GetInfoNodes()))),
            };
            return nodes;
        }
    }
}
