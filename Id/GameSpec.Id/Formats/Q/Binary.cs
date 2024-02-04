using GameSpec.Meta;
using GameSpec.Platforms;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// https://www.gamers.org/dEngine/quake/spec/quake-spec34/qkspec_3.htm
namespace GameSpec.Id.Formats.Q
{
    #region Binary_Lump

    public unsafe class Binary_Lump : IHaveMetaInfo, ITexture
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Lump(r, f, s));
        public static Binary_Lump Palette;
        public static Binary_Lump Colormap;

        #region Records

        public byte[][] PaletteRecords;
        public byte[][] ColormapRecords;

        public static byte[] ToLightPixel(int light, int pixel) => Palette.PaletteRecords[Colormap.ColormapRecords[(light >> 3) & 0x1F][pixel]];

        byte[] Pixels;
        static (object gl, object vulken, object unity, object unreal) Format = (
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedInt8888),
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedInt8888),
            TextureUnityFormat.Unknown,
            TextureUnrealFormat.Unknown);

        #endregion

        // file: PAK0.PAK:gfx/bigbox.lmp
        public Binary_Lump(BinaryReader r, FileSource f, PakFile s)
        {
            switch (Path.GetFileNameWithoutExtension(f.Path))
            {
                case "palette":
                    PaletteRecords = r.ReadFArray(s => s.ReadBytes(3).Concat(new byte[] { 0 }).ToArray(), 256);
                    Palette = this;
                    return;
                case "colormap":
                    ColormapRecords = r.ReadFArray(s => s.ReadBytes(256), 32);
                    Colormap = this;
                    return;
                default:
                    s.Game.Ensure();
                    var palette = Palette?.PaletteRecords ?? throw new NotImplementedException();
                    var width = Width = r.ReadInt32();
                    var height = Height = r.ReadInt32();
                    Pixels = r.ReadBytes(width * height).SelectMany(x => ToLightPixel(32 << 3, x)).ToArray();
                    return;
            }
        }

        public IDictionary<string, object> Data { get; } = null;
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; } = 0;
        public int MipMaps { get; } = 1;
        public TextureFlags Flags { get; } = 0;

        public unsafe byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            format = (Platform.Type)platform switch
            {
                Platform.Type.OpenGL => Format.gl,
                Platform.Type.Vulken => Format.vulken,
                Platform.Type.Unity => Format.unity,
                Platform.Type.Unreal => Format.unreal,
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            ranges = null;
            return Pixels;
        }
        public void End() { }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
                new MetaInfo("Texture", items: new List<MetaInfo> {
                    new MetaInfo($"Width: {Width}"),
                    new MetaInfo($"Height: {Height}"),
                })
            };
    }

    #endregion

    #region Binary_XXX

    public unsafe class Binary_XXX : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_XXX(r));

        #region Records

        #endregion

        // file: xxxx
        public Binary_XXX(BinaryReader r)
        {
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
            => new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "XXX File" }),
                new MetaInfo("XXX", items: new List<MetaInfo> {
                    //new MetaInfo($"Records: {Records.Length}"),
                })
            };
    }

    #endregion
}
