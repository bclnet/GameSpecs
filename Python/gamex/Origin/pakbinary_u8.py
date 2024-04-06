import os
from io import BytesIO
from typing import Callable
from gamex.filesrc import FileSource
from gamex.pak import PakBinaryT
from gamex.util import _pathExtension

# typedefs
class Reader: pass
class BinaryPakFile: pass
class FamilyGame: pass
class IFileSystem: pass
class FileOption: pass

# PakBinary_U8
class PakBinary_U8(PakBinaryT):

    #region Factories

    @staticmethod
    def objectFactoryFactory(source: FileSource, game: FamilyGame) -> (FileOption, Callable):
        match source.path.lower():
            case _: pass

    #endregion

    #region Headers

    #endregion

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        pass
        
    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        pass
