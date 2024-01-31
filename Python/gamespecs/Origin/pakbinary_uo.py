import os
from io import BytesIO
from pathlib import Path
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
        struct = ('<i2q2i', 28)
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
    def read(self, source: BinaryPakFile, r: Reader, tag: object) -> None:
        if source.pakPath.endswith('.uop'): self.readUop(source, r)
        else: self.readIdx(source, r)
        
    #region UOP

    UOP_MAGIC = 0x50594D

    def readUop(self, source: BinaryPakFile, r: Reader):
        def parse():
            match source.pakPath:
                case 'artLegacyMUL.uop': return ('.tga', 0x14000, 0x13FDC, False, lambda i: f'land/file{i:05x}.land' if i < 0x4000 else f'static/file{i:05x}.art')
                case 'gumpartLegacyMUL.uop': return (".tga", 0xFFFF, 0, True, lambda i: f'file{i:05x}.tex')
                case 'soundLegacyMUL.uop': return (".dat", 0xFFF, 0, False, lambda i: f'file{i:05x}.wav')
                case _: return (None, 0, 0, False, lambda i: f'file{i:05x}.dat')
        extension, length, idxLength, extra, pathFunc = parse()
        uopPattern = Path(source.pakPath).stem.lower()

        # read header
        header = r.readS(self.UopHeader)
        if header.magic != self.UOP_MAGIC: raise Exception('BAD MAGIC')

        # record count
        self.count = idxLength if idxLength > 0 else 0

        # find hashes
        hashes = {}
        for i in range(length):
            hashes[self.createUopHash(f'build/{uopPattern}/{i:08}{extension}'.encode('ascii'))] = i

        # load empties
        source.files = files = [None]*length
        for i in range(length):
            files[i] = FileSource(
                id = i,
                path = pathFunc(i),
                offset = -1,
                fileSize = -1,
                compressed = -1
                )

    @staticmethod
    def createUopHash(s: str) -> int:
        length = len(s)
        eax = ecx = edx = ebx = esi = edi = 0
        ebx = edi = esi = length + 0xDEADBEEF
        for i in range(0, length - 12, 12):
            edi = ((s[i + 7] << 24) | (s[i + 6] << 16) | (s[i + 5] << 8) | s[i + 4]) & 0xffffff + edi
            esi = ((s[i + 11] << 24) | (s[i + 10] << 16) | (s[i + 9] << 8) | s[i + 8]) & 0xffffff + esi
            edx = ((s[i + 3] << 24) | (s[i + 2] << 16) | (s[i + 1] << 8) | s[i]) & 0xffffff - esi
            edx = (edx + ebx) ^ (esi >> 28) ^ (esi << 4); esi += edi
            edi = (edi - edx) ^ (edx >> 26) ^ (edx << 6); edx += esi
            esi = (esi - edi) ^ (edi >> 24) ^ (edi << 8); edi += edx
            ebx = (edx - esi) ^ (esi >> 16) ^ (esi << 16); esi += edi
            edi = (edi - ebx) ^ (ebx >> 13) ^ (ebx << 19); ebx += esi
            esi = (esi - edi) ^ (edi >> 28) ^ (edi << 4); edi += ebx
        length2 = length - i
        if length2 > 0:
            if length2 <= 12: esi += (s[i + 11] << 24) & 0xffffff
            if length2 <= 11: esi += (s[i + 10] << 16) & 0xffffff
            if length2 <= 10: esi += (s[i + 9] << 8) & 0xffffff
            if length2 <= 9: esi += s[i + 8]
            if length2 <= 8: edi += (s[i + 7] << 24) & 0xffffff
            if length2 <= 7: edi += (s[i + 6] << 16) & 0xffffff
            if length2 <= 6: edi += (s[i + 5] << 8) & 0xffffff
            if length2 <= 5: edi += s[i + 4]
            if length2 <= 4: ebx += (s[i + 3] << 24) & 0xffffff
            if length2 <= 3: ebx += (s[i + 2] << 16) & 0xffffff
            if length2 <= 2: ebx += (s[i + 1] << 8) & 0xffffff
            if length2 <= 1: ebx += s[i]
            esi = (esi ^ edi) - ((edi >> 18) ^ (edi << 14))
            ecx = (esi ^ ebx) - ((esi >> 21) ^ (esi << 11))
            edi = (edi ^ ecx) - ((ecx >> 7) ^ (ecx << 25))
            esi = (esi ^ edi) - ((edi >> 16) ^ (edi << 16))
            edx = (esi ^ ecx) - ((esi >> 28) ^ (esi << 4))
            edi = (edi ^ edx) - ((edx >> 18) ^ (edx << 14))
            eax = (esi ^ edi) - ((edi >> 8) ^ (edi << 24))
            return (edi << 32) & 0xffffffffffff | eax
        return (esi << 32) & 0xffffffffffff | eax

    #endregion

    #region IDX

    def readIdx(self, source: BinaryPakFile, r: Reader):
        pass

    #endregion

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        pass
