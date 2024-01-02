import os, time
from typing import Any
from enum import Enum, Flag
from io import BytesIO
from .openstk_poly import Reader
from .metadata import MetaManager, MetaManager, MetaItem, MetaInfo, MetaContent

class FileOption(Flag):
    Default = 0x0
    Supress = 0x10

class FileSource:
    EmptyObjectFactory = lambda a, b, c: None
    def __init__(self, id = None, path = None, compressed = None, position = None, fileSize = None, packedSize = None, crypted = None, hash = None, pak = None, tag = None):
        self.id = id
        self.path = path
        self.compressed = compressed
        self.position = position
        self.fileSize = fileSize
        self.packedSize = packedSize
        self.crypted = crypted
        self.hash = hash
        self.pak = pak
        self.tag = tag
        # cache
        self.cachedObjectFactory = None
        self.cachedOption = None
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
    def valid(self) -> bool: return True
    def close(self) -> None:
        self.status = self.PakStatus.Closing
        self.closing()
        self.status = self.PakStatus.Closed
        return self
    def closing(self) -> None: pass
    def open(self, items = None, manager = None) -> None:
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
    def opening(self) -> None: pass
    def contains(self, path: FileSource | str | int) -> bool: pass
    def loadFileData(self, path: FileSource | str | int, option: FileOption = FileOption.Default) -> bytes: pass
    def loadFileObject(self, path: FileSource | str | int) -> object: pass
    #region Transform
    def transformFileObject(self, transformTo: object, source: object): pass
    #endregion
    #region Metadata
    def getMetaFilters(self, manager: MetaManager) -> list[MetaItem.Filter]:
        return [MetaItem.Filter(
            key = x.key,
            value = x.value
            ) for x in self.family.fileManager.filters[self.game.id].items()] \
            if self.family.fileManager and self.game.id in self.family.fileManager.filters else None
    def getMetaInfos(self, manager: MetaManager, item: MetaItem) -> list[MetaItem]: raise Exception('Not Implemented')
    def getMetaItems(self, manager: MetaManager) -> list[MetaInfo]: raise Exception('Not Implemented')
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
        # metadata/factory
        self.metadataInfos = {}
        self.objectFactoryFactoryMethod = None
        # binary
        self.files = {}
        self.filesById = {}
        self.filesByPath = {}
        self.pathSkip = 0

    def _getReader(self) -> Reader: return Reader(self.fileSystem.openReader(self.filePath))

    def valid(self) -> bool: return self.files != None
    
    def opening(self) -> None:
        if self.useReader:
            with self._getReader() as r: self.read(r)
        else: self.read(None)
        self.process()

    def contains(self, path: FileSource | str | int) -> bool:
        if not path: raise Exception('Null')
        elif isinstance(path, str):
            pak, nextPath = self.tryFindSubPak(path)
            return pak.contains(nextPath) if pak else \
                s.replace('\\', '/') in self.filesByPath if self.filesByPath else None
        elif isinstance(path, int):
            return path in self.filesById if self.filesById else None
        else: raise Exception(f'Unknown: {path}')

    def loadFileData(self, path: FileSource | str | int, option: FileOption = FileOption.Default) -> bytes:
        if not path: raise Exception('Null')
        elif isinstance(path, FileSource):
            if self.useReader:
                with self._getReader() as r: return self.readData(r, path)
            else: return self.readData(None, path)
        elif isinstance(path, str):
            pak, nextPath = self.tryFindSubPak(path)
            return pak.loadFileData(nextPath) if pak else \
                self.loadFileData(file) if self.filesByPath and (path := path.replace('\\', '/')) in self.filesByPath and (file := self.filesByPath[path]) else None
        elif isinstance(path, int):
            return self.loadFileData(file) if self.filesById and path in self.filesById and (file := self.filesById[path]) else None
        else: raise Exception(f'Unknown: {path}')

    def loadFileObject(self, type: type, path: FileSource | str | int, option: FileOption = FileOption.Default) -> object:
        if not path: raise Exception('Null')
        elif isinstance(path, FileSource):
            data = self.loadFileData(path, option)
            if not data: return None
            objectFactory = self._ensureCachedObjectFactory(path)
            if objectFactory == FileSource.EmptyObjectFactory:
                # obj = data if type == typeof(Stream) || type == typeof(object) else None
                # raise Exception(f'Stream not returned for {path.path} with {type.Name}')
                # return obj
                pass
            r = Reader(data)
            task = objectFactory(r, path, self)
            # if not task:
            return 'BLA'

        elif isinstance(path, str):
            pak, nextPath = self.tryFindSubPak(path)
            return pak.loadFileData(nextPath) if pak else \
                self.loadFileData(file) if self.filesByPath and (path := path.replace('\\', '/')) in self.filesByPath and (file := self.filesByPath[path]) else None
        elif isinstance(path, int):
            return self.loadFileData(file) if self.filesById and path in self.filesById and (file := self.filesById[path]) else None
        else: raise Exception(f'Unknown: {path}')

    def _ensureCachedObjectFactory(self, file: FileSource):
        if file.cachedObjectFactory: return file.cachedObjectFactory
        option, factory = self.objectFactoryFactoryMethod(file, self.game)
        file.cachedObjectOption = option
        file.cachedObjectFactory = factory or FileSource.EmptyObjectFactory
        return file.cachedObjectFactory

    def process(self) -> None:
        if self.files and self.useFileId: self.filesById = { x.id:x for x in self.files if x }
        if self.files: self.filesByPath = { x.path:x for x in self.files if x }
        if self.pakBinary: self.pakBinary.process()

    def tryFindSubPak(self, path) -> (object, str):
        paths = path.split(':', 2)
        p = paths[0].replace('\\', '/')
        pak = self.filesByPath[p].pak if len(paths) > 1 and p in self.filesByPath else None
        return pak, paths[1] if pak else None, None

    #region PakBinary
    def read(self, r: Reader, tag: object = None) -> None: return self.pakBinary.read(self, r, tag)

    def readData(self, r: Reader, file: FileSource) -> bytes: return self.pakBinary.readData(self, r, file)
    #endregion

    #region Metadata
    def getMetaInfos(self, manager: MetaManager, item: MetaItem) -> list[MetaInfo]:
        return MetaManager.getMetaInfos(manager, self, item.source if isinstance(item.source, FileSource) else None) if self.valid() else None

    def getMetaItems(self, manager: MetaManager) -> list[MetaItem]:
        return MetaManager.getMetaItems(manager, self) if self.valid() else None
    #endregion

class ManyPakFile(BinaryPakFile):
    def __init__(self, basis, game, name: str, fileSystem, paths, tag: object = None, visualPathSkip = 0):
        super().__init__(game, fileSystem, name, None, tag)
        if isinstance(basis, BinaryPakFile):
            self.getMetaItemsMethod = basis.getMetaItemsMethod
            self.getObjectFactoryFactory = basis.getObjectFactoryFactory
        self.paths = paths
        self.useReader = False

    #region PakBinary
    def read(self, r: Reader, tag: object = None):
        self.files = [FileSource(
            path = s.replace('\\', '/'),
            pak = self.game.createPakFileType(self.fileSystem, s) if self.game.isPakFile(s) else None,
            fileSize = self.fileSystem.fileInfo(s).st_size
            )
            for s in self.paths]

    def readData(self, r: Reader, file: FileSource):
        return None if file.pak else \
            BytesIO(self.fileSystem.openReader(file.path).read(file.fileSize))
    #endregion

class MultiPakFile(PakFile):
    def __init__(self, game, name: str, fileSystem, pakFiles, tag: object = None):
        super().__init__(game, name, tag)
        self.pakFiles = pakFiles

    def closing(self):
        for pakFile in self.pakFiles: pakFile.close()

    def opening(self):
        for pakFile in self.pakFiles: pakFile.open()

    def _filterPakFiles(path: str) -> (str, list[PakFile]):
        if not path.startswith('>'): return path, self.pakFiles
        path, nextPath = path[1:].split(':', 1)
        return [x for x in self.pakFiles if x.name.startswith(path)], nextPath

    def loadFileData(self, path: FileSource | str | int, option: FileOption = FileOption.Default) -> bytes:
        if not path: raise Exception('Null')
        elif isinstance(path, str):
            paks, nextPath = self._filterPakFiles(s)
            value = next(iter([x for x in paks if x.valid() and x.contains(nextPath)]), None)
            if not value: raise Exception(f'Could not find file {path}')
            return value.loadFileData(nextPath, option)
        elif isinstance(path, int):
            value = next(iter([x for x in self.pakFiles if x.valid() and x.contains(i)]), None)
            if not value: raise Exception(f'Could not find file {path}')
            return value.loadFileData(i, option)
        else: raise Exception(f'Unknown: {path}')

    def loadFileObject(self, path: FileSource | str | int) -> object:
        if not path: raise Exception('Null')
        elif isinstance(path, str):
            paks, nextPath = self._filterPakFiles(s)
            value = next(iter([x for x in paks if x.valid() and x.contains(nextPath)]), None)
            if not value: raise Exception(f'Could not find file {path}')
            return value.loadFileObject(nextPath, option)
        elif isinstance(path, int):
            value = next(iter([x for x in self.pakFiles if x.valid() and x.contains(i)]), None)
            if not value: raise Exception(f'Could not find file {path}')
            return value.loadFileObject(i, option)
        else: raise Exception(f'Unknown: {path}')

    #region Metadata
    def getMetaItems(self, manager: MetaManager) -> list[MetaInfo]:
        root = []
        for pakFile in [x for x in self.pakFiles if x.valid()]:
            root.append(MetaItem(pakFile, pakFile.name, manager.packageIcon, pakFile=pakFile, items=pakFile.getMetaItems(manager)))
        return root
    #endregion
