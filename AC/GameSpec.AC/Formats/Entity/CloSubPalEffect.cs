using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class CloSubPalEffect : IGetExplorerInfo
    {
        /// <summary>
        /// Icon portal.dat 0x06000000
        /// </summary>
        public readonly uint Icon;
        public readonly CloSubPalette[] CloSubPalettes;

        public CloSubPalEffect(BinaryReader r)
        {
            Icon = r.ReadUInt32();
            CloSubPalettes = r.ReadL32Array(x => new CloSubPalette(x));
        }

        //: Entity.ClothingSubPaletteEffect
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Icon: {Icon:X8}", clickable: true),
                new ExplorerInfoNode("SubPalettes", items: CloSubPalettes.Select(x => {
                    var items = (x as IGetExplorerInfo).GetInfoNodes();
                    var name = items[1].Name.Replace("Palette Set: ", "");
                    items.RemoveAt(1);
                    return new ExplorerInfoNode(name, items: items, clickable: true);
                })),
            };
            return nodes;
        }
    }
}
