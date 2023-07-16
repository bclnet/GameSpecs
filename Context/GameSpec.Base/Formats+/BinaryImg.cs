using GameSpec.Metadata;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Formats
{
    public unsafe class BinaryImg : IGetMetadataInfo, ITexture
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinaryImg(r, f));

        enum Formats { Bmp, Jpg, Tga }

        //public BinaryImg() { }
        public BinaryImg(BinaryReader r, FileMetadata f)
        {
            Format = Path.GetExtension(f.Path).ToLowerInvariant() switch
            {
                ".bmp" => (Formats.Bmp, (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte), TextureUnityFormat.RGB24),
                ".jpg" => (Formats.Jpg, (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte), TextureUnityFormat.RGB24),
                ".tga" => (Formats.Tga, (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte), TextureUnityFormat.RGB24),
                _ => throw new ArgumentOutOfRangeException(nameof(f.Path), Path.GetExtension(f.Path)),
            };
            var sourceData = r.ReadBytes((int)f.FileSize);
            Image = new Bitmap(new MemoryStream(sourceData));
            Width = Image.Width;
            Height = Image.Height;
        }

        Bitmap Image;
        (Formats x, object gl, object unity) Format;

        public IDictionary<string, object> Data => null;
        public int Width { get; }
        public int Height { get; }
        public int Depth => 0;
        public TextureFlags Flags => 0;
        public object UnityFormat => Format.unity;
        public object GLFormat => Format.gl;
        public int NumMipMaps => 1;
        public Span<byte> this[int index]
        {
            get
            {
                var d = new byte[Width * Height * 3];
                var data = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                var s = (byte*)data.Scan0.ToPointer();
                for (var i = 0; i < d.Length; i += 3) { d[i + 0] = s[i + 0]; d[i + 1] = s[i + 1]; d[i + 2] = s[i + 2]; }
                Image.UnlockBits(data);
                return d;
            }
            set => throw new NotImplementedException();
        }
        public void MoveToData(out bool forward) => forward = true;

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag) => new List<MetadataInfo> {
            new MetadataInfo(null, new MetadataContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new MetadataInfo($"{nameof(BinaryImg)}", items: new List<MetadataInfo> {
                new MetadataInfo($"Width: {Width}"),
                new MetadataInfo($"Height: {Height}"),
                new MetadataInfo($"Type: {Format}"),
            })
        };
    }
}
