import os
from .pakfile import BinaryPakFile
from .Base.pakbinary_zip import PakBinary_Zip
from .Bioware.pakbinary_aurora import PakBinary_Aurora
from .Bioware.pakbinary_myp import PakBinary_Myp
from .util import _pathExtension

class BiowarePakFile(BinaryPakFile):
    @staticmethod
    def getPakBinary(game, extension):
        if extension == '.zip': return PakBinary_Zip()
        match game.engine:
            case 'Aurora': return PakBinary_Aurora()
            case 'HeroEngine': return PakBinary_Myp()
            case _: raise Exception(f'Unknown: {game.engine}')

    def __init__(self, game, fileSystem, filePath, tag):
        super().__init__(game, fileSystem, filePath, self.getPakBinary(game, _pathExtension(filePath).lower()), tag)