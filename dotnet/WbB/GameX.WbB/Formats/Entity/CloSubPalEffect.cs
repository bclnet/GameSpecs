using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class CloSubPalEffect : IHaveMetaInfo
    {
        /// <summary>
        /// Icon portal.dat 0x06000000
        /// </summary>
        public readonly uint Icon;
        public readonly CloSubPalette[] CloSubPalettes;

        public CloSubPalEffect(BinaryReader r)
        {
            Icon = r.ReadUInt32();
            CloSubPalettes = r.ReadL32FArray(x => new CloSubPalette(x));
        }

        //: Entity.ClothingSubPaletteEffect
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Icon: {Icon:X8}", clickable: true),
                new MetaInfo("SubPalettes", items: CloSubPalettes.Select(x => {
                    var items = (x as IHaveMetaInfo).GetInfoNodes();
                    var name = items[1].Name.Replace("Palette Set: ", "");
                    items.RemoveAt(1);
                    return new MetaInfo(name, items: items, clickable: true);
                })),
            };
            return nodes;
        }
    }
}
