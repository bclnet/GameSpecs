using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x0F. 
    /// They contain, as the name may imply, a set of palettes (0x04 files)
    /// </summary>
    [PakFileType(PakFileType.PaletteSet)]
    public class PaletteSet : FileType, IHaveMetaInfo
    {
        public uint[] PaletteList;

        public PaletteSet(BinaryReader r)
        {
            Id = r.ReadUInt32();
            PaletteList = r.ReadL32TArray<uint>(sizeof(uint));
        }

        /// <summary>
        /// Returns the palette ID (uint, 0x04 file) from the Palette list based on the corresponding hue
        /// Hue is mostly (only?) used in Character Creation data.
        /// "Hue" referred to as "shade" in acclient.c
        /// </summary>
        public uint GetPaletteID(double hue)
        {
            // Make sure the PaletteList has valid data and the hue is within valid ranges
            if (PaletteList.Length == 0 || hue < 0 || hue > 1) return 0;
            // This was the original function - had an issue specifically with Aerfalle's Pallium, WCID 8133
            // var palIndex = Convert.ToInt32(Convert.ToDouble(PaletteList.Count - 0.000001) * hue); // Taken from acclient.c (PalSet::GetPaletteID)
            // Hue is stored in DB as a percent of the total, so do some math to figure out the int position
            var palIndex = (int)((PaletteList.Length - 0.000001) * hue); // Taken from acclient.c (PalSet::GetPaletteID)
            // Since the hue numbers are a little odd, make sure we're in the bounds.
            if (palIndex < 0) palIndex = 0;
            if (palIndex > PaletteList.Length - 1) palIndex = PaletteList.Length - 1;
            return PaletteList[palIndex];
        }

        //: FileTypes.Palette
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(PaletteSet)}: {Id:X8}", items: PaletteList.Select(
                    x => new MetaInfo($"{x:X8}", clickable: true)
                )),
            };
            return nodes;
        }
    }
}
