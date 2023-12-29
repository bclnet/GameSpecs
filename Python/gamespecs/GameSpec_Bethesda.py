import os
from .familymgr import Family, FamilyGame
from .pakfile import BinaryPakFile
from .Bethesda.pakbinary_bsa import PakBinary_Bsa
from .Bethesda.pakbinary_ba2 import PakBinary_Ba2

class BethesdaFamily(Family):
    def __init__(self, elem):
        super().__init__(elem)

class BethesdaGame(FamilyGame):
    def __init__(self, family, id, elem, dgame):
        super().__init__(family, id, elem, dgame)

class BethesdaPakFile(BinaryPakFile):
    @staticmethod
    def getPakBinary(game, extension):
        match extension:
            case '': return PakBinary_Bsa()
            case '.bsa': return PakBinary_Bsa()
            case '.ba2': return PakBinary_Ba2()
            case _: raise Exception(f'Unknown: {extension}')

    def __init__(self, game, fileSystem, filePath, tag):
        super().__init__(game, fileSystem, filePath, self.getPakBinary(game, os.path.splitext(filePath)[1].lower()), tag)
