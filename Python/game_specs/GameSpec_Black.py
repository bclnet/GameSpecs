from .pakfile import BinaryPakManyFile
from .Black.pakbinary_dat import PakBinary_Dat

class BlackPakFile(BinaryPakManyFile):
    @staticmethod
    def getPakBinary(game, filePath):
        return PakBinary_Dat()

    def __init__(self, game, fileSystem, filePath, tag):
        super().__init__(game, fileSystem, filePath, self.getPakBinary(game, filePath), tag)