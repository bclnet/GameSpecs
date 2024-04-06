using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class SoundDesc : IHaveMetaInfo
    {
        public readonly AmbientSTBDesc[] STBDesc;

        public SoundDesc(BinaryReader r)
            => STBDesc = r.ReadL32FArray(x => new AmbientSTBDesc(x));

        //: Entity.SoundDesc
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo("SoundTable", items: STBDesc.Select((x, i) => {
                    var items = (x as IHaveMetaInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("Ambient Sound Table ID: ", "");
                    items.RemoveAt(0);
                    return new MetaInfo($"{i}: {name}", items: items, clickable: true);
                })),
            };
            return nodes;
        }
    }
}
