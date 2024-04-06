using GameX.WbB.Formats.Entity;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x40.
    /// It is essentially a map to a specific texture file (spritemap) that contains all the characters in this font.
    /// </summary>
    [PakFileType(PakFileType.Font)]
    public class Font : FileType, IHaveMetaInfo
    {
        public readonly uint MaxCharHeight;
        public readonly uint MaxCharWidth;
        //public uint NumCharacters => (uint)CharDescs.Length;
        public readonly FontCharDesc[] CharDescs;
        public readonly uint NumHorizontalBorderPixels;
        public readonly uint NumVerticalBorderPixels;
        public readonly uint BaselineOffset;
        public readonly uint ForegroundSurfaceDataID; // This is a DataID to a Texture (0x06) type, if set
        public readonly uint BackgroundSurfaceDataID; // This is a DataID to a Texture (0x06) type, if set

        public Font(BinaryReader r)
        {
            Id = r.ReadUInt32();
            MaxCharHeight = r.ReadUInt32();
            MaxCharWidth = r.ReadUInt32();
            CharDescs = r.ReadL32FArray(x => new FontCharDesc(x));
            NumHorizontalBorderPixels = r.ReadUInt32();
            NumVerticalBorderPixels = r.ReadUInt32();
            BaselineOffset = r.ReadUInt32();
            ForegroundSurfaceDataID = r.ReadUInt32();
            BackgroundSurfaceDataID = r.ReadUInt32();
        }

        //: New
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(Font)}: {Id:X8}", items: new List<MetaInfo> {
                })
            };
            return nodes;
        }
    }
}
