using GameSpec.Metadata;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Formats
{
    // https://github.com/dreamstalker/rehlds/blob/master/rehlds/engine/model.cpp
    // https://greg-kennedy.com/hl_materials/
    // https://github.com/tmp64/BSPRenderer
    public unsafe class BinaryWad : ITexture, IGetMetadataInfo
    {
        struct CharInfo
        {
            public ushort StartOffset;
            public ushort CharWidth;
        }

        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinaryWad(r, f));

        enum Formats : byte
        {
            None = 0,
            Tex2 = 0x40,
            Pic = 0x42,
            Tex = 0x43,
            Fnt = 0x45
        }

        public BinaryWad(BinaryReader r, FileMetadata f)
        {
            var type = Path.GetExtension(f.Path) switch
            {
                ".tex2" => Formats.Tex2,
                ".pic" => Formats.Pic,
                ".tex" => Formats.Tex,
                ".fnt" => Formats.Fnt,
                _ => Formats.None
            };
            if (type == Formats.Tex2)
            {
                width = (int)r.ReadUInt32();
                height = (int)r.ReadUInt32();
                var pixelSize = width * height;
                pixels = new byte[][] { r.ReadBytes(pixelSize) };
                palette = r.ReadBytes(r.ReadUInt16() * 3);
                r.Skip(2);
                //r.EnsureComplete();
            }
            else if (type == Formats.Tex2 || type == Formats.Tex)
            {
                name = r.ReadFAString(16); // r.Skip(16); // Skip name
                width = (int)r.ReadUInt32();
                height = (int)r.ReadUInt32();
                var pixelSize = width * height;
                var offsets = new[] { (long)r.ReadUInt32(), (long)r.ReadUInt32(), (long)r.ReadUInt32(), (long)r.ReadUInt32() }; // r.Skip(16); // Skip pixel offsets
                if (r.Position() != offsets[0]) throw new Exception("BAD OFFSET");
                pixels = new byte[][] { r.ReadBytes(pixelSize), r.ReadBytes(pixelSize >> 2), r.ReadBytes(pixelSize >> 4), r.ReadBytes(pixelSize >> 8) };
                palette = r.ReadBytes(r.ReadUInt16());
                //r.EnsureComplete();
            }
            else if (type == Formats.Fnt)
            {
                width = 0x100;
                r.ReadUInt32();
                r.ReadUInt32();
                var infoArray = r.ReadTArray<CharInfo>(sizeof(CharInfo), 0x100);
                pixels = new byte[][] { };
            }
            // validate
            if (width > 0x1000 || height > 0x1000) throw new FormatException("Texture width or height exceeds maximum size!");
            else if (width == 0 || height == 0) throw new FormatException("Texture width and height must be larger than 0!");

            Format = type switch
            {
                Formats.Tex2 => (type, (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte), (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte), TextureUnityFormat.RGB24, TextureUnityFormat.RGB24),
                var x when x == Formats.Tex2 || x == Formats.Tex => (type, (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte), (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte), TextureUnityFormat.RGB24, TextureUnityFormat.RGB24),
                Formats.Fnt => (type, (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte), (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte), TextureUnityFormat.RGB24, TextureUnityFormat.RGB24),
                _ => throw new ArgumentOutOfRangeException(nameof(type), $"{type}"),
            };
        }

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

        public byte[] Begin(int platform, out object format, out Range[] mips)
        {
            void FlattenPalette(int index)
            {
                // flatten pallate
                var source = pixels[index];
                var data = new byte[width * height * 3];
                fixed (byte* _ = data)
                    for (int i = 0, pi = 0; i < source.Length; i++, pi += 3)
                    {
                        var pa = source[i] * 3;
                        if (pa + 2 > palette.Length) continue;
                        _[pi + 0] = palette[pa + 0];
                        _[pi + 1] = palette[pa + 1];
                        _[pi + 2] = palette[pa + 2];
                    }
                //return data;
            }

            format = (FamilyPlatform.Type)platform switch
            {
                FamilyPlatform.Type.OpenGL => Format.gl,
                FamilyPlatform.Type.Unity => Format.unity,
                FamilyPlatform.Type.Unreal => Format.unreal,
                FamilyPlatform.Type.Vulken => Format.vulken,
                FamilyPlatform.Type.StereoKit => throw new NotImplementedException("StereoKit"),
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            mips = null;
            return null;
        }
        public void End() { }

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag) => new List<MetadataInfo> {
            new MetadataInfo(null, new MetadataContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new MetadataInfo("Texture", items: new List<MetadataInfo> {
                new MetadataInfo($"Format: {Format.type}"),
                new MetadataInfo($"Width: {Width}"),
                new MetadataInfo($"Height: {Height}"),
                new MetadataInfo($"Mipmaps: {MipMaps}"),
            }),
        };
    }
}
