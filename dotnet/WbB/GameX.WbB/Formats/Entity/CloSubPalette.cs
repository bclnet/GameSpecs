using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class CloSubPalette : IHaveMetaInfo
    {
        /// <summary>
        /// Contains a list of valid offsets & color values
        /// </summary>
        public readonly CloSubPaletteRange[] Ranges;
        /// <summary>
        /// Icon portal.dat 0x0F000000
        /// </summary>
        public readonly uint PaletteSet;

        public CloSubPalette(BinaryReader r)
        {
            Ranges = r.ReadL32FArray(x => new CloSubPaletteRange(x));
            PaletteSet = r.ReadUInt32();
        }

        //: Entity.ClothingSubPalette
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                Ranges.Length == 1
                    ? new MetaInfo($"Range: {Ranges[0]}")
                    : new MetaInfo($"SubPalette Ranges", items: Ranges.Select(x => new MetaInfo($"{x}"))),
                new MetaInfo($"Palette Set: {PaletteSet:X8}", clickable: true),
            };
            return nodes;
        }
    }
}
