using GameSpec.AC.Formats.Entity;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x11. 
    /// Contains info on what objects to display at what distance to help with render performance (e.g. low-poly very far away, but high-poly when close)
    /// </summary>
    [PakFileType(PakFileType.DegradeInfo)]
    public class GfxObjDegradeInfo : FileType, IGetExplorerInfo
    {
        public readonly GfxObjInfo[] Degrades;

        public GfxObjDegradeInfo(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Degrades = r.ReadL32Array(x => new GfxObjInfo(x));
        }

        //: FileTypes.DegradeInfo
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(GfxObjDegradeInfo)}: {Id:X8}", items: new List<ExplorerInfoNode> {
                    new ExplorerInfoNode("Starter Areas", items: Degrades.Select(x => {
                        var items = (x as IGetExplorerInfo).GetInfoNodes();
                        var name = items[0].Name.Replace("Id: ", "");
                        items.RemoveAt(0);
                        return new ExplorerInfoNode(name, items: items, clickable: true);
                    })),
                })
            };
            return nodes;
        }
    }
}
