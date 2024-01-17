import os
from io import BytesIO
from gamespecs.filesrc import FileSource
from gamespecs.pakfile import PakBinaryT

# typedefs
class Reader: pass
class ITexture: pass

# PakBinary_Zip
class PakBinary_Zip(PakBinaryT):

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        raise NotImplementedError()
