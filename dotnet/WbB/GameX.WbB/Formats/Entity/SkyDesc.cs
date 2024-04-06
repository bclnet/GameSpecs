using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class SkyDesc : IHaveMetaInfo
    {
        public readonly double TickSize;
        public readonly double LightTickSize;
        public readonly DayGroup[] DayGroups;

        public SkyDesc(BinaryReader r)
        {
            TickSize = r.ReadDouble();
            LightTickSize = r.ReadDouble(); r.Align();
            DayGroups = r.ReadL32FArray(x => new DayGroup(x));
        }

        //: Entity.SkyDesc
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"TickSize: {TickSize}"),
                new MetaInfo($"LightTickSize: {LightTickSize}"),
                new MetaInfo("DayGroups", items: DayGroups.Select((x, i) => new MetaInfo($"{i:D2}", items: (x as IHaveMetaInfo).GetInfoNodes()))),
            };
            return nodes;
        }
    }
}
