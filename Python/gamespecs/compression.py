from io import BytesIO

# typedefs
class Reader: pass

def decompressUnknown(r: Reader, length: int, newLength: int) -> bytes: raise NotImplementedError()
def decompressZlib(r: Reader, length: int, newLength: int, noHeader: bool = False, full: bool = True) -> bytes: 
    import zlib
    return \
        zlib.decompress(r.read(length), wbits = (-15 if noHeader else 0)) if full else \
        zlib.decompressobj(wbits = (-15 if noHeader else 0)).decompress(r.read(length))
def decompressZstd(r: Reader, length: int, newLength: int) -> bytes: raise NotImplementedError()
def decompressLzss(r: Reader, length: int, newLength: int) -> bytes:
    from ._LIB.compression.lzss import Lzss
    return Lzss(BytesIO(r.read(length)), newLength).decompress()
def decompressBlast(r: Reader, length: int, newLength: int) -> bytes:
    from ._LIB.compression.blast import Blast
    decoder = Blast()
    fs = r.read(length)
    os = bytearray(newLength)
    decoder.decompress(fs, os)
    return os
def decompressLz4(r: Reader, length: int, newLength: int) -> bytes: raise NotImplementedError()
def decompressZlib2(r: Reader, length: int, newLength: int) -> bytes: raise NotImplementedError()