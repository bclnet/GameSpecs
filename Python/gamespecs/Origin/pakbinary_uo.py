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
        struct = ('<q3iQIh', 34)
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
            
        # load files
        nextBlock = header.nextBlock
        r.seek(nextBlock)
        while True:
            filesCount = r.readInt32()
            nextBlock = r.readInt64()
            for i in range(filesCount):
                record = r.readS(self.UopRecord)
                if record.offset == 0 or record.hash not in hashes: continue
                idx = hashes[record.hash]

                if idx < 0 or idx > length:
                    raise Exception('hashes dictionary and files collection have different count of entries!')

                file = files[idx]
                file.offset = record.offset + record.headerLength
                file.fileSize = record.fileSize

                if not extra: continue

                def peekLambda(x):
                    r.seek(file.offset)
                    extra = r.read(8)
                    extra1 = ((extra[3] << 24) | (extra[2] << 16) | (extra[1] << 8) | extra[0]) & 0xffff
                    extra2 = ((extra[7] << 24) | (extra[6] << 16) | (extra[5] << 8) | extra[4]) & 0xffff
                    file.offset += 8
                    file.compressed = extra1 << 16 | extra2
                r.peek(peekLambda)
            if r.f.seek(nextBlock, os.SEEK_SET): break

    @staticmethod
    def createUopHash(s: str) -> int:
        length = len(s)
        eax = ecx = edx = ebx = esi = edi = 0
        ebx = edi = esi = length + 0xDEADBEEF
        for i in range(0, length, 12):
            if not (i + 12 < length): break
            edi = ((((s[i + 7] << 24) | (s[i + 6] << 16) | (s[i + 5] << 8) | s[i + 4]) & 0xffffffff) + edi) & 0xffffffff
            esi = ((((s[i + 11] << 24) | (s[i + 10] << 16) | (s[i + 9] << 8) | s[i + 8]) & 0xffffffff) + esi) & 0xffffffff
            edx = ((((s[i + 3] << 24) | (s[i + 2] << 16) | (s[i + 1] << 8) | s[i]) & 0xffffffff) - esi) & 0xffffffff
            edx = ((edx + ebx) ^ (esi >> 28) ^ (esi << 4)) & 0xffffffff; esi = (esi + edi) & 0xffffffff
            edi = ((edi - edx) ^ (edx >> 26) ^ (edx << 6)) & 0xffffffff; edx = (edx + esi) & 0xffffffff
            esi = ((esi - edi) ^ (edi >> 24) ^ (edi << 8)) & 0xffffffff; edi = (edi + edx) & 0xffffffff
            ebx = ((edx - esi) ^ (esi >> 16) ^ (esi << 16)) & 0xffffffff; esi = (esi + edi) & 0xffffffff
            edi = ((edi - ebx) ^ (ebx >> 13) ^ (ebx << 19)) & 0xffffffff; ebx = (ebx + esi) & 0xffffffff
            esi = ((esi - edi) ^ (edi >> 28) ^ (edi << 4)) & 0xffffffff; edi = (edi + ebx) & 0xffffffff
        length2 = length - i
        if length2 > 0:
            if length2 >= 12: esi = (esi + (s[i + 11] << 24) & 0xffffffff) & 0xffffffff
            if length2 >= 11: esi = (esi + (s[i + 10] << 16) & 0xffffffff) & 0xffffffff
            if length2 >= 10: esi = (esi + (s[i + 9] << 8) & 0xffffffff) & 0xffffffff
            if length2 >= 9: esi = (esi + (s[i + 8]) & 0xffffffff) & 0xffffffff
            if length2 >= 8: edi = (edi + (s[i + 7] << 24) & 0xffffffff) & 0xffffffff
            if length2 >= 7: edi = (edi + (s[i + 6] << 16) & 0xffffffff) & 0xffffffff
            if length2 >= 6: edi = (edi + (s[i + 5] << 8) & 0xffffffff) & 0xffffffff
            if length2 >= 5: edi = (edi + (s[i + 4]) & 0xffffffff) & 0xffffffff
            if length2 >= 4: ebx = (ebx + (s[i + 3] << 24) & 0xffffffff) & 0xffffffff
            if length2 >= 3: ebx = (ebx + (s[i + 2] << 16) & 0xffffffff) & 0xffffffff
            if length2 >= 2: ebx = (ebx + (s[i + 1] << 8) & 0xffffffff) & 0xffffffff
            if length2 >= 1: ebx = (ebx + (s[i]) & 0xffffffff) & 0xffffffff
            esi = ((esi ^ edi) - ((edi >> 18) ^ (edi << 14))) & 0xffffffff
            ecx = ((esi ^ ebx) - ((esi >> 21) ^ (esi << 11))) & 0xffffffff
            edi = ((edi ^ ecx) - ((ecx >> 7) ^ (ecx << 25))) & 0xffffffff
            esi = ((esi ^ edi) - ((edi >> 16) ^ (edi << 16))) & 0xffffffff
            edx = ((esi ^ ecx) - ((esi >> 28) ^ (esi << 4))) & 0xffffffff
            edi = ((edi ^ edx) - ((edx >> 18) ^ (edx << 14))) & 0xffffffff
            eax = ((esi ^ edi) - ((edi >> 8) ^ (edi << 24))) & 0xffffffff
            return (edi << 32) & 0xffffffffffffffff | eax
        return (esi << 32) & 0xffffffffffffffff | eax

    #endregion

    #region IDX

    def readIdx(self, source: BinaryPakFile, r: Reader):
        pass

    #endregion

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        pass
