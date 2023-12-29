import os
from io import BytesIO

DICT_SIZE = 4096
MIN_MATCH = 3
MAX_MATCH = 18

class Lzss:
    def __init__(self, stream: BytesIO, uncompressedSize: int):
        self.stream = stream
        self.uncompressedSize = uncompressedSize
        self.streamLength = stream.seek(0, os.SEEK_END)
        stream.seek(0, os.SEEK_SET)
        self.out = bytearray()
        self.dict = bytearray()
        self.NR = 0
        self.DO = 0
        self.DI = 0
        self.OI = 0
    
    def _readByte(self) -> int: self.NR += 1; return int.from_bytes(self.stream.read(1), 'big', signed=False)
    def _readInt16(self) -> int: return int.from_bytes(self.stream.read(2), 'big', signed=True)
    def _readDict(self) -> int: ret = self.dict[self.DO % DICT_SIZE]; self.DO+=1; return ret
    def _lastByte(self) -> bool: return self.stream.tell() == self.streamLength
    def _readBlock(self, N):
        s = self.stream
        self.NR = 0
        if N < 0: self._writeBytes(s.read(N * -1))
        else:
            self._clearDict()
            while self.NR < N and not self._lastByte():
                num1 = self._readByte()
                if self.NR >= N or self._lastByte(): break
                for index1 in range(8):
                    if (num1 % 2) == 1:
                        self._writeByte(self._readByte())
                        if self.NR >= N: return
                    else:
                        if self.NR >= N: return
                        self.DO = self._readByte()
                        if self.NR >= N: return
                        num2 = self._readByte()
                        self.DO |= ((num2 & 240) << 4) & 0xFFFF
                        num3 = (num2 & 15) + MIN_MATCH
                        for index2 in range(num3): self._writeByte(self._readDict())
                    num1 >>= 1
                    if self._lastByte(): return
    def _clearDict(self) -> None:
         for index in range(DICT_SIZE): self.dict[index] = 32
         self.DI = DICT_SIZE - MAX_MATCH
    def _writeByte(self, b) -> None: 
        self.out[self.OI] = b; self.OI += 1
        self.dict[self.DI % DICT_SIZE] = b; self.DI += 1
    def _writeBytes(self, b) -> None:
        for num in b:
            if self.OI >= self.uncompressedSize: break
            self.out[self.OI] = num; self.OI += 1
    def decompress(self) -> bytearray:
        self.out = bytearray(self.uncompressedSize)
        self.dict = bytearray(DICT_SIZE)
        while not self._lastByte():
            N = self._readInt16()
            if N != 0: self._readBlock(N)
            else: break
        return self.out