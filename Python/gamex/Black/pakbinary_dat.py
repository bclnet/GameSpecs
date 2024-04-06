import os
from io import BytesIO
from gamex.filesrc import FileSource
from gamex.pak import PakBinaryT
from gamex.compression import decompressLzss, decompressZlib

# typedefs
class Reader: pass
class BinaryPakFile: pass

# PakBinary_Dat
class PakBinary_Dat(PakBinaryT):

    #region F1/F2

    F1_HEADER_FILEID = 0x000000001
    class F1_Header:
        struct = ('>4I', 16)
        def __init__(self, tuple):
            self.directoryCount, \
            self.unknown1, \
            self.unknown2, \
            self.unknown3 = tuple
    class F1_Directory:
        struct = ('>4I', 16)
        def __init__(self, tuple):
            self.fileCount, \
            self.unknown1, \
            self.unknown2, \
            self.unknown3 = tuple
    class F1_File:
        struct = ('>4I', 16)
        def __init__(self, tuple):
            self.attributes, \
            self.offset, \
            self.size, \
            self.packedSize = tuple

    F2_HEADER_FILEID = 0x000000011
    class F2_Header:
        struct = ('<2I', 8)
        def __init__(self, tuple):
            self.treeSize, \
            self.dataSize = tuple
    class F2_File:
        struct = ('<B3I', 13)
        def __init__(self, tuple):
            self.type, \
            self.realSize, \
            self.packedSize, \
            self.offset = tuple

    #endregion

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        gameId = source.game.id

        # Fallout
        if gameId == 'Fallout':
            source.magic = self.F1_HEADER_FILEID
            header = r.readS(self.F1_Header)
            directoryPaths = [r.readL8Encoding().replace('\\', '/') for x in range(header.directoryCount)]

            # create file metadatas
            source.files = files = []
            for i in range(header.directoryCount):
                directory = r.readS(self.F1_Directory)
                directoryPath = f'{directoryPaths[i]}/' if directoryPaths[i] != '.' else ''
                for _ in range(directory.fileCount):
                    path = directoryPath + r.readL8Encoding().replace('\\', '/')
                    file = r.readS(self.F1_File)
                    files.append(FileSource(
                        path = path,
                        compressed = file.attributes & 0x40,
                        offset = file.offset,
                        fileSize = file.size,
                        packedSize = file.packedSize
                        ))
        
        # Fallout2
        elif gameId == 'Fallout2':
            source.magic = self.F2_HEADER_FILEID
            r.seek(r.length() - 8)
            header = r.readS(self.F2_Header)
            r.seek(header.dataSize - header.treeSize - 8)

            # create file metadatas
            source.files = files = []
            filenum = r.readInt32()
            for i in range(filenum):
                path = r.readL32Encoding().replace('\\', '/')
                file = r.readS(self.F2_File)
                files.append(FileSource(
                    path = path,
                    compressed = file.type,
                    fileSize = file.realSize,
                    packedSize = file.packedSize,
                    offset = file.offset
                    ))

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        magic = source.magic
        # F1
        if magic == self.F1_HEADER_FILEID:
            r.seek(file.offset)
            return BytesIO(
                r.read(file.packedSize) if file.compressed == 0 else \
                decompressLzss(r, file.packedSize, file.fileSize)
                )
        # F2
        elif magic == self.F2_HEADER_FILEID:
            r.seek(file.offset)
            return BytesIO(
                decompressZlib(r, file.packedSize, -1) if r.peek(lambda z : z.readUInt16()) == 0xda78 else \
                r.read(file.packedSize)
                )
        else: raise Exception('BAD MAGIC')
