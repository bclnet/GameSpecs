using System;
using System.Runtime.InteropServices;

// https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dds-pixelformat
namespace OpenStack.Graphics.DirectX
{
    /// <summary>
    /// FourCC
    /// </summary>
    public enum FourCC : uint
    {
        DXT1 = 0x31545844, // MAKEFOURCC("DXT1"); // DXT1
        DXT2 = 0x32545844, // MAKEFOURCC("DXT2");
        DXT3 = 0x33545844, // MAKEFOURCC("DXT3"); // DXT3
        DXT4 = 0x34545844, // MAKEFOURCC("DXT4");
        DXT5 = 0x35545844, // MAKEFOURCC("DXT5"); // DXT5
        RXGB = 0x42475852, // MAKEFOURCC("RXGB");
        ATI1 = 0x31495441, // MAKEFOURCC("ATI1");
        ATI2 = 0x32495441, // MAKEFOURCC("ATI2"); // ATI2
        A2XY = 0x59583241, // MAKEFOURCC("A2XY");
        DX10 = 0x30315844, // MAKEFOURCC("DX10"); // DX10
    }

    /// <summary>
    /// Values which indicate what type of data is in the surface.
    /// </summary>
    [Flags]
    public enum DDPF : uint
    {
        /// <summary>
        /// Texture contains alpha data; dwRGBAlphaBitMask contains valid data.
        /// </summary>
        ALPHAPIXELS = 0x00000001,
        /// <summary>
        /// Used in some older DDS files for alpha channel only uncompressed data (dwRGBBitCount contains the alpha channel bitcount; dwABitMask contains valid data)
        /// </summary>
        ALPHA = 0x00000002,
        /// <summary>
        /// Texture contains compressed RGB data; dwFourCC contains valid data.
        /// </summary>
        FOURCC = 0x00000004,
        /// <summary>
        /// Texture contains uncompressed RGB data; dwRGBBitCount and the RGB masks (dwRBitMask, dwGBitMask, dwBBitMask) contain valid data.
        /// </summary>
        RGB = 0x00000040,
        /// <summary>
        /// Used in some older DDS files for YUV uncompressed data (dwRGBBitCount contains the YUV bit count; dwRBitMask contains the Y mask, dwGBitMask contains the U mask, dwBBitMask contains the V mask)
        /// </summary>
        YUV = 0x00000200,
        /// <summary>
        /// Used in some older DDS files for single channel color uncompressed data (dwRGBBitCount contains the luminance channel bit count; dwRBitMask contains the channel mask). Can be combined with DDPF_ALPHAPIXELS for a two channel DDS file.
        /// </summary>
        LUMINANCE = 0x00020000,
        /// <summary>
        /// The normal
        /// </summary>
        NORMAL = 0x80000000, // Custom nv flag
    }

    /// <summary>
    /// Surface pixel format.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
    public struct DDS_PIXELFORMAT
    {
        public const int SizeOf = 32;

        /// <summary>
        /// Structure size; set to 32 (bytes).
        /// </summary>
        public uint dwSize;
        /// <summary>
        /// Values which indicate what type of data is in the surface.
        /// </summary>
        [MarshalAs(UnmanagedType.U4)] public DDPF dwFlags;
        /// <summary>
        /// Four-character codes for specifying compressed or custom formats. Possible values include: DXT1, DXT2, DXT3, DXT4, or DXT5. A FourCC of DX10 indicates the prescense of the DDS_HEADER_DXT10 extended header, and the dxgiFormat member of that structure indicates the true format. When using a four-character code, dwFlags must include DDPF_FOURCC.
        /// </summary>
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] dwFourCC;
        [MarshalAs(UnmanagedType.U4)] public FourCC dwFourCC;
        /// <summary>
        /// Number of bits in an RGB (possibly including alpha) format. Valid when dwFlags includes DDPF_RGB, DDPF_LUMINANCE, or DDPF_YUV.
        /// </summary>
        public uint dwRGBBitCount;
        /// <summary>
        /// Red (or lumiannce or Y) mask for reading color data. For instance, given the A8R8G8B8 format, the red mask would be 0x00ff0000.
        /// </summary>
        public uint dwRBitMask;
        /// <summary>
        /// Green (or U) mask for reading color data. For instance, given the A8R8G8B8 format, the green mask would be 0x0000ff00.
        /// </summary>
        public uint dwGBitMask;
        /// <summary>
        /// Blue (or V) mask for reading color data. For instance, given the A8R8G8B8 format, the blue mask would be 0x000000ff.
        /// </summary>
        public uint dwBBitMask;
        /// <summary>
        /// Alpha mask for reading alpha data. dwFlags must include DDPF_ALPHAPIXELS or DDPF_ALPHA. For instance, given the A8R8G8B8 format, the alpha mask would be 0xff000000.
        /// </summary>
        public uint dwABitMask;
    }
}