from . import pakfile
from .Black.pakbinary import PakBinaryBlackDat

class BlackPakFile(pakfile.BinaryPakManyFile):
    @staticmethod
    def getPakBinary(game, filePath):
        return PakBinaryBlackDat()

    def __init__(self, game, fileSystem, filePath, tag):
        super().__init__(game, fileSystem, filePath, self.getPakBinary(game, filePath), tag)
    def __repr__(self): return f'{self.game}'