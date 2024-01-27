using GameSpec.Formats;
using GameSpec.Meta;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_RadarColor : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_RadarColor(r));

        #region Records

        public static uint[] Colors = new uint[0x20000];

        #endregion

        // file: radarcol.mul
        public Binary_RadarColor(BinaryReader r)
        {
            const int multiplier = 0xFF / 0x1F;
            // Prior to 7.0.7.1, all clients have 0x10000 colors. Newer clients have fewer colors.
            var colorCount = (int)r.BaseStream.Length >> 1;
            for (var i = 0; i < colorCount; i++)
            {
                var c = (uint)r.ReadUInt16();
                Colors[i] = 0xFF000000 |
                        ((((c >> 10) & 0x1F) * multiplier)) |
                        ((((c >> 5) & 0x1F) * multiplier) << 8) |
                        (((c & 0x1F) * multiplier) << 16);
            }
            // fill the remainder of the color table with non-transparent magenta.
            for (var i = colorCount; i < Colors.Length; i++) Colors[i] = 0xFFFF00FF;
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Radar Color File" }),
                new MetaInfo("RadarColor", items: new List<MetaInfo> {
                    new MetaInfo($"Colors: {Colors.Length}"),
                })
            };
            return nodes;
        }
    }
}
