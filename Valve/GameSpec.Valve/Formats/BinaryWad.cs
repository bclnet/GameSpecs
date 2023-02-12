using GameSpec.Metadata;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Threading.Tasks;

namespace GameSpec.Formats
{
    public unsafe class BinaryWad : ITextureInfo, IGetMetadataInfo
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinaryWad(r, f));

        public BinaryWad(BinaryReader r, FileMetadata f) => Read(r, f);

        int width;
        int height;
        int mipMaps;
        byte[][] data;

        public void Read(BinaryReader r, FileMetadata f)
        {
            var type = Path.GetExtension(f.Path) switch
            {
                ".pic" => 0x42,
                ".tex" => 0x43,
                ".fnt" => 0x45,
                _ => 0
            };
            if (type == 0x42)
            {
                mipMaps = 1;
                width = (int)r.ReadUInt32();
                height = (int)r.ReadUInt32();
                var pixels = r.ReadBytes(Width * Height);
                var palette = r.ReadBytes(r.ReadUInt16() * 3);
                var data = new byte[Width * Height * 3];
                fixed (byte* _ = data)
                    for (int i = 0, j = 0; i < pixels.Length; i++, j += 3)
                    {
                    }

                //    var uiPixelIndex = i + j * width;
                //    var uiPaletteIndex = (hlUInt)pixels[uiPixelIndex] * 3;

                //    uiPixelIndex *= 3;
                //    lpPixelData[uiPixelIndex + 0] = lpPalette[uiPaletteIndex + 0];
                //    lpPixelData[uiPixelIndex + 1] = lpPalette[uiPaletteIndex + 1];
                //    lpPixelData[uiPixelIndex + 2] = lpPalette[uiPaletteIndex + 2];
                //}
            }
            else if (type == 0x43)
            {
                mipMaps = 4;
                var name = r.ReadFString(16);
                //r.Skip(16); // Scan past name.
                width = (int)r.ReadUInt32();
                height = (int)r.ReadUInt32();
                r.Skip(16); // Scan past pixel offset.
                var pixelSize = Width * Height;
                //Pixels = r.ReadBytes(Width * Height);
                //Palette = r.ReadBytes(r.ReadUInt16());

                //// Get pixel offset.
                //hlUInt uiPixelOffset = *(hlUInt*)lpData;
                //lpData += 16;

                //lpPixels = (hlByte*)pView->GetView() + uiPixelOffset;

                //hlUInt uiPixelSize = uiWidth * uiHeight;

                //switch (uiMipmap)
                //{
                //    case 1:
                //        lpData += (uiPixelSize);
                //        break;
                //    case 2:
                //        lpData += (uiPixelSize) + (uiPixelSize / 4);
                //        break;
                //    case 3:
                //        lpData += (uiPixelSize) + (uiPixelSize / 4) + (uiPixelSize / 16);
                //        break;
                //}

                //// Scan past data.
                //lpData += (uiPixelSize) + (uiPixelSize / 4) + (uiPixelSize / 16) + (uiPixelSize / 64);

                //// Get palette size.
                //uiPaletteSize = (hlUInt)(*(hlUInt16*)lpData);
                //lpData += sizeof(hlUInt16);

                //// Get palette.
                //lpPalette = lpData;
            }

            //switch (uiMipmap)
            //{
            //    case 1:
            //        Width /= 2;
            //        Height /= 2;
            //        break;
            //    case 2:
            //        Width /= 4;
            //        Height /= 4;
            //        break;
            //    case 3:
            //        Width /= 8;
            //        Height /= 8;
            //        break;
            //}
        }

        public IDictionary<string, object> Data => null;
        public int Width => width;
        public int Height => height;
        public int Depth => 0;
        public TextureFlags Flags => 0;
        public object UnityFormat => TextureUnityFormat.RGBA32;
        public object GLFormat => TextureGLFormat.Rgba;
        public int NumMipMaps => mipMaps;
        public byte[] this[int index]
        {
            get
            {
                //var uncompressedSize = this.GetMipMapTrueDataSize(index);
                //return _r.ReadBytes(uncompressedSize);
                return null;
            }
            set => throw new NotImplementedException();
        }
        public void MoveToData() { }

        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag) => new List<MetadataInfo> {
            new MetadataInfo(null, new MetadataContent { Type = "Texture", Name = Path.GetFileName(file.Path), Value = this }),
            //new MetadataInfo("DDS Texture", items: new List<MetadataInfo> {
            //    new MetadataInfo($"Width: {Header.dwWidth}"),
            //    new MetadataInfo($"Height: {Header.dwHeight}"),
            //    new MetadataInfo($"Mipmaps: {Header.dwMipMapCount}"),
            //}),
        };
    }
}
