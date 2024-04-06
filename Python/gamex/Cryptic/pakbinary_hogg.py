import os
from io import BytesIO
from openstk.poly import Reader
from gamex.filesrc import FileSource
from gamex.pak import PakBinaryT
from gamex.compression import decompressLzss, decompressZlib

# typedefs
class BinaryPakFile: pass

# PakBinary_Dat
class PakBinary_Hogg(PakBinaryT):

    #region Headers

    MAGIC = 0xDEADF00D

    class Header:
        struct = ('<IHHIIII', 24)
        def __init__(self, tuple):
            self.magic, \
            self.version, \
            self.operationJournalSection, \
            self.fileEntrySection, \
            self.attributeEntrySection, \
            self.dataFileNumber, \
            self.fileJournalSection = tuple

    class FileJournalHeader:
        struct = ('<III', 12)
        def __init__(self, tuple):
            self.unknown1, \
            self.size, \
            self.size2 = tuple

    class FileEntry:
        struct = ('<qiIIIHHi', 32)
        def __init__(self, tuple):
            self.offset, \
            self.fileSize, \
            self.timestamp, \
            self.checksum, \
            self.unknown4, \
            self.unknown5, \
            self.unknown6, \
            self.id = tuple

    class AttributeEntry:
        struct = ('<iiII', 16)
        def __init__(self, tuple):
            self.pathId, \
            self.excerptId, \
            self.uncompressedSize, \
            self.flags = tuple

    #endregion

    # read
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None:
        # read header
        header = r.readS(self.Header)
        if header.magic != self.MAGIC: raise Exception('BAD MAGIC')
        if header.version < 10 or header.version > 11: raise Exception('BAD Version')
        if header.operationJournalSection > 1024: raise Exception('BAD Journal')
        if header.fileEntrySection != header.attributeEntrySection << 1: raise Exception('data entry / compression info section size mismatch')
        numFiles = header.attributeEntrySection >> 4

        # skip journals
        r.skip(header.operationJournalSection)
        fileJournalPosition = r.tell()
        r.skip(header.fileJournalSection)

        # read files
        fileEntries = r.readTArray(self.FileEntry, numFiles)
        attributeEntries = r.readTArray(self.AttributeEntry, numFiles)
        files = [None] * numFiles
        for i in range(numFiles):
            s = fileEntries[i]
            a = attributeEntries[i]
            files[i] = FileSource(
                        id = s.id,
                        offset = s.offset,
                        fileSize = s.fileSize,
                        packedSize = a.uncompressedSize,
                        compressed = 1 if a.uncompressedSize > 0 else 0
                        )

        # read "Datalist" file
        dataListFile = files[0]
        if dataListFile.id != 0 or dataListFile.fileSize == -1: raise Exception('BAD DataList')
        fileAttribs = {}
        with Reader(self.readData(source, r, dataListFile)) as r2:
            if r2.readUInt32() != 0: raise Exception('BAD DataList')
            count = r2.readInt32()
            for i in range(count): fileAttribs[i] = r2.read(r2.readUInt32())

        # read file journal
        r.seek(fileJournalPosition)
        fileJournalHeader = r.readS(self.FileJournalHeader)
        endPosition = r.tell() + fileJournalHeader.size
        while r.tell() < endPosition:
            action = r.readByte()
            targetId = r.readInt32()
            match action:
                case 1: fileAttribs[targetId] = r.read(r.readUInt32())
                case 2: del fileAttribs[targetId]

        # assign file path
        for i in range(numFiles):
            file = files[i]
            file.path = fileAttribs[attributeEntries[i].pathId][:-1].decode('ascii')
            if file.path.endswith('.hogg'): file.pak = PakBinary_Hogg.SubPakFile(self, file, source, source.game, source.fileSystem, file.path, file.tag)
        
        # remove filesize of -1 and file 0
        source.files = [x for x in files if x.fileSize != -1 and x.id != 0]

    # readData
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource) -> BytesIO:
        r.seek(file.offset)
        return BytesIO(
            decompressZlib(r, file.packedSize, file.fileSize) if file.compressed != 0 else \
            r.read(file.fileSize)
            )
