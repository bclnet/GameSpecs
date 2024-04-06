using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class StarterArea : IHaveMetaInfo
    {
        public readonly string Name;
        public readonly Position[] Locations;

        public StarterArea(BinaryReader r)
        {
            Name = r.ReadString();
            Locations = r.ReadC32FArray(x => new Position(x));
        }

        //: Entity.StarterArea
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Name: {Name}"),
                new MetaInfo("Locations", items: Locations.Select(x => {
                    var items = (x as IHaveMetaInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("ObjCellID: ", "");
                    items.RemoveAt(0);
                    return new MetaInfo(name, items: items, clickable: true);
                })),
            };
            return nodes;
        }
    }
}
