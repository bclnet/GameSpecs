import os

# FileSource
class FileSource:
    emptyObjectFactory = lambda a, b, c: None
    def __init__(self, id = None, path = None, compressed = None, offset = None, fileSize = None, packedSize = None, crypted = None, hash = None, pak = None, parts = None, tag = None):
        self.id = id
        self.path = path
        self.compressed = compressed
        self.offset = offset
        self.fileSize = fileSize
        self.packedSize = packedSize
        self.crypted = crypted
        self.hash = hash
        self.pak = pak
        self.parts = parts
        self.tag = tag
        # cache
        self.cachedObjectFactory = None
        self.cachedOption = None
    def __repr__(self): return f'{self.path}:{self.fileSize}'
