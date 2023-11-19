using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class CloSubPalette : IGetMetadataInfo
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
            Ranges = r.ReadL32Array(x => new CloSubPaletteRange(x));
            PaletteSet = r.ReadUInt32();
        }

        //: Entity.ClothingSubPalette
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                Ranges.Length == 1
                    ? new MetadataInfo($"Range: {Ranges[0]}")
                    : new MetadataInfo($"SubPalette Ranges", items: Ranges.Select(x => new MetadataInfo($"{x}"))),
                new MetadataInfo($"Palette Set: {PaletteSet:X8}", clickable: true),
            };
            return nodes;
        }
    }
}
