import os
from .pakfile import BinaryPakFile
from .util import _pathExtension

class MonolithPakFile(BinaryPakFile):
    @staticmethod
    def getPakBinary(game, extension):
        pass

    def __init__(self, game, fileSystem, filePath, tag):
        super().__init__(game, fileSystem, filePath, self.getPakBinary(game, _pathExtension(filePath).lower()), tag)