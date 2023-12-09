import os
from io import BytesIO
from typing import Any
from ..pakbinary import PakBinary
from ..pakfile import FileSource, BinaryPakFile
from ..utils import Reader

RES_MAGIC = 0x04534552

class PakBinary_Void(PakBinary):

    _instance = None
    def __new__(cls):
        if cls._instance is None: cls._instance = super().__new__(cls)
        return cls._instance

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: Any = None) -> None:
        if os.path.splitext(source.filePath)[0] != '.index': raise Exception('must be a .index file')
        source.files = files = []
        pass

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        pass
