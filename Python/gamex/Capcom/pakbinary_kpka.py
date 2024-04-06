import os
from io import BytesIO
from gamex.filesrc import FileSource
from gamex.pak import PakBinaryT
from gamex.compression import decompressZlib, decompressZstd
from gamex.util import _guessExtension
from ..Resources.Capcom import RE

# typedefs
class Reader: pass
class BinaryPakFile: pass

# PakBinary_Kpka
class PakBinary_Kpka(PakBinaryT):

    #region K

    K_MAGIC = 0x414b504b
    class K_Header:
        struct = ('<BBhiI', 12)
        def __init__(self, tuple):
            self.majorVersion, \
            self.minorVersion, \
            self.feature, \
            self.numFiles, \
            self.hash = tuple
    class K_FileV2:
        struct = ('<qqQ', 24)
        def __init__(self, tuple):
            self.offset, \
            self.fileSize, \
            self.hashName = tuple
    class K_FileV4:
        struct = ('<QqqqqQ', 48)
        def __init__(self, tuple):
            self.hashName, \
            self.offset, \
            self.packedSize, \
            self.fileSize, \
            self.flag, \
            self.checksum = tuple

    #endregion

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        magic = r.readUInt32()
        if magic != self.K_MAGIC: raise Exception('BAD MAGIC')

        # get hashlookup
        hashLookup = RE.getHashLookup(f'{source.game.resource}.list') if source.game.resource else None

        # get header
        header = r.readS(self.K_Header)
        if header.majorVersion != 2 and header.majorVersion != 4 or header.minorVersion != 0: raise Exception('BAD VERSION')

        # decrypt table
        tr = r
        if header.feature == 8:
            entrySize = K_FileV2.struct[1] if header.majorVersion == 2 else K_FileV4.struct[1]
            table = r.read(header.numFiles * entrySize)
            key = r.read(128)
            # tr = BinaryReader(decryptTable(table, decryptKey(key)))

        # get files
        if header.majorVersion == 2:
            source.files = [FileSource(
                path = hashLookup[x.hashName].replace('\\', '/') if hashLookup and x.hashName in hashLookup else \
                    f'_unknown/{x.hashName:0>16x}{_getExtension(r, x.offset, 0)}',
                offset = x.offset,
                fileSize = x.fileSize,
            ) for x in tr.readTArray(self.K_FileV2, header.numFiles)]
        elif header.majorVersion == 4:
            compressed:int
            source.files = [FileSource(
                compressed = (compressed := _getCompressed(x.flag)),
                path = hashLookup[x.hashName].replace('\\', '/') if hashLookup and x.hashName in hashLookup else \
                    f'_unknown/{x.hashName:0>16x}{_getExtension(r, x.offset, compressed)}',
                offset = x.offset,
                packedSize = x.packedSize,
                fileSize = x.fileSize,
            ) for x in tr.readTArray(self.K_FileV4, header.numFiles)]

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        r.seek(file.offset)
        return BytesIO(_decompress(r, file.compressed, file.packedSize, file.fileSize))

Modulus = int.from_bytes([
    0x7D, 0x0B, 0xF8, 0xC1, 0x7C, 0x23, 0xFD, 0x3B, 0xD4, 0x75, 0x16, 0xD2, 0x33, 0x21, 0xD8, 0x10,
    0x71, 0xF9, 0x7C, 0xD1, 0x34, 0x93, 0xBA, 0x77, 0x26, 0xFC, 0xAB, 0x2C, 0xEE, 0xDA, 0xD9, 0x1C,
    0x89, 0xE7, 0x29, 0x7B, 0xDD, 0x8A, 0xAE, 0x50, 0x39, 0xB6, 0x01, 0x6D, 0x21, 0x89, 0x5D, 0xA5,
    0xA1, 0x3E, 0xA2, 0xC0, 0x8C, 0x93, 0x13, 0x36, 0x65, 0xEB, 0xE8, 0xDF, 0x06, 0x17, 0x67, 0x96,
    0x06, 0x2B, 0xAC, 0x23, 0xED, 0x8C, 0xB7, 0x8B, 0x90, 0xAD, 0xEA, 0x71, 0xC4, 0x40, 0x44, 0x9D,
    0x1C, 0x7B, 0xBA, 0xC4, 0xB6, 0x2D, 0xD6, 0xD2, 0x4B, 0x62, 0xD6, 0x26, 0xFC, 0x74, 0x20, 0x07,
    0xEC, 0xE3, 0x59, 0x9A, 0xE6, 0xAF, 0xB9, 0xA8, 0x35, 0x8B, 0xE0, 0xE8, 0xD3, 0xCD, 0x45, 0x65,
    0xB0, 0x91, 0xC4, 0x95, 0x1B, 0xF3, 0x23, 0x1E, 0xC6, 0x71, 0xCF, 0x3E, 0x35, 0x2D, 0x6B, 0xE3,
    0x00
    ])

Exponent = int.from_bytes([
    0x01, 0x00, 0x01, 0x00
    ])

@staticmethod
def _decompress(r: Reader, compressed: int, length: int, newLength: int = 0, full: bool = True) -> bytes:
    return r.read(length) if compressed == 0 else \
        decompressZlib(r, length, newLength, noHeader = True, full = full) if compressed == 'Z' else \
        decompressZstd(r, length, newLength) if compressed == 'S' else \
        None

@staticmethod
def _getCompressed(f: int) -> int:
    return (0 if f >> 16 > 0 else 'Z') if (f & 0xF) == 1 else \
        (0 if f >> 16 > 0 else 'S') if (f & 0xF) == 2 else \
        0

@staticmethod
def _getExtension(r: Reader, offset: int, compressed: int, full: bool = True) -> str:
    r.seek(offset)
    return _guessExtension(_decompress(r, compressed, 150, full = False))
