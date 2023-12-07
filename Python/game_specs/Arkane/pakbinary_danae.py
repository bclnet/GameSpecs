import os
from struct import unpack
from ..pakbinary import FileInfo, PakBinary
from ..pakfile import BinaryPakFile

class PakBinary_Danae(PakBinary):
    F1_HEADER_FILEID = 0x000000001
    F2_HEADER_FILEID = 0x000000011

    _instance = None
    def __new__(cls):
        if cls._instance is None: cls._instance = super().__new__(cls)
        return cls._instance
    def read(self, source: BinaryPakFile, r, tag = None):
        gameId = source.game.id
        
        # fallout
        if gameId == 'Fallout':
            source.magic = self.F1_HEADER_FILEID
            header = unpack('>IIII', r.read(16))
            directoryNames = [readL8Encoding(r) for x in range(0, header[0])]

            # create file metadatas
            source.files = files = []
            for i in range(0, header[0]):
                contentBlock = unpack('>IIII', r.read(16))
                directoryPrefix = f'{directoryNames[i]}\\' if directoryNames[i] != '.' else ''
                for _ in range(0, contentBlock[0]):
                    path = directoryPrefix + readL8Encoding(r)
                    block = unpack('>IIII', r.read(16))
                    files.append(FileInfo(
                        path = path,
                        compression = block[0] & 0x40,
                        position = block[1],
                        fileSize = block[2],
                        packedSize = block[3]))
        elif gameId == 'Fallout2':
            source.magic = self.F2_HEADER_FILEID
            r.seek(getLength(r) - 8)
            header = unpack('=II', r.read(8))
            r.seek(header[1] - header[0] - 8)

            # create file metadatas
            source.files = files = []
            filenum = readInt32(r)
            for i in range(0, filenum):
                path = readL32Encoding(r)
                block = unpack('=BIII', r.read(13))
                files.append(FileInfo(
                    path = path,
                    compression = block[0],
                    fileSize = block[1],
                    packedSize = block[2],
                    position = block[3]))

def getLength(r):
    prev = r.tell(); length = r.seek(0, os.SEEK_END); r.seek(prev)
    return length

def readInt32(r):
    return int.from_bytes(r.read(4), 'little')

def readL8Encoding(r, encoding = None):
    return r.read(int.from_bytes(r.read(1))).decode('ascii' if not encoding else encoding)

def readL32Encoding(r, encoding = None):
    return r.read(int.from_bytes(r.read(4), 'little')).decode('ascii' if not encoding else encoding)