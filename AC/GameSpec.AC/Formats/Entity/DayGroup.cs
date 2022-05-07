using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameSpec.AC.Formats.Entity
{
    public class DayGroup : IGetExplorerInfo
    {
        public readonly float ChanceOfOccur;
        public readonly string DayName;
        public readonly SkyObject[] SkyObjects;
        public readonly SkyTimeOfDay[] SkyTime;

        public DayGroup(BinaryReader r)
        {
            ChanceOfOccur = r.ReadSingle();
            DayName = r.ReadL16String(Encoding.Default); r.AlignBoundary();
            SkyObjects = r.ReadL32Array(x => new SkyObject(x));
            SkyTime = r.ReadL32Array(x => new SkyTimeOfDay(x));
        }

        //: Entity.DayGroup
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"ChanceOfOccur: {ChanceOfOccur}"),
                new ExplorerInfoNode($"Weather: {DayName}"),
                new ExplorerInfoNode("SkyObjects", items: SkyObjects.Select((x, i) => new ExplorerInfoNode($"{i}", items: (x as IGetExplorerInfo).GetInfoNodes()))),
                new ExplorerInfoNode("SkyTimesOfDay", items: SkyTime.Select((x, i) => new ExplorerInfoNode($"{i}", items: (x as IGetExplorerInfo).GetInfoNodes()))),
            };
            return nodes;
        }
    }
}
