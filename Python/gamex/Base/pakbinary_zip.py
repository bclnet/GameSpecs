import os
from io import BytesIO
from zipfile import ZipFile
from gamex.pak import BinaryPakFile
from gamex.filesrc import FileSource
from gamex.pak import PakBinaryT

# typedefs
class Reader: pass

# PakBinary_Zip
class PakBinary_Zip(PakBinaryT):
    def __init__(self, key: str | bytes = None):
        self.key = key

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        source.useReader = False
        pak: ZipFile
        source.tag = pak = ZipFile(r.f)
        match self.key:
            case None: pass
            case s if isinstance(key, str): pak.setpassword(s)
            case z if isinstance(key, bytes): raise NotImplementedError()
        source.files = [FileSource(
            path = s.filename, #.replace('\\', '/'),
            packedSize = s.compress_size,
            fileSize = s.file_size,
            tag = s
            ) for s in pak.infolist() if not s.is_dir()]

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        pak: ZipFile = source.tag
        print(pak.read(file.path))
