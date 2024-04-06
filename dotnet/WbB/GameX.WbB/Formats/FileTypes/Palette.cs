using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x04. 
    /// </summary>
    [PakFileType(PakFileType.Palette)]
    public class Palette : FileType, IHaveMetaInfo
    {
        /// <summary>
        /// Color data is stored in ARGB format
        /// </summary>
        public uint[] Colors;

        public Palette(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Colors = r.ReadL32TArray<uint>(sizeof(uint));
        }

        //: FileTypes.Palette
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(Palette)}: {Id:X8}", items: Colors.Select(
                    x => new MetaInfo(ColorX.ToRGBA(x))
                )),
            };
            return nodes;
        }
    }
}
