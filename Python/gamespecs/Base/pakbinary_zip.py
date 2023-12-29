import os
from io import BytesIO
from typing import Any
from ..pakbinary import PakBinary
from ..pakfile import FileSource, BinaryPakFile
from ..openstk_poly import Reader

class PakBinary_Zip(PakBinary):
    _instance = None
    def __new__(cls):
        if cls._instance is None: cls._instance = super().__new__(cls)
        return cls._instance

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: Any = None) -> None:
        raise Exception('Not Implemented')
