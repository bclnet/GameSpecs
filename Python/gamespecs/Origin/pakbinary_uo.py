import os
from io import BytesIO
from gamespecs.filesrc import FileSource
from gamespecs.pak import PakBinaryT
from gamespecs.util import _pathExtension

# typedefs
class Reader: pass
class BinaryPakFile: pass
class FamilyGame: pass
class IFileSystem: pass

# PakBinary_UO
class PakBinary_UO(PakBinaryT):

    #region Headers

    class IdxFile:
        struct = ('<3i', 26)
        def __init__(self, tuple):
            self.offset, \
            self.fileSize, \
            self.extra = tuple

    class UopHeader:
        struct = ('<i2q2i', 26)
        def __init__(self, tuple):
            self.magic, \
            self.versionSignature, \
            self.nextBlock, \
            self.blockCapacity, \
            self.count = tuple

    class UopRecord:
        struct = ('<h3iHIu', 26)
        def __init__(self, tuple):
            self.offset, \
            self.headerLength, \
            self.compressedLength, \
            self.decompressedLength, \
            self.hash, \
            self.adler32, \
            self.flag = tuple
        @property
        def fileSize(self) -> int: self.compressedLength if self.flag == 1 else self.decompressedLength

    #endregion

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        if source.pakPath.endswith('.uop'): self.readUop(source, r)
        else: self.readIdx(source, r)
        
    #region UOP

    UOP_MAGIC = 0x50594D

    def readUop(self, source: BinaryPakFile, r: Reader):
        extension, length, idxLength, extra, pathFunc
        def parse():
            match source.PakPath:
                case 'artLegacyMUL.uop': ('.tga', 0x14000, 0x13FDC, False, lambda i: f'land/file{i:x5}.land' if i < 0x4000 else f'static/file{i:x5}.art'),
                case 'gumpartLegacyMUL.uop': (".tga", 0xFFFF, 0, True, lambda i: f'file{i:x5}.tex'),
                case 'soundLegacyMUL.uop': (".dat", 0xFFF, 0, False, lambda i: f'file{i:x5}.wav'),
                case _: (None, 0, 0, False, lambda i: f'file{i:x5}.dat')
        extension, length, idxLength, extra, pathFunc = parse()
        uopPattern = os.path.basename(source.pakPath).toLower()

        # read header
        header = r.readS(UopHeader.struct)
        if header.magic != UOP_MAGIC: raise Exception('BAD MAGIC')

        # record count
        self.count = idxLength if idxLength > 0 else 0

        # find hashes
        hashes = {}
        for i in range(length):
            hashes.tryAdd(self.createUopHash(f'build/{uopPattern}/{i:D8}{uopEntryExtension}'), i)

        # load empties
        source.files = files = [None]*length
        for i in range(files.length):
            files[i] = FileSource(
                id = i,
                path = pathFunc(i),
                offset = -1,
                fileSize = -1,
                compressed = -1
                )

    #endregion

    #region IDX

    def readIdx(self, source: BinaryPakFile, r: Reader):
        pass

    #endregion

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        pass
