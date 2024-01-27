using GameSpec.Meta;
using GameSpec.Platforms;
using OpenStack.Graphics;
using OpenStack.Graphics.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Formats
{
    #region Binary_Bik

    public class Binary_Bik : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Bik(r, (int)f.FileSize));

        public Binary_Bik(BinaryReader r, int fileSize) => Data = r.ReadBytes(fileSize);

        public byte[] Data;

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "BIK Video" }),
        };
    }

    #endregion

    #region Binary_Dds

    // https://github.com/paroj/nv_dds/blob/master/nv_dds.cpp
    public class Binary_Dds : ITexture, IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Dds(r));

        public Binary_Dds(BinaryReader r, bool readMagic = true)
        {
            Bytes = DDS_HEADER.Read(r, readMagic, out Header, out HeaderDXT10, out Format);
            var numMipMaps = Math.Max(1, Header.dwMipMapCount);
            var offset = 0;
            Mips = new Range[numMipMaps];
            for (var i = 0; i < numMipMaps; i++)
            {
                int w = (int)Header.dwWidth >> i, h = (int)Header.dwHeight >> i;
                if (w == 0 || h == 0) { Mips[i] = -1..; continue; }
                var size = ((w + 3) / 4) * ((h + 3) / 4) * Format.blockSize;
                var remains = Math.Min(size, Bytes.Length - offset);
                Mips[i] = remains > 0 ? offset..(offset + remains) : -1..;
                offset += remains;
            }
        }

        DDS_HEADER Header;
        DDS_HEADER_DXT10? HeaderDXT10;
        (object type, int blockSize, object gl, object vulken, object unity, object unreal) Format;
        byte[] Bytes;
        Range[] Mips;

        public IDictionary<string, object> Data => null;
        public int Width => (int)Header.dwWidth;
        public int Height => (int)Header.dwHeight;
        public int Depth => 0;
        public int MipMaps => (int)Header.dwMipMapCount;
        public TextureFlags Flags => 0;

        public byte[] Begin(int platform, out object format, out Range[] mips)
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
            mips = Mips;
            return Bytes;
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

    #endregion

    #region Binary_Fsb

    public class Binary_Fsb : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Fsb(r, (int)f.FileSize));

        public Binary_Fsb(BinaryReader r, int fileSize) => Data = r.ReadBytes(fileSize);

        public byte[] Data;

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "FSB Audio" }),
        };
    }

    #endregion

    #region Binary_Img

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
            Format = (formatType,
                (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte),
                (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte),
                TextureUnityFormat.RGB24,
                TextureUnrealFormat.Unknown);
            Image = new Bitmap(new MemoryStream(r.ReadBytes((int)f.FileSize)));
            Width = Image.Width;
            Height = Image.Height;
        }

        byte[] Bytes;
        Bitmap Image;
        (Formats type, object gl, object vulken, object unity, object unreal) Format;

        public IDictionary<string, object> Data { get; } = null;
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; } = 0;
        public int MipMaps { get; } = 1;
        public TextureFlags Flags { get; } = 0;

        public unsafe byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            byte[] BmpToBytes()
            {
                var d = new byte[Width * Height * 3];
                var data = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                var s = (byte*)data.Scan0.ToPointer();
                for (var i = 0; i < d.Length; i += 3) { d[i + 0] = s[i + 0]; d[i + 1] = s[i + 1]; d[i + 2] = s[i + 2]; }
                Image.UnlockBits(data);
                return d;
            }

            Bytes = BmpToBytes();
            format = (Platform.Type)platform switch
            {
                Platform.Type.OpenGL => Format.gl,
                Platform.Type.Vulken => Format.vulken,
                Platform.Type.Unity => Format.unity,
                Platform.Type.Unreal => Format.unreal,
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

    #endregion

    #region Binary_Msg

    public class Binary_Msg : IHaveMetaInfo
    {
        public static Func<BinaryReader, FileSource, PakFile, Task<object>> Factory(string message) => (BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Msg(message));

        public Binary_Msg(string message) => Message = message;

        public string Message;

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = Message }),
        };
    }

    #endregion

    #region Binary_Snd

    public class Binary_Snd : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Snd(r, (int)f.FileSize));

        public Binary_Snd(BinaryReader r, int fileSize) => Data = r.ReadBytes(fileSize);

        public byte[] Data;

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "AudioPlayer", Name = Path.GetFileName(file.Path), Value = new MemoryStream(Data), Tag = Path.GetExtension(file.Path) }),
        };
    }

    #endregion

    #region Binary_Txt

    public class Binary_Txt : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Txt(r, (int)f.FileSize));

        public Binary_Txt(BinaryReader r, int fileSize) => Data = r.ReadEncoding(fileSize);

        public string Data;

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = Data }),
        };
    }

    #endregion
}
