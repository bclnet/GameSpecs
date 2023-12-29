import os
from . import BinaryPakFile
from .Arkane.pakbinary_danae import PakBinary_Danae
from .Arkane.pakbinary_void import PakBinary_Void

class ArkanePakFile(BinaryPakFile):
    @staticmethod
    def getPakBinary(game, extension):
        match game.engine:
            case 'Danae': return PakBinary_Danae()
            case 'Void': return PakBinary_Void()
            # case 'CryEngine': return PakBinary_Void()
            # case 'Unreal': return PakBinary_Void()
            # case 'Valve': return PakBinary_Void()
            # case 'idTech7': return PakBinary_Void()
            case _: raise Exception(f'Unknown: {game.engine}')

    def __init__(self, game, fileSystem, filePath, tag):
        super().__init__(game, fileSystem, filePath, self.getPakBinary(game, os.path.splitext(filePath)[1].lower()), tag)