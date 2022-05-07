using GameSpec.AC.Formats.Entity;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x12. 
    /// </summary>
    [PakFileType(PakFileType.Scene)]
    public class Scene : FileType, IGetExplorerInfo
    {
        public readonly ObjectDesc[] Objects;

        public Scene(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Objects = r.ReadL32Array(x => new ObjectDesc(x));
        }

        //: FileTypes.Scene
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(Scene)}: {Id:X8}", items: new List<ExplorerInfoNode> {
                    new ExplorerInfoNode("Objects", items: Objects.Select(x => {
                        var items = (x as IGetExplorerInfo).GetInfoNodes();
                        var name = items[0].Name.Replace("Object ID: ", "");
                        items.RemoveAt(0);
                        return new ExplorerInfoNode(name, items: items, clickable: true);
                    })),
                })
            };
            return nodes;
        }
    }
}
