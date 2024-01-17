import os
from typing import Callable
from gamespecs.pakfile import BinaryPakFile
from .Black.pakbinary_dat import PakBinary_Dat
from .util import _pathExtension

# typedefs
class FamilyGame: pass
class PakBinary: pass
class IFileSystem: pass
class FileSource: pass
class FileOption: pass

# BlackPakFile
class BlackPakFile(BinaryPakFile):
    def __init__(self, game: FamilyGame, fileSystem: IFileSystem, filePath: str, tag: object = None):
        super().__init__(game, fileSystem, filePath, self.getPakBinary(game, _pathExtension(filePath).lower()), tag)

    #region Factories
    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> PakBinary:
        return PakBinary_Dat()

    @staticmethod
    def objectFactoryFactory(source: FileSource, game: FamilyGame) -> (FileOption, Callable):
        match _pathExtension(source.path).lower():
            case _: return (0, None)
    #endregion
