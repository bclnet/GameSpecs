using GameSpec.Formats;
using GameSpec.Metadata;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Bethesda.Formats
{
    public unsafe class BinaryRix : IGetMetadataInfo, ITexture
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinaryRix(r, f));

        // Header
        #region Header
        // https://falloutmods.fandom.com/wiki/RIX_File_Format

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct Header
        {
            public uint Magic;                      // RIX3 - the file signature 
            public ushort Width;                    // 640 - width of the image
            public ushort Height;                   // 480 - height of the image
            public byte PaletteType;                // VGA - Palette type
            public byte StorageType;                // linear - Storage type
        }

        #endregion

        public BinaryRix(BinaryReader r, FileMetadata f)
        {
            var header = r.ReadT<Header>(sizeof(Header));
            var rgb = r.ReadBytes(256 * 3);
            var rgba32 = stackalloc uint[256];
            fixed (byte* s = rgb)
            {
                var d = rgba32;
                var _ = s;
                for (var i = 0; i < 256; i++, _ += 3)
                    d[i] = (uint)(0x00 << 24 | _[2] << (16 + 2) | _[1] << (8 + 2) | _[0]);
            }
            var data = r.ReadBytes(header.Width * header.Height);
            var image = new byte[header.Width * header.Height * 4];
            fixed (byte* image_ = image)
            {
                var _ = image_;
                for (var j = 0; j < data.Length; j++, _ += 4) *(uint*)_ = rgba32[data[j]];
            }

            Bytes = image;
            Format = ((TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte), (TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte), TextureUnityFormat.RGBA32, TextureUnrealFormat.R8G8B8A8);
            Width = header.Width;
            Height = header.Height;
        }

        byte[] Bytes;
        (object gl, object vulken, object unity, object unreal) Format;

        public IDictionary<string, object> Data => null;
        public int Width { get; }
        public int Height { get; }
        public int Depth => 0;
        public int MipMaps => 1;
        public TextureFlags Flags => 0;

        public byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            format = (FamilyPlatform.Type)platform switch
            {
                FamilyPlatform.Type.OpenGL => Format.gl,
                FamilyPlatform.Type.Unity => Format.unity,
                FamilyPlatform.Type.Unreal => Format.unreal,
                FamilyPlatform.Type.Vulken => Format.vulken,
                FamilyPlatform.Type.StereoKit => throw new NotImplementedException("StereoKit"),
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            ranges = null;
            return Bytes;
        }
        public void End() { }

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag) => new List<MetadataInfo> {
            new MetadataInfo(null, new MetadataContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new MetadataInfo($"{nameof(BinaryRix)}", items: new List<MetadataInfo> {
                new MetadataInfo($"Width: {Width}"),
                new MetadataInfo($"Height: {Height}"),
            })
        };
    }
}
