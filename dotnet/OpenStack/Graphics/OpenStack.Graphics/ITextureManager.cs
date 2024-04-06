using System;
using System.Collections.Generic;
using System.IO;

namespace OpenStack.Graphics
{
    public interface ITextureManager<Texture>
    {
        Texture BuildSolidTexture(int width, int height, params float[] rgba);
        Texture BuildNormalMap(Texture source, float strength);
        Texture DefaultTexture { get; }
        Texture LoadTexture(object key, out IDictionary<string, object> data, Range? rng = null);
        void PreloadTexture(string path);
        void DeleteTexture(object key);
    }

    public enum TextureFlags : int
    {
#pragma warning disable 1591
        SUGGEST_CLAMPS = 0x00000001,
        SUGGEST_CLAMPT = 0x00000002,
        SUGGEST_CLAMPU = 0x00000004,
        NO_LOD = 0x00000008,
        CUBE_TEXTURE = 0x00000010,
        VOLUME_TEXTURE = 0x00000020,
        TEXTURE_ARRAY = 0x00000040,
#pragma warning restore 1591
    }

    /// <summary>
	/// TextureUnityFormat
	/// </summary>
    public enum TextureUnityFormat : short
    {
        Unknown = 0,
        ATC_RGB4 = -127,
        ATC_RGBA8 = -127,
        PVRTC_2BPP_RGB = -127,
        PVRTC_2BPP_RGBA = -127,
        PVRTC_4BPP_RGB = -127,
        PVRTC_4BPP_RGBA = -127,
        Alpha8 = 1,
        ARGB4444 = 2,
        RGB24 = 3,
        RGBA32 = 4,
        ARGB32 = 5,
        RGB565 = 7,
        R16 = 9,
        DXT1 = 10,
        DXT3_POLYFILL = 11,
        DXT5 = 12,
        RGBA4444 = 13,
        BGRA32 = 14,
        RHalf = 15,
        RGHalf = 16,
        RGBAHalf = 17,
        RFloat = 18,
        RGFloat = 19,
        RGBAFloat = 20,
        YUY2 = 21,
        RGB9e5Float = 22,
        BC6H = 24,
        BC7 = 25,
        BC4 = 26,
        BC5 = 27,
        DXT1Crunched = 28,
        DXT5Crunched = 29,
        PVRTC_RGB2 = 30,
        PVRTC_RGBA2 = 31,
        PVRTC_RGB4 = 32,
        PVRTC_RGBA4 = 33,
        ETC_RGB4 = 34,
        EAC_R = 41,
        EAC_R_SIGNED = 42,
        EAC_RG = 43,
        EAC_RG_SIGNED = 44,
        ETC2_RGB = 45,
        ETC2_RGBA1 = 46,
        ETC2_RGBA8 = 47,
        ASTC_4x4 = 48,
        ASTC_RGB_4x4 = 48,
        ASTC_5x5 = 49,
        ASTC_RGB_5x5 = 49,
        ASTC_6x6 = 50,
        ASTC_RGB_6x6 = 50,
        ASTC_8x8 = 51,
        ASTC_RGB_8x8 = 51,
        ASTC_10x10 = 52,
        ASTC_RGB_10x10 = 52,
        ASTC_12x12 = 53,
        ASTC_RGB_12x12 = 53,
        ASTC_RGBA_4x4 = 54,
        ASTC_RGBA_5x5 = 55,
        ASTC_RGBA_6x6 = 56,
        ASTC_RGBA_8x8 = 57,
        ASTC_RGBA_10x10 = 58,
        ASTC_RGBA_12x12 = 59,
        ETC_RGB4_3DS = 60,
        ETC_RGBA8_3DS = 61,
        RG16 = 62,
        R8 = 63,
        ETC_RGB4Crunched = 64,
        ETC2_RGBA8Crunched = 65,
        ASTC_HDR_4x4 = 66,
        ASTC_HDR_5x5 = 67,
        ASTC_HDR_6x6 = 68,
        ASTC_HDR_8x8 = 69,
        ASTC_HDR_10x10 = 70,
        ASTC_HDR_12x12 = 71
    }

    /// <summary>
	/// TextureUnrealFormat
	/// </summary>
	public enum TextureUnrealFormat : int
    {
        /// <summary/>
        Unknown = 0,
        /// <summary/>
        A32B32G32R32F = 1,
        /// <summary/>
        B8G8R8A8 = 2,
        /// <summary>G8 means Gray/Grey, not Green, typically actually uses a red format with replication of R to RGB<summary/>
        G8 = 3,
        /// <summary>G16 means Gray/Grey like G8</summary>
        G16 = 4,
        /// <summary/>
        DXT1 = 5,
        /// <summary/>
        DXT3 = 6,
        /// <summary/>
        DXT5 = 7,
        /// <summary/>
        UYVY = 8,
        /// <summary>16F</summary>
        FloatRGB = 9,
        /// <summary>16F</summary>
        FloatRGBA = 10,
        /// <summary/>
        DepthStencil = 11,
        /// <summary/>
        ShadowDepth = 12,
        /// <summary/>
        R32Float = 13,
        /// <summary/>
        G16R16 = 14,
        /// <summary/>
        G16R16F = 15,
        /// <summary/>
        G16R16FFilter = 16,
        /// <summary/>
        G32R32F = 17,
        /// <summary/>
        A2B10G10R10 = 18,
        /// <summary/>
        A16B16G16R16 = 19,
        /// <summary/>
        D24 = 20,
        /// <summary/>
        R16F = 21,
        /// <summary/>
        R16FFilter = 22,
        /// <summary/>
        BC5 = 23,
        /// <summary/>
        V8U8 = 24,
        /// <summary/>
        A1 = 25,
        /// <summary/>
        FloatR11G11B10 = 26,
        /// <summary/>
        A8 = 27,
        /// <summary/>
        R32UInt = 28,
        /// <summary/>
        R32SInt = 29,
        /// <summary/>
        PVRTC2 = 30,
        /// <summary/>
        PVRTC4 = 31,
        /// <summary/>
        R16UInt = 32,
        /// <summary/>
        R16SInt = 33,
        /// <summary/>
        R16G16B16A16UInt = 34,
        /// <summary/>
        R16G16B16A16SInt = 35,
        /// <summary/>
        R5G6B5UNorm = 36,
        /// <summary/>
        R8G8B8A8 = 37,
        /// <summary>Only used for legacy loading; do NOT us</summary>
        A8R8G8B8 = 38,
        /// <summary/>
        BC4 = 39,
        /// <summary/>
        R8G8 = 40,
        /// <summary>Unsupported Format</summary>
        ATCRGB = 41,
        /// <summary>Unsupported Format</summary>
        ATCRGBAE = 42,
        /// <summary/>
        ATCRGBAI = 43,
        /// <summary>Used for creating SRVs to alias a DepthStencil buffer to read Stencil. Don't use for creating textures</summary>
        X24G8 = 44,
        /// <summary>Unsupported Format</summary>
        ETC1 = 45,
        /// <summary/>
        ETC2RGB = 46,
        /// <summary/>
        ETC2RGBA = 47,
        /// <summary/>
        R32G32B32A32UInt = 48,
        /// <summary/>
        R16G16UInt = 49,
        /// <summary>8.00 bpp</summary>
        ASTC4x4 = 50,
        /// <summary>3.56 bpp</summary>
        ASTC6x6 = 51,
        /// <summary>2.00 bpp</summary>
        ASTC8x8 = 52,
        /// <summary>1.28 bpp</summary>
        ASTC10x10 = 53,
        /// <summary>0.89 bpp</summary>
        ASTC12x12 = 54,
        /// <summary/>
        BC6H = 55,
        /// <summary/>
        BC7 = 56,
        /// <summary/>
        R8UInt = 57,
        /// <summary/>
        L8 = 58,
        /// <summary/>
        XGXR8 = 59,
        /// <summary/>
        R8G8B8A8UInt = 60,
        /// <summary/>
        R8G8B8A8SNorm = 61,
        /// <summary/>
        R16G16B16A16UNorm = 62,
        /// <summary/>
        R16G16B16A16SNorm = 63,
        /// <summary/>
        PLATFORMHDR0 = 64,
        /// <summary>Reserved</summary>
        PLATFORMHDR1 = 65,
        /// <summary>Reserved</summary>
        PLATFORMHDR2 = 66,
        /// <summary/>
        NV12 = 67,
        /// <summary/>
        R32G32UInt = 68,
        /// <summary/>
        ETC2R11EAC = 69,
        /// <summary/>
        ETC2RG11EAC = 70,
        /// <summary/>
        R8 = 71,
        /// <summary/>
        B5G5R5A1UNorm = 72,
        /// <summary/>
        ASTC4x4HDR = 73,
        /// <summary/>
        ASTC6x6HDR = 74,
        /// <summary/>
        ASTC8x8HDR = 75,
        /// <summary/>
        ASTC10x10HDR = 76,
        /// <summary/>
        ASTC12x12HDR = 77,
        /// <summary/>
        G16R16SNorm = 78,
        /// <summary/>
        R8G8UInt = 79,
        /// <summary/>
        R32G32B32UInt = 80,
        /// <summary/>
        R32G32B32SInt = 81,
        /// <summary/>
        R32G32B32F = 82,
        /// <summary/>
        R8SInt = 83,
        /// <summary/>
        R64UInt = 84,
        /// <summary/>
        R9G9B9EXP5 = 85,
        /// <summary/>
        P010 = 86,
        /// <summary/>
        MAX = 87,
    }

    /// <summary>
	/// TextureGLPixelFormat
	/// </summary>
    public enum TextureGLPixelFormat
    {
        Unknown = 0,
        /// <summary>
        /// GL_UNSIGNED_SHORT = 0x1403
        /// </summary>
        UnsignedShort = 0x1403,
        /// <summary>
        /// GL_UNSIGNED_INT = 0x1405
        /// </summary>
        UnsignedInt = 0x1405,
        /// <summary>
        /// GL_COLOR_INDEX = 0x1900
        /// </summary>
        ColorIndex = 0x1900,
        /// <summary>
        /// GL_STENCIL_INDEX = 0x1901
        /// </summary>
        StencilIndex = 0x1901,
        /// <summary>
        /// GL_DEPTH_COMPONENT = 0x1902
        /// </summary>
        DepthComponent = 0x1902,
        /// <summary>
        /// GL_RED = 0x1903
        /// </summary>
        Red = 0x1903,
        /// <summary>
        /// GL_RED_EXT = 0x1903
        /// </summary>
        RedExt = 0x1903,
        /// <summary>
        /// GL_GREEN = 0x1904
        /// </summary>
        Green = 0x1904,
        /// <summary>
        /// GL_BLUE = 0x1905
        /// </summary>
        Blue = 0x1905,
        /// <summary>
        /// GL_ALPHA = 0x1906
        /// </summary>
        Alpha = 0x1906,
        /// <summary>
        /// GL_RGB = 0x1907
        /// </summary>
        Rgb = 0x1907,
        /// <summary>
        /// GL_RGBA = 0x1908
        /// </summary>
        Rgba = 0x1908,
        /// <summary>
        /// GL_LUMINANCE = 0x1909
        /// </summary>
        Luminance = 0x1909,
        /// <summary>
        /// GL_LUMINANCE_ALPHA = 0x190A
        /// </summary>
        LuminanceAlpha = 0x190A,
        /// <summary>
        /// GL_ABGR_EXT = 0x8000
        /// </summary>
        AbgrExt = 0x8000,
        /// <summary>
        /// GL_CMYK_EXT = 0x800C
        /// </summary>
        CmykExt = 0x800C,
        /// <summary>
        /// GL_CMYKA_EXT = 0x800D
        /// </summary>
        CmykaExt = 0x800D,
        /// <summary>
        /// GL_BGR = 0x80E0
        /// </summary>
        Bgr = 0x80E0,
        /// <summary>
        /// GL_BGRA = 0x80E1
        /// </summary>
        Bgra = 0x80E1,
        /// <summary>
        /// GL_YCRCB_422_SGIX = 0x81BB
        /// </summary>
        Ycrcb422Sgix = 0x81BB,
        /// <summary>
        /// GL_YCRCB_444_SGIX = 0x81BC
        /// </summary>
        Ycrcb444Sgix = 0x81BC,
        /// <summary>
        /// GL_RG = 0x8227
        /// </summary>
        Rg = 0x8227,
        /// <summary>
        /// GL_RG_INTEGER = 0x8228
        /// </summary>
        RgInteger = 0x8228,
        /// <summary>
        /// GL_R5_G6_B5_ICC_SGIX = 0x8466
        /// </summary>
        R5G6B5IccSgix = 0x8466,
        /// <summary>
        /// GL_R5_G6_B5_A8_ICC_SGIX = 0x8467
        /// </summary>
        R5G6B5A8IccSgix = 0x8467,
        /// <summary>
        /// GL_ALPHA16_ICC_SGIX = 0x8468
        /// </summary>
        Alpha16IccSgix = 0x8468,
        /// <summary>
        /// GL_LUMINANCE16_ICC_SGIX = 0x8469
        /// </summary>
        Luminance16IccSgix = 0x8469,
        /// <summary>
        /// GL_LUMINANCE16_ALPHA8_ICC_SGIX = 0x846B
        /// </summary>
        Luminance16Alpha8IccSgix = 0x846B,
        /// <summary>
        /// GL_DEPTH_STENCIL = 0x84F9
        /// </summary>
        DepthStencil = 0x84F9,
        /// <summary>
        /// GL_RED_INTEGER = 0x8D94
        /// </summary>
        RedInteger = 0x8D94,
        /// <summary>
        /// GL_GREEN_INTEGER = 0x8D95 
        /// </summary>
        GreenInteger = 0x8D95,
        /// <summary>
        /// GL_BLUE_INTEGER = 0x8D96
        /// </summary>
        BlueInteger = 0x8D96,
        /// <summary>
        /// GL_ALPHA_INTEGER = 0x8D97
        /// </summary>
        AlphaInteger = 0x8D97,
        /// <summary>
        /// GL_RGB_INTEGER = 0x8D98
        /// </summary>
        RgbInteger = 0x8D98,
        /// <summary>
        /// GL_RGBA_INTEGER = 0x8D99
        /// </summary>
        RgbaInteger = 0x8D99,
        /// <summary>
        /// GL_BGR_INTEGER = 0x8D9A
        /// </summary>
        BgrInteger = 0x8D9A,
        /// <summary>
        /// GL_BGRA_INTEGER = 0x8D9B 
        /// </summary>
        BgraInteger = 0x8D9B
    }

    /// <summary>
	/// TextureGLPixelType
	/// </summary>
    public enum TextureGLPixelType
    {
        /// <summary>
        /// GL_BYTE = 0x1400
        /// </summary>
        Byte = 0x1400,
        /// <summary>
        /// GL_UNSIGNED_BYTE = 0x1401
        /// </summary>
        UnsignedByte = 0x1401,
        /// <summary>
        /// GL_SHORT = 0x1402
        /// </summary>
        Short = 0x1402,
        /// <summary>
        /// GL_UNSIGNED_SHORT = 0x1403
        /// </summary>
        UnsignedShort = 0x1403,
        /// <summary>
        /// GL_INT = 0x1404
        /// </summary>
        Int = 0x1404,
        /// <summary>
        /// GL_UNSIGNED_INT = 0x1405
        /// </summary>
        UnsignedInt = 0x1405,
        /// <summary>
        /// GL_FLOAT = 0x1406
        /// </summary>
        Float = 0x1406,
        /// <summary>
        /// GL_HALF_FLOAT = 0x140B
        /// </summary>
        HalfFloat = 0x140B,
        /// <summary>
        /// GL_BITMAP = 0x1A00
        /// </summary>
        Bitmap = 0x1A00,
        /// <summary>
        /// GL_UNSIGNED_BYTE_3_3_2 = 0x8032
        /// </summary>
        UnsignedByte332 = 0x8032,
        /// <summary>
        /// GL_UNSIGNED_BYTE_3_3_2_EXT = 0x8032
        /// </summary>
        UnsignedByte332Ext = 0x8032,
        /// <summary>
        /// GL_UNSIGNED_SHORT_4_4_4_4 = 0x8033
        /// </summary>
        UnsignedShort4444 = 0x8033,
        /// <summary>
        /// GL_UNSIGNED_SHORT_4_4_4_4_EXT = 0x8033
        /// </summary>
        UnsignedShort4444Ext = 0x8033,
        /// <summary>
        /// GL_UNSIGNED_SHORT_5_5_5_1 = 0x8034
        /// </summary>
        UnsignedShort5551 = 0x8034,
        /// <summary>
        /// GL_UNSIGNED_SHORT_5_5_5_1_EXT = 0x8034
        /// </summary>
        UnsignedShort5551Ext = 0x8034,
        /// <summary>
        /// GL_UNSIGNED_INT_8_8_8_8 = 0x8035
        /// </summary>
        UnsignedInt8888 = 0x8035,
        /// <summary>
        /// GL_UNSIGNED_INT_8_8_8_8_EXT = 0x8035
        /// </summary>
        UnsignedInt8888Ext = 0x8035,
        /// <summary>
        /// GL_UNSIGNED_INT_10_10_10_2 = 0x8036
        /// </summary>
        UnsignedInt1010102 = 0x8036,
        /// <summary>
        /// GL_UNSIGNED_INT_10_10_10_2_EXT = 0x8036
        /// </summary>
        UnsignedInt1010102Ext = 0x8036,
        /// <summary>
        /// GL_UNSIGNED_BYTE_2_3_3_REVERSED = 0x8362
        /// </summary>
        UnsignedByte233Reversed = 0x8362,
        /// <summary>
        /// GL_UNSIGNED_SHORT_5_6_5 = 0x8363
        /// </summary>
        UnsignedShort565 = 0x8363,
        /// <summary>
        /// GL_UNSIGNED_SHORT_5_6_5_REVERSED = 0x8364
        /// </summary>
        UnsignedShort565Reversed = 0x8364,
        /// <summary>
        /// GL_UNSIGNED_SHORT_4_4_4_4_REVERSED = 0x8365
        /// </summary>
        UnsignedShort4444Reversed = 0x8365,
        /// <summary>
        /// GL_UNSIGNED_SHORT_1_5_5_5_REVERSED = 0x8366
        /// </summary>
        UnsignedShort1555Reversed = 0x8366,
        /// <summary>
        /// GL_UNSIGNED_INT_8_8_8_8_REVERSED = 0x8367
        /// </summary>
        UnsignedInt8888Reversed = 0x8367,
        /// <summary>
        /// GL_UNSIGNED_INT_2_10_10_10_REVERSED = 0x8368
        /// </summary>
        UnsignedInt2101010Reversed = 0x8368,
        /// <summary>
        /// GL_UNSIGNED_INT_24_8 = 0x84FA
        /// </summary>
        UnsignedInt248 = 0x84FA,
        /// <summary>
        /// GL_UNSIGNED_INT_10F_11F_11F_REV = 0x8C3B
        /// </summary>
        UnsignedInt10F11F11FRev = 0x8C3B,
        /// <summary>
        /// GL_UNSIGNED_INT_5_9_9_9_REV = 0x8C3E
        /// </summary>
        UnsignedInt5999Rev = 0x8C3E,
        /// <summary>
        /// GL_FLOAT_32_UNSIGNED_INT_24_8_REV = 0x8DAD
        /// </summary>
        Float32UnsignedInt248Rev = 0x8DAD
    }

    /// <summary>
	/// TextureGLFormat
	/// </summary>
    public enum TextureGLFormat
    {
        /// <summary>
        /// GL_DEPTH_COMPONENT = 0x1902
        /// </summary>
        DepthComponent = 0x1902,
        /// <summary>
        /// GL_RED = 0x1903 (compress only)
        /// </summary>
        Red = 0x1903,
        /// <summary>
        /// GL_RED_EXT = 0x1903 (dup, compress only)
        /// </summary>
        RedExt = 0x1903,
        /// <summary>
        /// GL_ALPHA = 0x1906 (internal only)
        /// </summary>
        Alpha = 0x1906,
        /// <summary>
        /// GL_RGB = 0x1907
        /// </summary>
        Rgb = 0x1907,
        /// <summary>
        /// GL_RGBA = 0x1908
        /// </summary>
        Rgba = 0x1908,
        /// <summary>
        /// GL_LUMINANCE = 0x1909 (internal only)
        /// </summary>
        Luminance = 0x1909,
        /// <summary>
        /// GL_LUMINANCE_ALPHA = 0x190A (internal only)
        /// </summary>
        LuminanceAlpha = 0x190A,
        /// <summary>
        /// GL_R3_G3_B2 = 0x2A10
        /// </summary>
        R3G3B2 = 0x2A10,
        /// <summary>
        /// GL_ALPHA4 = 0x803B
        /// </summary>
        Alpha4 = 0x803B,
        /// <summary>
        /// GL_ALPHA8 = 0x803C
        /// </summary>
        Alpha8 = 0x803C,
        /// <summary>
        /// GL_ALPHA12 = 0x803D
        /// </summary>
        Alpha12 = 0x803D,
        /// <summary>
        /// GL_ALPHA16 = 0x803E
        /// </summary>
        Alpha16 = 0x803E,
        /// <summary>
        /// GL_LUMINANCE4 = 0x803F
        /// </summary>
        Luminance4 = 0x803F,
        /// <summary>
        /// GL_LUMINANCE8 = 0x8040
        /// </summary>
        Luminance8 = 0x8040,
        /// <summary>
        /// GL_LUMINANCE12 = 0x8041
        /// </summary>
        Luminance12 = 0x8041,
        /// <summary>
        /// GL_LUMINANCE16 = 0x8042
        /// </summary>
        Luminance16 = 0x8042,
        /// <summary>
        /// GL_LUMINANCE4_ALPHA4 = 0x8043
        /// </summary>
        Luminance4Alpha4 = 0x8043,
        /// <summary>
        /// GL_LUMINANCE6_ALPHA2 = 0x8044
        /// </summary>
        Luminance6Alpha2 = 0x8044,
        /// <summary>
        /// GL_LUMINANCE8_ALPHA8 = 0x8045
        /// </summary>
        Luminance8Alpha8 = 0x8045,
        /// <summary>
        /// GL_LUMINANCE12_ALPHA4 = 0x8046
        /// </summary>
        Luminance12Alpha4 = 0x8046,
        /// <summary>
        /// GL_LUMINANCE12_ALPHA12 = 0x8047
        /// </summary>
        Luminance12Alpha12 = 0x8047,
        /// <summary>
        /// GL_LUMINANCE16_ALPHA16 = 0x8048
        /// </summary>
        Luminance16Alpha16 = 0x8048,
        /// <summary>
        /// GL_INTENSITY = 0x8049
        /// </summary>
        Intensity = 0x8049,
        /// <summary>
        /// GL_INTENSITY4 = 0x804A
        /// </summary>
        Intensity4 = 0x804A,
        /// <summary>
        /// GL_INTENSITY8 = 0x804B
        /// </summary>
        Intensity8 = 0x804B,
        /// <summary>
        /// GL_INTENSITY12 = 0x804C
        /// </summary>
        Intensity12 = 0x804C,
        /// <summary>
        /// GL_INTENSITY16 = 0x804D
        /// </summary>
        Intensity16 = 0x804D,
        /// <summary>
        /// GL_RGB2_EXT = 0x804E
        /// </summary>
        Rgb2Ext = 0x804E,
        /// <summary>
        /// GL_RGB4 = 0x804F
        /// </summary>
        Rgb4 = 0x804F,
        /// <summary>
        /// GL_RGB4_EXT = 0x804F (compress only)
        /// </summary>
        Rgb4Ext = 0x804F,
        Rgb5 = 0x8050,              //: GL_RGB5 = 0x8050
        Rgb5Ext = 0x8050,           //: GL_RGB5_EXT = 0x8050 (dup, compress only)
        Rgb8 = 0x8051,              //: GL_RGB8 = 0x8051
        Rgb8Ext = 0x8051,           //: GL_RGB8_EXT = 0x8051 (dup, compress only)
        Rgb8Oes = 0x8051,           //: GL_RGB8_OES = 0x8051 (dup, compress only)
        Rgb10 = 0x8052,             //: GL_RGB10 = 0x8052
        Rgb10Ext = 0x8052,          //: GL_RGB10_EXT = 0x8052 (dup, compress only)
        Rgb12 = 0x8053,             //: GL_RGB12 = 0x8053
        Rgb12Ext = 0x8053,          //: GL_RGB12_EXT = 0x8053 (dup, compress only)
        Rgb16 = 0x8054,             //: GL_RGB16 = 0x8054
        Rgb16Ext = 0x8054,          //: GL_RGB16_EXT = 0x8054 (dup, compress only)
        Rgba2 = 0x8056,             //: GL_RGBA2 = 0x8055 (internal only)
        Rgba4 = 0x8056,             //: GL_RGBA4 = 0x8056
        Rgba4Ext = 0x8056,          //: GL_RGBA4_EXT = 0x8056 (dup, compress only)
        Rgba4Oes = 0x8056,          //: GL_RGBA4_OES = 0x8056 (dup, compress only)
        Rgb5A1 = 0x8057,            //: GL_RGB5_A1 = 0x8057
        Rgb5A1Ext = 0x8057,         //: GL_RGB5_A1_EXT = 0x8057 (dup, compress only)
        Rgb5A1Oes = 0x8057,         //: GL_RGB5_A1_OES = 0x8057 (dup, compress only)
        Rgba8 = 0x8058,             //: GL_RGBA8 = 0x8058
        Rgba8Ext = 0x8058,          //: GL_RGBA8_EXT = 0x8058 (dup, compress only)
        Rgba8Oes = 0x8058,          //: GL_RGBA8_OES = 0x8058 (dup, compress only)
        Rgb10A2 = 0x8059,           //: GL_RGB10_A2 = 0x8059
        Rgb10A2Ext = 0x8059,        //: GL_RGB10_A2_EXT = 0x8059 (dup, compress only)
        Rgba12 = 0x805A,            //: GL_RGBA12 = 0x805A
        Rgba12Ext = 0x805A,         //: GL_RGBA12_EXT = 0x805A (dup, compress only)
        Rgba16 = 0x805B,            //: GL_RGBA16 = 0x805B
        Rgba16Ext = 0x805B,         //: GL_RGBA16_EXT = 0x805B (dup, compress only)
        DualAlpha4Sgis = 0x8110,        //: GL_DUAL_ALPHA4_SGIS = 0x8110
        DualAlpha8Sgis = 0x8111,        //: GL_DUAL_ALPHA8_SGIS = 0x8111
        DualAlpha12Sgis = 0x8112,       //: GL_DUAL_ALPHA12_SGIS = 0x8112
        DualAlpha16Sgis = 0x8113,       //: GL_DUAL_ALPHA16_SGIS = 0x8113
        DualLuminance4Sgis = 0x8114,    //: GL_DUAL_LUMINANCE4_SGIS = 0x8114
        DualLuminance8Sgis = 0x8115,    //: GL_DUAL_LUMINANCE8_SGIS = 0x8115
        DualLuminance12Sgis = 0x8116,   //: GL_DUAL_LUMINANCE12_SGIS = 0x8116
        DualLuminance16Sgis = 0x8117,   //: GL_DUAL_LUMINANCE16_SGIS = 0x8117
        DualIntensity4Sgis = 0x8118,    //: GL_DUAL_INTENSITY4_SGIS = 0x8118
        DualIntensity8Sgis = 0x8119,    //: GL_DUAL_INTENSITY8_SGIS = 0x8119
        DualIntensity12Sgis = 0x811A,   //: GL_DUAL_INTENSITY12_SGIS = 0x811A
        DualIntensity16Sgis = 0x811B,   //: GL_DUAL_INTENSITY16_SGIS = 0x811B
        DualLuminanceAlpha4Sgis = 0x811C,//: GL_DUAL_LUMINANCE_ALPHA4_SGIS = 0x811C
        DualLuminanceAlpha8Sgis = 33053,//: GL_DUAL_LUMINANCE_ALPHA8_SGIS = 0x811D
        QuadAlpha4Sgis = 0x811E,        //: GL_QUAD_ALPHA4_SGIS = 0x811E
        QuadAlpha8Sgis = 0x811F,        //: GL_QUAD_ALPHA8_SGIS = 0x811F
        QuadLuminance4Sgis = 0x8120,    //: GL_QUAD_LUMINANCE4_SGIS = 0x8120
        QuadLuminance8Sgis = 0x8121,    //: GL_QUAD_LUMINANCE8_SGIS = 0x8121
        QuadIntensity4Sgis = 0x8122,    //: GL_QUAD_INTENSITY4_SGIS = 0x8122
        QuadIntensity8Sgis = 0x8123,    //: GL_QUAD_INTENSITY8_SGIS = 0x8123
        DepthComponent16 = 0x81A5,      //: GL_DEPTH_COMPONENT16 = 0x81A5
        DepthComponent16Arb = 0x81A5,   //: GL_DEPTH_COMPONENT16_ARB = 0x81A5 (dup, compress only)
        DepthComponent16Oes = 0x81A5,   //: GL_DEPTH_COMPONENT16_OES = 0x81A5 (dup, compress only)
        DepthComponent16Sgix = 0x81A5,  //: GL_DEPTH_COMPONENT16_SGIX = 0x81A5 (dup)
        DepthComponent24 = 0x81A6,      //: GL_DEPTH_COMPONENT24 = 0x81A6 (internal only)
        DepthComponent24Arb = 0x81A6,   //: GL_DEPTH_COMPONENT24_ARB = 0x81A6 (dup, compress only)
        DepthComponent24Oes = 0x81A6,   //: GL_DEPTH_COMPONENT24_OES = 0x81A6 (dup, compress only)
        DepthComponent24Sgix = 0x81A6,  //: GL_DEPTH_COMPONENT24_SGIX = 0x81A6
        DepthComponent32 = 0x81A7,      //: GL_DEPTH_COMPONENT32 = 0x81A7 (internal only)
        DepthComponent32Arb = 0x81A7,   //: GL_DEPTH_COMPONENT32_ARB = 0x81A7 (dup, compress only)
        DepthComponent32Oes = 0x81A7,   //: GL_DEPTH_COMPONENT32_OES = 0x81A7 (dup, compress only)
        DepthComponent32Sgix = 0x81A7,  //: GL_DEPTH_COMPONENT32_SGIX = 0x81A7 (dup)
        CompressedRed = 0x8225,         //: GL_COMPRESSED_RED = 0x8225
        CompressedRg = 0x8226,          //: GL_COMPRESSED_RG = 0x8226
        Rg = 0x8227,                //: GL_RG = 0x8227 (compress only)
        R8 = 0x8229,                //: GL_R8 = 0x8229
        R8Ext = 0x8229,             //: GL_R8_EXT = 0x8229 (dup, compress only)
        R16 = 0x822A,               //: GL_R16 = 0x822A
        R16Ext = 0x822A,            //: GL_R16_EXT = 0x822A (dup, compress only)
        Rg8 = 0x822B,               //: GL_RG8 = 0x822B
        Rg8Ext = 0x822B,            //: GL_RG8_EXT = 0x822B (dup, compress only)
        Rg16 = 0x822C,              //: GL_RG16 = 0x822C
        Rg16Ext = 0x822C,           //: GL_RG16_EXT = 0x822C (dup, compress only)
        R16f = 0x822D,              //: GL_R16F = 0x822D
        R16fExt = 0x822D,           //: GL_R16F_EXT = 0x822D (dup, compress only)
        R32f = 0x822E,              //: GL_R32F = 0x822E
        R32fExt = 0x822E,           //: GL_R32F_EXT = 0x822E (dup, compress only)
        Rg16f = 0x822F,             //: GL_RG16F = 0x822F
        Rg16fExt = 0x822F,          //: GL_RG16F_EXT = 0x822F (dup, compress only)
        Rg32f = 0x8230,             //: GL_RG32F = 0x8230
        Rg32fExt = 0x8230,          //: GL_RG32F_EXT = 0x8230 (dup, compress only)
        R8i = 0x8231,               //: GL_R8I = 0x8231
        R8ui = 0x8232,              //: GL_R8UI = 0x8232
        R16i = 0x8233,              //: GL_R16I = 0x8233
        R16ui = 0x8234,             //: GL_R16UI = 0x8234
        R32i = 0x8235,              //: GL_R32I = 0x8235
        R32ui = 0x8236,             //: GL_R32UI = 0x8236
        Rg8i = 0x8237,              //: GL_RG8I = 0x8237
        Rg8ui = 0x8238,             //: GL_RG8UI = 0x8238
        Rg16i = 0x8239,             //: GL_RG16I = 0x8239
        Rg16ui = 0x823A,            //: GL_RG16UI = 0x823A
        Rg32i = 0x823B,             //: GL_RG32I = 0x823B
        Rg32ui = 0x823C,            //: GL_RG32UI = 0x823C
        CompressedRgbS3tcDxt1Ext = 0x83F0,//: GL_COMPRESSED_RGB_S3TC_DXT1_EXT = 0x83F0
        CompressedRgbaS3tcDxt1Ext = 0x83F1,//: GL_COMPRESSED_RGBA_S3TC_DXT1_EXT = 0x83F1
        CompressedRgbaS3tcDxt3Ext = 0x83F2,//: GL_COMPRESSED_RGBA_S3TC_DXT3_EXT = 0x83F2
        CompressedRgbaS3tcDxt5Ext = 0x83F3,//: GL_COMPRESSED_RGBA_S3TC_DXT5_EXT = 0x83F3
        RgbIccSgix = 0x8460, //: GL_RGB_ICC_SGIX = 0x8460 (internal only)
        RgbaIccSgix = 0x8461, //: GL_RGBA_ICC_SGIX = 0x8461 (internal only)
        AlphaIccSgix = 0x8462, //: GL_ALPHA_ICC_SGIX = 0x8462 (internal only)
        LuminanceIccSgix = 0x8460, //: GL_LUMINANCE_ICC_SGIX = 0x8463 (internal only)
        IntensityIccSgix = 0x8464, //: GL_INTENSITY_ICC_SGIX = 0x8460 (internal only)
        LuminanceAlphaIccSgix = 0x8465, //: GL_LUMINANCE_ALPHA_ICC_SGIX = 0x8465 (internal only)
        R5G6B5IccSgix = 0x8466, //: GL_R5_G6_B5_ICC_SGIX = 0x8466 (internal only)
        R5G6B5A8IccSgix = 0x8467, //: GL_R5_G6_B5_A8_ICC_SGIX = 0x8467 (internal only)
        Alpha16IccSgix = 0x8468, //: GL_RGB_ICC_SGIX = 0x8468 (internal only)
        Luminance16IccSgix = 0x8469, //: GL_LUMINANCE16_ICC_SGIX = 0x8469 (internal only)
        Intensity16IccSgix = 0x846A, //: GL_INTENSITY16_ICC_SGIX = 0x846A (internal only)
        Luminance16Alpha8IccSgix = 0x846B, //: GL_LUMINANCE16_ALPHA8_ICC_SGIX = 0x846B (internal only)
        CompressedAlpha = 0x8469, //: GL_COMPRESSED_ALPHA = 0x84E9 (internal only)
        CompressedLuminance = 0x84EA, //: GL_COMPRESSED_LUMINANCE = 0x84EA (internal only)
        CompressedLuminanceAlpha = 0x84EB, //: GL_COMPRESSED_LUMINANCE_ALPHA = 0x84EB (internal only)
        CompressedIntensity = 0x84EC, //: GL_COMPRESSED_INTENSITY = 0x84EC (internal only)
        CompressedRgb = 0x84ED,     //: GL_COMPRESSED_RGB = 0x84ED
        CompressedRgba = 0x84EE,    //: GL_COMPRESSED_RGBA = 0x84EE
        DepthStencil = 0x84F9,      //: GL_DEPTH_STENCIL = 0x84F9
        DepthStencilExt = 0x84F9,   //: GL_DEPTH_STENCIL_EXT = 0x84F9 (dup, compress only)
        DepthStencilNv = 0x84F9,    //: GL_DEPTH_STENCIL_NV = 0x84F9 (dup, compress only)
        DepthStencilOes = 0x84F9,   //: GL_DEPTH_STENCIL_OES = 0x84F9 (dup, compress only)
        DepthStencilMesa = 0x8750,  //: GL_DEPTH_STENCIL_MESA = 0x8750 (compress only)
        Rgba32f = 0x8814,           //: GL_RGBA32F = 0x8814
        Rgba32fArb = 0x8814,        //: GL_RGBA32F_ARB = 0x8814 (dup, compress only)
        Rgba32fExt = 0x8814,        //: GL_RGBA32F_EXT = 0x8814 (dup, compress only)
        Rgba16f = 0x881A,           //: GL_RGBA16F = 0x881A
        Rgba16fArb = 0x881A,        //: GL_RGBA16F_ARB = 0x881A (dup, compress only)
        Rgba16fExt = 0x881A,        //: GL_RGBA16F_EXT = 0x881A (dup, compress only)
        Rgb16f = 0x881B,            //: GL_RGB16F = 0x881B
        Rgb16fArb = 0x881B,         //: GL_RGB16F_ARB = 0x881B (dup, compress only)
        Rgb16fExt = 0x881B,         //: GL_RGB16F_EXT = 0x881B (dup, compress only)
        Depth24Stencil8 = 0x88F0,   //: GL_DEPTH24_STENCIL8 = 0x88F0
        Depth24Stencil8Ext = 0x88F0,//: GL_DEPTH24_STENCIL8_EXT = 0x88F0 (dup, compress only)
        Depth24Stencil8Oes = 0x88F0,//: GL_DEPTH24_STENCIL8_OES = 0x88F0 (dup, compress only)
        R11fG11fB10f = 0x8C3A,      //: GL_R11F_G11F_B10F = 0x8C3A
        R11fG11fB10fApple = 0x8C3A, //: GL_R11F_G11F_B10F_APPLE = 0x8C3A (dup, compress only)
        R11fG11fB10fExt = 0x8C3A,   //: GL_R11F_G11F_B10F_EXT = 0x8C3A (dup, compress only)
        Rgb9E5 = 0x8C3D,            //: GL_RGB9_E5 = 0x8C3D
        Rgb9E5Apple = 0x8C3D,       //: GL_RGB9_E5_APPLE = 0x8C3D (dup, compress only)
        Rgb9E5Ext = 0x8C3D,         //: GL_RGB9_E5_EXT = 0x8C3D (dup, compress only)
        Srgb = 0x8C40,              //: GL_SRGB = 0x8C40
        SrgbExt = 0x8C40,           //: GL_SRGB_EXT = 0x8C40 (dup, compress only)
        Srgb8 = 0x8C41,             //: GL_SRGB8 = 0x8C41
        Srgb8Ext = 0x8C41,          //: GL_SRGB8_EXT = 0x8C41 (dup, compress only)
        Srgb8Nv = 0x8C41,           //: GL_SRGB8_NV = 0x8C41 (dup, compress only)
        SrgbAlpha = 0x8C42,         //: GL_SRGB_ALPHA = 0x8C42
        SrgbAlphaExt = 0x8C42,      //: GL_SRGB_ALPHA_EXT = 0x8C42 (dup, compress only)
        Srgb8Alpha8 = 0x8C43,       //: GL_SRGB8_ALPHA8 = 0x8C43
        Srgb8Alpha8Ext = 0x8C43,    //: GL_SRGB8_ALPHA8_EXT = 0x8C43 (dup, compress only)
        SluminanceAlpha = 0x8C44,       //: GL_SLUMINANCE_ALPHA = 0x8C44 (internal only)
        Sluminance8Alpha8 = 0x8C45,       //: GL_SLUMINANCE8_ALPHA8 = 0x8C45 (internal only)
        Sluminance = 0x8C46,       //: GL_SLUMINANCE = 0x8C46 (internal only)
        Sluminance8 = 0x8C47,       //: GL_SLUMINANCE8 = 0x8C47 (internal only)
        CompressedSrgb = 0x8C48,    //: GL_COMPRESSED_SRGB = 0x8C48
        CompressedSrgbAlpha = 0x8C49,//: GL_COMPRESSED_SRGB_ALPHA = 0x8C49
        CompressedSluminance = 0x8C4A,       //: GL_COMPRESSED_SLUMINANCE = 0x8C4A (internal only)
        CompressedSluminanceAlpha = 0x8C4B,       //: GL_COMPRESSED_SLUMINANCE_ALPHA = 0x8C4B (internal only)
        CompressedSrgbS3tcDxt1Ext = 0x8C4C,//: GL_COMPRESSED_SRGB_S3TC_DXT1_EXT = 0x8C4C
        CompressedSrgbAlphaS3tcDxt1Ext = 0x8C4D,//: GL_COMPRESSED_SRGB_ALPHA_S3TC_DXT1_EXT = 0x8C4D
        CompressedSrgbAlphaS3tcDxt3Ext = 0x8C4E,//: GL_COMPRESSED_SRGB_ALPHA_S3TC_DXT3_EXT = 0x8C4E
        CompressedSrgbAlphaS3tcDxt5Ext = 0x8C4F,//: GL_COMPRESSED_SRGB_ALPHA_S3TC_DXT5_EXT = 0x8C4F
        DepthComponent32f = 0x8CAC, //: GL_DEPTH_COMPONENT32F = 0x8CAC
        Depth32fStencil8 = 0x8CAD,  //: GL_DEPTH32F_STENCIL8 = 0x8CAD
        Rgba32ui = 0x8D70,          //: GL_RGBA32UI = 0x8D70
        Rgb32ui = 0x8D71,           //: GL_RGB32UI = 0x8D71
        Rgba16ui = 0x8D76,          //: GL_RGBA16UI = 0x8D76
        Rgb16ui = 0x8D77,           //: GL_RGB16UI = 0x8D77
        Rgba8ui = 0x8D7C,           //: GL_RGBA8UI = 0x8D7C
        Rgb8ui = 0x8D7D,            //: GL_RGB8UI = 0x8D7D
        Rgba32i = 0x8D82,           //: GL_RGBA32I = 0x8D82
        Rgb32i = 0x8D83,            //: GL_RGB32I = 0x8D83
        Rgba16i = 0x8D88,           //: GL_RGBA16I = 0x8D88
        Rgb16i = 0x8D89,            //: GL_RGB16I = 0x8D89
        Rgba8i = 0x8D8E,            //: GL_RGBA8I = 0x8D8E
        Rgb8i = 0x8D8F,             //: GL_RGB8I = 0x8D8F
        DepthComponent32fNv = 0x8DAB,//: GL_DEPTH_COMPONENT32F_NV = 0x8DAB (compress only)
        Depth32fStencil8Nv = 0x8DAC,//: GL_DEPTH32F_STENCIL8_NV = 0x8DAC (compress only)
        Float32UnsignedInt248Rev = 0x8DAD,//: GL_FLOAT_32_UNSIGNED_INT_24_8_REV = 0x8DAD (internal only)
        CompressedRedRgtc1 = 0x8DBB,//: GL_COMPRESSED_RED_RGTC1 = 0x8DBB
        CompressedRedRgtc1Ext = 0x8DBB,//: GL_COMPRESSED_RED_RGTC1_EXT = 0x8DBB (dup, compress only)
        CompressedSignedRedRgtc1 = 0x8DBC,//: GL_COMPRESSED_SIGNED_RED_RGTC1 = 0x8DBC
        CompressedSignedRedRgtc1Ext = 0x8DBC, //: GL_COMPRESSED_SIGNED_RED_RGTC1_EXT = 0x8DBC (dup, compress only)
        CompressedRgRgtc2 = 0x8DBD, //: GL_COMPRESSED_RG_RGTC2 = 0x8DBD
        CompressedSignedRgRgtc2 = 0x8DBE, //: GL_COMPRESSED_SIGNED_RG_RGTC2 = 0x8DBE
        CompressedRgbaBptcUnorm = 0x8E8C, //: GL_COMPRESSED_RGBA_BPTC_UNORM = 0x8E8C
        CompressedSrgbAlphaBptcUnorm = 0x8E8D, //: GL_COMPRESSED_SRGB_ALPHA_BPTC_UNORM = 0x8E8D
        CompressedRgbBptcSignedFloat = 0x8E8E, //: GL_COMPRESSED_RGB_BPTC_SIGNED_FLOAT = 0x8E8E
        CompressedRgbBptcUnsignedFloat = 0x8E8F, //: GL_COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT = 0x8E8F
        R8Snorm = 0x8F94,           //: GL_R8_SNORM = 0x8F94
        Rg8Snorm = 0x8F95,          //: GL_RG8_SNORM = 0x8F95
        Rgb8Snorm = 0x8F96,         //: GL_RGB8_SNORM = 0x8F96
        Rgba8Snorm = 0x8F97,        //: GL_RGBA8_SNORM = 0x8F97
        R16Snorm = 0x8F98,          //: GL_R16_SNORM = 0x8F98
        R16SnormExt = 0x8F98,       //: GL_R16_SNORM_EXT = 0x8F98 (dup, compress only)
        Rg16Snorm = 0x8F99,         //: GL_RG16_SNORM = 0x8F99
        Rg16SnormExt = 0x8F99,      //: GL_RG16_SNORM_EXT = 0x8F99 (dup, compress only)
        Rgb16Snorm = 0x8F9A,        //: GL_RGB16_SNORM = 0x8F9A
        Rgb16SnormExt = 0x8F9A,     //: GL_RGB16_SNORM_EXT = 0x8F9A (dup, compress only)
        Rgba16Snorm = 0x8F9B,        //: GL_RGBA16_SNORM = 0x8F9B (internal only)
        Rgb10A2ui = 0x906F,         //: GL_RGB10_A2UI = 0x906F (compress only)
        One = 1, //: GL_ONE = 1 (internal only)
        Two = 2, //: GL_TWO = 2 (internal only)
        Three = 3, //: GL_THREE = 3 (internal only)
        Four = 4, //: GL_FOUR = 4 (internal only)
        CompressedR11Eac = 0x9270,  //: GL_COMPRESSED_R11_EAC = 0x9270 (compress only)
        CompressedSignedR11Eac = 0x9271, //: GL_COMPRESSED_SIGNED_R11_EAC = 0x9271 (compress only)
        CompressedRg11Eac = 0x9272, //: GL_COMPRESSED_RG11_EAC = 0x9272 (compress only)
        CompressedSignedRg11Eac = 0x9273, //: GL_COMPRESSED_SIGNED_RG11_EAC = 0x9273 (compress only)
        CompressedRgb8Etc2 = 0x9274, //: GL_COMPRESSED_RGB8_ETC2 = 0x9274 (compress only)
        CompressedSrgb8Etc2 = 0x9275, //: GL_COMPRESSED_SRGB8_ETC2 = 0x9275 (compress only)
        CompressedRgb8PunchthroughAlpha1Etc2 = 0x9276, //: GL_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2 = 0x9276 (compress only)
        CompressedSrgb8PunchthroughAlpha1Etc2 = 0x9277, //: GL_COMPRESSED_SRGB8_PUNCHTHROUGH_ALPHA1_ETC2 = 0x9277 (compress only)
        CompressedRgba8Etc2Eac = 0x9278, //: GL_COMPRESSED_RGBA8_ETC2_EAC = 0x9278 (compress only)
        CompressedSrgb8Alpha8Etc2Eac = 0x9279 //: GL_COMPRESSED_SRGB8_ALPHA8_ETC2_EAC = 0x9279 (compress only)
    }

    /// <summary>
    /// ITexture
    /// </summary>
    public interface ITexture
    {
        IDictionary<string, object> Data { get; }
        int Width { get; }
        int Height { get; }
        int Depth { get; }
        int MipMaps { get; }
        TextureFlags Flags { get; }

        void Select(int id);
        byte[] Begin(int platform, out object format, out Range[] mips);
        void End();
    }

    /// <summary>
    /// ITextureMultiple
    /// </summary>
    public interface ITextureMultiple
    {
        int Fps { get; }
        int FrameMaxIndex { get; }
        void FrameSelect(int index);
    }
}