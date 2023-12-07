import os
from enum import Enum
from .reader import Reader

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
    def __init__(self, game, fileSystem, filePath, pakBinary, tag = None):
        super().__init__(game, os.path.basename(filePath) if os.path.basename(filePath) else os.path.basename(os.path.dirname(filePath)), tag)
        self.fileSystem = fileSystem
        self.filePath = filePath
        self.pakBinary = pakBinary
        self.reader = True
    def opening(self):
        if self.reader:
            with self.fileSystem.open(self.filePath, 'rb') as f: self.pakBinary.read(self, Reader(f))
        else: self.pakBinary.read(self, None)
        self.process()
    def loadFileData(self, path):
        pass
    def process(self):
        if self.pakBinary: self.pakBinary.process()

class ManyPakFile(BinaryPakFile):
    def __init__(self, basis, game, name, fileSystem, paths, tag = None, visualPathSkip = 0):
        self.basis = basis
    def opening(self): return f'opening'
    def closing(self): return f'closing'

class MultiPakFile(PakFile):
    def __init__(self, game, name, fileSystem, pakFiles, tag = None):
        pass
    def opening(self): return f'opening'
    def closing(self): return f'closing'
