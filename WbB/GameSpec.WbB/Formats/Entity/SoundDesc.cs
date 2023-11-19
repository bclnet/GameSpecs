using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class SoundDesc : IGetMetadataInfo
    {
        public readonly AmbientSTBDesc[] STBDesc;

        public SoundDesc(BinaryReader r)
            => STBDesc = r.ReadL32Array(x => new AmbientSTBDesc(x));

        //: Entity.SoundDesc
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo("SoundTable", items: STBDesc.Select((x, i) => {
                    var items = (x as IGetMetadataInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("Ambient Sound Table ID: ", "");
                    items.RemoveAt(0);
                    return new MetadataInfo($"{i}: {name}", items: items, clickable: true);
                })),
            };
            return nodes;
        }
    }
}
