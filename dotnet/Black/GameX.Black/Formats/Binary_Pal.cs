using GameX.Formats;
using GameX.Meta;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Black.Formats
{
    public unsafe class Binary_Pal : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Pal(r, f));

        public uint[] Rgba32 = new uint[256];

        public Binary_Pal(BinaryReader r, FileSource f)
        {
            var rgb = r.ReadBytes(256 * 3);
            fixed (byte* s = rgb)
            fixed (uint* d = Rgba32)
            {
                var _ = s;
                for (var i = 0; i < 256; i++, _ += 3)
                    d[i] = (uint)(0x00 << 24 | _[2] << (16 + 2) | _[1] << (8 + 2) | _[0]);
                //d[0] = uint.MaxValue;
            }
        }

        public void SetColors()
        {
            for (var i = 229; i <= 232; i++) Rgba32[i] = 0x00ff0000; // animated green (for radioactive waste)
            for (var i = 233; i <= 237; i++) Rgba32[i] = 0x0000ff00; // bright blue (computer screens)
            for (var i = 238; i <= 247; i++) Rgba32[i] = 0xff000000; // orange, red and yellow (for fires)
            for (var i = 248; i <= 254; i++) Rgba32[i] = 0x0000ff00; // bright blue (computer screens)
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Null", Name = Path.GetFileName(file.Path), Value = this }),
            new MetaInfo($"{nameof(Binary_Pal)}", items: new List<MetaInfo> {
            })
        };
    }
}
