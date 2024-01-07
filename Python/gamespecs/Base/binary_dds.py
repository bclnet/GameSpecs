import os
from io import BytesIO
from typing import Any
from openstk.poly import Reader
from openstk.gfx_dds import DDS_HEADER
from openstk.gfx_texturemgr import TextureFlags
from ..pakfile import FileSource, PakFile
from ..metamgr import MetaManager, MetaInfo, MetaContent, IHaveMetaInfo

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

    def begin(self, platform: int) -> (bytes, object, list[Any]):
        match platform:
            case FamilyPlatform.Type.OpenGL: format = Format.gl
            case FamilyPlatform.Type.Unity: format = Format.unity
            case FamilyPlatform.Type.Unreal: format = Format.unreal
            case FamilyPlatform.Type.Vulken: format = Format.vulken
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
