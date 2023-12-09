from io import BytesIO
import zlib
from ._LIB.compression.lzss import Lzss

def decompressZlib(r, length, newLength): return zlib.decompress(r.read(length))
def decompressLzss(r, length, newLength): return Lzss(BytesIO(r.read(length)), newLength).decompress()
def decompressBlast(r, length, newLength): raise Exception('Not Implemented')