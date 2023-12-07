using GameSpec.WbB.Formats.Entity;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x12. 
    /// </summary>
    [PakFileType(PakFileType.Scene)]
    public class Scene : FileType, IGetMetadataInfo
    {
        public readonly ObjectDesc[] Objects;

        public Scene(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Objects = r.ReadL32Array(x => new ObjectDesc(x));
        }

        //: FileTypes.Scene
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{nameof(Scene)}: {Id:X8}", items: new List<MetadataInfo> {
                    new MetadataInfo("Objects", items: Objects.Select(x => {
                        var items = (x as IGetMetadataInfo).GetInfoNodes();
                        var name = items[0].Name.Replace("Object ID: ", "");
                        items.RemoveAt(0);
                        return new MetadataInfo(name, items: items, clickable: true);
                    })),
                })
            };
            return nodes;
        }
    }
}
