import os
from .pakfile import BinaryPakFile

class FrontierPakFile(BinaryPakFile):
    @staticmethod
    def getPakBinary(game, extension):
        pass

    def __init__(self, game, fileSystem, filePath, tag):
        super().__init__(game, fileSystem, filePath, self.getPakBinary(game, os.path.splitext(filePath)[1].lower()), tag)