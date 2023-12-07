import os
from struct import unpack

class Reader:
    def __init__(self, f): self.f = f

    # normal
    def read(self, size): return self.f.read(size)
    def length(self):
        f = self.f
        pos = f.tell(); length = f.seek(0, os.SEEK_END); f.seek(pos, os.SEEK_SET)
        return length

    # position
    def seek(self, pos): return self.f.seek(pos, os.SEEK_SET)
    def skip(self, pos): return self.f.seek(pos, os.SEEK_CUR)
    def peek(self, action, offset = 0):
        f = self.f
        pos = f.tell()
        if offset != 0: f.seek(offset, os.SEEK_CUR)
        value = action(self)
        f.seek(pos)
        return value

    # struct
    def readT(self, t, size): return unpack(t, self.f.read(size))

    # primatives
    def readSByte(self): return int.from_bytes(self.f.read(1), 'little', signed=True)
    def readInt16(self): return int.from_bytes(self.f.read(2), 'little', signed=True)
    def readInt32(self): return int.from_bytes(self.f.read(4), 'little', signed=True)
    def readInt64(self): return int.from_bytes(self.f.read(8), 'little', signed=True)
    def readByte(self): return int.from_bytes(self.f.read(1), 'little', signed=False)
    def readUInt16(self): return int.from_bytes(self.f.read(2), 'little', signed=False)
    def readUInt32(self): return int.from_bytes(self.f.read(4), 'little', signed=False)
    def readUInt64(self): return int.from_bytes(self.f.read(8), 'little', signed=False)

    # string
    def readL8Encoding(self, encoding = None): return self.f.read(int.from_bytes(self.f.read(1), 'little')).decode('ascii' if not encoding else encoding)
    # def readL16Encoding(self, encoding = None): return self.f.read(int.from_bytes(self.f.read(2), 'little')).decode('ascii' if not encoding else encoding)
    def readL32Encoding(self, encoding = None): return self.f.read(int.from_bytes(self.f.read(4), 'little')).decode('ascii' if not encoding else encoding)

    # compression
    def decompressZlib(self, length, newLength): return None
    def decompressLzss(self, length, newLength): return None