using GameX.WbB.Formats.Entity;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x12. 
    /// </summary>
    [PakFileType(PakFileType.Scene)]
    public class Scene : FileType, IHaveMetaInfo
    {
        public readonly ObjectDesc[] Objects;

        public Scene(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Objects = r.ReadL32FArray(x => new ObjectDesc(x));
        }

        //: FileTypes.Scene
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(Scene)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo("Objects", items: Objects.Select(x => {
                        var items = (x as IHaveMetaInfo).GetInfoNodes();
                        var name = items[0].Name.Replace("Object ID: ", "");
                        items.RemoveAt(0);
                        return new MetaInfo(name, items: items, clickable: true);
                    })),
                })
            };
            return nodes;
        }
    }
}
