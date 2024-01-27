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
    public unsafe class Binary_MultiMap : IHaveMetaInfo, ITexture
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_MultiMap(r, f));

        #region Records

        Bitmap Image;
        static (object gl, object vulken, object unity, object unreal) Format = (
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            TextureUnityFormat.Unknown,
            TextureUnrealFormat.Unknown);

        #endregion

        // file: Multimap.rle
        public Binary_MultiMap(BinaryReader r, FileSource f)
        {
            if (f.Path.StartsWith("facet"))
            {
                Width = r.ReadInt16();
                Height = r.ReadInt16();

                var bmp = Image = new Bitmap(Width, Height, PixelFormat.Format16bppArgb1555);
                var bd = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly,
                    PixelFormat.Format16bppArgb1555);
                var line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;

                for (var y = 0; y < Height; y++, line += delta)
                {
                    var colorsCount = r.ReadInt32() / 3;
                    ushort* cur = line, endline = line + delta;
                    for (var c = 0; c < colorsCount; c++)
                    {
                        var count = r.ReadByte();
                        var color = r.ReadInt16();
                        var end = cur + count;
                        while (cur < end)
                        {
                            if (cur > endline) break;
                            *cur++ = (ushort)(color ^ 0x8000);
                        }
                    }
                }
                bmp.UnlockBits(bd);
            }
            else
            {
                Width = r.ReadInt32();
                Height = r.ReadInt32();

                var bmp = Image = new Bitmap(Width, Height, PixelFormat.Format16bppArgb1555);
                var bd = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly,
                    PixelFormat.Format16bppArgb1555);
                var line = (ushort*)bd.Scan0;
                var delta = bd.Stride >> 1;

                var cur = line;
                var len = (int)(r.BaseStream.Length - r.BaseStream.Position);

                var b = new byte[len];
                r.Read(b, 0, len);

                int j = 0, x = 0;
                while (j != len)
                {
                    var pixel = b[j++];
                    var count = pixel & 0x7f;

                    // black or white color
                    var c = (pixel & 0x80) != 0 ? (ushort)0x8000 : (ushort)0xffff;

                    for (var i = 0; i < count; ++i)
                    {
                        cur[x++] = c;
                        if (x < Width) continue;
                        cur += delta;
                        x = 0;
                    }
                }
                bmp.UnlockBits(bd);
            }
        }

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
            return BmpToBytes();
        }
        public void End() { }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
                new MetaInfo("MultiMap", items: new List<MetaInfo> {
                    new MetaInfo($"Width: {Image.Width}"),
                    new MetaInfo($"Height: {Image.Height}"),
                })
            };
            return nodes;
        }
    }
}
