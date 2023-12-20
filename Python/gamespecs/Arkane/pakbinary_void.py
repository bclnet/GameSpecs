import os
from io import BytesIO
from typing import Any
from ..pakbinary import PakBinary
from ..pakfile import FileSource, BinaryPakFile
from ..familymgr import FamilyGame
from ..filesys import FileSystem
from ..openstk_poly import Reader


class PakBinary_Void(PakBinary):

    class SubPakFile(BinaryPakFile):
        def __init__(self, game: FamilyGame, fileSystem: FileSystem, filePath: str, tag: Any = None):
            super().__init__(game, fileSystem, filePath, PakBinary_Void(), tag)
            self.open()

    _instance = None
    def __new__(cls):
        if cls._instance is None: cls._instance = super().__new__(cls)
        return cls._instance

    class V_File:
        struct = ('>QIIIIH', 26)
        def __init__(self, tuple):
            self.position, \
            self.fileSize, \
            self.packedSize, \
            self.unknown1, \
            self.flags, \
            self.flags2 = tuple

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: Any = None) -> None:
        # must be .index file
        if os.path.splitext(source.filePath)[1] != '.index':
            raise Exception('must be a .index file')
        source.files = files = []

        # master.index file
        if source.filePath == 'master.index':
            RES_MAGIC = 0x04534552
            SubMarker = 0x18000000
            EndMarker = 0x01000000
            
            magic = r.readUInt32E()
            if magic != RES_MAGIC:
                raise Exception('BAD MAGIC')
            r.skip(4)
            first = True
            while True:
                pathSize = r.readUInt32()
                if pathSize == SubMarker: first = False; pathSize = r.readUInt32()
                elif pathSize == EndMarker: break
                path = r.readFString(pathSize).replace('\\', '/')
                packId = 0 if first else r.readUInt16()
                if not path.endswith('.index'): continue
                files.append(FileSource(
                    path = path,
                    pak = self.SubPakFile(source.game, source.fileSystem, path)
                    ))
            return

        # find files
        fileSystem = source.fileSystem
        resourcePath = f'{source.filePath[:-6]}.resources'
        if not fileSystem.fileExists(resourcePath):
            raise Exception('Unable to find resources extension')
        sharedResourcePath = next((x for x in ['shared_2_3.sharedrsc',
            'shared_2_3_4.sharedrsc',
            'shared_1_2_3.sharedrsc',
            'shared_1_2_3_4.sharedrsc'] if fileSystem.fileExists(x)), None)
        source.files = files = []
        r.seek(4)
        mainFileSize = r.readUInt32E()
        r.skip(24)
        numFiles = r.readUInt32E()
        for _ in range(numFiles):
            id = r.readUInt32E()
            tag1 = r.readL32Encoding()
            tag2 = r.readL32Encoding()
            path = (r.readL32Encoding() or '').replace('\\', '/')
            file = r.readT(self.V_File)
            useSharedResources = (file.flags & 0x20) != 0 and file.flags2 == 0x8000
            if useSharedResources and not sharedResourcePath:
                raise Exception('sharedResourcePath not available')
            newPath = sharedResourcePath if useSharedResources else resourcePath
            files.append(FileSource(
                id = id,
                path = path,
                compressed = 1 if file.fileSize != file.packedSize else 0,
                fileSize = file.fileSize,
                packedSize = file.packedSize,
                position = file.position,
                tag = (newPath, tag1, tag2)
                ))

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        pass
