import os
from typing import Callable
from gamespecs.pakfile import BinaryPakFile
from .Cig.pakbinary_p4k import PakBinary_P4k
from .util import _pathExtension

# typedefs
class FamilyGame: pass
class PakBinary: pass
class IFileSystem: pass
class FileSource: pass
class FileOption: pass

# CigPakFile
class CigPakFile(BinaryPakFile):
    def __init__(self, game: FamilyGame, fileSystem: IFileSystem, filePath: str, tag: object = None):
        super().__init__(game, fileSystem, filePath, PakBinary_P4k(), tag)

    #region Factories
    @staticmethod
    def objectFactoryFactory(source: FileSource, game: FamilyGame) -> (FileOption, Callable):
        match _pathExtension(source.path).lower():
            case _: return (0, None)
    #endregion
