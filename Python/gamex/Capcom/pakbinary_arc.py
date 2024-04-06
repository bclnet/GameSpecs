import os
from io import BytesIO
from gamex.filesrc import FileSource
from gamex.pak import PakBinaryT
from gamex.compression import decompressZlib, decompressZstd
from gamex.util import _guessExtension

# typedefs
class Reader: pass
class BinaryPakFile: pass

# PakBinary_Arc
class PakBinary_Arc(PakBinaryT):

    #region K

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

    #endregion

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        magic = r.readUInt32()
        if magic != self.K_MAGIC: raise Exception('BAD MAGIC')

        header = r.readS(self.K_Header)
        kfiles = r.readTArray(self.K_File, header.numFiles)

        # get files
        source.files = files = [FileSource(
            compressed = x.compress,
            path = x.path.decode('utf8').strip('\x00').replace('\\', '/'),
            packedSize = x.zsize,
            fileSize = x.size,
            offset = x.offset
        ) for x in kfiles]

        # add extension
        for file in files:
            r.seek(file.offset)
            buf = _decompress(r, 150, full=False)
            file.path += _guessExtension(buf)

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        r.seek(file.offset)
        return BytesIO(_decompress(r, file.compressed, file.packedSize, file.fileSize))

@staticmethod
def _decompress(r: Reader, length: int, newLength: int=0, full: bool=True) -> bytes:
    return decompressZlib(r, length, newLength, full=full)