using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class SoundData : IHaveMetaInfo
    {
        public readonly SoundTableData[] Data;
        public readonly uint Unknown;

        public SoundData(BinaryReader r)
        {
            Data = r.ReadL32FArray(x => new SoundTableData(x));
            Unknown = r.ReadUInt32();
        }

        //: Entity.SoundData
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo("SoundTable", items: Data.Select(x => {
                    var items = (x as IHaveMetaInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("Sound ID: ", "");
                    items.RemoveAt(0);
                    return new MetaInfo(name, items: items, clickable: true);
                })),
                new MetaInfo($"Unknown: {Unknown}"),
            };
            return nodes;
        }
    }
}
