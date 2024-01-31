import os, re, struct, numpy as np
from io import BytesIO
from gamespecs.filesrc import FileSource
from gamespecs.pak import PakBinary
from gamespecs.meta import MetaInfo, MetaContent, IHaveMetaInfo

# typedefs
class Reader: pass
class BinaryPakFile: pass
class PakFile: pass
class MetaManager: pass

# Binary_Anim
class Binary_Anim(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Anim(r)

    def __init__(self, r: Reader):
        pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'Anim File')),
        MetaInfo('Anim', items = [
            # MetaInfo(f'Default: {Default.GumpID}')
            ])
        ]

# Binary_Animdata
class Binary_Animdata(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Animdata(r)

    #region Records

    class AnimRecord:
        struct = ('<64s4B', 68)
        def __init__(self, tuple):
            self.frames, \
            self.unknown, \
            self.frameCount, \
            self.frameInterval, \
            self.startInterval = tuple
            self.frames = struct.unpack('<64B', self.frames)

    class Record:
        def __init__(self, record: object):
            self.frames = record.frames
            self.frameCount = record.frameCount
            self.frameInterval = record.frameInterval
            self.startInterval = record.startInterval

    records: dict[int, Record] = {}

    #endregion

    def __init__(self, r: Reader):
        id = 0
        length = int(r.length / (4 + (8 * (64 + 4))))
        for i in range(length):
            r.skip(4)
            records = r.readSArray(self.AnimRecord, 8)
            for j in range(8):
                record = records[j]
                if record.frameCount > 0:
                    self.records[id] = self.Record(record)
                id += 1                    

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'Animdata File')),
        MetaInfo('Animdata', items = [
            MetaInfo(f'Records: {len(self.records)}')
            ])
        ]

# Binary_AsciiFont
class Binary_AsciiFont(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_AsciiFont(r)

    #region Records

    class AsciiFont:
        characters: list[list[int]] = [None]*224
        height: int = 0
        def __init__(self, r: Reader):
            
            r.readByte()
            for i in range(224):
                width = r.readByte()
                height = r.readByte()
                r.readByte()
                if width <= 0 or height <= 0: continue

                if height > self.height and i < 96: self.height = height

                length = width * height
                dt = np.uint16
                # dt = dt.newbyteorder('>')
                bd = list(np.frombuffer(r.read(length << 1), dtype = dt))
                for j in range(length):
                    if bd[j] != 0: bd[j] ^= 0x8000
                self.characters[i] = np.array(bd).tobytes()
    
    fonts: list[AsciiFont] = [None]*10

    #endregion

    def __init__(self, r: Reader):
        for i in range(len(self.fonts)):
            self.fonts[i] = self.AsciiFont(r)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'AsciiFont File')),
        MetaInfo('AsciiFont', items = [
            MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_BodyConverter
class Binary_BodyConverter(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_BodyConverter(r)

    #region Records

    table1: list[int]
    table2: list[int]
    table3: list[int]
    table4: list[int]

    def contains(body: int) -> bool:
          return True if self.table1 & body >= 0 & body < len(self.table1) & self.table1[body] != -1 else \
          True if self.table2 & body >= 0 & body < len(self.table2) & self.table2[body] != -1 else \
          True if self.table3 & body >= 0 & body < len(self.table3) & self.table3[body] != -1 else \
          True if self.table4 & body >= 0 & body < len(self.table4) & self.table4[body] != -1 else \
          False
    
    def convert(body: int) -> (int, int):
        if self.table1 & body >= 0 & body < len(self.table1):
            val = self.table1[body]
            if val != -1: return (2, val)
        if self.table2 & body >= 0 & body < len(self.table2):
            val = self.table2[body]
            if val != -1: return (3, val)
        if self.table3 & body >= 0 & body < len(self.table3):
            val = self.table3[body]
            if val != -1: return (4, val)
        if self.table4 & body >= 0 & body < len(self.table4):
            val = self.table4[body]
            if val != -1: return (5, val)
        return (1, body)

    def getTrueBody(fileType: int, index: int) -> int:
        match fileType:
            case 1: return index
            case 2:
                if self.table1 & index >= 0:
                    for i in range(len(self.table1)):
                        if self.table1[i] == index: return i
            case 3:
                if self.table2 & index >= 0:
                    for i in range(len(self.table2)):
                        if self.table2[i] == index: return i
            case 4:
                if self.table3 & index >= 0:
                    for i in range(len(self.table3)):
                        if self.table3[i] == index: return i
            case 5:
                if self.table4 & index >= 0:
                    for i in range(len(self.table4)):
                        if self.table4[i] == index: return i
            case _: return index
        return -1

    #endregion

    def __init__(self, r: Reader):
        list1 = []; list2 = []; list3 = []; list4 = []
        max1 = max2 = max3 = max4 = 0

        line: str
        while (line := r.readLine()):
            line = line.strip()
            if not line or line.startswith('#') or line.startswith('"#'): continue

            # try:
            split = re.split('\t| ', line)
            hasOriginalBodyId = split[0].isdecimal()
            if not hasOriginalBodyId: continue
            original = int(split[0])

            anim2 = int(split[1]) if split[1].isdecimal() else -1
            anim3 = int(split[2]) if split[2].isdecimal() else -1
            anim4 = int(split[3]) if split[3].isdecimal() else -1
            anim5 = int(split[4]) if split[4].isdecimal() else -1

            if anim2 != -1:
                if anim2 == 68: anim2 = 122
                if original > max1: max1 = original
                list1.append(original)
                list1.append(anim2)
            if anim3 != -1:
                if original > max2: max2 = original
                list2.append(original)
                list2.append(anim3)
            if anim4 != -1:
                if original > max3: max3 = original
                list3.append(original)
                list3.append(anim4)
            if anim5 != -1:
                if original > max4: max4 = original
                list4.append(original)
                list4.append(anim5)
            # except: pass

            self.table1 = [-1]*(max1 + 1)
            for i in range(0, len(list1), 2): self.table1[list1[i]] = list1[i + 1]

            self.table2 = [-1]*(max2 + 1)
            for i in range(0, len(list2), 2): self.table2[list2[i]] = list2[i + 1]

            self.table3 = [-1]*(max3 + 1)
            for i in range(0, len(list3), 2): self.table3[list3[i]] = list3[i + 1]

            self.table4 = [-1]*(max4 + 1)
            for i in range(0, len(list4), 2): self.table4[list4[i]] = list4[i + 1]

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'BodyConverter Config')),
        MetaInfo('BodyConverter', items = [
            MetaInfo(f'Table1: {len(self.table1)}'),
            MetaInfo(f'Table2: {len(self.table2)}'),
            MetaInfo(f'Table3: {len(self.table3)}'),
            MetaInfo(f'Table4: {len(self.table4)}'),
            ])
        ]

# Binary_BodyTable
class Binary_BodyTable(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_BodyTable(r)

    #region Records

    class Record:
        def __init__(self, oldId: int, newId: int, newHue: int):
            self.oldId = oldId
            self.newId = newId
            self.newHue = newHue

    records: dict[int, Record] = {}
    
    #endregion

    def __init__(self, r: Reader):
        line: str
        while (line := r.readLine()):
            line = line.strip()
            if not line or line.startswith('#') or line.startswith('"#'): continue

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'BodyTable config)),
        MetaInfo('BodyTable', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_CalibrationInfo
class Binary_CalibrationInfo(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_CalibrationInfo(r)

    #region Records

    #endregion

    def __init__(self, r: Reader):
        pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'CalibrationInfo File')),
        MetaInfo('CalibrationInfo', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_XX
class Binary_XX(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_XX(r)

    #region Records

    #endregion

    def __init__(self, r: Reader):
        pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'XX File')),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_XX
class Binary_XX(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_XX(r)

    #region Records

    #endregion

    def __init__(self, r: Reader):
        pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'XX File')),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_XX
class Binary_XX(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_XX(r)

    #region Records

    #endregion

    def __init__(self, r: Reader):
        pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'XX File')),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_XX
class Binary_XX(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_XX(r)

    #region Records

    #endregion

    def __init__(self, r: Reader):
        pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'XX File')),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_XX
class Binary_XX(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_XX(r)

    #region Records

    #endregion

    def __init__(self, r: Reader):
        pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'XX File')),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_XX
class Binary_XX(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_XX(r)

    #region Records

    #endregion

    def __init__(self, r: Reader):
        pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'XX File')),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]

# Binary_XX
class Binary_XX(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_XX(r)

    #region Records

    #endregion

    def __init__(self, r: Reader):
        pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'XX File')),
        MetaInfo('XX', items = [
            # MetaInfo(f'Fonts: {len(self.fonts)}')
            ])
        ]
