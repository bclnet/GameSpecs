using GameSpec.Formats;
using GameSpec.Meta;
using GameSpec.Platforms;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats.UO
{
    public unsafe class Binary_Gump : IHaveMetaInfo, ITexture
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Gump(r, (int)f.FileSize, f.Compressed));

        #region Records

        static byte[] _pixels;
        static byte[] _colors;

        Bitmap Image;
        static (object gl, object vulken, object unity, object unreal) Format = (
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            TextureUnityFormat.Unknown,
            TextureUnrealFormat.Unknown);

        #endregion

        // file: gumpartLegacyMUL.uop:file00000.tex
        public Binary_Gump(BinaryReader r, int length, int extra)
        {
            int width = Width = (extra >> 16) & 0xFFFF;
            int height = Height = extra & 0xFFFF;
            if (width <= 0 || height <= 0) return;
            ToBitmap(r.ReadBytes(length));
        }

        public void ToBitmap(byte[] data)
        {
            int width = Width, height = Height;
            fixed (byte* _ = data)
            {
                var bmp = Image = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
                var bd = bmp.LockBits(
                    new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

                var lookup = (int*)_;
                var dat = (ushort*)_;
                var line = (ushort*)bd.Scan0;
                var delta = bd.Stride >> 1;

                for (var y = 0; y < height; ++y, line += delta)
                {
                    var count = *lookup++ * 2;

                    ushort* cur = line, end = line + bd.Width;
                    while (cur < end)
                    {
                        var color = dat[count++];
                        var next = cur + dat[count++];

                        if (color == 0) cur = next;
                        else
                        {
                            color ^= 0x8000;
                            while (cur < next) *cur++ = color;
                        }
                    }
                }
                bmp.UnlockBits(bd);
            }
        }

        public void BitmapWithHue(byte[] data, Binary_Hues.Record hue, bool onlyHueGrayPixels)
        {
            int width = Width, height = Height;
            fixed (byte* _ = data)
            {
                if (width <= 0 || height <= 0) return;

                int bytesPerLine = width << 1, bytesPerStride = (bytesPerLine + 3) & ~3, bytesForImage = height * bytesPerStride;
                int pixelsPerStride = (width + 1) & ~1, pixelsPerStrideDelta = pixelsPerStride - width;

                if (_pixels == null || _pixels.Length < bytesForImage) _pixels = new byte[(bytesForImage + 2047) & ~2047];

                _colors ??= new byte[128];

                fixed (ushort* hueColors_ = hue.Colors)
                fixed (byte* pixels_ = _pixels)
                fixed (byte* colors_ = _colors)
                {
                    var hueColors = hueColors_;
                    var hueColorsEnd = hueColors + 32;
                    var colors = (ushort*)colors_;
                    var colorsOpaque = colors;

                    while (hueColors < hueColorsEnd) *colorsOpaque++ = *hueColors++;

                    var pixelsStart = (ushort*)pixels_;

                    var lookup = (int*)_;
                    int* lookupEnd = lookup + height, pixelRleStart = lookup, pixelRle;
                    ushort* pixel = pixelsStart, rleEnd, pixelEnd = pixel + width;

                    ushort color, count;
                    if (onlyHueGrayPixels)
                        while (lookup < lookupEnd)
                        {
                            pixelRle = pixelRleStart + *lookup++;
                            rleEnd = pixel;

                            while (pixel < pixelEnd)
                            {
                                color = *(ushort*)pixelRle;
                                count = *(1 + (ushort*)pixelRle);
                                ++pixelRle;

                                rleEnd += count;

                                if (color != 0 && (color & 0x1F) == ((color >> 5) & 0x1F) && (color & 0x1F) == ((color >> 10) & 0x1F)) color = colors[color >> 10];
                                else if (color != 0) color ^= 0x8000;

                                while (pixel < rleEnd) *pixel++ = color;
                            }

                            pixel += pixelsPerStrideDelta;
                            pixelEnd += pixelsPerStride;
                        }
                    else
                        while (lookup < lookupEnd)
                        {
                            pixelRle = pixelRleStart + *lookup++;
                            rleEnd = pixel;

                            while (pixel < pixelEnd)
                            {
                                color = *(ushort*)pixelRle;
                                count = *(1 + (ushort*)pixelRle);
                                ++pixelRle;

                                rleEnd += count;

                                if (color != 0) color = colors[color >> 10];

                                while (pixel < rleEnd) *pixel++ = color;
                            }

                            pixel += pixelsPerStrideDelta;
                            pixelEnd += pixelsPerStride;
                        }

                    Image = new Bitmap(width, height, bytesPerStride, PixelFormat.Format16bppArgb1555, (IntPtr)pixelsStart);
                }
            }
        }

        public IDictionary<string, object> Data { get; } = null;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; } = 0;
        public int MipMaps { get; } = 1;
        public TextureFlags Flags { get; } = 0;

        public unsafe byte[] Begin(int platform, out object format, out Range[] ranges)
        {
            byte[] BmpToBytes()
            {
                var d = new byte[Width * Height * 2];
                var data = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                var s = (byte*)data.Scan0;
                for (var i = 0; i < d.Length; i += 2) { d[i + 0] = s[i + 0]; d[i + 1] = s[i + 1]; }
                Image.UnlockBits(data);
                return d;
            }

            format = (Platform.Type)platform switch
            {
                Platform.Type.OpenGL => Format.gl,
                Platform.Type.Vulken => Format.vulken,
                Platform.Type.Unity => Format.unity,
                Platform.Type.Unreal => Format.unreal,
                _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
            };
            ranges = null;
            return Image != null ? BmpToBytes() : null;
        }
        public void End() { }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
                new MetaInfo("Gump", items: new List<MetaInfo> {
                    new MetaInfo($"Width: {Image?.Width}"),
                    new MetaInfo($"Height: {Image?.Height}"),
                })
            };
            return nodes;
        }
    }
}
