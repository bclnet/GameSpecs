import os
from io import BytesIO
from gamespecs.filesrc import FileSource
from gamespecs.pak import PakBinaryT

# typedefs
class Reader: pass
class BinaryPakFile: pass

# PakBinary_Aurora
class PakBinary_Aurora(PakBinaryT):

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        raise Exception('BAD MAGIC')
