using GameX.Formats;
using GameX.Meta;
using GameX.Platforms;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Black.Formats
{
    public unsafe class Binary_Rix : IHaveMetaInfo, ITexture
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Rix(r, f));

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

        byte[] Bytes;
        (object gl, object vulken, object unity, object unreal) Format;

        public Binary_Rix(BinaryReader r, FileSource f)
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
            Format = (
                (TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte),
                (TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte),
                TextureUnityFormat.RGBA32,
                TextureUnrealFormat.R8G8B8A8);
            Width = header.Width;
            Height = header.Height;
        }

        // ITexture
        public IDictionary<string, object> Data => null;
        public int Width { get; }
        public int Height { get; }
        public int Depth => 0;
        public int MipMaps => 1;
        public TextureFlags Flags => 0;

        public void Select(int id) { }
        public byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            format = (Platform.Type)platform switch
            {
                Platform.Type.OpenGL => Format.gl,
                Platform.Type.Unity => Format.unity,
                Platform.Type.Unreal => Format.unreal,
                Platform.Type.Vulken => Format.vulken,
                Platform.Type.StereoKit => throw new NotImplementedException("StereoKit"),
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            ranges = null;
            return Bytes;
        }
        public void End() { }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new MetaInfo($"{nameof(Binary_Rix)}", items: new List<MetaInfo> {
                new MetaInfo($"Width: {Width}"),
                new MetaInfo($"Height: {Height}"),
            })
        };
    }
}
