import os
from io import BytesIO
from typing import Any
from ..pakfile import FileSource, PakFile
from ..openstk_dds import DDS_HEADER
from ..openstk_poly import Reader
from ..openstk_texturemgr import TextureFlags

class Binary_Dds:
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Dds(r)
    def __init__(self, r: Reader, readMagic: bool = True):
        self.bytes, header, headerDXT10, format = DDS_HEADER.read(r, readMagic)
        numMipMaps = max(1, header.dwMipMapCount)
        offset = 0
        self.mips = [range(-1, 0)] * numMipMaps
        for i in range(numMipMaps):
            w = header.dwWidth >> i; h = header.dwHeight >> i
            if w == 0 or h == 0: self.mips[i] = range(-1, 0); continue
            size = int(((w + 3) / 4) * ((h + 3) / 4)) * format[1]
            remains = min(size, len(self.bytes) - offset)
            self.mips[i] = range(offset, (offset + remains)) if remains > 0 else range(-1, 0)
            offset += remains
    def data() -> dict[str, object]: return None
    def width() -> int: return header.dwWidth
    def height() -> int: return header.dwHeight
    def depth() -> int: return 0
    def mipMaps() -> int: return header.dwMipMapCount
    def flags() -> TextureFlags: return 0

    def begin(platform: int, out object format, out Range[] mips) -> (bytes, object, list[Any]):
        format = (FamilyPlatform.Type)platform switch
        {
            FamilyPlatform.Type.OpenGL => Format.gl,
            FamilyPlatform.Type.Unity => Format.unity,
            FamilyPlatform.Type.Unreal => Format.unreal,
            FamilyPlatform.Type.Vulken => Format.vulken,
            FamilyPlatform.Type.StereoKit => throw new NotImplementedException("StereoKit"),
            _ => throw new ArgumentOutOfRangeException(nameof(platform), $"{platform}"),
        }
        mips = Mips
        return Bytes
        def end(): pass

        def getInfoNodes(self, resource: MetaManager, file: FileSource, tag: object) -> list<MetaInfo>: return [
            MetaInfo(null, MetaContent(Type = "Texture", Name = os.path.basename(file.path), Value = self)),
            MetaInfo("Texture", items = [
                MetaInfo($"Format: {Format.type}"),
                MetaInfo($"Width: {Width}"),
                MetaInfo($"Height: {Height}"),
                MetaInfo($"Mipmaps: {MipMaps}")
                ])
            ]
