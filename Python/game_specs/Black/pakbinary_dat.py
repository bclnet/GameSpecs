import os
from io import BytesIO
from ..pakbinary import PakBinary
from ..pakfile import FileSource, BinaryPakFile
from ..reader import Reader

class PakBinary_Dat(PakBinary):
    F1_HEADER_FILEID = 0x000000001
    F2_HEADER_FILEID = 0x000000011

    _instance = None
    def __new__(cls):
        if cls._instance is None: cls._instance = super().__new__(cls)
        return cls._instance

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        gameId = source.game.id
        # Fallout
        if gameId == 'Fallout':
            source.magic = self.F1_HEADER_FILEID
            h_directoryCount, h_unknown1, h_unknown2, h_unknown3 = r.readT('>IIII', 16)
            directoryNames = [r.readL8Encoding() for x in range(0, h_directoryCount)]

            # create file metadatas
            source.files = files = []
            for i in range(0, h_directoryCount):
                cb_fileCount, cb_unknown1, cb_unknown2, cb_unknown3 = r.readT('>IIII', 16)
                directoryPrefix = f'{directoryNames[i]}\\' if directoryNames[i] != '.' else ''
                for _ in range(0, cb_fileCount):
                    path = directoryPrefix + r.readL8Encoding()
                    bk_compressed, bk_position, bk_fileSize, bk_packedSize = r.readT('>IIII', 16)
                    files.append(FileSource(
                        path = path,
                        compressed = bk_compressed & 0x40,
                        position = bk_position,
                        fileSize = bk_fileSize,
                        packedSize = bk_packedSize))
        
        # Fallout2
        elif gameId == 'Fallout2':
            source.magic = self.F2_HEADER_FILEID
            r.seek(r.length() - 8)
            h_treeSize, h_dataSize = r.readT('=II', 8)
            r.seek(h_dataSize - h_treeSize - 8)

            # create file metadatas
            source.files = files = []
            filenum = r.readInt32()
            for i in range(0, filenum):
                path = r.readL32Encoding()
                bk_compressed, bk_fileSize, bk_packedSize, bk_position = r.readT('=BIII', 13)
                files.append(FileSource(
                    path = path,
                    compressed = bk_compressed,
                    fileSize = bk_fileSize,
                    packedSize = bk_packedSize,
                    position = bk_position))

    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        magic = source.magic
        # F1
        if magic == self.F1_HEADER_FILEID:
            r.seek(file.position)
            return BytesIO(
                r.read(file.packedSize) if file.compressed == 0 else \
                r.decompressLzss(file.packedSize, file.fileSize))
        # F2
        elif magic == self.F2_HEADER_FILEID:
            r.seek(file.position)
            return BytesIO(
                r.decompressZlib(file.packedSize, -1) if r.peek(lambda z : z.readUInt16()) == 0xda78 else \
                r.read(file.packedSize))
        else: raise Exception('BAD MAGIC')
