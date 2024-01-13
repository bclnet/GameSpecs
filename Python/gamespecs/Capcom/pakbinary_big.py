import os
from io import BytesIO
from typing import Any
from openstk.poly import Reader
from ..pakbinary import PakBinary
from ..pakfile import FileSource, BinaryPakFile

class PakBinary_Big(PakBinary):
    _instance = None
    def __new__(cls):
        if cls._instance is None: cls._instance = super().__new__(cls)
        return cls._instance

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: Any = None) -> None:
        raise NotImplementedError()
