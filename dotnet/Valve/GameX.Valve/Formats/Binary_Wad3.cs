using GameX.Meta;
using GameX.Platforms;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GameX.Formats
{
    // https://github.com/dreamstalker/rehlds/blob/master/rehlds/engine/model.cpp
    // https://greg-kennedy.com/hl_materials/
    // https://github.com/tmp64/BSPRenderer
    public unsafe class Binary_Wad3 : ITexture, IHaveMetaInfo
    {
        struct CharInfo
        {
            public ushort StartOffset;
            public ushort CharWidth;
        }

        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Wad3(r, f));

        enum Formats : byte
        {
            None = 0,
            Tex2 = 0x40,
            Pic = 0x42,
            Tex = 0x43,
            Fnt = 0x46
        }

        public Binary_Wad3(BinaryReader r, FileSource f)
        {
            var type = Path.GetExtension(f.Path) switch
            {
                ".pic" => Formats.Pic,
                ".tex" => Formats.Tex,
                ".tex2" => Formats.Tex2,
                ".fnt" => Formats.Fnt,
                _ => Formats.None
            };
            transparent = Path.GetFileName(f.Path).StartsWith('{');
            Format = transparent
                ? (type, (TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte), (TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte), TextureUnityFormat.RGBA32, TextureUnityFormat.RGBA32)
                : (type, (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte), (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte), TextureUnityFormat.RGB24, TextureUnityFormat.RGB24);
            if (type == Formats.Tex2 || type == Formats.Tex) name = r.ReadFYString(16); // r.Skip(16); // Skip name
            width = (int)r.ReadUInt32();
            height = (int)r.ReadUInt32();

            // validate
            if (width > 0x1000 || height > 0x1000) throw new FormatException("Texture width or height exceeds maximum size!");
            else if (width == 0 || height == 0) throw new FormatException("Texture width and height must be larger than 0!");

            // read pixel offsets
            if (type == Formats.Tex2 || type == Formats.Tex)
            {
                var offsets = new[] { r.ReadUInt32(), r.ReadUInt32(), r.ReadUInt32(), r.ReadUInt32() }; // r.Skip(16); // Skip pixel offsets
                if (r.BaseStream.Position != offsets[0]) throw new Exception("BAD OFFSET");
            }
            else if (type == Formats.Fnt)
            {
                width = 0x100;
                var rowCount = r.ReadUInt32();
                var rowHeight = r.ReadUInt32();
                var charInfos = r.ReadTArray<CharInfo>(sizeof(CharInfo), 0x100);
            }

            // read pixels
            var pixelSize = width * height;
            pixels = type == Formats.Tex2 || type == Formats.Tex
                ? new byte[][] { r.ReadBytes(pixelSize), r.ReadBytes(pixelSize >> 2), r.ReadBytes(pixelSize >> 4), r.ReadBytes(pixelSize >> 8) }
                : new byte[][] { r.ReadBytes(pixelSize) };

            // read pallet
            r.Skip(2);
            palette = r.ReadBytes(0x100 * 3);

            //if (type == Formats.Pic) r.Skip(2);
            //r.EnsureComplete();
        }

        bool transparent;
        string name;
        int width;
        int height;
        byte[][] pixels;
        byte[] palette;

        (Formats type, object gl, object vulken, object unity, object unreal) Format;

        public IDictionary<string, object> Data => null;
        public int Width => width;
        public int Height => height;
        public int Depth => 0;
        public int MipMaps => pixels.Length;
        public TextureFlags Flags => 0;

        public void Select(int id) { }
        public byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            static void PaletteRgba8(Span<byte> data, byte[] source, byte[] palette)
            {
                fixed (byte* _ = data)
                    for (int i = 0, pi = 0; i < source.Length; i++, pi += 4)
                    {
                        var pa = source[i] * 3;
                        //if (pa + 3 > palette.Length) continue;
                        _[pi + 0] = palette[pa + 0];
                        _[pi + 1] = palette[pa + 1];
                        _[pi + 2] = palette[pa + 2];
                        _[pi + 3] = 0xFF;
                    }
            }
            static void PaletteRgb8(Span<byte> data, byte[] source, byte[] palette)
            {
                fixed (byte* _ = data)
                    for (int i = 0, pi = 0; i < source.Length; i++, pi += 3)
                    {
                        var pa = source[i] * 3;
                        //if (pa + 3 > palette.Length) continue;
                        _[pi + 0] = palette[pa + 0];
                        _[pi + 1] = palette[pa + 1];
                        _[pi + 2] = palette[pa + 2];
                    }
            }

            format = (Platform.Type)platform switch
            {
                Platform.Type.OpenGL => Format.gl,
                Platform.Type.Unity => Format.unity,
                Platform.Type.Unreal => Format.unreal,
                Platform.Type.Vulken => Format.vulken,
                Platform.Type.StereoKit => throw new NotImplementedException("StereoKit"),
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            var bytes = new byte[pixels.Sum(x => x.Length) * 4];
            ranges = new Range[pixels.Length];
            byte[] p;
            for (int index = 0, offset = 0; index < pixels.Length; index++, offset += p.Length * 4)
            {
                p = pixels[index];
                var range = ranges[index] = new Range(offset, offset + p.Length * 4);
                if (transparent) PaletteRgba8(bytes.AsSpan(range), p, palette);
                else PaletteRgb8(bytes.AsSpan(range), p, palette);
            }
            return bytes;
        }
        public void End() { }

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new MetaInfo("Texture", items: new List<MetaInfo> {
                new MetaInfo($"Format: {Format.type}"),
                new MetaInfo($"Width: {Width}"),
                new MetaInfo($"Height: {Height}"),
                new MetaInfo($"Mipmaps: {MipMaps}"),
            }),
        };
    }
}
