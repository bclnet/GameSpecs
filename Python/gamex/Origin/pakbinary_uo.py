import os, numpy as np
from ctypes import c_ulong, c_ulonglong
from io import BytesIO
from typing import Callable
from pathlib import Path
from gamex.filesrc import FileSource
from gamex.pak import PakBinaryT
from gamex.util import _pathExtension

# typedefs
class Reader: pass
class BinaryPakFile: pass
class FamilyGame: pass
class IFileSystem: pass
class FileOption: pass

# PakBinary_UO
class PakBinary_UO(PakBinaryT):

    #region Factories

    @staticmethod
    def objectFactoryFactory(source: FileSource, game: FamilyGame) -> (FileOption, Callable):
        match source.path.lower():
            case 'animdata.mul': return (0, Binary_Animdata.factory)
            case 'fonts.mul': return (0, Binary_AsciiFont.factory)
            case 'bodyconv.def': return (0, Binary_BodyConverter.factory)
            case 'body.def': return (0, Binary_BodyTable.factory)
            case 'calibration.cfg': return (0, Binary_CalibrationInfo.factory)
            case 'gump.def': return (0, Binary_GumpDef.factory)
            case 'hues.mul': return (0, Binary_Hues.factory)
            case 'mobtypes.txt': return (0, Binary_MobType.factory)
            case x if x == 'multimap.rle' or x.startswith('facet') == 'facet': return (0, Binary_MultiMap.factory)
            case 'music/digital/config.txt': return (0, Binary_MusicDef.factory)
            case 'radarcol.mul': return (0, Binary_RadarColor.factory)
            case 'skillgrp.mul': return (0, Binary_SkillGroups.factory)
            case 'speech.mul': return (0, Binary_SpeechList.factory)
            case 'tiledata.mul': return (0, Binary_TileData.factory)
            case x if x.startswith('cliloc'): return (0, Binary_StringTable.factory)
            case 'verdata.mul': return (0, Binary_Verdata.factory)
            # server
            case 'data/containers.cfg': return (0, ServerBinary_Container.factory)
            case 'data/bodytable.cfg': return (0, ServerBinary_BodyTable.factory)
            case _:
                match _pathExtension(source.path).lower():
                    case '.anim': return (0, Binary_Anim.factory)
                    case '.tex': return (0, Binary_Gump.factory)
                    case '.land': return (0, Binary_Land.factory)
                    case '.light': return (0, Binary_Light.factory)
                    case '.art': return (0, Binary_Static.factory)
                    case '.multi': return (0, Binary_Multi.factory)
                    case _: (0, None)

    #endregion

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
        def fileSize(self) -> int: return self.compressedLength if self.flag == 1 else self.decompressedLength

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

                # load extra
                if not extra: continue
                def peekLambda(x):
                    r.seek(file.offset)
                    extra = r.read(8)
                    extra1 = ((extra[3] << 24) | (extra[2] << 16) | (extra[1] << 8) | extra[0]) & 0xffff
                    extra2 = ((extra[7] << 24) | (extra[6] << 16) | (extra[5] << 8) | extra[4]) & 0xffff
                    file.offset += 8
                    file.compressed = extra1 << 16 | extra2
                r.peek(peekLambda)
            if r.f.seek(nextBlock, os.SEEK_SET) == 0: break

    @staticmethod
    def createUopHash2(s: str) -> int:
        eax = c_ulong(); ebx = c_ulong(); ecx = c_ulong(); edx = c_ulong()
        esi = c_ulong(); edi = c_ulong()
        length = len(s)
        ebx.value = edi.value = esi.value = length + 0xDEADBEEF
        for i in range(0, length, 12):
            if not (i + 12 < length): break
            edi.value = c_ulong((s[i + 7] << 24) | (s[i + 6] << 16) | (s[i + 5] << 8) | s[i + 4]).value + edi.value
            esi.value = c_ulong((s[i + 11] << 24) | (s[i + 10] << 16) | (s[i + 9] << 8) | s[i + 8]).value + esi.value
            edx.value = c_ulong((s[i + 3] << 24) | (s[i + 2] << 16) | (s[i + 1] << 8) | s[i]).value - esi.value
            edx.value = (edx.value + ebx.value) ^ (esi.value >> 28) ^ (esi.value << 4); esi.value += edi.value
            edi.value = (edi.value - edx.value) ^ (edx.value >> 26) ^ (edx.value << 6); edx.value += esi.value
            esi.value = (esi.value - edi.value) ^ (edi.value >> 24) ^ (edi.value << 8); edi.value += edx.value
            ebx.value = (edx.value - esi.value) ^ (esi.value >> 16) ^ (esi.value << 16); esi.value += edi.value
            edi.value = (edi.value - ebx.value) ^ (ebx.value >> 13) ^ (ebx.value << 19); ebx.value += esi.value
            esi.value = (esi.value - edi.value) ^ (edi.value >> 28) ^ (edi.value << 4); edi.value += ebx.value

        length2 = length - i
        if length2 > 0:
            if length2 >= 12: esi.value += s[i + 11] << 24
            if length2 >= 11: esi.value += s[i + 10] << 16
            if length2 >= 10: esi.value += s[i + 9] << 8
            if length2 >= 9: esi.value += s[i + 8]
            if length2 >= 8: edi.value += s[i + 7] << 24
            if length2 >= 7: edi.value += s[i + 6] << 16
            if length2 >= 6: edi.value += s[i + 5] << 8
            if length2 >= 5: edi.value += s[i + 4]
            if length2 >= 4: ebx.value += s[i + 3] << 24
            if length2 >= 3: ebx.value += s[i + 2] << 16
            if length2 >= 2: ebx.value += s[i + 1] << 8
            if length2 >= 1: ebx.value += s[i]
            esi.value = (esi.value ^ edi.value) - ((edi.value >> 18) ^ (edi.value << 14))
            ecx.value = (esi.value ^ ebx.value) - ((esi.value >> 21) ^ (esi.value << 11))
            edi.value = (edi.value ^ ecx.value) - ((ecx.value >> 7) ^ (ecx.value << 25))
            esi.value = (esi.value ^ edi.value) - ((edi.value >> 16) ^ (edi.value << 16))
            edx.value = (esi.value ^ ecx.value) - ((esi.value >> 28) ^ (esi.value << 4))
            edi.value = (edi.value ^ edx.value) - ((edx.value >> 18) ^ (edx.value << 14))
            eax.value = (esi.value ^ edi.value) - ((edi.value >> 8) ^ (edi.value << 24))
            return c_ulonglong(edi.value << 32).value | eax.value
        return c_ulonglong(esi << 32).value | eax.value

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
        def parse():
            match source.pakPath:
                case 'anim.idx': return ('anim.mul', 0x40000, 6, lambda i: f'file{i:05x}.anim')
                case 'anim2.idx': return ('anim2.mul', 0x10000, -1, lambda i: f'file{i:05x}.anim')
                case 'anim3.idx': return ('anim3.mul', 0x20000, -1, lambda i: f'file{i:05x}.anim')
                case 'anim4.idx': return ('anim4.mul', 0x20000, -1, lambda i: f'file{i:05x}.anim')
                case 'anim5.idx': return ('anim5.mul', 0x20000, -1, lambda i: f'file{i:05x}.anim')
                case 'artidx.mul': return ('art.mul', 0x14000, 4, lambda i: f'land/file{i:05x}.land' if i < 0x4000 else f'static/file{i:05x}.art')
                case 'gumpidx.mul': return ('Gumpart.mul', 0xFFFF, 12, lambda i: f'file{i:05x}.tex')
                case 'multi.idx': return ('multi.mul', 0x2200, 14, lambda i: f'file{i:05x}.multi')
                case 'lightidx.mul': return ('light.mul', 0x4000, -1, lambda i: f'file{i:05x}.light')
                case 'skills.idx': return ('Skills.mul', 55, 16, lambda i: f'file{i:05x}.skill')
                case 'soundidx.mul': return ('sound.mul', 0x1000, 8, lambda i: f'file{i:05x}.wav')
                case 'texidx.mul': return ('texmaps.mul', 0x4000, 10, lambda i: f'file{i:05x}.dat')
                case _: raise Exception()
        mulPath, length, fileId, pathFunc = parse()
        source.pakPath = mulPath

        # record count
        self.count = r.length / 12

        # load files
        id = 0
        files: list[FileSource] = []
        source.files = files = [FileSource(
            id = id,
            path = pathFunc(id),
            offset = s.offset,
            fileSize = s.fileSize,
            compressed = s.extra,
            tag = (id := id + 1),
            ) for s in r.readSArray(self.IdxFile, self.count)]

        # fill with empty
        for i in range(self.count, length):
            files.append(FileSource(
                id = i,
                path = pathFunc(i),
                offset = -1,
                fileSize = -1,
                compressed = -1,
                ))

        # apply patch
        verdata = Binary_Verdata.instance
        if verdata and fileId in verdata.patches:
            patches = [patch for patch in verdata.patches[fileId] if patch.index > 0 and patch.index < len(files)]
            for patch in patches:
                file = files[patch.index]
                file.offset = patch.offset
                file.fileSize = patch.fileSize | (1 << 31)
                file.compressed = patch.extra

        # public static int Art_MaxItemId
        #     => Art_Instance.Count >= 0x13FDC ? 0xFFDC // High Seas
        #     : Art_Instance.Count == 0xC000 ? 0x7FFF // Stygian Abyss
        #     : 0x3FFF; // ML and older

        # public static bool Art_IsUOAHS
        #     => Art_MaxItemId >= 0x13FDC;

        # public static ushort Art_ClampItemId(int itemId, bool checkMaxId = true)
        #     => itemId < 0 || (checkMaxId && itemId > Art_MaxItemId) ? (ushort)0U : (ushort)itemId;

    #endregion

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        if file.offset < 0: return None
        fileSize = file.fileSize & 0x7FFFFFFF
        if (file.fileSize & (1 << 31)) != 0:
            return Binary_Verdata.instance.readData(file.offset, fileSize)
        r.seek(file.offset)
        return BytesIO(r.read(fileSize))
