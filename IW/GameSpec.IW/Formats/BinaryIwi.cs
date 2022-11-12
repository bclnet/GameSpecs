using GameSpec.Formats;
using GameSpec.Metadata;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.IW.Formats
{
    // https://github.com/DentonW/DevIL/blob/master/DevIL/src-IL/src/il_iwi.cpp
    public class BinaryIwi : ITextureInfo, IGetMetadataInfo
    {
        public static Task<object> Factory(BinaryReader r, FileMetadata f, PakFile s) => Task.FromResult((object)new BinaryIwi(r));

        BinaryReader _r;

        public enum FORMAT : byte
        {
            /// <summary>
            /// ARGB32
            /// </summary>
            ARGB32 = 0x01,
            /// <summary>
            /// RGB24
            /// </summary>
            RGB24 = 0x02,
            /// <summary>
            /// GA16
            /// </summary>
            GA16 = 0x03,
            /// <summary>
            /// A8
            /// </summary>
            A8 = 0x04,
            /// <summary>
            /// JPG
            /// </summary>
            JPG = 0x07,
            /// <summary>
            /// DXT1
            /// </summary>
            DXT1 = 0x0B,
            /// <summary>
            /// DXT3
            /// </summary>
            DXT3 = 0x0C,
            /// <summary>
            /// DXT5
            /// </summary>
            DXT5 = 0x0D,
        }

        /// <summary>
        /// Describes a IWI file header.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        public unsafe struct HEADER
        {
            public const int SizeOf = 8;
            /// <summary>
            /// IWi
            /// </summary>
            public const uint MAGIC = 0x69574900;
            /// <summary>
            /// Format
            /// </summary>
            [MarshalAs(UnmanagedType.U1)] public FORMAT Format;
            /// <summary>
            /// Usage
            /// </summary>
            public byte Usage;
            /// <summary>
            /// Width
            /// </summary>
            public ushort Width;
            /// <summary>
            /// Height
            /// </summary>
            public ushort Height;
            /// <summary>
            /// Depth
            /// </summary>
            public ushort Depth;

            /// <summary>
            /// Verifies this instance.
            /// </summary>
            public void Verify()
            {
                if (Width == 0 || Height == 0)
                    throw new FormatException($"Invalid DDS file header");
                if (Format >= FORMAT.DXT1 && Format <= FORMAT.DXT5 && Width != NextPower2(Width) && Height != NextPower2(Height))
                    throw new FormatException($"DXT images must have power-of-2 dimensions..");
            }

            static int NextPower2(int n)
            {
                var power = 1;
                while (power < n) power <<= 1;
                return power;
            }

            /// <summary>
            /// ReadAndDecode
            /// </summary>
            //public void ReadAndDecode(TextureInfo source, BinaryReader r)
            //{
            //    //var hasMipmaps = source.HasMipmaps = dwCaps.HasFlag(DDSCAPS.MIPMAP);
            //    //source.Mipmaps = hasMipmaps ? (ushort)dwMipMapCount : (ushort)1U;
            //    source.Width = (int)Width;
            //    source.Height = (int)Height;
            //    source.BytesPerPixel = 4;
            //    // If the DDS file contains uncompressed data.
            //    //if (ddspf.dwFlags.HasFlag(DDPF.RGB))
            //    //{
            //    //    // some permutation of RGB
            //    //    if (!ddspf.dwFlags.HasFlag(DDPF.ALPHAPIXELS)) throw new NotImplementedException("Unsupported DDS file pixel format.");
            //    //    else
            //    //    {
            //    //        // some permutation of RGBA
            //    //        if (ddspf.dwRGBBitCount != 32) throw new FormatException("Invalid DDS file pixel format.");
            //    //        else if (ddspf.dwBBitMask == 0x000000FF && ddspf.dwGBitMask == 0x0000FF00 && ddspf.dwRBitMask == 0x00FF0000 && ddspf.dwABitMask == 0xFF000000) source.UnityFormat = TextureUnityFormat.BGRA32;
            //    //        else if (ddspf.dwABitMask == 0x000000FF && ddspf.dwRBitMask == 0x0000FF00 && ddspf.dwGBitMask == 0x00FF0000 && ddspf.dwBBitMask == 0xFF000000) source.UnityFormat = TextureUnityFormat.ARGB32;
            //    //        else throw new NotImplementedException("Unsupported DDS file pixel format.");
            //    //        source.Data = new byte[!hasMipmaps ? (int)(dwPitchOrLinearSize * dwHeight) : TextureInfo.GetMipmapDataSize(source.Width, source.Height, source.BytesPerPixel)];
            //    //        r.ReadToEnd(source.Data);
            //    //    }
            //    //}
            //    //else if (ddspf.dwFourCC == DXT1)
            //    //{
            //    //    source.UnityFormat = TextureUnityFormat.ARGB32;
            //    //    source.Data = DecodeDXT1ToARGB(r.ReadToEnd(), dwWidth, dwHeight, ddspf, source.Mipmaps);
            //    //}
            //    //else if (ddspf.dwFourCC == DXT3)
            //    //{
            //    //    source.UnityFormat = TextureUnityFormat.ARGB32;
            //    //    source.Data = DecodeDXT3ToARGB(r.ReadToEnd(), dwWidth, dwHeight, ddspf, source.Mipmaps);
            //    //}
            //    //else if (ddspf.dwFourCC == DXT5)
            //    //{
            //    //    source.UnityFormat = TextureUnityFormat.ARGB32;
            //    //    source.Data = DecodeDXT5ToARGB(r.ReadToEnd(), dwWidth, dwHeight, ddspf, source.Mipmaps);
            //    //}
            //    //else throw new NotImplementedException("Unsupported DDS file pixel format.");
            //}
        }

        public BinaryIwi() { }
        public BinaryIwi(BinaryReader r)
        {
            _r = r;
            var magic = r.ReadUInt32();
            Version = (byte)(magic >> 24);
            magic <<= 8;
            if (magic != HEADER.MAGIC) throw new FormatException($"Invalid IWI file magic: \"{magic}\".");
            if (Version == 0x08) r.Seek(8);
            Header = r.ReadT<HEADER>(HEADER.SizeOf);
            Header.Verify();
        }

        public HEADER Header;
        public byte Version;

        public IDictionary<string, object> Data => null;
        public int Width => (int)Header.Width;
        public int Height => (int)Header.Height;
        public int Depth => 0;
        public TextureFlags Flags => 0;
        public TextureUnityFormat UnityFormat => Header.Format switch
        {
            FORMAT.DXT1 => TextureUnityFormat.DXT1,
            //FORMAT.DXT3 => TextureUnityFormat.DXT3,
            FORMAT.DXT5 => TextureUnityFormat.DXT5,
            _ => throw new ArgumentOutOfRangeException(nameof(Header.Format), $"{Header.Format}"),
        };
        public TextureGLFormat GLFormat => Header.Format switch
        {
            FORMAT.DXT1 => TextureGLFormat.CompressedRgbaS3tcDxt1Ext,
            FORMAT.DXT3 => TextureGLFormat.CompressedRgbaS3tcDxt3Ext,
            FORMAT.DXT5 => TextureGLFormat.CompressedRgbaS3tcDxt5Ext,
            _ => throw new ArgumentOutOfRangeException(nameof(Header.Format), $"{Header.Format}"),
        };
        public int NumMipMaps => (int)0;//Header.dwMipMapCount;
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
            new MetadataInfo("DDS Texture", items: new List<MetadataInfo> {
                new MetadataInfo($"Width: {Header.Width}"),
                new MetadataInfo($"Height: {Header.Height}"),
                new MetadataInfo($"Mipmaps: {0}"),
            }),
        };
    }
}
