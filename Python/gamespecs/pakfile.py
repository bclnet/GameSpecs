import sys, os, re, time
from typing import Callable
from enum import Enum, Flag
from io import BytesIO
from openstk.poly import Reader
from .filesrc import FileSource
from .metamgr import MetaManager, MetaItem
from .util import _throw

# typedef
class FamilyGame: pass
class IFileSystem: pass
class PakBinary: pass
class MetaInfo: pass

# FileOption
class FileOption(Flag):
    Default = 0x0
    Supress = 0x10

# PakFile
class PakFile:
    class FuncObjectFactoryFactory: pass
    class PakStatus(Enum):
        Opening = 1
        Opened = 2
        Closing = 3
        Closed = 4

    def __init__(self, fileSystem: IFileSystem, game: FamilyGame, edition: Edition, name: str, tag: object = None):
        self.status = self.PakStatus.Closed
        self.fileSystem = fileSystem
        self.family = game.family
        self.game = game
        self.edition = edition
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
    def open(self, items: list[MetaItem] = None, manager: MetaManager = None) -> None:
        if self.status != self.PakStatus.Closed: return self
        self.status = self.PakStatus.Opening
        start = time.time()
        self.opening()
        end = time.time()
        self.status = self.PakStatus.Opened
        elapsed = round(end - start, 4)
        if items != None:
            for item in self.getMetaItems(manager): items.append(item)
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
    def getMetaInfos(self, manager: MetaManager, item: MetaItem) -> list[MetaItem]: raise NotImplementedError()
    def getMetaItems(self, manager: MetaManager) -> list[MetaInfo]: raise NotImplementedError()
    #endregion

# BinaryPakFile
class BinaryPakFile(PakFile):
    def __init__(self, fileSystem: IFileSystem, game: FamilyGame, edition: Edition, filePath: str, pakBinary: PakBinary, tag: object = None):
        name = os.path.basename(filePath) if os.path.basename(filePath) else \
            os.path.basename(os.path.dirname(filePath))
        super().__init__(fileSystem, game, edition, name, tag)
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

    def getReader(self, path: str = None, retainInPool: int = 10) -> Reader:
        path = path or self.filePath
        if not self.fileSystem.fileExists(path): return None
        return self.fileSystem.openReader(path)

    def valid(self) -> bool: return self.files != None
    
    def opening(self) -> None:
        if self.useReader and (ctx := self.getReader()):
            with ctx as r: self.read(r)
        else: self.read(None)
        self.process()

    def contains(self, path: FileSource | str | int) -> bool:
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str):
                pak, s2 = self._findSubPak(s)
                return pak.contains(s2) if pak else \
                    s.replace('\\', '/') in self.filesByPath if self.filesByPath else None
            case i if isinstance(path, int):
                return i in self.filesById if self.filesById else None
            case _: raise Exception(f'Unknown: {path}')

    def loadFileData(self, path: FileSource | str | int, option: FileOption = FileOption.Default) -> bytes:
        match path:
            case None: raise Exception('Null')
            case f if isinstance(path, FileSource):
                if self.useReader and (ctx := self.getReader()):
                    with ctx as r: return self.readData(r, f)
                else: return self.readData(None, f)
            case s if isinstance(path, str):
                pak, s2 = self._findSubPak(s)
                return pak.loadFileData(s2) if pak else \
                    self.loadFileData(file) if self.filesByPath and (s := s.replace('\\', '/')) in self.filesByPath and (file := self.filesByPath[s]) else None
            case i if isinstance(path, int):
                return self.loadFileData(file) if self.filesById and i in self.filesById and (file := self.filesById[i]) else None
            case _: raise Exception(f'Unknown: {path}')

    def loadFileObject(self, type: type, path: FileSource | str | int, option: FileOption = FileOption.Default) -> object:
        match path:
            case None: raise Exception('Null')
            case f if isinstance(path, FileSource):
                data = self.loadFileData(f, option)
                if not data: return None
                objectFactory = self._ensureCachedObjectFactory(f)
                if objectFactory != FileSource.emptyObjectFactory:
                    r = Reader(data)
                    try:
                        task = objectFactory(r, f, self)
                        if task:
                            value = task
                            return value
                    except: print(sys.exc_info()[1]); raise
                    return data if type == BytesIO or type == object else \
                        _throw(f'Stream not returned for {f.path} with {type}')
            case s if isinstance(path, str):
                pak, s2 = self._findSubPak(s)
                return pak.loadFileData(s2) if pak else \
                    self.loadFileData(file) if self.filesByPath and (s := s.replace('\\', '/')) in self.filesByPath and (file := self.filesByPath[s]) else None
            case i if isinstance(path, int):
                return self.loadFileData(file) if self.filesById and i in self.filesById and (file := self.filesById[i]) else None
            case _: raise Exception(f'Unknown: {path}')

    def _ensureCachedObjectFactory(self, file: FileSource) -> Callable:
        if file.cachedObjectFactory: return file.cachedObjectFactory
        option, factory = self.objectFactoryFactoryMethod(file, self.game)
        file.cachedObjectOption = option
        file.cachedObjectFactory = factory or FileSource.emptyObjectFactory
        return file.cachedObjectFactory

    def process(self) -> None:
        if self.files and self.useFileId: self.filesById = { x.id:x for x in self.files if x }
        if self.files: self.filesByPath = { x.path:x for x in self.files if x }
        if self.pakBinary: self.pakBinary.process(self)

    def _findSubPak(self, path: str) -> (object, str):
        paths = path.split(':', 2)
        p = paths[0].replace('\\', '/')
        pak = self.filesByPath[p].pak if len(paths) > 1 and p in self.filesByPath else None
        return pak, (paths[1] if pak else None)

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

# ManyPakFile
class ManyPakFile(BinaryPakFile):
    def __init__(self, basis: PakFile, fileSystem: IFileSystem, game: FamilyGame, edition: Edition, name: str, paths: list[str], tag: object = None, pathSkip: int = 0):
        super().__init__(fileSystem, game, edition, name, None, tag)
        self.pathSkip = pathSkip
        if isinstance(basis, BinaryPakFile):
            self.getObjectFactoryFactory = basis.getObjectFactoryFactory
        self.paths = paths
        self.useReader = False

    #region PakBinary
    def read(self, r: Reader, tag: object = None) -> None:
        self.files = [FileSource(
            path = s.replace('\\', '/'),
            pak = self.game.createPakFileType(self.fileSystem, self.edition, s) if self.game.isPakFile(s) else None,
            fileSize = self.fileSystem.fileInfo(s).st_size
            )
            for s in self.paths]

    def readData(self, r: Reader, file: FileSource) -> BytesIO:
        return None if file.pak else \
            BytesIO(self.fileSystem.openReader(file.path).read(file.fileSize))
    #endregion

# MultiPakFile
class MultiPakFile(PakFile):
    def __init__(self, fileSystem: IFileSystem, game: FamilyGame, edition: Edition, name: str, pakFiles: list[PakFile], tag: object = None):
        super().__init__(fileSystem, game, edition, name, tag)
        self.pakFiles = pakFiles

    def closing(self):
        for pakFile in self.pakFiles: pakFile.close()

    def opening(self):
        for pakFile in self.pakFiles: pakFile.open()

    def contains(path: object) -> bool:
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str):
                paks, s2 = self._filterPakFiles(s)
                return any(x.valid() and x.contains(s2) for x in paks)
            case i if isinstance(path, int): return any(x.valid() and x.contains(i) in self.pakFiles)
            case _: raise Exception(f'Unknown: {path}')

    def _findPakFiles(self, path: str) -> (list[PakFile], str):
        paths = re.split('\\\\|/|:', path, 1)
        if len(paths) == 1: return self.pakFiles, path
        path, nextPath = paths
        pakFiles = [x for x in self.pakFiles if x.name.startswith(path)]
        for pakFile in pakFiles: pakFile.open()
        return pakFiles, nextPath

    def loadFileData(self, path: FileSource | str | int, option: FileOption = FileOption.Default) -> bytes:
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str):
                pakFiles, s2 = self._findPakFiles(s)
                value = next(iter([x for x in pakFiles if x.valid() and x.contains(s2)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.loadFileData(s2, option)
            case i if isinstance(path, int):
                value = next(iter([x for x in self.pakFiles if x.valid() and x.contains(i)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.loadFileData(i, option)
            case _: raise Exception(f'Unknown: {path}')

    def loadFileObject(self, path: FileSource | str | int) -> object:
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str):
                pakFiles, s2 = self._findPakFiles(s)
                value = next(iter([x for x in pakFiles if x.valid() and x.contains(s2)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.loadFileObject(s2, option)
            case i if isinstance(path, int):
                value = next(iter([x for x in self.pakFiles if x.valid() and x.contains(i)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.loadFileObject(i, option)
            case _: raise Exception(f'Unknown: {path}')

    #region Metadata
    def getMetaItems(self, manager: MetaManager) -> list[MetaInfo]:
        root = []
        for pakFile in [x for x in self.pakFiles if x.valid()]:
            root.append(MetaItem(pakFile, pakFile.name, manager.packageIcon, pakFile = pakFile, items = pakFile.getMetaItems(manager)))
        return root
    #endregion

# PakBinary
class PakBinary:
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None: pass
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource, option: FileOption = None): pass
    def process(self, source: BinaryPakFile): pass
    def handleException(self, source: object, option: FileOption, message: str):
        print(message)
        if (option & FileOption.Supress) != 0: raise Exception(message)

# PakBinaryT
class PakBinaryT(PakBinary):
    _instance = None
    def __new__(cls):
        if cls._instance is None: cls._instance = super().__new__(cls)
        return cls._instance

    class SubPakFile(BinaryPakFile):
        def __init__(self, parent: PakBinary, file: FileSource, source: BinaryPakFile, fileSystem: IFileSystem, game: FamilyGame, edition: Edition, filePath: str, tag: object = None):
            super().__init__(fileSystem, game, edition, filePath, parent._instance, tag)
            self.file = file
            self.source = source
            self.objectFactoryFactoryMethod = source.objectFactoryFactoryMethod
            self.useReader = file == None
            # self.open()

        def read(self, r: Reader, tag: object = None):
            if self.useReader: super().read(r, tag); return
            with Reader(self.readData(self.source.getReader(), self.file)) as r2:
                self.pakBinary.read(self, r2, tag)
            