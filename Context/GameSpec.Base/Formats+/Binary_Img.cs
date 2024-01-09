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
    public class Binary_Img : IHaveMetaInfo, ITexture
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Img(r, f));

        enum Formats { Bmp, Gif, Exif, Jpg, Png, Tiff }

        public Binary_Img(BinaryReader r, FileSource f)
        {
            var formatType = Path.GetExtension(f.Path).ToLowerInvariant() switch
            {
                ".bmp" => Formats.Bmp,
                ".gif" => Formats.Gif,
                ".exif" => Formats.Exif,
                ".jpg" => Formats.Jpg,
                ".png" => Formats.Png,
                ".tiff" => Formats.Tiff,
                _ => throw new ArgumentOutOfRangeException(nameof(f.Path), Path.GetExtension(f.Path)),
            };
            Format = (formatType, (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte), (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte), TextureUnityFormat.RGB24, TextureUnrealFormat.Unknown);
            Bytes = r.ReadBytes((int)f.FileSize);
            Image = new Bitmap(new MemoryStream(Bytes));
            Width = Image.Width;
            Height = Image.Height;
        }

        byte[] Bytes;
        Bitmap Image;
        (Formats type, object gl, object vulken, object unity, object unreal) Format;

        public IDictionary<string, object> Data => null;
        public int Width { get; }
        public int Height { get; }
        public int Depth => 0;
        public int MipMaps => 1;
        public TextureFlags Flags => 0;

        public unsafe byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            void ConvertToBmp()
            {
                var d = new byte[Width * Height * 3];
                var data = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                var s = (byte*)data.Scan0.ToPointer();
                for (var i = 0; i < d.Length; i += 3) { d[i + 0] = s[i + 0]; d[i + 1] = s[i + 1]; d[i + 2] = s[i + 2]; }
                Image.UnlockBits(data);
                Bytes = d;
            }

            ConvertToBmp();
            format = (Platform.Type)platform switch
            {
                Platform.Type.OpenGL => Format.gl,
                Platform.Type.Vulken => Format.vulken,
                Platform.Type.Unity => Format.unity,
                Platform.Type.Unreal => Format.unreal,
                Platform.Type.StereoKit => throw new NotImplementedException("StereoKit"),
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            ranges = null;
            return Bytes;
        }
        public void End() { }

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new MetaInfo($"{nameof(Binary_Img)}", items: new List<MetaInfo> {
                new MetaInfo($"Format: {Format.type}"),
                new MetaInfo($"Width: {Width}"),
                new MetaInfo($"Height: {Height}"),
            })
        };
    }
}
