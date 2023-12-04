import os
from .pakfile import BinaryPakFile

class FileInfo:
    def __init__(self, path, compression = None, position = None, fileSize = None, packedSize = None):
        self.path = path
        self.compression = compression
        self.position = position
        self.fileSize = fileSize
        self.packedSize = packedSize
    def __repr__(self): return f'{self.path}:{self.fileSize}'

class PakBinary:
    def read(self, source: BinaryPakFile, r, tag = None):
        pass
    def readData(self, source: BinaryPakFile, r, tag = None):
        pass
    def process(self):
        pass
