using System;
using System.IO;
using System.Runtime.InteropServices;
using static OpenStack.Graphics.DirectX.FourCC;
using static OpenStack.Graphics.DirectX.DXGI_FORMAT;

// https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-header
// https://learn.microsoft.com/en-us/windows/win32/direct3ddds/dx-graphics-dds-pguide
namespace OpenStack.Graphics.DirectX
{
    /// <summary>
    /// Flags to indicate which members contain valid data.
    /// </summary>
    [Flags]
    public enum DDSD : uint
    {
        /// <summary>
        /// Required in every .dds file.
        /// </summary>
        CAPS = 0x00000001,
        /// <summary>
        /// Required in every .dds file.
        /// </summary>
        HEIGHT = 0x00000002,
        /// <summary>
        /// Required in every .dds file.
        /// </summary>
        WIDTH = 0x00000004,
        /// <summary>
        /// Required when pitch is provided for an uncompressed texture.
        /// </summary>
        PITCH = 0x00000008,
        /// <summary>
        /// Required in every .dds file.
        /// </summary>
        PIXELFORMAT = 0x00001000,
        /// <summary>
        /// Required in a mipmapped texture.
        /// </summary>
        MIPMAPCOUNT = 0x00020000,
        /// <summary>
        /// Required when pitch is provided for a compressed texture.
        /// </summary>
        LINEARSIZE = 0x00080000,
        /// <summary>
        /// Required in a depth texture.
        /// </summary>
        DEPTH = 0x00800000,
        HEADER_FLAGS_TEXTURE = CAPS | HEIGHT | WIDTH | PIXELFORMAT,
        HEADER_FLAGS_MIPMAP = MIPMAPCOUNT,
        HEADER_FLAGS_VOLUME = DEPTH,
        HEADER_FLAGS_PITCH = PITCH,
        HEADER_FLAGS_LINEARSIZE = LINEARSIZE,
    }

    /// <summary>
    /// Specifies the complexity of the surfaces stored.
    /// </summary>
    [Flags]
    public enum DDSCAPS : uint //: dwSurfaceFlags
    {
        /// <summary>
        /// Optional; must be used on any file that contains more than one surface (a mipmap, a cubic environment map, or mipmapped volume texture).
        /// </summary>
        COMPLEX = 0x00000008,
        /// <summary>
        /// Required
        /// </summary>
        TEXTURE = 0x00001000,
        /// <summary>
        /// Optional; should be used for a mipmap.
        /// </summary>
        MIPMAP = 0x00400000,
        SURFACE_FLAGS_MIPMAP = COMPLEX | MIPMAP,
        SURFACE_FLAGS_TEXTURE = TEXTURE,
        SURFACE_FLAGS_CUBEMAP = COMPLEX,
    }

    /// <summary>
    /// Additional detail about the surfaces stored.
    /// </summary>
    [Flags]
    public enum DDSCAPS2 : uint //: dwCubemapFlags
    {
        /// <summary>
        /// Required for a cube map.
        /// </summary>
        CUBEMAP = 0x00000200,
        /// <summary>
        /// Required when these surfaces are stored in a cube map.	
        /// </summary>
        CUBEMAPPOSITIVEX = 0x00000400,
        /// <summary>
        /// Required when these surfaces are stored in a cube map.
        /// </summary>
        CUBEMAPNEGATIVEX = 0x00000800,
        /// <summary>
        /// Required when these surfaces are stored in a cube map.
        /// </summary>
        CUBEMAPPOSITIVEY = 0x00001000,
        /// <summary>
        /// Required when these surfaces are stored in a cube map.
        /// </summary>
        CUBEMAPNEGATIVEY = 0x00002000,
        /// <summary>
        /// Required when these surfaces are stored in a cube map.
        /// </summary>
        CUBEMAPPOSITIVEZ = 0x00004000,
        /// <summary>
        /// Required when these surfaces are stored in a cube map.
        /// </summary>
        CUBEMAPNEGATIVEZ = 0x00008000,
        /// <summary>
        /// Required for a volume texture.
        /// </summary>
        VOLUME = 0x00200000,
        CUBEMAP_POSITIVEX = CUBEMAP | CUBEMAPPOSITIVEX,
        CUBEMAP_NEGATIVEX = CUBEMAP | CUBEMAPNEGATIVEX,
        CUBEMAP_POSITIVEY = CUBEMAP | CUBEMAPPOSITIVEY,
        CUBEMAP_NEGATIVEY = CUBEMAP | CUBEMAPNEGATIVEY,
        CUBEMAP_POSITIVEZ = CUBEMAP | CUBEMAPPOSITIVEZ,
        CUBEMAP_NEGATIVEZ = CUBEMAP | CUBEMAPNEGATIVEZ,
        CUBEMAP_ALLFACES = CUBEMAPPOSITIVEX | CUBEMAPNEGATIVEX | CUBEMAPPOSITIVEY | CUBEMAPNEGATIVEY | CUBEMAPPOSITIVEZ | CUBEMAPNEGATIVEZ,
        FLAGS_VOLUME = VOLUME,
    }

    /// <summary>
    /// Describes a DDS file header.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
    public unsafe struct DDS_HEADER
    {
        //public static uint MAKEFOURCC(string text) => ((uint)(byte)(text[0]) | ((uint)(byte)(text[1]) << 8) | ((uint)(byte)(text[2]) << 16 | ((uint)(byte)(text[3]) << 24)));
        /// <summary>
        /// Struct
        /// </summary>
        public static (string, int) Struct = ($"<7I44s{"8I"}5I", SizeOf);
        /// <summary>
        /// MAGIC
        /// </summary>
        public const uint MAGIC = 0x20534444; // "DDS "
        /// <summary>
        /// Struct
        /// </summary>
        public const int SizeOf = 124;
        /// <summary>
        /// Size of structure. This member must be set to 124.
        /// </summary>
        /// <value>
        /// The size of the dw.
        /// </value>
        public uint dwSize; //: 124
        /// <summary>
        /// Flags to indicate which members contain valid data.
        /// </summary>
        [MarshalAs(UnmanagedType.U4)] public DDSD dwFlags;
        /// <summary>
        /// Surface height (in pixels).
        /// </summary>
        public uint dwHeight;
        /// <summary>
        /// Surface width (in pixels).
        /// </summary>
        public uint dwWidth;
        /// <summary>
        /// The pitch or number of bytes per scan line in an uncompressed texture; the total number of bytes in the top level texture for a compressed texture. For information about how to compute the pitch, see the DDS File Layout section of the Programming Guide for DDS.
        /// </summary>
        public uint dwPitchOrLinearSize;
        /// <summary>
        /// Depth of a volume texture (in pixels), otherwise unused.
        /// </summary>
        public uint dwDepth; // only if DDS_HEADER_FLAGS_VOLUME is set in flags
        /// <summary>
        /// Number of mipmap levels, otherwise unused.
        /// </summary>
        public uint dwMipMapCount;
        /// <summary>
        /// Unused.
        /// </summary>
        public fixed uint dwReserved1[11];
        /// <summary>
        /// The pixel format (see DDS_PIXELFORMAT).
        /// </summary>
        public DDS_PIXELFORMAT ddspf;
        /// <summary>
        /// Specifies the complexity of the surfaces stored.
        /// </summary>
        [MarshalAs(UnmanagedType.U4)] public DDSCAPS dwCaps;
        /// <summary>
        /// Additional detail about the surfaces stored.
        /// </summary>
        [MarshalAs(UnmanagedType.U4)] public DDSCAPS2 dwCaps2;
        /// <summary>
        /// The dw caps3
        /// </summary>
        public uint dwCaps3;
        /// <summary>
        /// The dw caps4
        /// </summary>
        public uint dwCaps4;
        /// <summary>
        /// The dw reserved2
        /// </summary>
        public uint dwReserved2;

        /// <summary>
        /// Verifies this instance.
        /// </summary>
        public readonly void Verify()
        {
            if (dwSize != 124) throw new FormatException($"Invalid DDS file header size: {dwSize}.");
            else if (!dwFlags.HasFlag(DDSD.HEIGHT | DDSD.WIDTH)) throw new FormatException($"Invalid DDS file flags: {dwFlags}.");
            else if (!dwCaps.HasFlag(DDSCAPS.TEXTURE)) throw new FormatException($"Invalid DDS file caps: {dwCaps}.");
            else if (ddspf.dwSize != 32) throw new FormatException($"Invalid DDS file pixel format size: {ddspf.dwSize}.");
        }

        /// <summary>
        /// Read
        /// </summary>
        /// https://gist.github.com/tilkinsc/13191c0c1e5d6b25fbe79bbd2288a673
        /// https://github.com/BinomialLLC/basis_universal/wiki/OpenGL-texture-format-enums-table
        /// https://www.g-truc.net/post-0335.html
        /// https://www.reedbeta.com/blog/understanding-bcn-texture-compression-formats/
        public static byte[] Read(BinaryReader r, bool readMagic, out DDS_HEADER header, out DDS_HEADER_DXT10? headerDXT10, out (object type, int blockSize, object gl, object vulken, object unity, object unreal) format)
        {
            if (readMagic)
            {
                var magic = r.ReadUInt32();
                if (magic != MAGIC) throw new FormatException($"Invalid DDS file magic: \"{magic}\".");
            }
            header = r.ReadS<DDS_HEADER>();
            header.Verify();
            ref DDS_PIXELFORMAT ddspf = ref header.ddspf;
            headerDXT10 = ddspf.dwFourCC == DX10
                ? r.ReadS<DDS_HEADER_DXT10>()
                : (DDS_HEADER_DXT10?)null;
            format = ddspf.dwFourCC switch
            {
                0 => MakeFormat(ref ddspf),
                DXT1 => (DXT1, 8, TextureGLFormat.CompressedRgbaS3tcDxt1Ext, TextureGLFormat.CompressedRgbaS3tcDxt1Ext, TextureUnityFormat.DXT1, TextureUnrealFormat.DXT1),
                DXT3 => (DXT3, 16, TextureGLFormat.CompressedRgbaS3tcDxt3Ext, TextureGLFormat.CompressedRgbaS3tcDxt3Ext, TextureUnityFormat.DXT3_POLYFILL, TextureUnrealFormat.DXT3),
                DXT5 => (DXT5, 16, TextureGLFormat.CompressedRgbaS3tcDxt5Ext, TextureGLFormat.CompressedRgbaS3tcDxt5Ext, TextureUnityFormat.DXT5, TextureUnrealFormat.DXT5),
                DX10 => (headerDXT10?.dxgiFormat) switch
                {
                    BC1_UNORM => (BC1_UNORM, 8, TextureGLFormat.CompressedRgbaS3tcDxt1Ext, TextureGLFormat.CompressedRgbaS3tcDxt1Ext, TextureUnityFormat.DXT1, TextureUnrealFormat.DXT1),
                    BC1_UNORM_SRGB => (BC1_UNORM_SRGB, 8, TextureGLFormat.CompressedSrgbS3tcDxt1Ext, TextureGLFormat.CompressedSrgbS3tcDxt1Ext, TextureUnityFormat.DXT1, TextureUnrealFormat.DXT1),
                    BC2_UNORM => (BC2_UNORM, 16, TextureGLFormat.CompressedRgbaS3tcDxt3Ext, TextureGLFormat.CompressedRgbaS3tcDxt3Ext, TextureUnityFormat.DXT3_POLYFILL, TextureUnrealFormat.DXT3),
                    BC2_UNORM_SRGB => (BC2_UNORM_SRGB, 16, TextureGLFormat.CompressedSrgbAlphaS3tcDxt1Ext, TextureGLFormat.CompressedSrgbAlphaS3tcDxt1Ext, TextureUnityFormat.Unknown, TextureUnrealFormat.Unknown),
                    BC3_UNORM => (BC3_UNORM, 16, TextureGLFormat.CompressedRgbaS3tcDxt5Ext, TextureGLFormat.CompressedRgbaS3tcDxt5Ext, TextureUnityFormat.DXT5, TextureUnrealFormat.DXT5),
                    BC3_UNORM_SRGB => (BC3_UNORM_SRGB, 16, TextureGLFormat.CompressedSrgbAlphaS3tcDxt5Ext, TextureGLFormat.CompressedSrgbAlphaS3tcDxt5Ext, TextureUnityFormat.Unknown, TextureUnrealFormat.Unknown),
                    BC4_UNORM => (BC4_UNORM, 8, TextureGLFormat.CompressedRedRgtc1, TextureGLFormat.CompressedRedRgtc1, TextureUnityFormat.BC4, TextureUnrealFormat.BC4),
                    BC4_SNORM => (BC4_SNORM, 8, TextureGLFormat.CompressedSignedRedRgtc1, TextureGLFormat.CompressedSignedRedRgtc1, TextureUnityFormat.BC4, TextureUnrealFormat.BC4),
                    BC5_UNORM => (BC5_UNORM, 16, TextureGLFormat.CompressedRgRgtc2, TextureGLFormat.CompressedRgRgtc2, TextureUnityFormat.BC5, TextureUnrealFormat.BC5),
                    BC5_SNORM => (BC5_SNORM, 16, TextureGLFormat.CompressedSignedRgRgtc2, TextureGLFormat.CompressedSignedRgRgtc2, TextureUnityFormat.BC5, TextureUnrealFormat.BC5),
                    BC6H_UF16 => (BC6H_UF16, 16, TextureGLFormat.CompressedRgbBptcUnsignedFloat, TextureGLFormat.CompressedRgbBptcUnsignedFloat, TextureUnityFormat.BC6H, TextureUnrealFormat.BC6H),
                    BC6H_SF16 => (BC6H_SF16, 16, TextureGLFormat.CompressedRgbBptcSignedFloat, TextureGLFormat.CompressedRgbBptcSignedFloat, TextureUnityFormat.BC6H, TextureUnrealFormat.BC6H),
                    BC7_UNORM => (BC7_UNORM, 16, TextureGLFormat.CompressedRgbaBptcUnorm, TextureGLFormat.CompressedRgbaBptcUnorm, TextureUnityFormat.BC7, TextureUnrealFormat.BC7),
                    BC7_UNORM_SRGB => (BC7_UNORM_SRGB, 16, TextureGLFormat.CompressedSrgbAlphaBptcUnorm, TextureGLFormat.CompressedSrgbAlphaBptcUnorm, TextureUnityFormat.BC7, TextureUnrealFormat.BC7),
                    R8_UNORM => (R8_UNORM, 1, (TextureGLFormat.R8, TextureGLPixelFormat.Red, TextureGLPixelType.Byte), (TextureGLFormat.R8, TextureGLPixelFormat.Red, TextureGLPixelType.Byte), TextureUnityFormat.R8, TextureUnrealFormat.R8), //: guess
                    _ => throw new ArgumentOutOfRangeException(nameof(headerDXT10.Value.dxgiFormat), $"{headerDXT10?.dxgiFormat}"),
                },
                // BC4U/BC4S/ATI2/BC55/R8G8_B8G8/G8R8_G8B8/UYVY-packed/YUY2-packed unsupported
                _ => throw new ArgumentOutOfRangeException(nameof(ddspf.dwFourCC), $"{ddspf.dwFourCC}"),
            };
            return r.ReadToEnd();
        }

        static (object type, int blockSize, object gl, object vulken, object unity, object unreal) MakeFormat(ref DDS_PIXELFORMAT f)
        {
            return ("Raw", (int)f.dwRGBBitCount >> 2,
                (TextureGLFormat.Rgba, TextureGLPixelFormat.Rgba, TextureGLPixelType.Byte),
                (TextureGLFormat.Rgba, TextureGLPixelFormat.Rgba, TextureGLPixelType.Byte),
                TextureUnityFormat.RGBA32,
                TextureUnrealFormat.R8G8B8A8);
        }

#if false

        /// <summary>
        /// ReadAndDecode
        /// </summary>
        public void ReadAndDecode(EmptyTexture source, BinaryReader r)
        {
            var hasMipmaps = source.HasMipmaps = dwCaps.HasFlag(DDSCAPS.MIPMAP);
            source.Mipmaps = hasMipmaps ? (ushort)dwMipMapCount : (ushort)1U;
            source.Width = (int)dwWidth;
            source.Height = (int)dwHeight;
            source.BytesPerPixel = 4;
            // If the DDS file contains uncompressed data.
            if (ddspf.dwFlags.HasFlag(DDPF.RGB))
            {
                // some permutation of RGB
                if (!ddspf.dwFlags.HasFlag(DDPF.ALPHAPIXELS)) throw new NotImplementedException("Unsupported DDS file pixel format.");
                else
                {
                    // some permutation of RGBA
                    if (ddspf.dwRGBBitCount != 32) throw new FormatException("Invalid DDS file pixel format.");
                    else if (ddspf.dwBBitMask == 0x000000FF && ddspf.dwGBitMask == 0x0000FF00 && ddspf.dwRBitMask == 0x00FF0000 && ddspf.dwABitMask == 0xFF000000) source.UnityFormat = TextureUnityFormat.BGRA32;
                    else if (ddspf.dwABitMask == 0x000000FF && ddspf.dwRBitMask == 0x0000FF00 && ddspf.dwGBitMask == 0x00FF0000 && ddspf.dwBBitMask == 0xFF000000) source.UnityFormat = TextureUnityFormat.ARGB32;
                    else throw new NotImplementedException("Unsupported DDS file pixel format.");
                    source.Data = new byte[!hasMipmaps ? (int)(dwPitchOrLinearSize * dwHeight) : TextureHelper.GetMipmapDataSize(source.Width, source.Height, source.BytesPerPixel)];
                    r.ReadToEnd(source.Data);
                }
            }
            else if (ddspf.dwFourCC == DXT1)
            {
                source.UnityFormat = TextureUnityFormat.ARGB32;
                source.Data = DecodeDXT1ToARGB(r.ReadToEnd(), dwWidth, dwHeight, ddspf, source.Mipmaps);
            }
            else if (ddspf.dwFourCC == DXT3)
            {
                source.UnityFormat = TextureUnityFormat.ARGB32;
                source.Data = DecodeDXT3ToARGB(r.ReadToEnd(), dwWidth, dwHeight, ddspf, source.Mipmaps);
            }
            else if (ddspf.dwFourCC == DXT5)
            {
                source.UnityFormat = TextureUnityFormat.ARGB32;
                source.Data = DecodeDXT5ToARGB(r.ReadToEnd(), dwWidth, dwHeight, ddspf, source.Mipmaps);
            }
            else throw new NotImplementedException("Unsupported DDS file pixel format.");
        }

        #region Decode

        /// <summary>
        /// Decodes a DXT1-compressed 4x4 block of texels using a prebuilt 4-color color table.
        /// </summary>
        /// <remarks>See https://msdn.microsoft.com/en-us/library/windows/desktop/bb694531(v=vs.85).aspx#BC1 </remarks>
        static GXColor32[] DecodeDXT1TexelBlock(BinaryReader r, GXColor[] colorTable)
        {
            Debug.Assert(colorTable.Length == 4);
            // Read pixel color indices.
            var colorIndices = new uint[16];
            var colorIndexBytes = new byte[4];
            r.Read(colorIndexBytes, 0, colorIndexBytes.Length);
            const uint bitsPerColorIndex = 2;
            for (var rowIndex = 0U; rowIndex < 4; rowIndex++)
            {
                var rowBaseColorIndexIndex = 4 * rowIndex;
                var rowBaseBitOffset = 8 * rowIndex;
                for (var columnIndex = 0U; columnIndex < 4; columnIndex++)
                {
                    // Color indices are arranged from right to left.
                    var bitOffset = rowBaseBitOffset + (bitsPerColorIndex * (3 - columnIndex));
                    colorIndices[rowBaseColorIndexIndex + columnIndex] = (uint)MathX.GetBits(bitOffset, bitsPerColorIndex, colorIndexBytes);
                }
            }
            // Calculate pixel colors.
            var colors = new GXColor32[16];
            for (var i = 0; i < 16; i++) colors[i] = colorTable[colorIndices[i]];
            return colors;
        }

        /// <summary>
        /// Builds a 4-color color table for a DXT1-compressed 4x4 block of texels and then decodes the texels.
        /// </summary>
        /// <remarks>See https://msdn.microsoft.com/en-us/library/windows/desktop/bb694531(v=vs.85).aspx#BC1 </remarks>
        static GXColor32[] DecodeDXT1TexelBlock(BinaryReader r, bool containsAlpha)
        {
            // Create the color table.
            var colorTable = new GXColor[4];
            colorTable[0] = r.ReadUInt16().B565ToColor();
            colorTable[1] = r.ReadUInt16().B565ToColor();
            if (!containsAlpha)
            {
                colorTable[2] = GXColor.Lerp(colorTable[0], colorTable[1], 1.0f / 3);
                colorTable[3] = GXColor.Lerp(colorTable[0], colorTable[1], 2.0f / 3);
            }
            else
            {
                colorTable[2] = GXColor.Lerp(colorTable[0], colorTable[1], 1.0f / 2);
                colorTable[3] = new GXColor(0, 0, 0, 0);
            }
            // Calculate pixel colors.
            return DecodeDXT1TexelBlock(r, colorTable);
        }

        /// <summary>
        /// Decodes a DXT3-compressed 4x4 block of texels.
        /// </summary>
        /// <remarks>See https://msdn.microsoft.com/en-us/library/windows/desktop/bb694531(v=vs.85).aspx#BC2 </remarks>
        static GXColor32[] DecodeDXT3TexelBlock(BinaryReader r)
        {
            // Read compressed pixel alphas.
            var compressedAlphas = new byte[16];
            for (var rowIndex = 0; rowIndex < 4; rowIndex++)
            {
                var compressedAlphaRow = r.ReadUInt16();
                // Each compressed alpha is 4 bits.
                for (var columnIndex = 0; columnIndex < 4; columnIndex++) compressedAlphas[(4 * rowIndex) + columnIndex] = (byte)((compressedAlphaRow >> (columnIndex * 4)) & 0xF);
            }
            // Calculate pixel alphas.
            var alphas = new byte[16];
            for (var i = 0; i < 16; i++) { var alphaPercent = (float)compressedAlphas[i] / 15; alphas[i] = (byte)Math.Round(alphaPercent * 255); }
            // Create the color table.
            var colorTable = new GXColor[4];
            colorTable[0] = r.ReadUInt16().B565ToColor();
            colorTable[1] = r.ReadUInt16().B565ToColor();
            colorTable[2] = GXColor.Lerp(colorTable[0], colorTable[1], 1.0f / 3);
            colorTable[3] = GXColor.Lerp(colorTable[0], colorTable[1], 2.0f / 3);
            // Calculate pixel colors.
            var colors = DecodeDXT1TexelBlock(r, colorTable);
            for (var i = 0; i < 16; i++) colors[i].A = alphas[i];
            return colors;
        }

        /// <summary>
        /// Decodes a DXT5-compressed 4x4 block of texels.
        /// </summary>
        /// <remarks>See https://msdn.microsoft.com/en-us/library/windows/desktop/bb694531(v=vs.85).aspx#BC3 </remarks>
        static GXColor32[] DecodeDXT5TexelBlock(BinaryReader r)
        {
            // Create the alpha table.
            var alphaTable = new float[8];
            alphaTable[0] = r.ReadByte();
            alphaTable[1] = r.ReadByte();
            if (alphaTable[0] > alphaTable[1])
            {
                for (var i = 0; i < 6; i++) alphaTable[2 + i] = MathX.Lerp(alphaTable[0], alphaTable[1], (float)(1 + i) / 7);
            }
            else
            {
                for (var i = 0; i < 4; i++) alphaTable[2 + i] = MathX.Lerp(alphaTable[0], alphaTable[1], (float)(1 + i) / 5);
                alphaTable[6] = 0;
                alphaTable[7] = 255;
            }

            // Read pixel alpha indices.
            var alphaIndices = new uint[16];
            var alphaIndexBytesRow0 = new byte[3];
            r.Read(alphaIndexBytesRow0, 0, alphaIndexBytesRow0.Length); Array.Reverse(alphaIndexBytesRow0); // Take care of little-endianness.
            var alphaIndexBytesRow1 = new byte[3];
            r.Read(alphaIndexBytesRow1, 0, alphaIndexBytesRow1.Length); Array.Reverse(alphaIndexBytesRow1); // Take care of little-endianness.
            const uint bitsPerAlphaIndex = 3U;
            alphaIndices[0] = (uint)MathX.GetBits(21, bitsPerAlphaIndex, alphaIndexBytesRow0);
            alphaIndices[1] = (uint)MathX.GetBits(18, bitsPerAlphaIndex, alphaIndexBytesRow0);
            alphaIndices[2] = (uint)MathX.GetBits(15, bitsPerAlphaIndex, alphaIndexBytesRow0);
            alphaIndices[3] = (uint)MathX.GetBits(12, bitsPerAlphaIndex, alphaIndexBytesRow0);
            alphaIndices[4] = (uint)MathX.GetBits(9, bitsPerAlphaIndex, alphaIndexBytesRow0);
            alphaIndices[5] = (uint)MathX.GetBits(6, bitsPerAlphaIndex, alphaIndexBytesRow0);
            alphaIndices[6] = (uint)MathX.GetBits(3, bitsPerAlphaIndex, alphaIndexBytesRow0);
            alphaIndices[7] = (uint)MathX.GetBits(0, bitsPerAlphaIndex, alphaIndexBytesRow0);
            alphaIndices[8] = (uint)MathX.GetBits(21, bitsPerAlphaIndex, alphaIndexBytesRow1);
            alphaIndices[9] = (uint)MathX.GetBits(18, bitsPerAlphaIndex, alphaIndexBytesRow1);
            alphaIndices[10] = (uint)MathX.GetBits(15, bitsPerAlphaIndex, alphaIndexBytesRow1);
            alphaIndices[11] = (uint)MathX.GetBits(12, bitsPerAlphaIndex, alphaIndexBytesRow1);
            alphaIndices[12] = (uint)MathX.GetBits(9, bitsPerAlphaIndex, alphaIndexBytesRow1);
            alphaIndices[13] = (uint)MathX.GetBits(6, bitsPerAlphaIndex, alphaIndexBytesRow1);
            alphaIndices[14] = (uint)MathX.GetBits(3, bitsPerAlphaIndex, alphaIndexBytesRow1);
            alphaIndices[15] = (uint)MathX.GetBits(0, bitsPerAlphaIndex, alphaIndexBytesRow1);
            // Create the color table.
            var colorTable = new GXColor[4];
            colorTable[0] = r.ReadUInt16().B565ToColor();
            colorTable[1] = r.ReadUInt16().B565ToColor();
            colorTable[2] = GXColor.Lerp(colorTable[0], colorTable[1], 1.0f / 3);
            colorTable[3] = GXColor.Lerp(colorTable[0], colorTable[1], 2.0f / 3);
            // Calculate pixel colors.
            var colors = DecodeDXT1TexelBlock(r, colorTable);
            for (var i = 0; i < 16; i++) colors[i].A = (byte)Math.Round(alphaTable[alphaIndices[i]]);
            return colors;
        }

        /// <summary>
        /// Copies a decoded texel block to a texture's data buffer. Takes into account DDS mipmap padding.
        /// </summary>
        /// <param name="decodedTexels">The decoded DDS texels.</param>
        /// <param name="argb">The texture's data buffer.</param>
        /// <param name="baseARGBIndex">The desired offset into the texture's data buffer. Used for mipmaps.</param>
        /// <param name="baseRowIndex">The base row index in the texture where decoded texels are copied.</param>
        /// <param name="baseColumnIndex">The base column index in the texture where decoded texels are copied.</param>
        /// <param name="textureWidth">The width of the texture.</param>
        /// <param name="textureHeight">The height of the texture.</param>
        static void CopyDecodedTexelBlock(GXColor32[] decodedTexels, byte[] argb, int baseARGBIndex, int baseRowIndex, int baseColumnIndex, int textureWidth, int textureHeight)
        {
            for (var i = 0; i < 4; i++) // row
                for (var j = 0; j < 4; j++) // column
                {
                    var rowIndex = baseRowIndex + i;
                    var columnIndex = baseColumnIndex + j;
                    // Don't copy padding on mipmaps.
                    if (rowIndex < textureHeight && columnIndex < textureWidth)
                    {
                        var decodedTexelIndex = (4 * i) + j;
                        var color = decodedTexels[decodedTexelIndex];
                        var ARGBPixelOffset = (textureWidth * rowIndex) + columnIndex;
                        var basePixelARGBIndex = baseARGBIndex + (4 * ARGBPixelOffset);
                        argb[basePixelARGBIndex] = color.A;
                        argb[basePixelARGBIndex + 1] = color.R;
                        argb[basePixelARGBIndex + 2] = color.G;
                        argb[basePixelARGBIndex + 3] = color.B;
                    }
                }
        }

        static byte[] DecodeDXT1ToARGB(byte[] compressedData, uint width, uint height, DDS_PIXELFORMAT pixelFormat, uint mipmapCount) => DecodeDXTToARGB(1, compressedData, width, height, pixelFormat, mipmapCount);
        static byte[] DecodeDXT3ToARGB(byte[] compressedData, uint width, uint height, DDS_PIXELFORMAT pixelFormat, uint mipmapCount) => DecodeDXTToARGB(3, compressedData, width, height, pixelFormat, mipmapCount);
        static byte[] DecodeDXT5ToARGB(byte[] compressedData, uint width, uint height, DDS_PIXELFORMAT pixelFormat, uint mipmapCount) => DecodeDXTToARGB(5, compressedData, width, height, pixelFormat, mipmapCount);

        /// <summary>
        /// Decodes DXT data to ARGB.
        /// </summary>
        static byte[] DecodeDXTToARGB(int DXTVersion, byte[] compressedData, uint width, uint height, DDS_PIXELFORMAT pixelFormat, uint mipmapCount)
        {
            var alphaFlag = pixelFormat.dwFlags.HasFlag(DDPF.ALPHAPIXELS);
            var containsAlpha = alphaFlag || (pixelFormat.dwRGBBitCount == 32 && pixelFormat.dwABitMask != 0);
            using var r = new BinaryReader(new MemoryStream(compressedData));
            var argb = new byte[TextureHelper.GetMipmapDataSize((int)width, (int)height, 4)];
            var mipMapWidth = (int)width;
            var mipMapHeight = (int)height;
            var baseARGBIndex = 0;
            for (var mipMapIndex = 0; mipMapIndex < mipmapCount; mipMapIndex++)
            {
                for (var rowIndex = 0; rowIndex < mipMapHeight; rowIndex += 4)
                    for (var columnIndex = 0; columnIndex < mipMapWidth; columnIndex += 4)
                    {
                        if (r.Position() == r.BaseStream.Length) return argb;
                        GXColor32[] colors = null;
                        colors = DXTVersion switch // Doing a switch instead of using a delegate for speed.
                        {
                            1 => DecodeDXT1TexelBlock(r, containsAlpha),
                            3 => DecodeDXT3TexelBlock(r),
                            5 => DecodeDXT5TexelBlock(r),
                            _ => throw new NotImplementedException($"Tried decoding a DDS file using an unsupported DXT format: DXT {DXTVersion}"),
                        };
                        CopyDecodedTexelBlock(colors, argb, baseARGBIndex, rowIndex, columnIndex, mipMapWidth, mipMapHeight);
                    }
                baseARGBIndex += mipMapWidth * mipMapHeight * 4;
                mipMapWidth /= 2;
                mipMapHeight /= 2;
            }
            return argb;
        }

        #endregion
#endif
    }
}