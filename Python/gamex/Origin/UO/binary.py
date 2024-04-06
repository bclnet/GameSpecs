import os, re, struct, numpy as np
from io import BytesIO
from openstk.gfx_texture import ITexture, TextureGLFormat, TextureGLPixelFormat, TextureGLPixelType, TextureUnityFormat, TextureUnrealFormat
from gamex.filesrc import FileSource
from gamex.pak import PakBinary
from gamex.meta import MetaInfo, MetaContent, IHaveMetaInfo

# typedefs
class Reader: pass
class BinaryPakFile: pass
class PakFile: pass
class MetaManager: pass
class TextureFlags: pass

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

            try:
                split = [x for x in re.split('\t| ', line) if x]
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
            except: pass

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

            try:
                index1 = line.find('{')
                index2 = line.find('}')

                param1 = line[:index1]
                param2 = line[index1 + 1: index2]
                param3 = line[(index2 + 1):]

                indexOf = param2.find(',')
                if indexOf > -1: param2 = param2[:indexOf].strip()

                oldId = int(param1)
                newId = int(param2)
                newHue = int(param3)
                self.records[oldId] = self.Record(oldId, newId, newHue)
            except: pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'BodyTable config')),
        MetaInfo('BodyTable', items = [
            MetaInfo(f'Records: {len(self.records)}')
            ])
        ]

# Binary_CalibrationInfo
class Binary_CalibrationInfo(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_CalibrationInfo(r)

    #region Records

    class Record:
        def __init__(self, mask: bytes, vals: bytes, detX: bytes, detY: bytes, detZ: bytes, detF: bytes):
            self.mask = mask
            self.vals = vals
            self.detX = detX
            self.detY = detY
            self.detZ = detZ
            self.detF = detF

    records: list[Record] = []

    defaultRecords: list[Record] = [
        Record(
            # Post 7.0.4.0 (Andreew)
            bytes([
                0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF,
                0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF
            ]),
            bytes([
                0xFF, 0xD0, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x11, 0x8B,
                0x82, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xD0, 0x5B, 0x83, 0x00, 0x00, 0x00, 0x00, 0x00, 0xEC
            ]),
            bytes([ 0x22, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x0C ]), # x
            bytes([ 0x22, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x08 ]), # y
            bytes([ 0x22, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x04 ]), # z
            bytes([ 0x22, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x10 ])),# f
        Record(
            # (arul) 6.0.9.x+ : Calibrates both
            bytes([
                0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF,
                0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF
            ]),
            bytes([
                0xFF, 0xD0, 0xE8, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x11, 0x8B,
                0x82, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xD0, 0x5E, 0xE9, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x0D
            ]),
            bytes([ 0x1F, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x0C ]),
            bytes([ 0x1F, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x08 ]),
            bytes([ 0x1F, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x04 ]),
            bytes([ 0x1F, 0x04, 0xFF, 0xFF, 0xFF, 0x04, 0x10 ])),
        Record(
            # Facet
            bytes([
                0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
            ]),
            bytes([
                0xA0, 0x00, 0x00, 0x00, 0x00, 0x84, 0xC0, 0x0F, 0x85, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x0D
            ]),
            bytes([]),
            bytes([]),
            bytes([]),
            bytes([ 0x01, 0x04, 0xFF, 0xFF, 0xFF, 0x01 ])),
        Record(
            # Location
            bytes([
                0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0x00, 0x00,
                0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x00
            ]),
            bytes([
                0x8B, 0x15, 0x00, 0x00, 0x00, 0x00, 0x83, 0xC4, 0x10, 0x66, 0x89, 0x5A, 0x00, 0xA1, 0x00, 0x00,
                0x00, 0x00, 0x66, 0x89, 0x78, 0x00, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x66, 0x89, 0x71, 0x00
            ]),
            bytes([ 0x02, 0x04, 0x04, 0x0C, 0x01, 0x02 ]),
            bytes([ 0x0E, 0x04, 0x04, 0x15, 0x01, 0x02 ]),
            bytes([ 0x18, 0x04, 0x04, 0x1F, 0x01, 0x02 ]),
            bytes([])),
        Record(
            # UO3D Only, calibrates both
            bytes([
                0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF,
                0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00
            ]),
            bytes([
                0xA1, 0x00, 0x00, 0x00, 0x00, 0x68, 0x40, 0x2E, 0x04, 0x01, 0x0F, 0xBF, 0x50, 0x00, 0x0F, 0xBF,
                0x48, 0x00, 0x52, 0x51, 0x0F, 0xBF, 0x50, 0x00, 0x52, 0x8D, 0x85, 0xE4, 0xFD, 0xFF, 0xFF, 0x68,
                0x00, 0x00, 0x00, 0x00, 0x50, 0xE8, 0x07, 0x44, 0x10, 0x00, 0x8A, 0x0D, 0x00, 0x00, 0x00, 0x00
            ]),
            bytes([ 0x01, 0x04, 0x04, 0x17, 0x01, 0x02 ]),
            bytes([ 0x01, 0x04, 0x04, 0x11, 0x01, 0x02 ]),
            bytes([ 0x01, 0x04, 0x04, 0x0D, 0x01, 0x02 ]),
            bytes([ 0x2C, 0x04, 0xFF, 0xFF, 0xFF, 0x01 ]))
        ]

    #endregion

    def __init__(self, r: Reader):
        line: str
        while (line := r.readLine()):
            line = line.strip()
            if line.lower() != 'begin': continue

            mask, vals, detx, dety, detz, detf
            if (mask := ReadBytes(r)) == None: continue
            if (vals := ReadBytes(r)) == None: continue
            if (detx := ReadBytes(r)) == None: continue
            if (dety := ReadBytes(r)) == None: continue
            if (detz := ReadBytes(r)) == None: continue
            if (detf := ReadBytes(r)) == None: continue
            self.records.append(self.Record(mask, vals, detx, dety, detz, detf))
        self.records += self.defaultRecords

    @staticmethod
    def readBytes(r: Reader) -> bytes:
        line = r.readLine()
        if not line: return None

        b = bytes((line.Length + 2) / 3)
        index = 0
        for i in range(0, line.length + 1, 3):
            ch = line[i + 0]
            cl = line[i + 1]

            if ch >= '0' & ch <= '9': ch -= '0'
            elif ch >= 'a' & ch <= 'f': ch -= ('a' - 10)
            elif ch >= 'A' & ch <= 'F': ch -= ('A' - 10)
            else: return None

            if cl >= '0' & cl <= '9': cl -= '0'
            elif cl >= 'a' & cl <= 'f': cl -= ('a' - 10)
            elif cl >= 'A' & cl <= 'F': cl -= ('A' - 10)
            else: return None

            b[index] = ((ch << 4) | cl) & 0xff; index += 1
        return b

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'CalibrationInfo File')),
        MetaInfo('CalibrationInfo', items = [
            MetaInfo(f'Records: {len(self.records)}')
            ])
        ]

# Binary_Gump
class Binary_Gump(IHaveMetaInfo, ITexture):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Gump(r, f.fileSize, f.compressed)

    #region Records
    
    format: list[object] = [
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            (TextureGLFormat.Rgba, TextureGLPixelFormat.Bgra, TextureGLPixelType.UnsignedShort1555Reversed),
            TextureUnityFormat.Unknown,
            TextureUnrealFormat.Unknown
            ]

    #endregion

    def __init__(self, r: Reader, length: int, extra: int):
        width = self.width = (extra >> 16) & 0xFFFF
        height = self.height = extra & 0xFFFF
        self.pixels = []
        if width <= 0 | height <= 0: return
        self.load(r.read(length), width, height)

    def load(self, data: bytes, width: int, height: int) -> None:
        bd = self.pixels = bytearray(width * height << 1)
        lookup = np.frombuffer(data, dtype = np.int32); lookup_ = 0; 
        dat = np.frombuffer(data, dtype = np.uint16)
        line = 0
        for y in range(0, height):
            count = lookup[lookup_] << 1; lookup_ += 1
            cur = line; end = line + width
            while cur < end:
                color = dat[count]; count += 1
                next = cur + dat[count]; count += 1

                if color == 0: cur = next
                else:
                    color ^= 0x8000
                    cur2 = cur << 1
                    while cur < next: bd[cur2:cur2+1] = color.tobytes(); cur += 1

    data: dict[str, object] = None
    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 1
    flags: TextureFlags = 0

    def begin(self, platform: int) -> (bytes, object, list[object]):
        match platform:
            case Platform.Type.OpenGL: format = Binary_Gump.format[1]
            case Platform.Type.Vulken: format = Binary_Gump.format[2]
            case Platform.Type.Unity: format = Binary_Gump.format[3]
            case Platform.Type.Unreal: format = Binary_Gump.format[4]
            case _: raise Exception('Unknown {platform}')
        return self.pixels, format, None
    def end(self): pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Gump', items = [
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}')
            ])
        ]

# Binary_GumpDef
class Binary_GumpDef(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_GumpDef(r)

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

# Binary_Hues
class Binary_Hues(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Hues(r)

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

# Binary_Land
class Binary_Land(IHaveMetaInfo):
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

# Binary_Light
class Binary_Light(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Light(r)

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

# Binary_MobType
class Binary_MobType(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_MobType(r)

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

# Binary_MultiMap
class Binary_MultiMap(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_MultiMap(r)

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

# Binary_MusicDef
class Binary_MusicDef(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_MusicDef(r)

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

# Binary_Multi
class Binary_Multi(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Multi(r)

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

# Binary_RadarColor
class Binary_RadarColor(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_RadarColor(r)

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

# Binary_SkillGroups
class Binary_SkillGroups(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_SkillGroups(r)

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

# Binary_Skills
class Binary_Skills(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Skills(r)

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

# Binary_Sound
class Binary_Sound(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Sound(r)

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

# Binary_SpeechList
class Binary_SpeechList(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_SpeechList(r)

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

# Binary_Static
class Binary_Static(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Static(r)

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

# Binary_StringTable
class Binary_StringTable(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_StringTable(r)

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

# Binary_TileData
class Binary_TileData(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_TileData(r)

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

# Binary_UnicodeFont
class Binary_UnicodeFont(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_UnicodeFont(r)

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

# Binary_Verdata
class Binary_Verdata(IHaveMetaInfo):
    @staticmethod
    def factory(r: Reader, f: FileSource, s: PakFile): return Binary_Verdata(r)

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
