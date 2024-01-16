import os
from io import BytesIO
from gamespecs.pakfile import FileSource, PakBinaryT

# typedefs
class Reader: pass
class BinaryPakFile: pass

# PakBinary_Unity
class PakBinary_Unity(PakBinaryT):

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        raise NotImplementedError()
