import os
from enum import Enum, Flag
from openstk.poly import Reader
from openstk.gfx_texture import TextureGLFormat, TextureUnityFormat, TextureUnrealFormat

class DDS_HEADER: pass

#region DDS_PIXELFORMAT

# FourCC
class FourCC(Enum):
    DXT1 = 0x31545844       # DXT1
    DXT2 = 0x32545844       # DXT2
    DXT3 = 0x33545844       # DXT3
    DXT4 = 0x34545844       # DXT4
    DXT5 = 0x35545844       # DXT5
    RXGB = 0x42475852       # RXGB
    ATI1 = 0x31495441       # ATI1
    ATI2 = 0x32495441       # ATI2
    A2XY = 0x59583241       # A2XY
    DX10 = 0x30315844       # DX10

# Values which indicate what type of data is in the surface.
class DDPF(Flag):
    ALPHAPIXELS = 0x00000001    # Texture contains alpha data; dwRGBAlphaBitMask contains valid data.
    ALPHA = 0x00000002          # Used in some older DDS files for alpha channel only uncompressed data (dwRGBBitCount contains the alpha channel bitcount; dwABitMask contains valid data)
    FOURCC = 0x00000004         # Texture contains compressed RGB data; dwFourCC contains valid data.
    RGB = 0x00000040            # Texture contains uncompressed RGB data; dwRGBBitCount and the RGB masks (dwRBitMask, dwGBitMask, dwBBitMask) contain valid data.
    YUV = 0x00000200            # Used in some older DDS files for YUV uncompressed data (dwRGBBitCount contains the YUV bit count; dwRBitMask contains the Y mask, dwGBitMask contains the U mask, dwBBitMask contains the V mask)
    LUMINANCE = 0x00020000      # Used in some older DDS files for single channel color uncompressed data (dwRGBBitCount contains the luminance channel bit count; dwRBitMask contains the channel mask). Can be combined with DDPF_ALPHAPIXELS for a two channel DDS file.
    NORMAL = 0x80000000         # The normal

# Surface pixel format.
class DDS_PIXELFORMAT:
    dwSize:int              # Structure size; set to 32 (bytes).
    dwFlags:DDPF            # Values which indicate what type of data is in the surface.
    dwFourCC:FourCC         # Four-character codes for specifying compressed or custom formats. Possible values include: DXT1, DXT2, DXT3, DXT4, or DXT5. A FourCC of DX10 indicates the prescense of the DDS_HEADER_DXT10 extended header, and the dxgiFormat member of that structure indicates the true format. When using a four-character code, dwFlags must include DDPF_FOURCC.
    dwRGBBitCount:int       # Number of bits in an RGB (possibly including alpha) format. Valid when dwFlags includes DDPF_RGB, DDPF_LUMINANCE, or DDPF_YUV.
    dwRBitMask:int          # Red (or lumiannce or Y) mask for reading color data. For instance, given the A8R8G8B8 format, the red mask would be 0x00ff0000.
    dwGBitMask:int          # Green (or U) mask for reading color data. For instance, given the A8R8G8B8 format, the green mask would be 0x0000ff00.
    dwBBitMask:int          # Blue (or V) mask for reading color data. For instance, given the A8R8G8B8 format, the blue mask would be 0x000000ff.
    dwABitMask:int          # Alpha mask for reading alpha data. dwFlags must include DDPF_ALPHAPIXELS or DDPF_ALPHA. For instance, given the A8R8G8B8 format, the alpha mask would be 0xff000000.

#endregion

#region DDS_HEADER_DXT10

class DDS_ALPHA_MODE(Enum):
    ALPHA_MODE_UNKNOWN = 0
    ALPHA_MODE_STRAIGHT = 1
    ALPHA_MODE_PREMULTIPLIED = 2
    ALPHA_MODE_OPAQUE = 3
    ALPHA_MODE_CUSTOM = 4

class D3D10_RESOURCE_DIMENSION(Enum):
    UNKNOWN = 0         # Resource is of unknown type.
    BUFFER = 1          # Resource is a buffer.
    TEXTURE1D = 2       # Resource is a 1D texture. The dwWidth member of DDS_HEADER specifies the size of the texture. Typically, you set the dwHeight member of DDS_HEADER to 1; you also must set the DDSD_HEIGHT flag in the dwFlags member of DDS_HEADER.
    TEXTURE2D = 3       # Resource is a 2D texture with an area specified by the dwWidth and dwHeight members of DDS_HEADER. You can also use this type to identify a cube-map texture. For more information about how to identify a cube-map texture, see miscFlag and arraySize members.
    TEXTURE3D = 4       # Resource is a 3D texture with a volume specified by the dwWidth, dwHeight, and dwDepth members of DDS_HEADER. You also must set the DDSD_DEPTH flag in the dwFlags member of DDS_HEADER.

# DDS header extension to handle resource arrays, DXGI pixel formats that don't map to the legacy Microsoft DirectDraw pixel format structures, and additional metadata.
class DDS_HEADER_DXT10:
    struct = (f'<5I', 20)
    def __init__(self, tuple):
        ddspf = self.ddspf = DDS_PIXELFORMAT()
        self.dxgiFormat, \
        self.resourceDimension, \
        self.miscFlag, \
        self.arraySize, \
        self.miscFlags2 = tuple
        self.dxgiFormat = DXGI_FORMAT(self.dxgiFormat)
        self.resourceDimension = D3D10_RESOURCE_DIMENSION(self.resourceDimension)
        self.dwCaps2 = DDSCAPS2(self.dwCaps2)

#endregion

#region DDS_HEADER

# Flags to indicate which members contain valid data.
class DDSD(Flag):
    CAPS = 0x00000001           # Required in every .dds file.
    HEIGHT = 0x00000002         # Required in every .dds file.
    WIDTH = 0x00000004          # Required in every .dds file.
    PITCH = 0x00000008          # Required when pitch is provided for an uncompressed texture.
    PIXELFORMAT = 0x00001000    # Required in every .dds file.
    MIPMAPCOUNT = 0x00020000    # Required in a mipmapped texture.
    LINEARSIZE = 0x00080000     # Required when pitch is provided for a compressed texture.
    DEPTH = 0x00800000          # Required in a depth texture.
    HEADER_FLAGS_TEXTURE = CAPS | HEIGHT | WIDTH | PIXELFORMAT
    HEADER_FLAGS_MIPMAP = MIPMAPCOUNT
    HEADER_FLAGS_VOLUME = DEPTH
    HEADER_FLAGS_PITCH = PITCH
    HEADER_FLAGS_LINEARSIZE = LINEARSIZE

# Specifies the complexity of the surfaces stored.
class DDSCAPS(Flag):
    COMPLEX = 0x00000008        # Optional; must be used on any file that contains more than one surface (a mipmap, a cubic environment map, or mipmapped volume texture).
    TEXTURE = 0x00001000        # Required
    MIPMAP = 0x00400000         # Optional; should be used for a mipmap.
    SURFACE_FLAGS_MIPMAP = COMPLEX | MIPMAP
    SURFACE_FLAGS_TEXTURE = TEXTURE
    SURFACE_FLAGS_CUBEMAP = COMPLEX

# Additional detail about the surfaces stored.
class DDSCAPS2(Flag):
    CUBEMAP = 0x00000200            # Required for a cube map.
    CUBEMAPPOSITIVEX = 0x00000400   # Required when these surfaces are stored in a cube map.
    CUBEMAPNEGATIVEX = 0x00000800   # Required when these surfaces are stored in a cube map.
    CUBEMAPPOSITIVEY = 0x00001000   # Required when these surfaces are stored in a cube map.
    CUBEMAPNEGATIVEY = 0x00002000   # Required when these surfaces are stored in a cube map.
    CUBEMAPPOSITIVEZ = 0x00004000   # Required when these surfaces are stored in a cube map.
    CUBEMAPNEGATIVEZ = 0x00008000   # Required when these surfaces are stored in a cube map.
    VOLUME = 0x00200000             # Required for a volume texture.
    CUBEMAP_POSITIVEX = CUBEMAP | CUBEMAPPOSITIVEX
    CUBEMAP_NEGATIVEX = CUBEMAP | CUBEMAPNEGATIVEX
    CUBEMAP_POSITIVEY = CUBEMAP | CUBEMAPPOSITIVEY
    CUBEMAP_NEGATIVEY = CUBEMAP | CUBEMAPNEGATIVEY
    CUBEMAP_POSITIVEZ = CUBEMAP | CUBEMAPPOSITIVEZ
    CUBEMAP_NEGATIVEZ = CUBEMAP | CUBEMAPNEGATIVEZ
    CUBEMAP_ALLFACES = CUBEMAPPOSITIVEX | CUBEMAPNEGATIVEX | CUBEMAPPOSITIVEY | CUBEMAPNEGATIVEY | CUBEMAPPOSITIVEZ | CUBEMAPNEGATIVEZ
    FLAGS_VOLUME = VOLUME

# Describes a DDS file header.
class DDS_HEADER:
    MAGIC = 0x20534444
    struct = (f'<7I44s{"8I"}5I', 124)
    def __init__(self, tuple):
        ddspf = self.ddspf = DDS_PIXELFORMAT()
        self.dwSize, \
        self.dwFlags, \
        self.dwHeight, \
        self.dwWidth, \
        self.dwPitchOrLinearSize, \
        self.dwDepth, \
        self.dwMipMapCount, \
        self.dwReserved1, \
        ddspf.dwSize, \
        ddspf.dwFlags, \
        ddspf.dwFourCC, \
        ddspf.dwRGBBitCount, \
        ddspf.dwRBitMask, \
        ddspf.dwGBitMask, \
        ddspf.dwBBitMask, \
        ddspf.dwABitMask, \
        self.dwCaps, \
        self.dwCaps2, \
        self.dwCaps3, \
        self.dwCaps4, \
        self.dwReserved2 = tuple
        self.dwFlags = DDSD(self.dwFlags)
        ddspf.dwFlags = DDPF(ddspf.dwFlags)
        ddspf.dwFourCC = FourCC(ddspf.dwFourCC)
        self.dwCaps = DDSCAPS(self.dwCaps)
        self.dwCaps2 = DDSCAPS2(self.dwCaps2)

    # Verifies this instance
    def _verify(self):
        if self.dwSize != 124: raise Exception(f'Invalid DDS file header size: {dwSize}.')
        elif not self.dwFlags & (DDSD.HEIGHT | DDSD.WIDTH): raise Exception(f'Invalid DDS file flags: {dwFlags}.')
        elif not self.dwCaps & DDSCAPS.TEXTURE: raise Exception(f'Invalid DDS file caps: {dwCaps}.')
        elif self.ddspf.dwSize != 32: raise Exception(f'Invalid DDS file pixel format size: {ddspf.dwSize}.')

    @staticmethod
    def read(r: Reader, readMagic: bool = True) -> (bytes, DDS_HEADER, DDS_HEADER_DXT10, object):
        if readMagic:
            magic = r.readUInt32()
            if magic != DDS_HEADER.MAGIC: raise Exception(f'Invalid DDS file magic: "{magic}".')
        header = r.readS(DDS_HEADER)
        header._verify()
        ddspf = header.ddspf
        headerDXT10 = r.ReadT(DDS_HEADER_DXT10) if ddspf.dwFourCC == FourCC.DX10 else None
        match ddspf.dwFourCC:
            case 0: format = _makeformat(ddspf)
            case FourCC.DXT1: format = (FourCC.DXT1, 8, TextureGLFormat.CompressedRgbaS3tcDxt1Ext, TextureGLFormat.CompressedRgbaS3tcDxt1Ext, TextureUnityFormat.DXT1, TextureUnrealFormat.DXT1)
            case FourCC.DXT3: format = (FourCC.DXT3, 16, TextureGLFormat.CompressedRgbaS3tcDxt3Ext, TextureGLFormat.CompressedRgbaS3tcDxt3Ext, TextureUnityFormat.DXT3_POLYFILL, TextureUnrealFormat.DXT3)
            case FourCC.DXT5: format = (FourCC.DXT5, 16, TextureGLFormat.CompressedRgbaS3tcDxt5Ext, TextureGLFormat.CompressedRgbaS3tcDxt5Ext, TextureUnityFormat.DXT5, TextureUnrealFormat.DXT5)
            case FourCC.DX10: 
                match headerDXT10.dxgiFormat:
                    case DXGI_FORMAT.BC1_UNORM: format = (BC1_UNORM, 8, TextureGLFormat.CompressedRgbaS3tcDxt1Ext, TextureGLFormat.CompressedRgbaS3tcDxt1Ext, TextureUnityFormat.DXT1, TextureUnrealFormat.DXT1)
                    case DXGI_FORMAT.BC1_UNORM_SRGB: format = (BC1_UNORM_SRGB, 8, TextureGLFormat.CompressedSrgbS3tcDxt1Ext, TextureGLFormat.CompressedSrgbS3tcDxt1Ext, TextureUnityFormat.DXT1, TextureUnrealFormat.DXT1)
                    case DXGI_FORMAT.BC2_UNORM: format = (BC2_UNORM, 16, TextureGLFormat.CompressedRgbaS3tcDxt3Ext, TextureGLFormat.CompressedRgbaS3tcDxt3Ext, TextureUnityFormat.DXT3_POLYFILL, TextureUnrealFormat.DXT3)
                    case DXGI_FORMAT.BC2_UNORM_SRGB : format = (BC2_UNORM_SRGB, 16, TextureGLFormat.CompressedSrgbAlphaS3tcDxt1Ext, TextureGLFormat.CompressedSrgbAlphaS3tcDxt1Ext, TextureUnityFormat.Unknown, TextureUnrealFormat.Unknown)
                    case DXGI_FORMAT.BC3_UNORM: format = (BC3_UNORM, 16, TextureGLFormat.CompressedRgbaS3tcDxt5Ext, TextureGLFormat.CompressedRgbaS3tcDxt5Ext, TextureUnityFormat.DXT5, TextureUnrealFormat.DXT5)
                    case DXGI_FORMAT.BC3_UNORM_SRGB: format = (BC3_UNORM_SRGB, 16, TextureGLFormat.CompressedSrgbAlphaS3tcDxt5Ext, TextureGLFormat.CompressedSrgbAlphaS3tcDxt5Ext, TextureUnityFormat.Unknown, TextureUnrealFormat.Unknown)
                    case DXGI_FORMAT.BC4_UNORM: format = (BC4_UNORM, 8, TextureGLFormat.CompressedRedRgtc1, TextureGLFormat.CompressedRedRgtc1, TextureUnityFormat.BC4, TextureUnrealFormat.BC4)
                    case DXGI_FORMAT.BC4_SNORM: format = (BC4_SNORM, 8, TextureGLFormat.CompressedSignedRedRgtc1, TextureGLFormat.CompressedSignedRedRgtc1, TextureUnityFormat.BC4, TextureUnrealFormat.BC4)
                    case DXGI_FORMAT.BC5_UNORM: format = (BC5_UNORM, 16, TextureGLFormat.CompressedRgRgtc2, TextureGLFormat.CompressedRgRgtc2, TextureUnityFormat.BC5, TextureUnrealFormat.BC5)
                    case DXGI_FORMAT.BC5_SNORM: format = (BC5_SNORM, 16, TextureGLFormat.CompressedSignedRgRgtc2, TextureGLFormat.CompressedSignedRgRgtc2, TextureUnityFormat.BC5, TextureUnrealFormat.BC5)
                    case DXGI_FORMAT.BC6H_UF16: format = (BC6H_UF16, 16, TextureGLFormat.CompressedRgbBptcUnsignedFloat, TextureGLFormat.CompressedRgbBptcUnsignedFloat, TextureUnityFormat.BC6H, TextureUnrealFormat.BC6H)
                    case DXGI_FORMAT.BC6H_SF16: format = (BC6H_SF16, 16, TextureGLFormat.CompressedRgbBptcSignedFloat, TextureGLFormat.CompressedRgbBptcSignedFloat, TextureUnityFormat.BC6H, TextureUnrealFormat.BC6H)
                    case DXGI_FORMAT.BC7_UNORM: format = (BC7_UNORM, 16, TextureGLFormat.CompressedRgbaBptcUnorm, TextureGLFormat.CompressedRgbaBptcUnorm, TextureUnityFormat.BC7, TextureUnrealFormat.BC7)
                    case DXGI_FORMAT.BC7_UNORM_SRGB: format = (BC7_UNORM_SRGB, 16, TextureGLFormat.CompressedSrgbAlphaBptcUnorm, TextureGLFormat.CompressedSrgbAlphaBptcUnorm, TextureUnityFormat.BC7, TextureUnrealFormat.BC7)
                    case DXGI_FORMAT.R8_UNORM: format = (R8_UNORM, 1, (TextureGLFormat.R8, TextureGLPixelFormat.Red, TextureGLPixelType.Byte), (TextureGLFormat.R8, TextureGLPixelFormat.Red, TextureGLPixelType.Byte), TextureUnityFormat.R8, TextureUnrealFormat.R8)
                    case _: raise Exception(f'Unknown dxgiFormat: 0x{ddspf.dxgiFormat:x}')
            case _: raise Exception(f'Unknown dwFourCC: 0x{ddspf.dwFourCC:x}')
        return r.readToEnd(), header, headerDXT10, format

    @staticmethod
    def _makeformat(f: DDS_PIXELFORMAT) -> object:
        return ('Raw', f.dwRGBBitCount >> 2, \
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Rgba, TextureGLPixelType.Byte), \
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Rgba, TextureGLPixelType.Byte), \
            TextureUnityFormat.RGBA32, \
            TextureUnrealFormat.R8G8B8A8)

#endregion

#region DIGI_FORMAT

# DirectX Graphics Infrastructure formats.
class DXGI_FORMAT(Enum):
    UNKNOWN = 0 # The format is not known.
    R32G32B32A32_TYPELESS = 1 # A four-component, 128-bit typeless format that supports 32 bits per channel including alpha.
    R32G32B32A32_FLOAT = 2 # A four-component, 128-bit floating-point format that supports 32 bits per channel including alpha.
    R32G32B32A32_UINT = 3 # A four-component, 128-bit unsigned-integer format that supports 32 bits per channel including alpha.
    R32G32B32A32_SINT = 4 # A four-component, 128-bit signed-integer format that supports 32 bits per channel including alpha.
    R32G32B32_TYPELESS = 5 # A three-component, 96-bit typeless format that supports 32 bits per color channel.
    R32G32B32_FLOAT = 6 # A three-component, 96-bit floating-point format that supports 32 bits per color channel.
    R32G32B32_UINT = 7 # A three-component, 96-bit unsigned-integer format that supports 32 bits per color channel.
    R32G32B32_SINT = 8 # A three-component, 96-bit signed-integer format that supports 32 bits per color channel.
    R16G16B16A16_TYPELESS = 9 # A four-component, 64-bit typeless format that supports 16 bits per channel including alpha.
    R16G16B16A16_FLOAT = 10 # A four-component, 64-bit floating-point format that supports 16 bits per channel including alpha.
    R16G16B16A16_UNORM = 11 # A four-component, 64-bit unsigned-normalized-integer format that supports 16 bits per channel including alpha.
    R16G16B16A16_UINT = 12 # A four-component, 64-bit unsigned-integer format that supports 16 bits per channel including alpha.
    R16G16B16A16_SNORM = 13 # A four-component, 64-bit signed-normalized-integer format that supports 16 bits per channel including alpha.
    R16G16B16A16_SINT = 14 # A four-component, 64-bit signed-integer format that supports 16 bits per channel including alpha.
    R32G32_TYPELESS = 15 # A two-component, 64-bit typeless format that supports 32 bits for the red channel and 32 bits for the green channel.
    R32G32_FLOAT = 16 # A two-component, 64-bit floating-point format that supports 32 bits for the red channel and 32 bits for the green channel.
    R32G32_UINT = 17 # A two-component, 64-bit unsigned-integer format that supports 32 bits for the red channel and 32 bits for the green channel.
    R32G32_SINT = 18 # A two-component, 64-bit signed-integer format that supports 32 bits for the red channel and 32 bits for the green channel.
    R32G8X24_TYPELESS = 19 # A two-component, 64-bit typeless format that supports 32 bits for the red channel, 8 bits for the green channel, and 24 bits are unused.
    D32_FLOAT_S8X24_UINT = 20 # A 32-bit floating-point component, and two unsigned-integer components (with an additional 32 bits). This format supports 32-bit depth, 8-bit stencil, and 24 bits are unused.
    R32_FLOAT_X8X24_TYPELESS = 21 # A 32-bit floating-point component, and two typeless components (with an additional 32 bits). This format supports 32-bit red channel, 8 bits are unused, and 24 bits are unused.
    X32_TYPELESS_G8X24_UINT = 22 # A 32-bit typeless component, and two unsigned-integer components (with an additional 32 bits). This format has 32 bits unused, 8 bits for green channel, and 24 bits are unused.
    R10G10B10A2_TYPELESS = 23 # A four-component, 32-bit typeless format that supports 10 bits for each color and 2 bits for alpha.
    R10G10B10A2_UNORM = 24 # A four-component, 32-bit unsigned-normalized-integer format that supports 10 bits for each color and 2 bits for alpha.
    R10G10B10A2_UINT = 25 # A four-component, 32-bit unsigned-integer format that supports 10 bits for each color and 2 bits for alpha.
    R11G11B10_FLOAT = 26 # Three partial-precision floating-point numbers encoded into a single 32-bit value (a variant of s10e5, which is sign bit, 10-bit mantissa, and 5-bit biased (15) exponent).
                            # There are no sign bits, and there is a 5-bit biased (15) exponent for each channel, 6-bit mantissa for R and G, and a 5-bit mantissa for B.
    R8G8B8A8_TYPELESS = 27 # A four-component, 32-bit typeless format that supports 8 bits per channel including alpha.
    R8G8B8A8_UNORM = 28 # A four-component, 32-bit unsigned-normalized-integer format that supports 8 bits per channel including alpha.
    R8G8B8A8_UNORM_SRGB = 29 # A four-component, 32-bit unsigned-normalized integer sRGB format that supports 8 bits per channel including alpha.
    R8G8B8A8_UINT = 30 # A four-component, 32-bit unsigned-integer format that supports 8 bits per channel including alpha.
    R8G8B8A8_SNORM = 31 # A four-component, 32-bit signed-normalized-integer format that supports 8 bits per channel including alpha.
    R8G8B8A8_SINT = 32 # A four-component, 32-bit signed-integer format that supports 8 bits per channel including alpha.
    R16G16_TYPELESS = 33 # A two-component, 32-bit typeless format that supports 16 bits for the red channel and 16 bits for the green channel.
    R16G16_FLOAT = 34 # A two-component, 32-bit floating-point format that supports 16 bits for the red channel and 16 bits for the green channel.
    R16G16_UNORM = 35 # A two-component, 32-bit unsigned-normalized-integer format that supports 16 bits each for the green and red channels.
    R16G16_UINT = 36 # A two-component, 32-bit unsigned-integer format that supports 16 bits for the red channel and 16 bits for the green channel.
    R16G16_SNORM = 37 # A two-component, 32-bit signed-normalized-integer format that supports 16 bits for the red channel and 16 bits for the green channel.
    R16G16_SINT = 38 # A two-component, 32-bit signed-integer format that supports 16 bits for the red channel and 16 bits for the green channel.
    R32_TYPELESS = 39 # A single-component, 32-bit typeless format that supports 32 bits for the red channel.
    D32_FLOAT = 40 # A single-component, 32-bit floating-point format that supports 32 bits for depth.
    R32_FLOAT = 41 # A single-component, 32-bit floating-point format that supports 32 bits for the red channel.
    R32_UINT = 42 # A single-component, 32-bit unsigned-integer format that supports 32 bits for the red channel.
    R32_SINT = 43 # A single-component, 32-bit signed-integer format that supports 32 bits for the red channel.
    R24G8_TYPELESS = 44 # A two-component, 32-bit typeless format that supports 24 bits for the red channel and 8 bits for the green channel.
    D24_UNORM_S8_UINT = 45 # A 32-bit z-buffer format that supports 24 bits for depth and 8 bits for stencil.
    R24_UNORM_X8_TYPELESS = 46 # A 32-bit format, that contains a 24 bit, single-component, unsigned-normalized integer, with an additional typeless 8 bits. This format has 24 bits red channel and 8 bits unused.
    X24_TYPELESS_G8_UINT = 47 # A 32-bit format, that contains a 24 bit, single-component, typeless format, with an additional 8 bit unsigned integer component. This format has 24 bits unused and 8 bits green channel.
    R8G8_TYPELESS = 48 # A two-component, 16-bit typeless format that supports 8 bits for the red channel and 8 bits for the green channel.
    R8G8_UNORM = 49 # A two-component, 16-bit unsigned-normalized-integer format that supports 8 bits for the red channel and 8 bits for the green channel.
    R8G8_UINT = 50 # A two-component, 16-bit unsigned-integer format that supports 8 bits for the red channel and 8 bits for the green channel.
    R8G8_SNORM = 51 # A two-component, 16-bit signed-normalized-integer format that supports 8 bits for the red channel and 8 bits for the green channel.
    R8G8_SINT = 52 # A two-component, 16-bit signed-integer format that supports 8 bits for the red channel and 8 bits for the green channel.
    R16_TYPELESS = 53 # A single-component, 16-bit typeless format that supports 16 bits for the red channel.
    R16_FLOAT = 54 # A single-component, 16-bit floating-point format that supports 16 bits for the red channel.
    D16_UNORM = 55 # A single-component, 16-bit unsigned-normalized-integer format that supports 16 bits for depth.
    R16_UNORM = 56 # A single-component, 16-bit unsigned-normalized-integer format that supports 16 bits for the red channel.
    R16_UINT = 57 # A single-component, 16-bit unsigned-integer format that supports 16 bits for the red channel.
    R16_SNORM = 58 # A single-component, 16-bit signed-normalized-integer format that supports 16 bits for the red channel.
    R16_SINT = 59 # A single-component, 16-bit signed-integer format that supports 16 bits for the red channel.
    R8_TYPELESS = 60 # A single-component, 8-bit typeless format that supports 8 bits for the red channel.
    R8_UNORM = 61 # A single-component, 8-bit unsigned-normalized-integer format that supports 8 bits for the red channel.
    R8_UINT = 62 # A single-component, 8-bit unsigned-integer format that supports 8 bits for the red channel.
    R8_SNORM = 63 # A single-component, 8-bit signed-normalized-integer format that supports 8 bits for the red channel.
    R8_SINT = 64 # A single-component, 8-bit signed-integer format that supports 8 bits for the red channel.
    A8_UNORM = 65 # A single-component, 8-bit unsigned-normalized-integer format for alpha only.
    R1_UNORM = 66 # A single-component, 1-bit unsigned-normalized integer format that supports 1 bit for the red channel.
    R9G9B9E5_SHAREDEXP = 67 # Three partial-precision floating-point numbers encoded into a single 32-bit value all sharing the same 5-bit exponent (variant of s10e5, which is sign bit, 10-bit mantissa, and 5-bit biased (15) exponent).
                            # There is no sign bit, and there is a shared 5-bit biased (15) exponent and a 9-bit mantissa for each channel.
    R8G8_B8G8_UNORM = 68 # A four-component, 32-bit unsigned-normalized-integer format. This packed RGB format is analogous to the UYVY format. Each 32-bit block describes a pair of pixels: (R8, G8, B8) and (R8, G8, B8) where the R8/B8 values are repeated, and the G8 values are unique to each pixel.
                            # Width must be even.
    G8R8_G8B8_UNORM = 69 # A four-component, 32-bit unsigned-normalized-integer format. This packed RGB format is analogous to the YUY2 format. Each 32-bit block describes a pair of pixels: (R8, G8, B8) and (R8, G8, B8) where the R8/B8 values are repeated, and the G8 values are unique to each pixel.
                            # Width must be even.
    BC1_TYPELESS = 70 # Four-component typeless block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC1_UNORM = 71 # Four-component block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC1_UNORM_SRGB = 72 # Four-component block-compression format for sRGB data. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC2_TYPELESS = 73 # Four-component typeless block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC2_UNORM = 74 # Four-component block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC2_UNORM_SRGB = 75 # Four-component block-compression format for sRGB data. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC3_TYPELESS = 76 # Four-component typeless block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC3_UNORM = 77 # Four-component block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC3_UNORM_SRGB = 78 # Four-component block-compression format for sRGB data. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC4_TYPELESS = 79 # One-component typeless block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC4_UNORM = 80 # One-component block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC4_SNORM = 81 # One-component block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC5_TYPELESS = 82 # Two-component typeless block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC5_UNORM = 83 # Two-component block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC5_SNORM = 84 # Two-component block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    B5G6R5_UNORM = 85 # A three-component, 16-bit unsigned-normalized-integer format that supports 5 bits for blue, 6 bits for green, and 5 bits for red.
    B5G5R5A1_UNORM = 86 # A four-component, 16-bit unsigned-normalized-integer format that supports 5 bits for each color channel and 1-bit alpha.
    B8G8R8A8_UNORM = 87 # A four-component, 32-bit unsigned-normalized-integer format that supports 8 bits for each color channel and 8-bit alpha.
    B8G8R8X8_UNORM = 88 # A four-component, 32-bit unsigned-normalized-integer format that supports 8 bits for each color channel and 8 bits unused.
    R10G10B10_XR_BIAS_A2_UNORM = 89 # A four-component, 32-bit 2.8-biased fixed-point format that supports 10 bits for each color channel and 2-bit alpha.
    B8G8R8A8_TYPELESS = 90 # A four-component, 32-bit typeless format that supports 8 bits for each channel including alpha.
    B8G8R8A8_UNORM_SRGB = 91 # A four-component, 32-bit unsigned-normalized standard RGB format that supports 8 bits for each channel including alpha.
    B8G8R8X8_TYPELESS = 92 # A four-component, 32-bit typeless format that supports 8 bits for each color channel, and 8 bits are unused.
    B8G8R8X8_UNORM_SRGB = 93 # A four-component, 32-bit unsigned-normalized standard RGB format that supports 8 bits for each color channel, and 8 bits are unused.
    BC6H_TYPELESS = 94 # A typeless block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC6H_UF16 = 95 # A block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC6H_SF16 = 96 # A block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC7_TYPELESS = 97 # A typeless block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC7_UNORM = 98 # A block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    BC7_UNORM_SRGB = 99 # A block-compression format. For information about block-compression formats, see Texture Block Compression in Direct3D 11.
    AYUV = 100 # Most common YUV 4:4:4 video resource format.
    Y410 = 101 # 10-bit per channel packed YUV 4:4:4 video resource format.
    Y416 = 102 # 16-bit per channel packed YUV 4:4:4 video resource format.
    NV12 = 103 # Most common YUV 4:2:0 video resource format.
    P010 = 104 # 10-bit per channel planar YUV 4:2:0 video resource format.
    P016 = 105 # 16-bit per channel planar YUV 4:2:0 video resource format.
    _420_OPAQUE = 106 # 8-bit per channel planar YUV 4:2:0 video resource format.
    YUY2 = 107 # Most common YUV 4:2:2 video resource format.
    Y210 = 108 # 10-bit per channel packed YUV 4:2:2 video resource format.
    Y216 = 109 # 16-bit per channel packed YUV 4:2:2 video resource format.
    NV11 = 110 # Most common planar YUV 4:1:1 video resource format.
    AI44 = 111 # 4-bit palletized YUV format that is commonly used for DVD subpicture.
    IA44 = 112 # 4-bit palletized YUV format that is commonly used for DVD subpicture.
    P8 = 113 # 8-bit palletized format that is used for palletized RGB data when the processor processes ISDB-T data and for palletized YUV data when the processor processes BluRay data.
    A8P8 = 114 # 8-bit palletized format with 8 bits of alpha that is used for palletized YUV data when the processor processes BluRay data.
    B4G4R4A4_UNORM = 115 # A four-component, 16-bit unsigned-normalized integer format that supports 4 bits for each channel including alpha.
    P208 = 130 # A video format; an 8-bit version of a hybrid planar 4:2:2 format.
    V208 = 131 # An 8 bit YCbCrA 4:4 rendering format.
    V408 = 132 # An 8 bit YCbCrA 4:4:4:4 rendering format.
    SAMPLER_FEEDBACK_MIN_MIP_OPAQUE = 133 # SAMPLER_FEEDBACK_MIN_MIP_OPAQUE
    SAMPLER_FEEDBACK_MIP_REGION_USED_OPAQUE = 134 # SAMPLER_FEEDBACK_MIP_REGION_USED_OPAQUE

#endregion