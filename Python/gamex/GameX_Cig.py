import os
from typing import Callable
from gamex.pak import BinaryPakFile
from .Cig.pakbinary_p4k import PakBinary_P4k
from .util import _pathExtension

# typedefs
class FamilyGame: pass
class PakBinary: pass
class PakState: pass
class FileSource: pass
class FileOption: pass

# CigPakFile
class CigPakFile(BinaryPakFile):
    def __init__(self, state: PakState):
        super().__init__(state, PakBinary_P4k())

    #region Factories
    @staticmethod
    def objectFactoryFactory(source: FileSource, game: FamilyGame) -> (FileOption, Callable):
        match _pathExtension(source.path).lower():
            case _: return (0, None)
    #endregion
