import os
from io import BytesIO
from enum import Enum
from gamex.filesrc import FileSource
from gamex.pak import PakBinaryT
# from gamex.compression import decompressLz4, decompressZlib2

# typedefs
class Reader: pass
class BinaryPakFile: pass

# PakBinary_Ba2
class PakBinary_Ba2(PakBinaryT):

    #region TES5

    # Default header data
    F4_BSAHEADER_FILEID = 0x58445442    # Magic for Fallout 4 BA2, the literal string "BTDX".
    F4_BSAHEADER_VERSION1 = 0x01      # Version number of a Fallout 4 BA2
    F4_BSAHEADER_VERSION2 = 0x02        # Version number of a Starfield BA2

    class F4_HeaderType(Enum):
        GNRL = 0x4c524e47
        DX10 = 0x30315844
        GNMF = 0x464d4e47
        Unknown = 0

    class F4_Header:
        struct = ('<IIIQ', 20)
        def __init__(self, tuple):
            self.version, \
            self.type, \
            self.numFiles, \
            self.nameTableOffset = tuple
            self.type = PakBinary_Ba2.F4_HeaderType(self.type)

    class F4_File:
        struct = ('<I4sIIQIII', 36)
        def __init__(self, tuple):
            self.nameHash, \
            self.ext, \
            self.dirHash, \
            self.flags, \
            self.offset, \
            self.packedSize, \
            self.fileSize, \
            self.align = tuple

    class F4_Texture:
        struct = ('<I4sIBBHHHBBBB', 24)
        def __init__(self, tuple):
            self.nameHash, \
            self.ext, \
            self.dirHash, \
            self.unk0C, \
            self.numChunks, \
            self.chunkHeaderSize, \
            self.height, \
            self.width, \
            self.numMips, \
            self.format, \
            self.isCubemap, \
            self.tileMode = tuple

    class F4_GNMF:
        struct = ('<I4sIBBH32sQIIII', 72)
        def __init__(self, tuple):
            self.nameHash, \
            self.ext, \
            self.dirHash, \
            self.unk0C, \
            self.numChunks, \
            self.unk0E, \
            self.header, \
            self.offset, \
            self.packedSize, \
            self.fileSize, \
            self.unk40, \
            self.align = tuple

    class F4_TextureChunk:
        struct = ('<QIIHHI', 24)
        def __init__(self, tuple):
            self.offset, \
            self.packedSize, \
            self.fileSize, \
            self.startMip, \
            self.endMip, \
            self.align = tuple

    #endregion

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        source.magic = magic = r.readUInt32()

        # Fallout 4 - Starfield
        if magic == self.F4_BSAHEADER_FILEID:
            header = r.readS(self.F4_Header)
            if header.version > self.F4_BSAHEADER_VERSION2:
                raise Exception('BAD MAGIC')
            source.version = header.version
            source.files = files = [None] * header.numFiles
            # version2
            # if header.version == self.F4_BSAHEADER_VERSION2: r.skip(8)

            # General BA2 Format
            match header.type:
                # General BA2 Format
                case self.F4_HeaderType.GNRL:
                    headerFiles = r.readTArray(self.F4_File, header.numFiles)
                    for i in range(header.numFiles):
                        headerFile = headerFiles[i]
                        files[i] = FileSource(
                            compressed = 1 if headerFile.packedSize != 0 else 0,
                            packedSize = headerFile.packedSize,
                            fileSize = headerFile.fileSize,
                            offset = headerFile.offset
                            )
                # Texture BA2 Format
                case self.F4_HeaderType.DX10:
                    for i in range(header.numFiles):
                        headerTexture = r.readS(self.F4_Texture)
                        headerTextureChunks = r.readTArray(self.F4_TextureChunk, headerTexture.numChunks)
                        firstChunk = headerTextureChunks[0]
                        files[i] = FileSource(
                            fileInfo = headerTexture,
                            packedSize = firstChunk.packedSize,
                            fileSize = firstChunk.fileSize,
                            offset = firstChunk.offset,
                            tag = headerTextureChunks
                            )
                # GNMF BA2 Format
                case self.F4_HeaderType.GNMF:
                    for i in range(header.numFiles):
                        headerGNMF = r.readS(self.F4_GNMF)
                        headerTextureChunks = r.readTArray(self.F4_TextureChunk, headerGNMF.numChunks)
                        files[i] = FileSource(
                            fileInfo = headerGNMF,
                            packedSize = headerGNMF.packedSize,
                            fileSize = headerGNMF.fileSize,
                            offset = headerGNMF.offset,
                            tag = headerTextureChunks
                            )
                case _: raise Exception(f'Unknown: {header.type}')

            # assign full names to each file
            if header.nameTableOffset > 0:
                r.seek(header.nameTableOffset)
                path = r.readL16Encoding().replace('\\', '/')
                for file in files: file.path = path

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        r.seek(file.offset)

        # General BA2 Format
        if file.fileInfo == None:
            return BytesIO(
                decompressZlib2(r, file.packedSize, file.fileSize) if file.compressed != 0 else \
                r.read(file.fileSize)
                )

        # Texture BA2 Format
        elif file.fileInfo is self.F4_Texture:
            pass

        # GNMF BA2 Format
        elif file.fileInfo is self.F4_GNMF:
            pass

        else: raise Exception(f'Unknown fileInfo: {file.fileInfo}')
