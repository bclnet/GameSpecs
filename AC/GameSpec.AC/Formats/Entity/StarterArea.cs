using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class StarterArea : IGetExplorerInfo
    {
        public readonly string Name;
        public readonly Position[] Locations;

        public StarterArea(BinaryReader r)
        {
            Name = r.ReadString();
            Locations = r.ReadC32Array(x => new Position(x));
        }

        //: Entity.StarterArea
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Name: {Name}"),
                new ExplorerInfoNode("Locations", items: Locations.Select(x => {
                    var items = (x as IGetExplorerInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("ObjCellID: ", "");
                    items.RemoveAt(0);
                    return new ExplorerInfoNode(name, items: items, clickable: true);
                })),
            };
            return nodes;
        }
    }
}
