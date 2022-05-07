using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class SoundData : IGetExplorerInfo
    {
        public readonly SoundTableData[] Data;
        public readonly uint Unknown;

        public SoundData(BinaryReader r)
        {
            Data = r.ReadL32Array(x => new SoundTableData(x));
            Unknown = r.ReadUInt32();
        }

        //: Entity.SoundData
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode("SoundTable", items: Data.Select(x => {
                    var items = (x as IGetExplorerInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("Sound ID: ", "");
                    items.RemoveAt(0);
                    return new ExplorerInfoNode(name, items: items, clickable: true);
                })),
                new ExplorerInfoNode($"Unknown: {Unknown}"),
            };
            return nodes;
        }
    }
}
