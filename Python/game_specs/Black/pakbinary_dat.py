import os
from io import BytesIO
from typing import Any
from ..pakbinary import PakBinary
from ..pakfile import FileSource, BinaryPakFile
from ..utils import Reader
from ..compression import decompressLzss, decompressZlib

class PakBinary_Dat(PakBinary):
    F1_HEADER_FILEID = 0x000000001
    class F1_Header:
        struct = ('>IIII', 16)
        def __init__(self, tuple):
            self.directoryCount, \
            self.unknown1, \
            self.unknown2, \
            self.unknown3 = tuple
    class F1_Directory:
        struct = ('>IIII', 16)
        def __init__(self, tuple):
            self.fileCount, \
            self.unknown1, \
            self.unknown2, \
            self.unknown3 = tuple
    class F1_File:
        struct = ('>IIII', 16)
        def __init__(self, tuple):
            self.attributes, \
            self.offset, \
            self.size, \
            self.packedSize = tuple

    F2_HEADER_FILEID = 0x000000011
    class F2_Header:
        struct = ('=II', 8)
        def __init__(self, tuple):
            self.treeSize, \
            self.dataSize = tuple
    class F2_File:
        struct = ('=BIII', 13)
        def __init__(self, tuple):
            self.type, \
            self.realSize, \
            self.packedSize, \
            self.offset = tuple

    _instance = None
    def __new__(cls):
        if cls._instance is None: cls._instance = super().__new__(cls)
        return cls._instance

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: Any = None) -> None:
        gameId = source.game.id
        # Fallout
        if gameId == 'Fallout':
            source.magic = self.F1_HEADER_FILEID
            header = r.readT(self.F1_Header)
            directoryPaths = [r.readL8Encoding().replace('\\', '/') for x in range(0, header.directoryCount)]

            # create file metadatas
            source.files = files = []
            for i in range(0, header.directoryCount):
                directory = r.readT(self.F1_Directory)
                directoryPath = f'{directoryPaths[i]}/' if directoryPaths[i] != '.' else ''
                for _ in range(0, directory.fileCount):
                    path = directoryPath + r.readL8Encoding().replace('\\', '/')
                    file = r.readT(self.F1_File)
                    files.append(FileSource(
                        path = path,
                        compressed = file.attributes & 0x40,
                        position = file.offset,
                        fileSize = file.size,
                        packedSize = file.packedSize))
        
        # Fallout2
        elif gameId == 'Fallout2':
            source.magic = self.F2_HEADER_FILEID
            r.seek(r.length() - 8)
            header = r.readT(self.F2_Header)
            r.seek(header.dataSize - header.treeSize - 8)

            # create file metadatas
            source.files = files = []
            filenum = r.readInt32()
            for i in range(0, filenum):
                path = r.readL32Encoding().replace('\\', '/')
                file = r.readT(self.F2_File)
                files.append(FileSource(
                    path = path,
                    compressed = file.type,
                    fileSize = file.realSize,
                    packedSize = file.packedSize,
                    position = file.offset))

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        magic = source.magic
        # F1
        if magic == self.F1_HEADER_FILEID:
            r.seek(file.position)
            return BytesIO(
                r.read(file.packedSize) if file.compressed == 0 else \
                decompressLzss(r, file.packedSize, file.fileSize))
        # F2
        elif magic == self.F2_HEADER_FILEID:
            r.seek(file.position)
            return BytesIO(
                decompressZlib(r, file.packedSize, -1) if r.peek(lambda z : z.readUInt16()) == 0xda78 else \
                r.read(file.packedSize))
        else: raise Exception('BAD MAGIC')
