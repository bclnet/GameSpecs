using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class SoundData : IGetMetadataInfo
    {
        public readonly SoundTableData[] Data;
        public readonly uint Unknown;

        public SoundData(BinaryReader r)
        {
            Data = r.ReadL32Array(x => new SoundTableData(x));
            Unknown = r.ReadUInt32();
        }

        //: Entity.SoundData
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo("SoundTable", items: Data.Select(x => {
                    var items = (x as IGetMetadataInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("Sound ID: ", "");
                    items.RemoveAt(0);
                    return new MetadataInfo(name, items: items, clickable: true);
                })),
                new MetadataInfo($"Unknown: {Unknown}"),
            };
            return nodes;
        }
    }
}
