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
    public unsafe class Binary_Land : IHaveMetaInfo, ITexture
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Land(r, (int)f.FileSize));

        #region Records

        Bitmap Image;
        static (object gl, object vulken, object unity, object unreal) Format = (
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            TextureUnityFormat.Unknown,
            TextureUnrealFormat.Unknown);

        #endregion

        // file: artLegacyMUL.uop:land/file00000.land
        public Binary_Land(BinaryReader r, int length)
        {
            fixed (byte* _ = r.ReadBytes(length))
            {
                var bmp = Image = new Bitmap(44, 44, PixelFormat.Format16bppArgb1555);
                var bd = bmp.LockBits(new Rectangle(0, 0, 44, 44), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

                var bdata = (ushort*)_;
                int xOffset = 21, xRun = 2;
                var line = (ushort*)bd.Scan0;
                var delta = bd.Stride >> 1;

                for (var y = 0; y < 22; ++y, --xOffset, xRun += 2, line += delta)
                {
                    ushort* cur = line + xOffset, end = cur + xRun;
                    while (cur < end) *cur++ = (ushort)(*bdata++ | 0x8000);
                }

                xOffset = 0; xRun = 44;
                for (var y = 0; y < 22; ++y, ++xOffset, xRun -= 2, line += delta)
                {
                    ushort* cur = line + xOffset, end = cur + xRun;
                    while (cur < end) *cur++ = (ushort)(*bdata++ | 0x8000);
                }
                bmp.UnlockBits(bd);
            }
        }

        public IDictionary<string, object> Data { get; } = null;
        public int Width { get; } = 44;
        public int Height { get; } = 44;
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
                new MetaInfo("Land", items: new List<MetaInfo> {
                    new MetaInfo($"Width: {Image.Width}"),
                    new MetaInfo($"Height: {Image.Height}"),
                })
            };
            return nodes;
        }
    }
}
