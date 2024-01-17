import os
from typing import Callable
from gamespecs.pakfile import BinaryPakFile
from .Cryptic.pakbinary_hogg import PakBinary_Hogg
from .util import _pathExtension

# typedefs
class FamilyGame: pass
class PakBinary: pass
class IFileSystem: pass
class FileSource: pass
class FileOption: pass

# CrypticPakFile
class CrypticPakFile(BinaryPakFile):
    def __init__(self, game: FamilyGame, fileSystem: IFileSystem, filePath: str, tag: object = None):
        super().__init__(game, fileSystem, filePath, self.getPakBinary(game, _pathExtension(filePath).lower()), tag)

    #region Factories
    @staticmethod
    def getPakBinary(game: FamilyGame, extension: str) -> object:
        return PakBinary_Hogg()

    @staticmethod
    def objectFactoryFactory(source: FileSource, game: FamilyGame) -> (FileOption, Callable):
        match _pathExtension(source.path).lower():
            case _: return (0, None)
    #endregion