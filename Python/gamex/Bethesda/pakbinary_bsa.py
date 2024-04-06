import os
from io import BytesIO
from gamex.filesrc import FileSource
from gamex.pak import PakBinaryT
from gamex.compression import decompressLz4, decompressZlib2

# typedefs
class Reader: pass
class BinaryPakFile: pass

# PakBinary_Bsa
class PakBinary_Bsa(PakBinaryT):

    #region TES4

    OB_BSAHEADER_FILEID = 0x00415342    # Magic for Oblivion BSA, the literal string "BSA\0".
    OB_BSAHEADER_VERSION = 0x67         # Version number of an Oblivion BSA
    F3_BSAHEADER_VERSION = 0x68         # Version number of a Fallout 3 BSA
    SSE_BSAHEADER_VERSION = 0x69        # Version number of a Skyrim SE BSA

    # Archive flags
    OB_BSAARCHIVE_PATHNAMES = 0x0001    # Whether the BSA has names for paths
    OB_BSAARCHIVE_FILENAMES = 0x0002    # Whether the BSA has names for files
    OB_BSAARCHIVE_COMPRESSFILES = 0x0004 # Whether the files are compressed
    F3_BSAARCHIVE_PREFIXFULLFILENAMES = 0x0100 # Whether the name is prefixed to the data?

    # Bitmasks for the size field in the header
    OB_BSAFILE_SIZEMASK = 0x3fffffff    # Bit mask with OB_HeaderFile:SizeFlags to get the compression status
    OB_BSAFILE_SIZECOMPRESS = 0xC0000000 # Bit mask with OB_HeaderFile:SizeFlags to get the compression status

    class OB_Header:
        struct = ('<IIIIIIII', 32)
        def __init__(self, tuple):
            self.version, \
            self.folderRecordOffset, \
            self.archiveFlags, \
            self.folderCount, \
            self.fileCount, \
            self.folderNameLength, \
            self.fileNameLength, \
            self.fileFlags = tuple

    class OB_Folder:
        struct = ('<QII', 16)
        def __init__(self, tuple):
            self.hash, \
            self.fileCount, \
            self.offset = tuple

    class OB_FolderSSE:
        struct = ('<QIIQ', 24)
        def __init__(self, tuple):
            self.hash, \
            self.fileCount, \
            self.unk, \
            self.offset = tuple

    class OB_File:
        struct = ('<QII', 16)
        def __init__(self, tuple):
            self.hash, \
            self.size, \
            self.offset = tuple

    #endregion

    #region TES3

    MW_BSAHEADER_FILEID = 0x00000100    # Magic for Morrowind BSA

    class MW_Header:
        struct = ('<II', 8)
        def __init__(self, tuple):
            self.hashOffset, \
            self.fileCount = tuple

    class MW_File:
        struct = ('<II', 8)
        def __init__(self, tuple):
            self.fileSize, \
            self.fileOffset = tuple
        def getSize(self):
            return self.fileSize & 0x3FFFFFFF if self.fileSize > 0 else 0

    #endregion

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        source.magic = magic = r.readUInt32()

        # Oblivion - Skyrim
        if magic == self.OB_BSAHEADER_FILEID:
            header = r.readS(self.OB_Header)
            if header.version != self.OB_BSAHEADER_VERSION \
                and header.version != self.F3_BSAHEADER_VERSION \
                and header.version != self.SSE_BSAHEADER_VERSION:
                raise Exception('BAD MAGIC')
            if (header.archiveFlags & self.OB_BSAARCHIVE_PATHNAMES) == 0 \
                or (header.archiveFlags & self.OB_BSAARCHIVE_FILENAMES) == 0:
                raise Exception('HEADER FLAGS')
            source.version = header.version

            # calculate some useful values
            compressedToggle = (header.archiveFlags & self.OB_BSAARCHIVE_COMPRESSFILES) > 0
            if header.version == self.F3_BSAHEADER_VERSION \
                or header.version == self.SSE_BSAHEADER_VERSION:
                source.tag = (header.archiveFlags & self.F3_BSAARCHIVE_PREFIXFULLFILENAMES) > 0

            # read-all folders
            foldersFiles = [x.fileCount for x in r.readTArray(self.OB_FolderSSE, header.folderCount)] if header.version == self.SSE_BSAHEADER_VERSION else \
                [x.fileCount for x in r.readTArray(self.OB_Folder, header.folderCount)]

            # read-all folder files
            fileIdx = 0
            source.files = files = [None] * header.fileCount
            for i in range(header.folderCount):
                folderName = r.readFString(r.readByte() - 1).replace('\\', '/')
                r.skip(1)
                headerFiles = r.readTArray(self.OB_File, foldersFiles[i])
                for headerFile in headerFiles:
                    compressed = (headerFile.size & self.OB_BSAFILE_SIZECOMPRESS) != 0
                    packedSize = headerFile.size ^ self.OB_BSAFILE_SIZECOMPRESS if compressed else headerFile.size
                    files[fileIdx] = FileSource(
                        path = folderName,
                        offset = headerFile.offset,
                        compressed = 1 if compressed ^ compressedToggle else 0,
                        packedSize = packedSize,
                        fileSize = packedSize & self.OB_BSAFILE_SIZEMASK if source.version == self.SSE_BSAHEADER_VERSION else packedSize
                        )
                    fileIdx += 1
            # read-all names
            for file in files:
                file.path = f'{file.path}/{r.readCString()}'

        # Morrowind
        elif magic == self.MW_BSAHEADER_FILEID:
            header = r.readS(self.MW_Header)
            dataOffset = 12 + header.hashOffset + (8 * header.fileCount)

            # create filesources
            source.files = files = [None] * header.fileCount
            headerFiles = r.readTArray(self.MW_File, header.fileCount)
            for i in range(header.fileCount):
                headerFile = headerFiles[i]
                size = headerFile.getSize()
                files[i] = FileSource(
                    offset = dataOffset + headerFile.fileOffset,
                    compressed = 0,
                    fileSize = size,
                    packedSize = size
                    )

            # read filename offsets
            filenameOffsets = r.readTArray(lambda x : x.readUInt32(), header.fileCount) # relative offset in filenames section

            # read filenames
            filenamesPosition = r.tell()
            for i in range(header.fileCount):
                r.seek(filenamesPosition + filenameOffsets[i])
                files[i].path = r.readZAString(1000).replace('\\', '/')
        else: raise Exception('BAD MAGIC')
    
    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        fileSize = file.packedSize & self.OB_BSAFILE_SIZEMASK if source.version == self.SSE_BSAHEADER_VERSION else file.packedSize
        r.seek(file.offset)
        if source.tag:
            prefixLength = r.readByte() + 1
            if source.version == self.SSE_BSAHEADER_VERSION: fileSize -= prefixLength
            r.seek(file.offset + prefixLength)

        # not compressed
        if fileSize <= 0 or file.compressed == 0:
            return BytesIO(r.read(fileSize))

        # compressed
        newFileSize = r.readUInt32(); fileSize -= 4
        return BytesIO(
            decompressLz4(r, fileSize, newFileSize) if source.version == self.SSE_BSAHEADER_VERSION else \
            decompressZlib2(r, fileSize, newFileSize)
            )
