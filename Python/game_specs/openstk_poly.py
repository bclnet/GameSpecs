import os, types
from typing import List, Any
from struct import unpack
from io import BytesIO

# finds a type
@staticmethod
def findType(klass):
    from importlib import import_module
    klass, modulePath = klass.rsplit(',', 1)
    try:
        _, className = klass.rsplit('.', 1)
        module = import_module(moduleName := f"game_specs.{modulePath.strip().replace('.', '_')}")
        return getattr(module, className)
    except (ImportError, AttributeError) as e:
        raise ImportError(klass)
        #moduleName, className 

class Reader:
    def __init__(self, f): self.f = f
    def __enter__(self): return self
    def __exit__(self, type, value, traceback): self.f.close()

    # primatives
    def readDouble(self): return float.from_bytes(self.f.read(8), 'little')
    def readSByte(self): return int.from_bytes(self.f.read(1), 'little', signed=True)
    def readInt16(self): return int.from_bytes(self.f.read(2), 'little', signed=True)
    def readInt32(self): return int.from_bytes(self.f.read(4), 'little', signed=True)
    def readInt64(self): return int.from_bytes(self.f.read(8), 'little', signed=True)
    def readSingle(self): return float.from_bytes(self.f.read(4), 'little')
    def readByte(self): return int.from_bytes(self.f.read(1), 'little', signed=False)
    def readUInt16(self): return int.from_bytes(self.f.read(2), 'little', signed=False)
    def readUInt32(self): return int.from_bytes(self.f.read(4), 'little', signed=False)
    def readUInt64(self): return int.from_bytes(self.f.read(8), 'little', signed=False)

    # normal
    def read(self, size: int): return self.f.read(size)
    def length(self):
        f = self.f
        pos = f.tell(); length = f.seek(0, os.SEEK_END); f.seek(pos, os.SEEK_SET)
        return length

    # endian
    def readDoubleE(self, bigEndian: bool = True): return float.from_bytes(self.f.read(8), 'big' if bigEndian else 'little')
    def readInt16E(self, bigEndian: bool = True): return int.from_bytes(self.f.read(2), 'big' if bigEndian else 'little', signed=True)
    def readInt32E(self, bigEndian: bool = True): return int.from_bytes(self.f.read(4), 'big' if bigEndian else 'little', signed=True)
    def readInt64E(self, bigEndian: bool = True): return int.from_bytes(self.f.read(8), 'big' if bigEndian else 'little', signed=True)
    def readSingleE(self, bigEndian: bool = True): return float.from_bytes(self.f.read(4), 'big' if bigEndian else 'little')
    def readUInt16E(self, bigEndian: bool = True): return int.from_bytes(self.f.read(2), 'big' if bigEndian else 'little', signed=False)
    def readUInt32E(self, bigEndian: bool = True): return int.from_bytes(self.f.read(4), 'big' if bigEndian else 'little', signed=False)
    def readUInt64E(self, bigEndian: bool = True): return int.from_bytes(self.f.read(8), 'big' if bigEndian else 'little', signed=False)

    # position
    def tell(self): return self.f.tell()
    def seek(self, pos: int): return self.f.seek(pos, os.SEEK_SET)
    def skip(self, pos: int): return self.f.seek(pos, os.SEEK_CUR)
    def peek(self, action, offset: int = 0):
        f = self.f
        pos = f.tell()
        if offset != 0: f.seek(offset, os.SEEK_CUR)
        value = action(self)
        f.seek(pos)
        return value

    # struct (https://docs.python.org/3/library/struct.html)
    def readT(self, cls: Any) -> Any:
        if isinstance(cls, types.LambdaType): return cls(self)
        pattern, size = cls.struct; return cls(unpack(pattern, self.f.read(size)))

    # string : chars
    def readCString(self) -> str:
        f = self.f
        length = 0; tell = f.tell(); maxPosition = self.length()
        while tell < maxPosition and int.from_bytes(f.read(1), 'little', signed=False) != 0: tell += 1; length += 1
        f.seek(0 - length - 1, os.SEEK_CUR)
        chars = f.read(length + 1)
        return chars.decode('utf-8') if length > 0 else None
        
    def readFString(self, length: int, zstring: bool = False) -> str: return self.f.read(length)[:length - 1 if zstring else length].decode('ascii') if length != 0 else None
    def readZAString(self, length: int = 65535) -> str:
        f = self.f; buf = BytesIO()
        while length > 0 and (c := f.read(1)) != b'\x00': length -= 1; buf.write(c)
        buf.seek(0)
        return buf.read().decode('ascii', 'ignore')
    # string : encoding
    def readL8Encoding(self, encoding: str = None): return self.f.read(int.from_bytes(self.f.read(1), 'little')).decode('ascii' if not encoding else encoding)
    def readL16Encoding(self, encoding: str = None): return self.f.read(int.from_bytes(self.f.read(2), 'little')).decode('ascii' if not encoding else encoding)
    def readL32Encoding(self, encoding: str = None): return self.f.read(int.from_bytes(self.f.read(4), 'little')).decode('ascii' if not encoding else encoding)

    # array
    def readTArray(self, cls: Any, count: int) -> List[Any]: return [self.readT(cls) for x in range(count)]