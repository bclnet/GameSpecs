import os
from io import BytesIO
from typing import Any
from openstk.poly import Reader
from ..pakbinary import PakBinary
from ..pakfile import FileSource, BinaryPakFile
from ..compression import decompressZlib, decompressZstd
from ..util import _guessExtension

class PakBinary_Arc(PakBinary):
    K_MAGIC = 0x00435241
    class K_Header:
        struct = ('<HH', 4)
        def __init__(self, tuple):
            self.version, \
            self.numFiles = tuple
    class K_File:
        struct = ('<64sIIII', 80)
        def __init__(self, tuple):
            self.path, \
            self.compress, \
            self.zsize, \
            self.size, \
            self.offset = tuple

    _instance = None
    def __new__(cls):
        if cls._instance is None: cls._instance = super().__new__(cls)
        return cls._instance

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: Any = None) -> None:
        magic = r.readUInt32()
        if magic != self.K_MAGIC: raise Exception('BAD MAGIC')

        header = r.readT(self.K_Header)
        kfiles = r.readTArray(self.K_File, header.numFiles)

        # get files
        source.files = files = [FileSource(
            compressed = x.compress,
            path = x.path.decode('utf8').strip('\x00').replace('\\', '/'),
            packedSize = x.zsize,
            fileSize = x.size,
            position = x.offset
        ) for x in kfiles]

        # add extension
        for file in files:
            r.seek(file.position)
            buf = _decompress(r, 150, full=False)
            file.path += _guessExtension(buf)

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        r.seek(file.position)
        return BytesIO(_decompress(r, file.compressed, file.packedSize, file.fileSize))

@staticmethod
def _decompress(r: Reader, length: int, newLength: int=0, full: bool=True) -> bytes:
    return decompressZlib(r, length, newLength, full=full)