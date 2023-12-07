using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class CloSubPalEffect : IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Icon: {Icon:X8}", clickable: true),
                new MetadataInfo("SubPalettes", items: CloSubPalettes.Select(x => {
                    var items = (x as IGetMetadataInfo).GetInfoNodes();
                    var name = items[1].Name.Replace("Palette Set: ", "");
                    items.RemoveAt(1);
                    return new MetadataInfo(name, items: items, clickable: true);
                })),
            };
            return nodes;
        }
    }
}
