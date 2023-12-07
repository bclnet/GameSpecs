import os
from enum import Enum

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

class BinaryPakFile(PakFile):
    def __init__(self, game, fileSystem, filePath, pakBinary, tag = None):
        super().__init__(game, os.path.basename(filePath) if os.path.basename(filePath) else os.path.basename(os.path.dirname(filePath)), tag)
        self.fileSystem = fileSystem
        self.filePath = filePath
        self.pakBinary = pakBinary
        self.reader = True
    def opening(self):
        if self.reader:
            with self.fileSystem.open(self.filePath, 'rb') as f: self.pakBinary.read(self, f)
        else: self.pakBinary.read(self, None)
        self.process()
    def process(self):
        if self.pakBinary: self.pakBinary.process()

class BinaryPakManyFile(BinaryPakFile):
    def __init__(self, game, fileSystem, filePath, pakBinary, tag = None):
        super().__init__(game, fileSystem, filePath, pakBinary, tag)

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
