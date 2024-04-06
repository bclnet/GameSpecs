import os
from typing import Callable
from gamex.pak import BinaryPakFile
from .Base.pakbinary_zip import PakBinary_Zip
from .Bioware.pakbinary_aurora import PakBinary_Aurora
from .Bioware.pakbinary_myp import PakBinary_Myp
from .util import _pathExtension

# typedefs
class FamilyGame: pass
class PakBinary: pass
class PakState: pass
class FileSource: pass
class FileOption: pass

# BiowarePakFile
class BiowarePakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, self.getPakBinary(state.game, _pathExtension(state.path).lower()))

    #region Factories
    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        if extension == '.zip': return PakBinary_Zip()
        match game.engine:
            case 'Aurora': return PakBinary_Aurora()
            case 'HeroEngine': return PakBinary_Myp()
            case _: raise Exception(f'Unknown: {game.engine}')

    @staticmethod
    def objectFactoryFactory(source: FileSource, game: FamilyGame) -> (FileOption, Callable):
        match _pathExtension(source.path).lower():
            case _: return (0, None)
    #endregion
