using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class ClothingBaseEffect : IGetMetadataInfo
    {
        public readonly CloObjectEffect[] CloObjectEffects;

        public ClothingBaseEffect(BinaryReader r)
            => CloObjectEffects = r.ReadL32Array(x => new CloObjectEffect(x));

        //: Entity.ClothingBaseEffect
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo("Object Effects", items: CloObjectEffects.OrderBy(i => i.Index).Select(x => {
                    var items = (x as IGetMetadataInfo).GetInfoNodes();
                    var name = items[0].Name;
                    items.RemoveAt(0);
                    return new MetadataInfo(name, items: items);
                })),
            };
            return nodes;
        }
    }
}
