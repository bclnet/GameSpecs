using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class ClothingBaseEffect : IGetExplorerInfo
    {
        public readonly CloObjectEffect[] CloObjectEffects;

        public ClothingBaseEffect(BinaryReader r)
            => CloObjectEffects = r.ReadL32Array(x => new CloObjectEffect(x));

        //: Entity.ClothingBaseEffect
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode("Object Effects", items: CloObjectEffects.OrderBy(i => i.Index).Select(x => {
                    var items = (x as IGetExplorerInfo).GetInfoNodes();
                    var name = items[0].Name;
                    items.RemoveAt(0);
                    return new ExplorerInfoNode(name, items: items);
                })),
            };
            return nodes;
        }
    }
}
