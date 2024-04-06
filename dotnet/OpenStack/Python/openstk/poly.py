import os, types, numpy as np
from typing import Callable
from struct import unpack
from io import BytesIO

moduleRoot = 'gamex'

# finds a type
@staticmethod
def findType(klass):
    from importlib import import_module
    klass, modulePath = klass.rsplit(',', 1)
    try:
        _, className = klass.rsplit('.', 1)
        module = import_module(moduleName := f"{moduleRoot}.{modulePath.strip().replace('.', '_')}")
        return getattr(module, className)
    except (ImportError, AttributeError) as e:
        raise ImportError(klass)

# IDisposable
class IDisposable:
    def dispose(self): pass

# Reader
class Reader:
    def __init__(self, f): self.f = f; self.__update()
    def __enter__(self): return self
    def __exit__(self, type, value, traceback): self.f.close()
    def __update(self):
        f = self.f
        pos = f.tell()
        self.length = f.seek(0, os.SEEK_END)
        f.seek(pos, os.SEEK_SET)

    # base
    def read(self, size: int): return self.f.read(size)
    def length(self): return self.length
    def copyTo(self, destination: BytesIO, resetAfter: bool = False): raise NotImplementedError()
    def readToEnd(self): length = self.length - self.f.tell(); return self.f.read(length)
    def readLine(self) -> str: return self.f.readline().decode('utf-8')

    # primatives : normal
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

    # primatives : endian
    def readDoubleE(self): return float.from_bytes(self.f.read(8), 'big')
    def readInt16E(self): return int.from_bytes(self.f.read(2), 'big', signed=True)
    def readInt32E(self): return int.from_bytes(self.f.read(4), 'big', signed=True)
    def readInt64E(self): return int.from_bytes(self.f.read(8), 'big', signed=True)
    def readSingleE(self): return float.from_bytes(self.f.read(4), 'big')
    def readUInt16E(self): return int.from_bytes(self.f.read(2), 'big', signed=False)
    def readUInt32E(self): return int.from_bytes(self.f.read(4), 'big', signed=False)
    def readUInt64E(self): return int.from_bytes(self.f.read(8), 'big', signed=False)

    # primatives : endianX
    def readDoubleX(self, endian: bool = True): return float.from_bytes(self.f.read(8), 'big' if endian else 'little')
    def readInt16X(self, endian: bool = True): return int.from_bytes(self.f.read(2), 'big' if endian else 'little', signed=True)
    def readInt32X(self, endian: bool = True): return int.from_bytes(self.f.read(4), 'big' if endian else 'little', signed=True)
    def readInt64X(self, endian: bool = True): return int.from_bytes(self.f.read(8), 'big' if endian else 'little', signed=True)
    def readSingleX(self, endian: bool = True): return float.from_bytes(self.f.read(4), 'big' if endian else 'little')
    def readUInt16X(self, endian: bool = True): return int.from_bytes(self.f.read(2), 'big' if endian else 'little', signed=False)
    def readUInt32X(self, endian: bool = True): return int.from_bytes(self.f.read(4), 'big' if endian else 'little', signed=False)
    def readUInt64X(self, endian: bool = True): return int.from_bytes(self.f.read(8), 'big' if endian else 'little', signed=False)

    # primatives : specialized
    def readBool32(self): return int.from_bytes(self.f.read(4), 'little', signed=False) != 0
    def readGuid(self): return self.f.read(16)
    def readCInt32(self): raise NotImplementedError()


    # position
    def align(self, align: int = 4): align -= 1; self.f.seek((self.f.tell() + align) & ~align, os.SEEK_SET); return self
    def tell(self): return self.f.tell()
    def seek(self, offset: int): self.f.seek(offset, os.SEEK_SET); return self
    def seekAndAlign(self, offset: int, align: int = 4): self.f.seek(offset + align - (offset % align) if offset % align else offset, os.SEEK_SET); return self
    def skip(self, count: int): self.f.seek(count, os.SEEK_CUR); return self
    def skipAndAlign(self, count: int, align: int = 4): offset = self.f.tell() + count; self.f.seek(offset + align - (offset % align) if offset % align else offset, os.SEEK_CUR); return self
    def end(self, offset: int): self.f.seek(offset, os.SEEK_END); return self
    def peek(self, action, offset: int = 0, origin: int = os.SEEK_CUR):
        f = self.f
        pos = f.tell()
        f.seek(offset, origin)
        value = action(self)
        f.seek(pos)
        return value


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

    
    # struct : single  - https://docs.python.org/3/library/struct.html 
    def readF(self, cls: object, factory: Callable) -> object: return factory(self)
    def readS(self, cls: object) -> object: pattern, size = cls.struct; return cls(unpack(pattern, self.f.read(size)))
    def readS2(self, cls: object, sizeOf: int) -> object: pattern, size = cls.struct; return cls(unpack(pattern, self.f.read(size)))
    def readT(self, cls: object, sizeOf: int) -> object: return unpack(cls, self.f.read(size))[0]

    # struct : array - factory
    def readL8FArray(self, cls: object, factory: Callable, endian: bool = False) -> list[object]: return self.readFArray(cls, factory, source.readByte())
    def readL16FArray(self, cls: object, factory: Callable, endian: bool = False) -> list[object]: return self.readFArray(cls, factory, source.readUInt16E(endian))
    def readL32FArray(self, cls: object, factory: Callable, endian: bool = False) -> list[object]: return self.readFArray(cls, factory, source.readUInt32E(endian))
    def readC32FArray(self, cls: object, factory: Callable, endian: bool = False) -> list[object]: return self.readFArray(cls, factory, source.readCInt32E(endian))
    def readFArray(self, cls: object, factory: Callable, count: int) -> list[object]: return [self.readF(cls, factory) for x in range(count)] if count else []

    # struct : array - struct
    def readL8SArray(self, cls: object, endian: bool = False) -> list[object]: return self.readSArray(cls, source.readByte())
    def readL16SArray(self, cls: object, endian: bool = False) -> list[object]: return self.readSArray(cls, source.readUInt16E(endian))
    def readL32SArray(self, cls: object, endian: bool = False) -> list[object]: return self.readSArray(cls, source.readUInt32E(endian))
    def readC32SArray(self, cls: object, endian: bool = False) -> list[object]: return self.readSArray(cls, source.readCInt32E(endian))
    def readSArray(self, cls: object, count: int) -> list[object]: return [self.readS(cls) for x in range(count)] if count else []

    # struct : array - type
    def readL8TArray(self, cls: object, sizeOf: int, endian: bool = False) -> list[object]: return self.readTArray(cls, sizeOf, source.readByte())
    def readL16TArray(self, cls: object, sizeOf: int, endian: bool = False) -> list[object]: return self.readTArray(cls, sizeOf, source.readUInt16(endian))
    def readL32TArray(self, cls: object, sizeOf: int, endian: bool = False) -> list[object]: return self.readTArray(cls, sizeOf, source.readUInt32(endian))
    def readC32TArray(self, cls: object, sizeOf: int, endian: bool = False) -> list[object]: return self.readTArray(cls, sizeOf, source.readCInt32(endian))
    def readTArray(self, cls: object, sizeOf: int, count: int) -> list[object]: return [self.readT(cls, sizeOf) for x in range(count)] if count else []

    # struct : each
    def readSEach(self, cls: object, count: int) -> list[object]: return [self.readS(cls) for x in range(count)] if count else []
    def readTEach(self, cls: object, sizeOf: int, count: int) -> list[object]: return [self.readT(cls, sizeOf) for x in range(count)] if count else []
    
    # struct : many - factory
    def readL8FMany(self, clsKey: object, keyFactory: Callable, valueFactory: Callable, endian: bool = False) -> list[object]: return self.readFMany(clsKey, keyFactory, valueFactory, source.readByte())
    def readL16FMany(self, clsKey: object, keyFactory: Callable, valueFactory: Callable, endian: bool = False) -> list[object]: return self.readFMany(clsKey, keyFactory, valueFactory, source.readUInt16(endian))
    def readL32FMany(self, clsKey: object, keyFactory: Callable, valueFactory: Callable, endian: bool = False) -> list[object]: return self.readFMany(clsKey, keyFactory, valueFactory, source.readUInt32(endian))
    def readC32FMany(self, clsKey: object, keyFactory: Callable, valueFactory: Callable, endian: bool = False) -> list[object]: return self.readFMany(clsKey, keyFactory, valueFactory, source.readCInt32(endian))
    def readFMany(self, clsKey: object, keyFactory: Callable, valueFactory: Callable) -> dict[object, object]: return {keyFactory(self):valueFactory(self) for x in range(count)} if count else {}

    # struct : many - struct
    def readL8SMany(self, clsKey: object, valueFactory: Callable, endian: bool = False) -> list[object]: return self.readSMany(clsKey, valueFactory, source.readByte())
    def readL16SMany(self, clsKey: object, valueFactory: Callable, endian: bool = False) -> list[object]: return self.readSMany(clsKey, valueFactory, source.readUInt16(endian))
    def readL32SMany(self, clsKey: object, valueFactory: Callable, endian: bool = False) -> list[object]: return self.readSMany(clsKey, valueFactory, source.readUInt32(endian))
    def readC32SMany(self, clsKey: object, valueFactory: Callable, endian: bool = False) -> list[object]: return self.readSMany(clsKey, valueFactory, source.readCInt32(endian))
    def readSMany(self, clsKey: object, valueFactory: Callable) -> dict[object, object]: return {self.readS(clsKey):valueFactory(self) for x in range(count)} if count else {}
    
    # struct : many - type
    def readL8TMany(self, clsKey: object, sizeOf: int, valueFactory: Callable, endian: bool = False) -> list[object]: return self.readTMany(clsKey, sizeOf, valueFactory, source.readByte())
    def readL16TMany(self, clsKey: object, sizeOf: int, valueFactory: Callable, endian: bool = False) -> list[object]: return self.readTMany(clsKey, sizeOf, valueFactory, source.readUInt16(endian))
    def readL32TMany(self, clsKey: object, sizeOf: int, valueFactory: Callable, endian: bool = False) -> list[object]: return self.readTMany(clsKey, sizeOf, valueFactory, source.readUInt32(endian))
    def readC32TMany(self, clsKey: object, sizeOf: int, valueFactory: Callable, endian: bool = False) -> list[object]: return self.readTMany(clsKey, sizeOf, valueFactory, source.readCInt32(endian))
    def readTMany(self, clsKey: object, sizeOf: int, valueFactory: Callable) -> dict[object, object]: return {self.readT(clsKey, sizeOf):valueFactory(self) for x in range(count)} if count else {}

    # numerics
    def readHalf(self) -> float: raise NotImplementedError()
    def readHalf16(self) -> float: raise NotImplementedError()
    def readVector2(self) -> np.ndarray: return np.array([self.readSingle(), self.readSingle()])
    def readHalfVector2(self) -> np.ndarray: return np.array([self.readHalf(), self.readHalf()])
    def readVector3(self) -> np.ndarray: return np.array([self.readSingle(), self.readSingle(), self.readSingle()])
    def readHalfVector3(self) -> np.ndarray: return np.array([self.readHalf(), self.readHalf(), self.readHalf()])
    def readHalf16Vector3(self) -> np.ndarray: return np.array([self.readHalf16(), self.readHalf16(), self.readHalf16()])
    def readVector4(self) -> np.ndarray: return np.array([self.readSingle(), self.readSingle(), self.readSingle(), self.readSingle()])
    def readHalfVector4(self) -> np.ndarray: return np.array([self.readHalf(), self.readHalf(), self.readHalf(), self.readHalf()])
    def readMatrix3x3(self) -> np.ndarray: return np.array([
        [self.readSingle(), self.readSingle(), self.readSingle()],
        [self.readSingle(), self.readSingle(), self.readSingle()],
        [self.readSingle(), self.readSingle(), self.readSingle()]
        ])
    