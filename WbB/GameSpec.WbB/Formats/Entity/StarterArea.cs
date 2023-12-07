using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class StarterArea : IGetMetadataInfo
    {
        public readonly string Name;
        public readonly Position[] Locations;

        public StarterArea(BinaryReader r)
        {
            Name = r.ReadString();
            Locations = r.ReadC32Array(x => new Position(x));
        }

        //: Entity.StarterArea
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Name: {Name}"),
                new MetadataInfo("Locations", items: Locations.Select(x => {
                    var items = (x as IGetMetadataInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("ObjCellID: ", "");
                    items.RemoveAt(0);
                    return new MetadataInfo(name, items: items, clickable: true);
                })),
            };
            return nodes;
        }
    }
}
