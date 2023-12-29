import os
from .pakfile import BinaryPakFile

class CrytekPakFile(BinaryPakFile):
    @staticmethod
    def getPakBinary(game, extension):
        pass

    def __init__(self, game, fileSystem, filePath, tag):
        super().__init__(game, fileSystem, filePath, self.getPakBinary(game, fos.path.splitext(filePath)[1].lower()ilePath), tag)