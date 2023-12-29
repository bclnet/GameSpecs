import os
from io import BytesIO
from typing import Any
from ..pakbinary import PakBinary
from ..pakfile import FileSource, BinaryPakFile
from ..openstk_poly import Reader
from ..compression import decompressZlib, decompressZstd
from ..util import _guessExtension

class PakBinary_Kpka(PakBinary):
    K_MAGIC = 0x414b504b
    class K_Header:
        struct = ('=BBhiI', 12)
        def __init__(self, tuple):
            self.majorVersion, \
            self.minorVersion, \
            self.feature, \
            self.numFiles, \
            self.hash = tuple
    class K_FileV2:
        struct = ('=QqqqqQ', 48)
        def __init__(self, tuple):
            self.nameCrc, \
            self.offset, \
            self.zsize, \
            self.size, \
            self.flag, \
            self.dummy = tuple
    class K_FileV4:
        struct = ('=QqqqqQ', 48)
        def __init__(self, tuple):
            self.nameCrc, \
            self.offset, \
            self.zsize, \
            self.size, \
            self.flag, \
            self.dummy = tuple

    _instance = None
    def __new__(cls):
        if cls._instance is None: cls._instance = super().__new__(cls)
        return cls._instance

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: Any = None) -> None:
        magic = r.readUInt32()
        if magic != self.K_MAGIC: raise Exception('BAD MAGIC')

        fileid = 0
        header = r.readT(self.K_Header)
        kfiles = r.readTArray(self.K_File, header.numFiles)

        # get files
        source.files = files = [FileSource(
            id = (fileid := fileid + 1),
            path = f'File{fileid:0>4x}',
            position = x.offset,
            packedSize = x.zsize,
            fileSize = x.size,
            compressed = x.flag
        ) for x in kfiles]

        # add extension
        for file in files:
            r.seek(file.position)
            buf = _decompress(r, file.compressed, 150, full=False)
            file.path += _guessExtension(buf)

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        r.seek(file.position)
        return BytesIO(_decompress(r, file.compressed, file.packedSize, file.fileSize))

@staticmethod
def _decompress(r: Reader, compressed: int, length: int, newLength: int=0, full: bool=True) -> bytes:
    return decompressZlib(r, length, newLength, noHeader=True, full=full) if (compressed & 1) != 0 else \
        r.read(length)

    # return decompressZlib(r, length, newLength, noHeader=True, full=full) if compressed == 1 else \
    # decompressZstd(r, length, newLength) if compressed == 2 else \
    # r.read(length) if (compressed & 1) == 0 else \
    # decompressZlib(r, length, newLength, noHeader=True, full=full)
