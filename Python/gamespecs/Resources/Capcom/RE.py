import os, pathlib
from zipfile import ZipFile
from io import BytesIO
from importlib import resources

class MurmurHash3:
    C1 = 0xcc9e2d51
    C2 = 0x1b873593
    Seed = 0xffffffff
    @staticmethod
    def hash(data: str) -> int: return MurmurHash3.hash2(data.encode('utf-16le'))
    @staticmethod
    def hash2(data: bytes) -> int:
        h = MurmurHash3.Seed; k = 0; l = 0
        r = BytesIO(data)
        chunk = r.read(4)
        while (chunkLength := len(chunk)) > 0:
            l += chunkLength
            match chunkLength:
                case 4:
                    k = (chunk[0] | chunk[1] << 8 | chunk[2] << 16 | chunk[3] << 24); k = k*MurmurHash3.C1&0xffffffff; k = MurmurHash3._rotl32(k, 15); k = k*MurmurHash3.C2&0xffffffff; h ^= k 
                    h = MurmurHash3._rotl32(h, 13); h = (h * 5 + 0xe6546b64) & 0xffffffff
                case 3: k = (chunk[0] | chunk[1] << 8 | chunk[2] << 16); k = k*MurmurHash3.C1&0xffffffff; k = MurmurHash3._rotl32(k, 15); k = k*MurmurHash3.C2&0xffffffff; h ^= k
                case 2: k = (chunk[0] | chunk[1] << 8); k = k*MurmurHash3.C1&0xffffffff; k = MurmurHash3._rotl32(k, 15); k = k*MurmurHash3.C2&0xffffffff; h ^= k
                case 1: k = (chunk[0]); k = k*MurmurHash3.C1&0xffffffff; k = MurmurHash3._rotl32(k, 15); k = k*MurmurHash3.C2&0xffffffff; h ^= k
            chunk = r.read(4)
        h ^= l
        h = MurmurHash3._fmix(h)
        return h

    @staticmethod
    def _rotl32(x:int, r:int) -> int: return (x << r)&0xffffffff | (x >> (32 - r))
    @staticmethod
    def _fmix(h:int) -> int:
        h ^= h >> 16; h = h*0x85ebca6b&0xffffffff
        h ^= h >> 13; h = h*0xc2b2ae35&0xffffffff
        h ^= h >> 16
        return h

file = resources.files().joinpath('RE.zip').open('rb')
pak: ZipFile = ZipFile(file, 'r')
hashEntries: dict[str, object] = { x.filename:x for x in pak.infolist() }
hashLookup: dict[str, dict[int, str]] = {}

@staticmethod
def getHashLookup(path: str) -> dict[int, str]:
    if path in hashLookup: return hashLookup[path]
    line: str
    value: dict[int, str] = {}
    with pak.open(hashEntries[path]) as r:
        while line := r.readline().decode('ascii').rstrip('\r\n'):
            hashLower = MurmurHash3.hash(line.lower())
            hashUpper = MurmurHash3.hash(line.upper())
            hash = (hashUpper << 32) & 0xffffffffffffffff | hashLower
            if hash in value:
                print(f'[COLLISION]: {value[hash]} <-> {line}')
            value[hash] = line
    return value
