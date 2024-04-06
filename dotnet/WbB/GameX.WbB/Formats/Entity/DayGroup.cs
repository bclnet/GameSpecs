using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameX.WbB.Formats.Entity
{
    public class DayGroup : IHaveMetaInfo
    {
        public readonly float ChanceOfOccur;
        public readonly string DayName;
        public readonly SkyObject[] SkyObjects;
        public readonly SkyTimeOfDay[] SkyTime;

        public DayGroup(BinaryReader r)
        {
            ChanceOfOccur = r.ReadSingle();
            DayName = r.ReadL16Encoding(Encoding.Default); r.Align();
            SkyObjects = r.ReadL32FArray(x => new SkyObject(x));
            SkyTime = r.ReadL32FArray(x => new SkyTimeOfDay(x));
        }

        //: Entity.DayGroup
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"ChanceOfOccur: {ChanceOfOccur}"),
                new MetaInfo($"Weather: {DayName}"),
                new MetaInfo("SkyObjects", items: SkyObjects.Select((x, i) => new MetaInfo($"{i}", items: (x as IHaveMetaInfo).GetInfoNodes()))),
                new MetaInfo("SkyTimesOfDay", items: SkyTime.Select((x, i) => new MetaInfo($"{i}", items: (x as IHaveMetaInfo).GetInfoNodes()))),
            };
            return nodes;
        }
    }
}
