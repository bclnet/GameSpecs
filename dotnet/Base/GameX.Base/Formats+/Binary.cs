using GameX.Meta;
using GameX.Platforms;
using OpenStack.Graphics;
using OpenStack.Graphics.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Formats
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

        public void Select(int id) { }
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

        public void Select(int id) { }
        public byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            unsafe byte[] BmpToBytes()
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

    #region Binary_Pcx

    // https://en.wikipedia.org/wiki/PCX
    // https://github.com/warpdesign/pcx-js/blob/master/js/pcx.js
    public unsafe class Binary_Pcx : IHaveMetaInfo, ITexture
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Pcx(r, f));

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct X_Header
        {
            public static (string, int) Struct = ("<4B6H48c2B4H54c", sizeof(X_Header));
            public byte Magic;              // Fixed header field valued at a hexadecimal
            public byte Version;            // Version number referring to the Paintbrush software release
            public byte Encoding;           // Method used for encoding the image data
            public byte Bpp;                // Number of bits constituting one plane
            public ushort XMin;             // Minimum x co-ordinate of the image position
            public ushort YMin;             // Minimum y co-ordinate of the image position
            public ushort XMax;             // Maximum x co-ordinate of the image position
            public ushort YMax;             // Maximum y co-ordinate of the image position
            public ushort HDpi;             // Horizontal image resolution in DPI
            public ushort VDpi;             // Vertical image resolution in DPI
            public fixed byte Palette[48];  // EGA palette for 16-color images
            public byte Reserved1;          // First reserved field
            public byte BitPlanes;          // Number of color planes constituting the pixel data
            public ushort Bpr;              // Number of bytes of one color plane representing a single scan line
            public ushort Mode;             // Mode in which to construe the palette
            public ushort HRes;             // horizontal resolution of the source system's screen
            public ushort VRes;             // vertical resolution of the source system's screen
            public fixed byte Reserved2[54]; // Second reserved field, intended for future extension
        }

        public Binary_Pcx(BinaryReader r, FileSource f)
        {
            Format = (
                (TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte),
                (TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte),
                TextureUnityFormat.RGBA32,
                TextureUnrealFormat.Unknown);
            Header = r.ReadS<X_Header>();
            if (Header.Magic != 0x0a) throw new FormatException("BAD MAGIC");
            else if (Header.Encoding == 0) throw new FormatException("NO COMPRESSION");
            Body = r.ReadToEnd();
            Planes = Header.BitPlanes;
            Width = Header.XMax - Header.XMin + 1;
            Height = Header.YMax - Header.YMin + 1;
        }

        X_Header Header;
        int Planes;
        byte[] Body;
        (object gl, object vulken, object unity, object unreal) Format;

        public IDictionary<string, object> Data { get; } = null;
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; } = 0;
        public int MipMaps { get; } = 1;
        public TextureFlags Flags { get; } = 0;

        /// <summary>
        /// Gets the palette either from the header (< 8 bit) or at the bottom of the file (8bit)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public Span<byte> GetPalette()
        {
            if (Header.Bpp == 8 && Body[^769] == 12) return Body.AsSpan(Body.Length - 768);
            else if (Header.Bpp == 1) fixed (byte* _ = Header.Palette) return new Span<byte>(_, 48);
            else throw new FormatException("Could not find 256 color palette.");
        }

        /// <summary>
        /// Set a color using palette index
        /// </summary>
        /// <param name="palette"></param>
        /// <param name="pixels"></param>
        /// <param name="pos"></param>
        /// <param name="index"></param>
        static void SetColorFromPalette(Span<byte> palette, byte[] pixels, int pos, int index)
        {
            var start = index * 3;
            pixels[pos] = palette[start];
            pixels[pos + 1] = palette[start + 1];
            pixels[pos + 2] = palette[start + 2];
            pixels[pos + 3] = 255; // alpha channel
        }

        /// <summary>
        /// Returns true if the 2 most-significant bits are set
        /// </summary>
        /// <param name="body"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        static bool Rle(byte[] body, int offset) => (body[offset] >> 6) == 3;

        /// <summary>
        /// Returns the length of the RLE run.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        static int RleLength(byte[] body, int offset) => body[offset] & 63;

        public void Select(int id) { }
        public byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            // Decodes 4bpp pixel data
            byte[] Decode4bpp()
            {
                var palette = GetPalette();
                var temp = new byte[Width * Height];
                var pixels = new byte[Width * Height * 4];
                int offset = 0, p, pos, length = 0, val = 0;

                // Simple RLE decoding: if 2 msb == 1 then we have to mask out count and repeat following byte count times
                var b = Body;
                for (var y = 0; y < Height; y++)
                    for (p = 0; p < Planes; p++)
                    {
                        // bpr holds the number of bytes needed to decode a row of plane: we keep on decoding until the buffer is full
                        pos = Width * y;
                        for (var _ = 0; _ < Header.Bpr; _++)
                        {
                            if (length == 0)
                                if (Rle(b, offset)) { length = RleLength(b, offset); val = b[offset + 1]; offset += 2; }
                                else { length = 1; val = b[offset++]; }
                            length--;

                            // Since there may, or may not be blank data at the end of each scanline, we simply check we're not out of bounds
                            if ((_ * 8) < Width)
                            {
                                for (var i = 0; i < 8; i++)
                                {
                                    var bit = (val >> (7 - i)) & 1;
                                    temp[pos + i] |= (byte)(bit << p);
                                    // we have all planes: we may set color using the palette
                                    if (p == Planes - 1) SetColorFromPalette(palette, pixels, (pos + i) * 4, temp[pos + i]);
                                }
                                pos += 8;
                            }
                        }
                    }
                return pixels;
            }

            // Decodes 8bpp (depth = 8/24bit) data
            byte[] Decode8bpp()
            {
                var palette = Planes == 1 ? GetPalette() : null;
                var pixels = new byte[Width * Height * 4];
                int offset = 0, p, pos, length = 0, val = 0;

                // Simple RLE decoding: if 2 msb == 1 then we have to mask out count and repeat following byte count times
                var b = Body;
                for (var y = 0; y < Height; y++)
                    for (p = 0; p < Planes; p++)
                    {
                        // bpr holds the number of bytes needed to decode a row of plane: we keep on decoding until the buffer is full
                        pos = 4 * Width * y + p;
                        for (var _ = 0; _ < Header.Bpr; _++)
                        {
                            if (length == 0)
                                if (Rle(b, offset)) { length = RleLength(b, offset); val = b[offset + 1]; offset += 2; }
                                else { length = 1; val = b[offset++]; }
                            length--;

                            // Since there may, or may not be blank data at the end of each scanline, we simply check we're not out of bounds
                            if (_ < Width)
                            {
                                if (Planes == 3)
                                {
                                    pixels[pos] = (byte)val;
                                    if (p == Planes - 1) pixels[pos + 1] = 255; // add alpha channel
                                }
                                else SetColorFromPalette(palette, pixels, pos, val);
                                pos += 4;
                            }
                        }
                    }
                return pixels;
            }

            var bytes = Header.Bpp switch
            {
                8 => Decode8bpp(),
                1 => Decode4bpp(),
                _ => throw new FormatException($"Unsupported bpp: {Header.Bpp}"),
            };
            format = (Platform.Type)platform switch
            {
                Platform.Type.OpenGL => Format.gl,
                Platform.Type.Vulken => Format.vulken,
                Platform.Type.Unity => Format.unity,
                Platform.Type.Unreal => Format.unreal,
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            ranges = null;
            return bytes;
        }
        public void End() { }

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            new MetaInfo($"{nameof(Binary_Pcx)}", items: new List<MetaInfo> {
                new MetaInfo($"Width: {Width}"),
                new MetaInfo($"Height: {Height}"),
            })
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
