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
    public unsafe class BinaryWad : ITextureInfo, IGetMetadataInfo
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinaryWad(r, f));

        public BinaryWad(BinaryReader r, FileMetadata f) => Read(r, f);

        int width;
        int height;
        byte[][] pixels;
        byte[] palette;

        public void Read(BinaryReader r, FileMetadata f)
        {
            var type = Path.GetExtension(f.Path) switch
            {
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
                r.EnsureComplete();
            }
            else if (type == 0x43)
            {
                var name = r.ReadFAString(16); // r.Skip(16); // Skip name
                width = (int)r.ReadUInt32();
                height = (int)r.ReadUInt32();
                var pixelSize = width * height;
                r.Skip(16); // Skip pixel offsets
                pixels = new byte[][] { r.ReadBytes(pixelSize), r.ReadBytes(pixelSize >> 2), r.ReadBytes(pixelSize >> 4), r.ReadBytes(pixelSize >> 8) };
                palette = r.ReadBytes(r.ReadUInt16());
                r.EnsureComplete();
            }
            else if (type == 0x45)
            {
                pixels = new byte[][] { };
            }
        }

        public IDictionary<string, object> Data => null;
        public int Width => width;
        public int Height => height;
        public int Depth => 0;
        public TextureFlags Flags => 0;
        public object UnityFormat => TextureUnityFormat.RGB24;
        public object GLFormat => (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte);
        public int NumMipMaps => pixels.Length;
        public byte[] this[int index]
        {
            get
            {
                var source = pixels[index];
                var data = new byte[width * height * 3];
                fixed (byte* _ = data)
                    for (int i = 0, pi = 0; i < source.Length; i++, pi += 3)
                    {
                        var pa = source[i] * 3;
                        _[pi + 0] = palette[pa + 0];
                        _[pi + 1] = palette[pa + 1];
                        _[pi + 2] = palette[pa + 2];
                    }
                return data;
            }
            set => throw new NotImplementedException();
        }
        public void MoveToData() { }

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
