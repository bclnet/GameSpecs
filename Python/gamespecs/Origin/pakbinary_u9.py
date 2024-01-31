import os
from io import BytesIO
from gamespecs.filesrc import FileSource
from gamespecs.pak import PakBinaryT
from gamespecs.util import _pathExtension

# typedefs
class Reader: pass
class BinaryPakFile: pass
class FamilyGame: pass
class IFileSystem: pass

# PakBinary_U9
class PakBinary_U9(PakBinaryT):

    #region Headers

    #endregion

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        pass
        
    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        pass
