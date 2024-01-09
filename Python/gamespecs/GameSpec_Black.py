import os
from .pakfile import BinaryPakFile
from .Black.pakbinary_dat import PakBinary_Dat
from .util import _pathExtension

class BlackPakFile(BinaryPakFile):
    @staticmethod
    def getPakBinary(game, extension):
        return PakBinary_Dat()

    def __init__(self, game, fileSystem, filePath, tag):
        super().__init__(game, fileSystem, filePath, self.getPakBinary(game, _pathExtension(filePath).lower()), tag)