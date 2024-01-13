import os
from io import BytesIO
from gamespecs.pakfile import FileSource, PakBinary

# typedefs
class Reader: pass
class BinaryPakFile: pass

# PakBinary_Unity
class PakBinary_Unity(PakBinary):
    _instance = None
    def __new__(cls):
        if cls._instance is None: cls._instance = super().__new__(cls)
        return cls._instance

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        raise NotImplementedError()
