import os, time
from enum import Enum
from io import BytesIO
from .openstk_poly import Reader
from .metadata import StandardMetadataItem, MetadataManager, MetadataItem, MetadataInfo, MetadataContent

class FileSource:
    def __init__(self, id = None, path = None, compressed = None, position = None, fileSize = None, packedSize = None, pak = None, tag = None):
        self.id = id
        self.path = path
        self.compressed = compressed
        self.position = position
        self.fileSize = fileSize
        self.packedSize = packedSize
        self.pak = pak
        self.tag = tag
    def __repr__(self): return f'{self.path}:{self.fileSize}'

class PakFile:
    class PakStatus(Enum):
        Opening = 1
        Opened = 2
        Closing = 3
        Closed = 4
    def __init__(self, game, name, tag = None):
        self.status = self.PakStatus.Closed
        self.family = game.family
        self.game = game
        self.name = name
        self.tag = tag
        self.graphic = None
    def __enter__(self): return self
    def __exit__(self, type, value, traceback): self.close()
    def __repr__(self): return f'{self.name}#{self.game.id}'
    def valid(self): return True
    def close(self):
        self.status = self.PakStatus.Closing
        self.closing()
        self.status = self.PakStatus.Closed
        return self
    def closing(self): pass
    def open(self, items = None, manager = None):
        if self.status != self.PakStatus.Closed: return self
        self.status = self.PakStatus.Opening
        start = time.time()
        self.opening()
        end = time.time()
        self.status = self.PakStatus.Opened
        elapsed = round(end - start, 4)
        # if items: items[]
        print(f'Opened: {self.name} @ {elapsed}ms')
        return self
    def opening(self): pass
    def loadFileData(self, path): pass

    #region Metadata

    def getMetadataFilters(self, manager: MetadataManager) -> list[MetadataItem.Filter]:
        return [MetadataItem.Filter(
            key = x.key,
            value = x.value
            ) for x in self.family.fileManager.filters[self.game.id].items()] \
            if self.family.fileManager and self.game.id in self.family.fileManager.filters else None
    def getMetadataInfos(self, manager: MetadataManager, item: MetadataItem) -> list[MetadataItem]: raise Exception('Not Implemented')
    def getMetadataItems(self, manager: MetadataManager) -> list[MetadataInfo]: raise Exception('Not Implemented')

    #endregion

class BinaryPakFile(PakFile):
    def __init__(self, game, fileSystem, filePath: str, pakBinary, tag: object = None):
        name = os.path.basename(filePath) if os.path.basename(filePath) else \
            os.path.basename(os.path.dirname(filePath))
        super().__init__(game, name, tag)
        self.fileSystem = fileSystem
        self.filePath = filePath
        self.pakBinary = pakBinary
        # options
        self.useReader = True
        self.useFileId = False

        # state
        self.fileMask = None
        self.params = {}
        self.magic = None
        self.version = None
        # self.cryptKey

        # metadata/factory
        self.getMetadataItemsMethod = StandardMetadataItem.getPakFiles
        self.metadataInfos = {}
        self.getObjectFactoryFactory = None

        # BinaryPakManyFile
        self.files = {}
        self.filesById = {}
        self.filesByPath = {}
        self.pathSkip = 0

    def valid(self): return self.files
    def opening(self):
        if self.useReader:
            with self._getReader() as r: self.read(r)
        else: self.read(None)
        self.process()
    def _getReader(self): return Reader(self.fileSystem.openReader(self.filePath))
    def loadFileData(self, path: FileSource | str | int):
        # FileSource
        if isinstance(path, FileSource):
            if self.useReader:
                with self._getReader() as r: return self.readData(r, path)
            else: return self.readData(None, path)
        # str
        elif isinstance(path, str) and self.filesByPath:
            pak, nextPath = self.tryFindSubPak(path)
            if pak: return pak.loadFileData(nextPath)
            return self.loadFileData(file) if (path := path.replace('\\', '/')) in self.filesByPath and (file := self.filesByPath[path]) else None
        # int
        elif isinstance(path, int) and self.filesById:
            return self.loadFileData(file) if path in self.filesById and (file := self.filesById[path]) else None
        else: raise Exception(f'Unknown: {path}')
    def process(self):
        if self.files and self.useFileId: self.filesById = {x.id:x for x in self.files if x}
        if self.files: self.filesByPath = {x.path:x for x in self.files if x}
        if self.pakBinary: self.pakBinary.process()
    def read(self, r: Reader, tag: object = None): return self.pakBinary.read(self, r, tag)
    def readData(self, r: Reader, file: FileSource): return self.pakBinary.readData(self, r, file)
    def tryFindSubPak(self, path):
        paths = path.split(':', 2)
        p = paths[0].replace('\\', '/')
        pak = self.filesByPath[p].pak if len(paths) > 1 and p in self.filesByPath else None
        return (pak, paths[1]) if pak else (None, None)

    #region Metadata
    
    def getMetadataInfos(self, manager: MetadataManager, item: MetadataItem) -> list[MetadataInfo]:
        nodes = []
        nodes.append(MetadataInfo(None, MetadataContent(type='Hex',name='TEST',value=BytesIO())))
        return nodes

    def getMetadataItems(self, manager: MetadataManager) -> list[MetadataItem]:
        return self.getMetadataItemsMethod(manager, self) if self.valid() and self.getMetadataItemsMethod else None
    
    #endregion

class ManyPakFile(BinaryPakFile):
    def __init__(self, basis, game, name: str, fileSystem, paths, tag: object = None, visualPathSkip = 0):
        super().__init__(game, fileSystem, name, None, tag)
        if isinstance(basis, BinaryPakFile):
            self.getMetadataItemsMethod = basis.getMetadataItemsMethod
            self.getObjectFactoryFactory = basis.getObjectFactoryFactory
            pass
        self.paths = paths
        self.useReader = False
    def read(self, r: Reader, tag: object = None):
        self.files = [FileSource(
            path = s.replace('\\', '/'),
            pak = self.game.createPakFileType(self.fileSystem, s) if self.game.isPakFile(s) else None,
            fileSize = self.fileSystem.fileInfo(s).st_size
            )
            for s in self.paths]

    def readData(self, r: Reader, file: FileSource):
        if file.pak: return None
        return BytesIO(self.fileSystem.openReader(file.path).read(file.fileSize))

class MultiPakFile(PakFile):
    def __init__(self, game, name: str, fileSystem, pakFiles, tag: object = None):
        super().__init__(game, name, tag)
        self.pakFiles = pakFiles

    def closing(self):
        for pakFile in self.pakFiles: pakFile.close()

    def opening(self):
        for pakFile in self.pakFiles: pakFile.open()

    #region Metadata

    def getMetadataItems(self, manager: MetadataManager) -> list[MetadataInfo]:
        root = []
        for pakFile in [x for x in self.pakFiles if x.valid()]:
            root.append(MetadataItem(pakFile, pakFile.name, manager.packageIcon, pakFile=pakFile, items=pakFile.getMetadataItems(manager)))
        return root

    #endregion
