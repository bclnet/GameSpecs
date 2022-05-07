using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class SoundDesc : IGetExplorerInfo
    {
        public readonly AmbientSTBDesc[] STBDesc;

        public SoundDesc(BinaryReader r)
            => STBDesc = r.ReadL32Array(x => new AmbientSTBDesc(x));

        //: Entity.SoundDesc
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode("SoundTable", items: STBDesc.Select((x, i) => {
                    var items = (x as IGetExplorerInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("Ambient Sound Table ID: ", "");
                    items.RemoveAt(0);
                    return new ExplorerInfoNode($"{i}: {name}", items: items, clickable: true);
                })),
            };
            return nodes;
        }
    }
}
