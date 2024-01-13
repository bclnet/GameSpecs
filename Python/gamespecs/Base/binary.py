import os
import imageio as iio
from enum import Enum
from io import BytesIO
from openstk.gfx_dds import DDS_HEADER
from openstk.gfx_texture import TextureGLFormat, TextureGLPixelFormat, TextureGLPixelType, TextureUnityFormat, TextureUnrealFormat
from gamespecs.pakfile import FileSource, PakBinary
from gamespecs.metamgr import MetaManager, MetaInfo, MetaContent, IHaveMetaInfo
from gamespecs.util import _pathExtension

# typedefs
class PakFile: pass
class Reader: pass
class ITexture: pass
class TextureFlags: pass
class MetaManager: pass
class MetaManager: pass

# Binary_Dds
class Binary_Dds(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Dds(r)

    def __init__(self, r: Reader, readMagic: bool = True):
        self.bytes, self.header, self.headerDXT10, self.format = DDS_HEADER.read(r, readMagic)
        numMipMaps = max(1, self.header.dwMipMapCount)
        offset = 0
        self.mips = [range(-1, 0)] * numMipMaps
        for i in range(numMipMaps):
            w = self.header.dwWidth >> i; h = self.header.dwHeight >> i
            if w == 0 or h == 0: self.mips[i] = range(-1, 0); continue
            size = int(((w + 3) / 4) * ((h + 3) / 4)) * self.format[1]
            remains = min(size, len(self.bytes) - offset)
            self.mips[i] = range(offset, (offset + remains)) if remains > 0 else range(-1, 0)
            offset += remains

    def data(self) -> dict[str, object]: return None
    def width(self) -> int: return self.header.dwWidth
    def height(self) -> int: return self.header.dwHeight
    def depth(self) -> int: return 0
    def mipMaps(self) -> int: return self.header.dwMipMapCount
    def flags(self) -> TextureFlags: return 0

    def begin(self, platform: int) -> (bytes, object, list[object]):
        match platform:
            case Platform.Type.OpenGL: format = self.format[1]
            case Platform.Type.Vulken: format = self.format[2]
            case Platform.Type.Unity: format = self.format[3]
            case Platform.Type.Unreal: format = self.format[4]
            case _: raise Exception('Unknown {platform}')
        return self.bytes, format, self.mips
    def end(self): pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Texture', items = [
            MetaInfo(f'Format: {self.format[0]}'),
            MetaInfo(f'Width: {self.width()}'),
            MetaInfo(f'Height: {self.height()}'),
            MetaInfo(f'Mipmaps: {self.mipMaps()}')
            ])
        ]

# Binary_Img
class Binary_Img(IHaveMetaInfo, ITexture):
    class Formats(Enum):
        Bmp = 1
        Gif = 2
        Exif = 3
        Jpg = 4
        Png = 5
        Tiff = 6
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Img(r, f)

    def __init__(self, r: Reader, f: FileSource):
        match _pathExtension(f.path).lower():
            case '.bmp': formatType = self.Formats.Bmp
            case '.gif': formatType = self.Formats.Gif
            case '.exif': formatType = self.Formats.Exif
            case '.jpg': formatType = self.Formats.Jpg
            case '.png': formatType = self.Formats.Png
            case '.tiff': formatType = self.Formats.Tiff
            case _: raise Exception(f'Unknown {_pathExtension(f.path)}')
        self.format = (formatType, (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte), (TextureGLFormat.Rgb8, TextureGLPixelFormat.Rgb, TextureGLPixelType.UnsignedByte), TextureUnityFormat.RGB24, TextureUnrealFormat.Unknown)
        self.bytes = r.read(f.fileSize)
        img = iio.imread(self.bytes)
        self.dwWidth, self.dwHeight, ukn = img.shape

    def data(self) -> dict[str, object]: return None
    def width(self) -> int: return self.dwWidth
    def height(self) -> int: return self.dwHeight
    def depth(self) -> int: return 0
    def mipMaps(self) -> int: return 1
    def flags(self) -> TextureFlags: return 0

    def begin(self, platform: int) -> (bytes, object, list[object]):
        match platform:
            case Platform.Type.OpenGL: format = self.format[1]
            case Platform.Type.Vulken: format = self.format[2]
            case Platform.Type.Unity: format = self.format[3]
            case Platform.Type.Unreal: format = self.format[4]
            case _: raise Exception('Unknown {platform}')
        return self.bytes, format, None
    def end(self): pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Texture', items = [
            MetaInfo(f'Format: {self.format[0]}'),
            MetaInfo(f'Width: {self.width()}'),
            MetaInfo(f'Height: {self.height()}'),
            MetaInfo(f'Mipmaps: {self.mipMaps()}')
            ])
        ]

# Binary_Snd
class Binary_Snd(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Snd(r, f.fileSize)

    def __init__(self, r: Reader, fileSize: int):
        self.data = r.read(fileSize)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'AudioPlayer', name = os.path.basename(file.path), value = self.data, tag = _pathExtension(file.path)))
        ]

# Binary_Txt
class Binary_Txt(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Txt(r, f.fileSize)

    def __init__(self, r: Reader, fileSize: int):
        self.data = r.read(fileSize)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self.data))
        ]
