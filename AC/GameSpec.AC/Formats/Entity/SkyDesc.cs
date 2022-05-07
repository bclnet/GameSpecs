using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class SkyDesc : IGetExplorerInfo
    {
        public readonly double TickSize;
        public readonly double LightTickSize;
        public readonly DayGroup[] DayGroups;

        public SkyDesc(BinaryReader r)
        {
            TickSize = r.ReadDouble();
            LightTickSize = r.ReadDouble(); r.AlignBoundary();
            DayGroups = r.ReadL32Array(x => new DayGroup(x));
        }

        //: Entity.SkyDesc
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"TickSize: {TickSize}"),
                new ExplorerInfoNode($"LightTickSize: {LightTickSize}"),
                new ExplorerInfoNode("DayGroups", items: DayGroups.Select((x, i) => new ExplorerInfoNode($"{i:D2}", items: (x as IGetExplorerInfo).GetInfoNodes()))),
            };
            return nodes;
        }
    }
}
