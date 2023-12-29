from io import BytesIO
import zlib
from ._LIB.compression.lzss import Lzss

def decompressUnknown(r, length:int, newLength:int)->bytes: raise Exception('Not Implemented')
def decompressZlib(r, length:int, newLength:int, noHeader:bool=False, full:bool=True)->bytes: return \
    zlib.decompress(r.read(length), wbits = (-15 if noHeader else 0)) if full else \
    zlib.decompressobj(wbits = (-15 if noHeader else 0)).decompress(r.read(length))
def decompressZstd(r, length:int, newLength:int)->bytes: raise Exception('Not Implemented')
def decompressLzss(r, length:int, newLength:int)->bytes: return Lzss(BytesIO(r.read(length)), newLength).decompress()
def decompressBlast(r, length:int, newLength:int)->bytes: raise Exception('Not Implemented')
def decompressLz4(r, length:int, newLength:int)->bytes: raise Exception('Not Implemented')
def decompressZlib2(r, length:int, newLength:int)->bytes: raise Exception('Not Implemented')