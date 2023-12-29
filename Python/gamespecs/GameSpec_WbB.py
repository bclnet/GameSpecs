import os
from .familymgr import FamilyGame
from .pakfile import BinaryPakFile

class WbBGame(FamilyGame):
    def __init__(self, family, id, elem, dgame):
        super().__init__(family, id, elem, dgame)

class WbBPakFile(BinaryPakFile):
    @staticmethod
    def getPakBinary(game, extension):
        pass

    def __init__(self, game, fileSystem, filePath, tag):
        super().__init__(game, fileSystem, filePath, self.getPakBinary(game, os.path.splitext(filePath)[1].lower()), tag)
