import os
from io import BytesIO
from gamespecs.filesrc import FileSource
from gamespecs.pakfile import PakBinaryT

# typedefs
class Reader: pass
class BinaryPakFile: pass

# PakBinary_Bundle
class PakBinary_Bundle(PakBinaryT):

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        raise NotImplementedError()
