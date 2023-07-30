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
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinaryWad(r, f));

        public BinaryWad(BinaryReader r, FileMetadata f) => Read(r, f);

        string name;
        int width;
        int height;
        byte[][] pixels;
        byte[] palette;

        public void Read(BinaryReader r, FileMetadata f)
        {
            var type = Path.GetExtension(f.Path) switch
            {
                ".tex2" => 0x40,
                ".pic" => 0x42,
                ".tex" => 0x43,
                ".fnt" => 0x45,
                _ => 0
            };
            if (type == 0x42)
            {
                width = (int)r.ReadUInt32();
                height = (int)r.ReadUInt32();
                var pixelSize = width * height;
                pixels = new byte[][] { r.ReadBytes(pixelSize) };
                palette = r.ReadBytes(r.ReadUInt16() * 3);
                r.Skip(2);
                //r.EnsureComplete();
            }
            else if (type == 0x40 || type == 0x43)
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
            else if (type == 0x45)
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
        }

        struct CharInfo
        {
            public ushort StartOffset;
            public ushort CharWidth;
        }

        public byte[] RawBytes => null;
        public IDictionary<string, object> Data => null;
        public int Width => width;
        public int Height => height;
        public int Depth => 0;
        public TextureFlags Flags => 0;
        public object UnityFormat => TextureUnityFormat.RGB24;
        public object GLFormat => (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte);
        public int NumMipMaps => pixels.Length;
        public Span<byte> this[int index]
        {
            get
            {
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
                return data;
            }
            set => throw new NotImplementedException();
        }
        public void MoveToData(out bool forward) => forward = true;

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag) => new List<MetadataInfo> {
            new MetadataInfo(null, new MetadataContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new MetadataInfo("Texture", items: new List<MetadataInfo> {
                new MetadataInfo($"Width: {Width}"),
                new MetadataInfo($"Height: {Height}"),
                new MetadataInfo($"Mipmaps: {NumMipMaps}"),
            }),
        };
    }
}
