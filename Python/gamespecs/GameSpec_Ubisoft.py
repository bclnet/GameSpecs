from .pakfile import BinaryPakFile

class UbisoftPakFile(BinaryPakFile):
    @staticmethod
    def getPakBinary(game, filePath):
        pass

    def __init__(self, game, fileSystem, filePath, tag):
        super().__init__(game, fileSystem, filePath, self.getPakBinary(game, filePath), tag)