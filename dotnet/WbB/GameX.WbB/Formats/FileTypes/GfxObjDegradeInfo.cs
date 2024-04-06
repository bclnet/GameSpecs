using GameX.WbB.Formats.Entity;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x11. 
    /// Contains info on what objects to display at what distance to help with render performance (e.g. low-poly very far away, but high-poly when close)
    /// </summary>
    [PakFileType(PakFileType.DegradeInfo)]
    public class GfxObjDegradeInfo : FileType, IHaveMetaInfo
    {
        public readonly GfxObjInfo[] Degrades;

        public GfxObjDegradeInfo(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Degrades = r.ReadL32FArray(x => new GfxObjInfo(x));
        }

        //: FileTypes.DegradeInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(GfxObjDegradeInfo)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo("Starter Areas", items: Degrades.Select(x => {
                        var items = (x as IHaveMetaInfo).GetInfoNodes();
                        var name = items[0].Name.Replace("Id: ", "");
                        items.RemoveAt(0);
                        return new MetaInfo(name, items: items, clickable: true);
                    })),
                })
            };
            return nodes;
        }
    }
}
