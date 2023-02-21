using GameSpec.AC.Formats.Props;
using GameSpec.Formats;
using GameSpec.Metadata;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace GameSpec.AC.Formats.FileTypes
{
    [PakFileType(PakFileType.Texture)]
    public unsafe class Texture : FileType, IGetMetadataInfo, ITextureInfo
    {
        public readonly int Unknown;
        public readonly SurfacePixelFormat Format;
        public readonly int Length;
        public readonly byte[] SourceData;
        public readonly uint[] Palette;

        public Texture(BinaryReader r, FamilyGame game)
        {
            Id = r.ReadUInt32();
            Unknown = r.ReadInt32();
            Width = r.ReadInt32();
            Height = r.ReadInt32();
            Format = (SurfacePixelFormat)r.ReadUInt32();
            Length = r.ReadInt32();
            SourceData = r.ReadBytes(Length);
            var hasPalette = Format == SurfacePixelFormat.PFID_INDEX16 || Format == SurfacePixelFormat.PFID_P8;
            if (hasPalette) game.Family.Ensure();
            Palette = hasPalette ? DatabaseManager.Portal.GetFile<Palette>(r.ReadUInt32()).Colors : null;
            if (Format == SurfacePixelFormat.PFID_CUSTOM_RAW_JPEG)
            {
                using var image = new Bitmap(new MemoryStream(SourceData));
                Width = image.Width;
                Height = image.Height;
            }
        }

        public IDictionary<string, object> Data => null;
        public int Width { get; }
        public int Height { get; }
        public int Depth => 0;
        public TextureFlags Flags => 0;
        public object UnityFormat => Format switch
        {
            SurfacePixelFormat.PFID_DXT1 => TextureUnityFormat.DXT1,
            //SurfacePixelFormat.PFID_DXT3 => TextureUnityFormat.DXT3,
            SurfacePixelFormat.PFID_DXT5 => TextureUnityFormat.DXT5,
            SurfacePixelFormat.PFID_CUSTOM_RAW_JPEG or
            SurfacePixelFormat.PFID_R8G8B8 or
            SurfacePixelFormat.PFID_CUSTOM_LSCAPE_R8G8B8 or
            SurfacePixelFormat.PFID_INDEX16 or
            SurfacePixelFormat.PFID_A8 or
            SurfacePixelFormat.PFID_CUSTOM_LSCAPE_ALPHA or
            SurfacePixelFormat.PFID_P8 or
            SurfacePixelFormat.PFID_R5G6B5 => TextureUnityFormat.RGB24,
            SurfacePixelFormat.PFID_A8R8G8B8 or
            SurfacePixelFormat.PFID_A4R4G4B4 => TextureUnityFormat.RGBA32,
            _ => throw new ArgumentOutOfRangeException(nameof(Format), $"{Format}"),
        };
        public object GLFormat => Format switch
        {
            SurfacePixelFormat.PFID_DXT1 => TextureGLFormat.CompressedRgbaS3tcDxt1Ext,
            SurfacePixelFormat.PFID_DXT3 => TextureGLFormat.CompressedRgbaS3tcDxt3Ext,
            SurfacePixelFormat.PFID_DXT5 => TextureGLFormat.CompressedRgbaS3tcDxt5Ext,
            SurfacePixelFormat.PFID_CUSTOM_RAW_JPEG or
            SurfacePixelFormat.PFID_R8G8B8 or
            SurfacePixelFormat.PFID_CUSTOM_LSCAPE_R8G8B8 or
            SurfacePixelFormat.PFID_A8 or
            SurfacePixelFormat.PFID_CUSTOM_LSCAPE_ALPHA or
            SurfacePixelFormat.PFID_R5G6B5 => (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte),
            SurfacePixelFormat.PFID_INDEX16 or
            SurfacePixelFormat.PFID_P8 or
            SurfacePixelFormat.PFID_A8R8G8B8 or
            SurfacePixelFormat.PFID_A4R4G4B4 => (TextureGLFormat.Rgba8, TextureGLPixelFormat.Rgba, TextureGLPixelType.UnsignedByte),
            _ => throw new ArgumentOutOfRangeException(nameof(Format), $"{Format}"),
        };
        public int NumMipMaps => 1;
        public byte[] this[int index]
        {
            get
            {
                // https://www.hanselman.com/blog/how-do-you-use-systemdrawing-in-net-core
                // https://stackoverflow.com/questions/1563038/fast-work-with-bitmaps-in-c-sharp
                switch (Format)
                {
                    case SurfacePixelFormat.PFID_CUSTOM_RAW_JPEG:
                        {
                            var d = new byte[Width * Height * 3];
                            using var image = new Bitmap(new MemoryStream(SourceData));
                            var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                            var s = (byte*)data.Scan0.ToPointer();
                            for (var i = 0; i < d.Length; i += 3) { d[i + 0] = s[i + 0]; d[i + 1] = s[i + 1]; d[i + 2] = s[i + 2]; }
                            image.UnlockBits(data);
                            return d;
                        }
                    case SurfacePixelFormat.PFID_DXT1:
                    case SurfacePixelFormat.PFID_DXT3:
                    case SurfacePixelFormat.PFID_DXT5: return SourceData;
                    case SurfacePixelFormat.PFID_R8G8B8: // RGB
                    case SurfacePixelFormat.PFID_CUSTOM_LSCAPE_R8G8B8: return SourceData;
                    //case SurfacePixelFormat.PFID_CUSTOM_LSCAPE_R8G8B8:
                    //    {
                    //        var d = new byte[Width * Height * 3];
                    //        var s = SourceData;
                    //        for (int i = 0; i < d.Length; i += 3) { d[i + 0] = s[i + 2]; d[i + 1] = s[i + 1]; d[i + 2] = s[i + 0]; }
                    //        return d;
                    //    }
                    case SurfacePixelFormat.PFID_A8R8G8B8: // ARGB format. Most UI textures fall into this category
                        {
                            var d = new byte[Width * Height * 4];
                            var s = SourceData;
                            for (var i = 0; i < d.Length; i += 4) { d[i + 0] = s[i + 1]; d[i + 1] = s[i + 2]; d[i + 2] = s[i + 3]; d[i + 3] = s[i + 0]; }
                            return d;
                        }
                    case SurfacePixelFormat.PFID_A8: // Greyscale, also known as Cairo A8.
                    case SurfacePixelFormat.PFID_CUSTOM_LSCAPE_ALPHA:
                        {
                            var d = new byte[Width * Height * 3];
                            var s = SourceData;
                            for (int i = 0, j = 0; i < d.Length; i += 3, j++) { d[i + 0] = s[j]; d[i + 1] = s[j]; d[i + 2] = s[j]; }
                            return d;
                        }
                    case SurfacePixelFormat.PFID_R5G6B5: // 16-bit RGB
                        {
                            var d = new byte[Width * Height * 3];
                            fixed (byte* _ = SourceData)
                            {
                                var s = (ushort*)_;
                                for (int i = 0, j = 0; i < d.Length; i += 4, j++)
                                {
                                    var val = s[j];
                                    d[i + 0] = (byte)((val >> 8 & 0xF) / 0xF * 255);
                                    d[i + 1] = (byte)((val >> 4 & 0xF) / 0xF * 255);
                                    d[i + 2] = (byte)((val & 0xF) / 0xF * 255);
                                }
                            }
                            return d;
                        }
                    case SurfacePixelFormat.PFID_A4R4G4B4:
                        {
                            var d = new byte[Width * Height * 4];
                            fixed (byte* s_ = SourceData)
                            {
                                var s = (ushort*)s_;
                                for (int i = 0, j = 0; i < d.Length; i += 4, j++)
                                {
                                    var val = s[j];
                                    d[i + 0] = (byte)(((val & 0xF800) >> 11) << 3);
                                    d[i + 1] = (byte)(((val & 0x7E0) >> 5) << 2);
                                    d[i + 2] = (byte)((val & 0x1F) << 3);
                                }
                            }
                            return d;
                        }
                    case SurfacePixelFormat.PFID_INDEX16: // 16-bit indexed colors. Index references position in a palette;
                        {
                            var p = Palette;
                            var d = new byte[Width * Height * 4];
                            fixed (byte* s_ = SourceData)
                            fixed (byte* d_ = d)
                            {
                                var s = (ushort*)s_;
                                var d2 = (uint*)d_;
                                for (var i = 0; i < d.Length >> 2; i++) d2[i] = p[s[i]];
                            }
                            return d;
                        }
                    case SurfacePixelFormat.PFID_P8: // Indexed
                        {
                            var p = Palette;
                            var d = new byte[Width * Height * 4];
                            var s = SourceData;
                            fixed (byte* d_ = d)
                            {
                                var d2 = (uint*)d_;
                                for (var i = 0; i < d.Length >> 2; i++) d2[i] = p[s[i]];
                            }
                            return d;
                        }
                    default: Console.WriteLine($"Unhandled SurfacePixelFormat ({Format}) in RenderSurface {Id:X8}"); return null;
                }
            }
            set => throw new NotImplementedException();
        }
        public void MoveToData() { }

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                //new MetadataInfo(null, new MetadataContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "PICTURE" }),
                new MetadataInfo(null, new MetadataContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
                new MetadataInfo($"{nameof(Texture)}: {Id:X8}", items: new List<MetadataInfo> {
                    new MetadataInfo($"Unknown: {Unknown}"),
                    new MetadataInfo($"Width: {Width}"),
                    new MetadataInfo($"Height: {Height}"),
                    new MetadataInfo($"Type: {Format}"),
                    new MetadataInfo($"Size: {Length} bytes"),
                })
            };
            return nodes;
        }
    }
}
