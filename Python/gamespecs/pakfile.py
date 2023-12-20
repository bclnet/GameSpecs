import os
from enum import Enum
from .openstk_poly import Reader

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
        OPENING = 1
        OPENED = 2
        CLOSING = 3
        CLOSED = 4
    def __init__(self, game, name, tag = None):
        self.family = game.family
        self.game = game
        self.name = name
        self.tag = tag
    def __enter__(self): return self
    def __exit__(self, type, value, traceback): self.close()
    def __repr__(self): return f'{self.name}#{self.game.id}'
    def close(self):
        self.status = self.PakStatus.CLOSING
        self.closing()
        self.status = self.PakStatus.CLOSED
        return self
    def closing(self): pass
    def open(self):
        self.status = self.PakStatus.CLOSING
        self.opening()
        self.status = self.PakStatus.CLOSED
        return self
    def opening(self): pass
    def loadFileData(self, path): pass

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
        # self.fileMask = None
        self.params = {}
        self.magic = None
        self.version = None
        # self.cryptKey
    def opening(self):
        if self.useReader:
            with self._getReader() as r: self.read(r)
        else: self.read(None)
        self.process()
    def _getReader(self): return Reader(self.fileSystem.open(self.filePath, 'rb'))
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

class ManyPakFile(BinaryPakFile):
    def __init__(self, basis, game, name: str, fileSystem, paths, tag: object = None, visualPathSkip = 0):
        super().__init__(game, fileSystem, name, None, tag)
        self.basis = basis
    def opening(self): return f'opening'
    def closing(self): return f'closing'

class MultiPakFile(PakFile):
    def __init__(self, game, name: str, fileSystem, pakFiles, tag: object = None):
        super().__init__(game, name, tag)
    def opening(self): return f'opening'
    def closing(self): return f'closing'
