using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class ClothingBaseEffect : IHaveMetaInfo
    {
        public readonly CloObjectEffect[] CloObjectEffects;

        public ClothingBaseEffect(BinaryReader r)
            => CloObjectEffects = r.ReadL32FArray(x => new CloObjectEffect(x));

        //: Entity.ClothingBaseEffect
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo("Object Effects", items: CloObjectEffects.OrderBy(i => i.Index).Select(x => {
                    var items = (x as IHaveMetaInfo).GetInfoNodes();
                    var name = items[0].Name;
                    items.RemoveAt(0);
                    return new MetaInfo(name, items: items);
                })),
            };
            return nodes;
        }
    }
}
