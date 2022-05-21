using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameSpec.Unity.Formats
{
    /// <summary>
    /// PakBinaryUnity
    /// </summary>
    /// <seealso cref="GameSpec.Formats.PakBinary" />
    public unsafe class PakBinaryUnity : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryUnity();
        readonly byte[] Key;
        public PakBinaryUnity(byte[] key = null) => Key = key;

        // File : ASSETS
        #region File : ASSETS

        internal class AssetsFile
        {
            public class FileInfo
            {
                public ulong Index;                     // 0x00 version < 0x0E : only uint32_t
                public long OffsCurFile;                // 0x08, version < 0x16 : only uint32_t
                public uint CurFileSize;                // 0x0C
                public uint CurFileTypeOrIndex;         // 0x10, starting with version 0x10, this is an index into the type tree
                                                        // inheritedUnityClass : for Unity classes, this is curFileType; for MonoBehaviours, this is 114
                                                        // version < 0x0B : inheritedUnityClass is uint, no scriptIndex exists
                public ushort InheritedUnityClass;      // 0x14, (MonoScript) only version < 0x10
                                                        // scriptIndex : for Unity classes, this is 0xFFFF;
                                                        // for MonoBehaviours, this is an index of the mono class, counted separately for each .assets file
                public ushort ScriptIndex;              // 0x16, only version <= 0x10
                public byte Unknown1;					// 0x18, only 0x0F <= version <= 0x10 //with alignment always a uint32_t

                public FileInfo(BinaryReader r, uint version, bool bigEndian)
                {
                    if (version >= 0x0E) r.Align();
                    Index = version >= 0x0E ? r.ReadUInt64E(bigEndian) : r.ReadUInt32E(bigEndian);
                    OffsCurFile = version >= 0x16 ? (long)r.ReadUInt64E(bigEndian) : r.ReadUInt32E(bigEndian);
                    CurFileSize = r.ReadUInt32E(bigEndian);
                    CurFileTypeOrIndex = r.ReadUInt32E(bigEndian);
                    InheritedUnityClass = version < 0x10 ? r.ReadUInt16E(bigEndian) : (ushort)0;
                    if (version < 0x0B) r.Skip(2);
                    ScriptIndex = version >= 0x0B && version <= 0x10 ? r.ReadUInt16E(bigEndian) : (ushort)0xffff;
                    Unknown1 = version >= 0x0F && version <= 0x10 ? r.ReadByte() : (byte)0;
                }

                public static int GetSize(int count, uint version)
                {
                    var sizePerFile = GetSize(version);
                    if (count == 0) return 0;
                    else if (version < 0x0F || version > 0x10) return count * sizePerFile;
                    else return ((sizePerFile + 3) & (~3)) * (count - 1) + sizePerFile;
                }
                public static int GetSize(uint version)
                {
                    if (version >= 0x16) return 24;
                    else if (version >= 0x11) return 20;
                    else if (version >= 0x10) return 23;
                    else if (version >= 0x0F) return 25;
                    else if (version == 0x0E) return 24;
                    else if (version >= 0x0B) return 20;
                    else return 20; //if (version >= 0x07)
                }
            }

            public struct FileHeader                           // Always big-endian
            {
                public ulong Unknown00;                 // 0x00 //format >= 0x16 only. Always 0?
                public uint Format;                     // 0x08
                public long MetadataSize;               // 0x10 //format < 0x16: uint32_t @ 0x00;
                public long FileSize;                   // 0x18 //format < 0x16: uint32_t @ 0x04;
                public long OffsFirstFile;              // 0x20 //format < 0x16: uint32_t @ 0x0C;
                                                        // 0 == little-endian; 1 == big-endian
                public bool BigEndian;                  // 0x20, for format < 0x16 @ 0x10, for format < 9 at (fileSize - metadataSize) right before TypeTree
                public byte[] Unknown;                  // 0x21, for format < 0x16 @ 0x11, exists for format >= 9

                public FileHeader(BinaryReader r)
                {
                    var beginPosition = r.Position();
                    var dw00 = r.ReadUInt32E();
                    var dw04 = r.ReadUInt32E();
                    Format = r.ReadUInt32E();
                    var dw0C = r.ReadUInt32E();
                    if (Format >= 0x16)
                    {
                        Unknown00 = (dw00 << 32) | dw04;
                        // dw0C is padding for format >= 0x16
                        MetadataSize = (long)r.ReadUInt64E();
                        FileSize = (long)r.ReadUInt64E();
                        OffsFirstFile = (long)r.ReadUInt64E();
                        BigEndian = r.ReadByte() == 1;
                        Unknown = r.ReadBytes(3);
                        r.Skip(4); // Padding
                    }
                    else
                    {
                        Unknown00 = 0;
                        MetadataSize = dw00;
                        FileSize = dw04;
                        OffsFirstFile = dw0C;
                        if (Format < 9 && FileSize > MetadataSize) { BigEndian = r.PeekAt(beginPosition + FileSize - MetadataSize, _ => _.ReadByte()) == 1; Unknown = new byte[3]; }
                        else { BigEndian = r.ReadBoolean(); Unknown = r.ReadBytes(3); }
                    }
                }
            };

            public struct FileDependency
            {
                // version < 6 : no bufferedPath
                // version < 5 : no bufferedPath, guid, type
                public string BufferedPath; // for buffered (type=1)
                public Guid Guid;
                public uint Type;
                public string AssetPath; // path to the .assets file

                public FileDependency(BinaryReader r, uint format, bool bigEndian)
                {
                    BufferedPath = format >= 6 ? r.ReadZASCII(1000) : null;
                    Guid = format >= 5 ? r.ReadGuid() : Guid.Empty;
                    Type = format >= 5 ? r.ReadUInt32E(bigEndian) : 0U;
                    AssetPath = r.ReadZASCII(1000);
                }
            }

            public class TypeField_0D
            {
                ushort Version;         // 0x00
                byte Depth;             // 0x02 //specifies the amount of parents
                                        // 0x01 : IsArray
                                        // 0x02 : IsRef
                                        // 0x04 : IsRegistry
                                        // 0x08 : IsArrayOfRefs
                byte IsArray;           // 0x03 //actually a bool for format <= 0x12, uint8_t since format 0x13
                uint TypeStringOffset;  // 0x04
                uint NameStringOffset;  // 0x08
                uint Size;              // 0x0C //size in bytes; if not static (if it contains an array), set to -1
                uint Index;             // 0x10
                                        // 0x0001 : is invisible(?), set for m_FileID and m_PathID; ignored if no parent field exists or the type is neither ColorRGBA, PPtr nor string
                                        // 0x0100 : ? is bool
                                        // 0x1000 : ?
                                        // 0x4000 : align bytes
                                        // 0x8000 : any child has the align bytes flag
                                        //=> if flags & 0xC000 and size != 0xFFFFFFFF, the size field matches the total length of this field plus its children.
                                        // 0x400000 : ?
                                        // 0x800000 : ? is non-primitive type
                                        // 0x02000000 : ? is UInt16 (called char)
                                        // 0x08000000 : has fixed buffer size? related to Array (i.e. this field or its only child or its father is an array), should be set for vector, Array and the size and data fields.
                uint Flags;             // 0x14 
                byte[] Unknown1;        // 0x18 //since format 0x12

                public TypeField_0D(BinaryReader r, uint format, bool bigEndian)
                {

                }
                //string GetTypeString(const char* stringTable, int stringTableLen);
                //string GetNameString(const char* stringTable, int stringTableLen);
            } // 0x18

            public class Type_0D //everything big endian
            {
                // Starting with U5.5, all MonoBehaviour types have MonoBehaviour's classId (114)
                // Before, the different MonoBehaviours had different negative classIds, starting with -1
                public int ClassId;                     // 0x00

                public byte Unknown16_1;                // format >= 0x10
                public ushort ScriptIndex;              // format >= 0x11 U5.5+, index to the MonoManager (usually 0xFFFF)

                // Script ID (md4 hash)
                public Guid ScriptIDHash;               // if classId < 0, 0x04..0x13

                // Type hash / properties hash (md4)
                public Guid TypeHash;                   // 0x04..0x13 or 0x14..0x23

                public TypeField_0D[] TypeFields;       // if (TypeTree.enabled), 0x14 or 0x24

                public string[] Strings;                // if (TypeTree.enabled), 0x18 or 0x28

                // For types from assetsFile.pSecondaryTypeList :
                public uint[] Deps;                     // format >= 0x15

                // For types in assetsFile.typeTree :
                public string[] Headers;                // format >= 0x15

                public Type_0D(bool hasTypeTree, BinaryReader r, uint version, bool bigEndian, bool secondaryTypeTree = false)
                {
                    ClassId = r.ReadInt32E(bigEndian);
                    Unknown16_1 = version >= 16 ? r.ReadByte() : (byte)0;
                    ScriptIndex = version >= 17 ? r.ReadUInt16E(bigEndian) : (ushort)0xffff;
                    if (ClassId < 0 || ClassId == 0x72 || ClassId == 0x7C90B5B3 || ((short)ScriptIndex) >= 0) ScriptIDHash = r.ReadGuid(); // MonoBehaviour
                    TypeHash = r.ReadGuid();
                    if (!hasTypeTree) return;

                    // has tree type
                    var dwVariableCount = (int)r.ReadUInt32E(bigEndian);
                    var dwStringTableLen = (int)r.ReadUInt32E(bigEndian);
                    var variableFieldsLen = dwVariableCount * (version >= 0x12 ? 32 : 24);
                    var typeTreeLen = variableFieldsLen + dwStringTableLen;

                    // read fields
                    var treeBuffer = r.ReadBytes(typeTreeLen);
                    using var tr = new BinaryReader(new MemoryStream());
                    TypeFields = tr.ReadTArray(_ => new TypeField_0D(tr, version, bigEndian), dwVariableCount);

                    // read strings
                    var appendNullTerminator = typeTreeLen == 0 || treeBuffer[typeTreeLen - 1] != 0;
                    tr.Position(variableFieldsLen);
                    var stringTable = tr.ReadZASCIIList();
                    if (appendNullTerminator) stringTable.Add(null);
                    Strings = stringTable.ToArray();

                    // read secondary
                    if (version >= 0x15)
                    {
                        //var depListLen = (int)r.ReadUInt32E(bigEndian); Deps = depListLen >= 0 ? r.ReadTArray(_ => r.ReadUInt32E(bigEndian)), depListLen) : new uint[0];
                        if (!secondaryTypeTree) Deps = r.ReadL32EArray<uint>(4, bigEndian);
                        else Headers = r.ReadZASCIIList().ToArray();
                    }
                }
            }

            public class TypeField_07 // everything big endian
            {
                public string Type; //null-terminated
                public string Name; //null-terminated
                public uint Size;
                public uint Index;
                public uint ArrayFlag;
                public uint Flags1;
                public uint Flags2; // Flag 0x4000 : align to 4 bytes after this field.
                public TypeField_07[] Children;

                public TypeField_07(bool hasTypeTree, BinaryReader r, uint version, bool bigEndian)
                {
                    Type = r.ReadZASCII(256);
                    Name = r.ReadZASCII(256);
                    Size = r.ReadUInt32E(bigEndian);
                    if (version == 2) r.Skip(4);
                    else if (version == 3) Index = unchecked((uint)-1);
                    else Index = r.ReadUInt32E(bigEndian);
                    ArrayFlag = r.ReadUInt32E(bigEndian);
                    Flags1 = r.ReadUInt32E(bigEndian);
                    Flags2 = version == 3 ? unchecked((uint)-1) : r.ReadUInt32E(bigEndian);
                    if (hasTypeTree) Children = r.ReadL32EArray((_, b) => new TypeField_07(true, r, version, bigEndian), bigEndian);
                }
            }

            public struct Type_07
            {
                public int ClassId; // big endian
                public TypeField_07 Base;

                public Type_07(bool hasTypeTree, BinaryReader r, uint version, bool bigEndian)
                {
                    ClassId = r.ReadInt32E(bigEndian);
                    Base = new TypeField_07(hasTypeTree, r, version, bigEndian);
                }
            }

            public struct Preload
            {
                public uint FileId;
                public ulong PathId;

                public Preload(BinaryReader r, uint format, bool bigEndian)
                {
                    FileId = r.ReadUInt32E(bigEndian);
                    if (format >= 0x0E) r.Align();
                    PathId = format >= 0x0E ? r.ReadUInt64E(bigEndian) : r.ReadUInt32E(bigEndian);
                }
            }

            public class TypeTree
            {
                public uint _fmt;                       // not stored here in the .assets file, the variable is just to remember the .assets file version
                // The actual 4-byte-alignment base starts here. Using the header as the base still works since its length is 20.
                public string UnityVersion;             // null-terminated; stored for .assets format > 6
                public uint Platform;                   // big endian; stored for .assets format > 6
                public bool HasTypeTree;                // stored for .assets format >= 13; Unity 5 only stores some metadata if it's set to false
                public int FieldCount;                 // big endian;
                public Type_0D[] Types_Unity5;
                public Type_07[] Types_Unity4;
                public uint dwUnknown;                  // actually belongs to the asset list; stored for .assets format < 14

                public TypeTree(BinaryReader r, uint version, bool bigEndian) // Minimum AssetsFile format : 6
                {
                    _fmt = version;
                    HasTypeTree = true;
                    if (version > 6)
                    {
                        UnityVersion = r.ReadZASCII(64);
                        if (UnityVersion[0] < '0' || UnityVersion[0] > '9') { FieldCount = 0; return; }
                        Platform = r.ReadUInt32E(bigEndian);
                    }
                    else
                    {
                        Platform = 0;
                        if (version == 6) UnityVersion = "Unsupported 2.6+";
                        else if (version == 5) UnityVersion = "Unsupported 2.0+";
                        else UnityVersion = "Unsupported Unknown";
                        FieldCount = 0; // not supported
                        return;
                    }

                    if (version >= 0x0D) HasTypeTree = r.ReadBoolean(); // Unity 5
                    FieldCount = (int)r.ReadUInt32E(bigEndian);
                    if (FieldCount > 0)
                    {
                        if (version < 0x0D) Types_Unity4 = r.ReadTArray(_ => new Type_07(HasTypeTree, r, version, bigEndian), FieldCount);
                        else Types_Unity5 = r.ReadTArray(_ => new Type_0D(HasTypeTree, r, version, bigEndian), FieldCount);
                    }
                    // actually belongs to the asset file info tree
                    dwUnknown = version < 0x0E ? r.ReadUInt32E(bigEndian) : 0;
                }
            }

            public bool Success;
            public FileHeader Header;
            public TypeTree Tree;
            public long AssetTablePos;
            public int AssetCount;
            public Preload[] Preloads;
            public FileDependency[] Dependencies;
            public Type_0D[] SecondaryTypes;       // format >= 0x14
            public string Unknown;                 // format >= 5; seemingly always empty

            public AssetsFile(BinaryReader r) //: was:AssetsFile(IAssetsReader *pReader)
            {
                Header = new FileHeader(r); var format = Header.Format; var bigEndian = Header.BigEndian;
                // simple validity check
                if (format == 0 || format > 0x40) throw new FormatException("Bad Header");
                if (format < 9) r.Position(Header.FileSize - Header.MetadataSize + 1);
                Tree = new TypeTree(r, format, bigEndian);
                if (Tree.UnityVersion[0] < '0' || Tree.UnityVersion[0] > '9') throw new FormatException("Bad Version");
                AssetTablePos = r.Position();
                //
                AssetCount = (int)r.ReadUInt32E(bigEndian);
                if (format >= 0x0E && AssetCount > 0) r.Align();
                r.Skip(FileInfo.GetSize(AssetCount, format));
                //
                Preloads = format >= 0x0B ? r.ReadL32EArray((_, b) => new Preload(r, format, bigEndian), bigEndian) : new Preload[0];
                Dependencies = r.ReadL32EArray((_, b) => new FileDependency(r, format, bigEndian), bigEndian);
                SecondaryTypes = format >= 0x14 ? r.ReadL32EArray((_, b) => new Type_0D(Tree.HasTypeTree, r, format, bigEndian), bigEndian) : new Type_0D[0];
                Unknown = r.ReadZASCII();
                // verify
                Success = Verify(r);
            }

            bool Verify(BinaryReader r)
            {
                string errorData = null;
                var format = Header.Format; var bigEndian = Header.BigEndian;
                if (format == 0 || format > 0x40) { errorData = "Invalid file format"; goto _fileFormatError; }
                if (Tree.UnityVersion[0] == 0 || Tree.UnityVersion[0] < '0' || Tree.UnityVersion[0] > '9') { errorData = $"Invalid version string of {Tree.UnityVersion}"; goto _fileFormatError; }
                Log($"INFO: The .assets file was built for Unity {Tree.UnityVersion}.");
                if (format > 0x16 || format < 0x08) Log("WARNING: AssetsTools (for .assets versions 8-22) wasn't tested with this .assets' version, likely parsing or writing the file won't work properly!");

                r.Position(AssetTablePos);
                var fileInfos = r.ReadL32EArray((_, b) => new FileInfo(_, format, bigEndian), bigEndian);
                Log($"INFO: The .assets file has {fileInfos.Length} assets (info list : {FileInfo.GetSize(format)} bytes)");
                if (fileInfos.Length > 0)
                {
                    if (Header.MetadataSize < 8) { errorData = "Invalid metadata size"; goto _fileFormatError; }
                    var lastFileInfo = fileInfos[^1];
                    if ((Header.OffsFirstFile + lastFileInfo.OffsCurFile + lastFileInfo.CurFileSize - 1) < Header.MetadataSize) { errorData = "Last asset begins before the header ends"; goto _fileFormatError; };
                    if (r.ReadBytesAt(Header.OffsFirstFile + lastFileInfo.OffsCurFile + lastFileInfo.CurFileSize - 1, 1).Length != 1) { errorData = "File data are cut off"; goto _fileFormatError; }
                }
                Log("SUCCESS: The .assets file seems to be ok!");
                return true;
            _fileFormatError:
                Log($"ERROR: Invalid .assets file (error message : '{errorData}')!");
                return false;
            }

            public AssetsTable CreateTable(BinaryReader r) => new AssetsTable(this, r);
        }

        internal class AssetsTable
        {
            AssetsFile File;
            public FileInfo[] FileInfos;

            public class FileInfo : AssetsFile.FileInfo
            {
                public uint CurFileType;
                public long AbsolutePos;

                public FileInfo(AssetsFile file, BinaryReader r, uint version, bool bigEndian) : base(r, version, bigEndian)
                {
                    var tree = file.Tree;
                    if (version >= 0x10)
                    {
                        if (CurFileTypeOrIndex >= tree.FieldCount) { CurFileType = 0x80000000; InheritedUnityClass = 0xffff; ScriptIndex = 0xffff; }
                        else
                        {
                            var classId = tree.Types_Unity5[CurFileTypeOrIndex].ClassId;
                            if (tree.Types_Unity5[CurFileTypeOrIndex].ScriptIndex != 0xffff) { CurFileType = (uint)(-1 - tree.Types_Unity5[CurFileTypeOrIndex].ScriptIndex); InheritedUnityClass = (ushort)classId; ScriptIndex = tree.Types_Unity5[CurFileTypeOrIndex].ScriptIndex; }
                            else { CurFileType = (uint)classId; InheritedUnityClass = (ushort)classId; ScriptIndex = 0xffff; }
                        }
                    }
                    else CurFileType = CurFileTypeOrIndex;
                    AbsolutePos = file.Header.OffsFirstFile + OffsCurFile;
                }

                bool ReadName(BinaryReader r, AssetsFile file, out string name)
                {
                    name = null;
                    if (!HasName(CurFileType)) return false;
                    var bigEndian = file.Header.BigEndian;
                    r.Position(AbsolutePos);
                    var nameSize = (int)r.ReadUInt32E(bigEndian);
                    if (nameSize + 4 >= CurFileSize || nameSize >= 4092) return false;
                    var buf = r.ReadBytes(nameSize);
                    if (buf.Length != nameSize) return false;
                    for (var i = 0; i < nameSize; i++) if (buf[i] < 0x20) return false;
                    name = Encoding.ASCII.GetString(buf);
                    return true;
                }

                static bool HasName(uint type)
                {
                    switch (type)
                    {
                        case 21:
                        case 27:
                        case 28:
                        case 43:
                        case 48:
                        case 49:
                        case 62:
                        case 72:
                        case 74:
                        case 83:
                        case 84:
                        case 86:
                        case 89:
                        case 90:
                        case 91:
                        case 93:
                        case 109:
                        case 115:
                        case 117:
                        case 121:
                        case 128:
                        case 134:
                        case 142:
                        case 150:
                        case 152:
                        case 156:
                        case 158:
                        case 171:
                        case 184:
                        case 185:
                        case 186:
                        case 187:
                        case 188:
                        case 194:
                        case 200:
                        case 207:
                        case 213:
                        case 221:
                        case 226:
                        case 228:
                        case 237:
                        case 238:
                        case 240:
                        case 258:
                        case 271:
                        case 272:
                        case 273:
                        case 290:
                        case 319:
                        case 329:
                        case 363:
                        case 850595691:
                        case 1480428607:
                        case 687078895:
                        case 825902497:
                        case 2083778819:
                        case 1953259897:
                        case 2058629509: return true;
                        default: return false;
                    }
                }
            }

            public AssetsTable(AssetsFile file, BinaryReader r)
            {
                File = file;
                var format = file.Header.Format; var bigEndian = file.Header.BigEndian;
                r.Position(file.AssetTablePos);
                FileInfos = r.ReadL32EArray((_, b) => new FileInfo(file, _, format, bigEndian), bigEndian);
            }
        }

        #endregion

        // File : BUNDLE
        #region File : BUNDLE

        internal class BundleFile
        {
            [Flags]
            public enum HeaderFlag : uint
            {
                Compressed = 0x3F,          // (flags & 0x3F) is the compression mode (0 = none; 1 = LZMA; 2-3 = LZ4)
                HasDirectoryInfo = 0x40,    // (flags & 0x40) says whether the bundle has directory info
                ListAtEnd = 0x80,           // (flags & 0x80) says whether the block and directory list is at the end
                Unknown = 0x100,
            }

            public class Directory
            {
                public ulong Offset;
                public ulong DecompressedSize;
                public uint Flags;
                public string Name;
            }

            public class Block
            {
                public uint DecompressedSize;
                public uint CompressedSize;
                public ushort Flags;
            }

            public class BlockAndDirectory
            {
                public ulong ChecksumLow;
                public ulong ChecksumHigh;
                public Block[] Blocks;
                public Directory[] Directories;

                public BlockAndDirectory(BinaryReader r)
                {
                    ChecksumLow = r.ReadUInt64();
                    ChecksumHigh = r.ReadUInt64();
                    Blocks = r.ReadL32EArray((_, b) => new Block
                    {
                        DecompressedSize = _.ReadUInt32E(),
                        CompressedSize = _.ReadUInt32E(),
                        Flags = _.ReadUInt16E(),
                    });
                    Directories = r.ReadL32EArray((_, b) => new Directory
                    {
                        Offset = _.ReadUInt64E(),
                        DecompressedSize = _.ReadUInt64E(),
                        Flags = _.ReadUInt32E(),
                        Name = _.ReadZASCII(400),
                    });
                }
            }

            public bool Success;
            public string Signature;                    // UnityFS, UnityRaw, UnityWeb or UnityArchive
            public uint FileVersion;                    // 3, 6, 7
            public string MinPlayerVersion;             // 5.x.x
            public string FileEngineVersion;            // exact unity engine version
            public ulong FileSize;
            public Block[] Blocks3;
            public Directory[] Directories3;
            public BlockAndDirectory BlockAndDirectory6;
            // Unity3
            public uint MinimumStreamedBytes;           // not always the file's size
            public uint DataOffs;
            public uint NumberOfAssetsToDownload;
            public uint Unknown2;                       // for fileVersion >= 3
            public byte Unknown3;
            // Unity6 - 5.3+
            public uint CompressedSize;                 // sizes for the blocks info
            public uint DecompressedSize;               // sizes for the blocks info
            public HeaderFlag Flags;

            public long GetU6InfoOffset()
            {
                if ((Flags & HeaderFlag.ListAtEnd) != 0) return FileSize == 0 ? -1 : (long)(FileSize - CompressedSize);
                else
                {
                    //if (Signature == "UnityWeb" || Signature == "UnityRaw") return 9L;
                    var ret = MinPlayerVersion.Length + FileEngineVersion.Length + 0x1AL;
                    ret += (Flags & HeaderFlag.Unknown) != 0 ? 0x0A : Signature.Length + 1;
                    if (FileVersion >= 7) ret = (ret + 15) & ~15; // 16 byte alignment
                    return ret;
                }
            }

            public long GetU6DataOffset()
            {
                var ret = 0L;
                if (Signature == "UnityArchive") return (int)CompressedSize;
                else if (Signature == "UnityFS" || Signature == "UnityWeb")
                {
                    ret = MinPlayerVersion.Length + FileEngineVersion.Length + 0x1A;
                    ret += (Flags & HeaderFlag.Unknown) != 0 ? 0x0A : Signature.Length + 1;
                    if (FileVersion >= 7) ret = (ret + 15) & ~15; // 16 byte alignment
                }
                if ((Flags & HeaderFlag.ListAtEnd) == 0) ret += (int)CompressedSize;
                return ret;
            }

            public BundleFile(BinaryReader r)
            {
                Signature = r.ReadZASCII(13);
                FileVersion = Signature == "UnityArchive" ? 6 : r.ReadUInt32E();
                // early exit
                if (FileVersion >= 6)
                {
                    if (FileVersion != 6 && FileVersion != 7) { Log("That file version is unknown!"); return; }
                }
                else if (FileVersion == 3)
                {
                    if (!Signature.StartsWith("UnityRaw", StringComparison.OrdinalIgnoreCase) || !Signature.StartsWith("UnityWeb", StringComparison.OrdinalIgnoreCase)) { Log("AssetBundleHeader : Unknown file type!"); return; }
                }

                // parse remaining header
                MinPlayerVersion = r.ReadZASCII(24);
                FileEngineVersion = r.ReadZASCII(64);
                var hasCompression = false;
                if (FileVersion >= 6)
                {
                    FileSize = r.ReadUInt64E();
                    CompressedSize = r.ReadUInt32E();
                    DecompressedSize = r.ReadUInt32E();
                    Flags = (HeaderFlag)r.ReadUInt32E();
                    if (Signature == "UnityArchive") Flags |= HeaderFlag.HasDirectoryInfo;
                    else if (Signature == "UnityWeb") { Signature = "UnityFS"; Flags |= HeaderFlag.Unknown; }
                    else if (Signature == "UnityRaw") Flags |= HeaderFlag.HasDirectoryInfo;
                    // read asset
                    var ds = r.BaseStream;
                    ds.Position = GetU6InfoOffset();
                    if ((Flags & HeaderFlag.Compressed) != 0)
                    {
                        var compressionType = (byte)((int)Flags & 0x3F);
                        if (compressionType < 4)
                        {
                            hasCompression = true;
                            try
                            {
                                var decompressSuccess = false;
                                switch (compressionType)
                                {
                                    case 0: if (CompressedSize == DecompressedSize) decompressSuccess = true; break;
                                    case 1: ds = new MemoryStream(r.DecompressLzma((int)CompressedSize, (int)DecompressedSize)); decompressSuccess = true; break;
                                    case 2: case 3: ds = new MemoryStream(r.DecompressLz4((int)CompressedSize, (int)DecompressedSize)); decompressSuccess = true; break;
                                }
                                if (!decompressSuccess || ds.Length != DecompressedSize) return;
                            }
                            catch
                            {
                                Log("AssetBundleFile.Read : Failed to decompress the directory!");
                                throw;
                            }
                        }
                    }
                    if (hasCompression || (Flags & HeaderFlag.Compressed) == 0)
                    {
                        var dr = new BinaryReader(ds);
                        BlockAndDirectory6 = new BlockAndDirectory(dr);
                    }
                }
                else if (FileVersion == 3)
                {
                    MinimumStreamedBytes = r.ReadUInt32E();
                    DataOffs = r.ReadUInt32E();
                    NumberOfAssetsToDownload = r.ReadUInt32E();
                    Blocks3 = r.ReadL32EArray((_, b) => new Block
                    {
                        CompressedSize = _.ReadUInt32E(),
                        DecompressedSize = _.ReadUInt32E(),
                    });
                    if (FileVersion >= 2) FileSize = r.ReadUInt32E();
                    if (FileVersion >= 3) Unknown2 = r.ReadUInt32E();
                    Unknown3 = r.ReadByte();
                    //
                    if (Signature.StartsWith("UnityRaw")) // compressed bundles only have an uncompressed header
                    {
                        r.Position(DataOffs);
                        Directories3 = r.ReadL32EArray((_, b) => new Directory
                        {
                            Name = _.ReadZASCII(400),
                            Offset = _.ReadUInt32E(),
                            DecompressedSize = _.ReadUInt32E(),
                        });
                    }
                    else return;
                }
                else { Log("AssetBundleFile.Read : Unknown file version!"); return; }

                // verify
                Success = Verify();
            }

            void Write(BinaryWriter w)
            {
                w.WriteZASCII(Signature);
                w.WriteE(FileVersion);
                w.WriteZASCII(MinPlayerVersion);
                w.WriteZASCII(FileEngineVersion);
                if (FileVersion >= 6)
                {
                    w.WriteE(FileSize);
                    w.WriteE(CompressedSize);
                    w.WriteE(DecompressedSize);
                    w.WriteE((uint)Flags);
                    if (Signature == "UnityWeb" || Signature == "UnityRaw") w.Write((byte)0);
                    if (FileVersion >= 7) w.WriteAlign(16);
                }
                else if (FileVersion == 3)
                {
                    w.WriteE(DataOffs);
                    w.WriteE(NumberOfAssetsToDownload);
                    w.WriteL32EArray(Blocks3, (_, b, v) =>
                    {
                        _.WriteE(v.CompressedSize);
                        _.WriteE(v.DecompressedSize);
                    });
                    if (FileVersion >= 2) w.WriteE((uint)FileSize);
                    if (FileVersion >= 3) w.WriteE(Unknown2);
                    w.WriteE(Unknown3);
                }
            }

            bool Verify()
            {
                if (FileVersion >= 6)
                {
                    if (BlockAndDirectory6 == null)
                    {
                        if ((Flags & HeaderFlag.Compressed) != 0) return true;
                        Log("[ERROR] Unable to process the bundle directory.");
                        return false;
                    }
                    else
                    {
                        foreach (var block in BlockAndDirectory6.Blocks) if ((block.Flags & 0x3F) != 0) return true;
                        return true;
                    }
                }
                else if (FileVersion == 3)
                {
                    if (Signature == "UnityWeb") return true;
                    else if (Blocks3 == null) { Log("[ERROR] Unable to process the bundle directory."); return false; }
                    return true;
                }
                Log("Open as bundle: [ERROR] Unknown bundle file version.");
                return false;
            }

            bool Unpack()
            {
                using var w = new BinaryWriter(File.OpenWrite(@"C:\T_\temp"));
                if (FileVersion >= 6)
                {
                    var compressionType = (byte)((int)Flags & 0x3F);
                    if (compressionType >= 4) return false;
                    if ((Flags & HeaderFlag.Unknown) != 0) Signature = "UnityWeb";
                    Write(w);
                    if ((Flags & HeaderFlag.Unknown) != 0) Signature = "UnityFS";
                    var curFilePos = GetU6InfoOffset();
                    var curUpFilePos = curFilePos;
                    try
                    {
                        //var decompressSuccess = false;
                        //switch (compressionType)
                        //{
                        //    case 0: if (compressedSize == bundleHeader6.decompressedSize) decompressSuccess = true; break;
                        //    case 1: ds = new MemoryStream(r.DecompressLzma((int)CompressedSize, (int)DecompressedSize)); decompressSuccess = true; break;
                        //    case 2: case 3: ds = new MemoryStream(r.DecompressLz4((int)CompressedSize, (int)DecompressedSize)); decompressSuccess = true; break;
                        //}
                    }
                    catch
                    {
                        Log("AssetBundleFile.Read : Failed to decompress the directory!");
                        throw;
                    }
                }
                else if (FileVersion == 3)
                {
                    if (Signature != "UnityWeb") return false;
                }
                return false;
            }

            public BundleTable CreateTable(BinaryReader r) => new BundleTable(this, r);
        }

        internal class BundleTable
        {
            BundleFile File;

            public BundleTable(BundleFile file, BinaryReader r)
            {
                File = file;
                //var format = file.Header.Format; var bigEndian = file.Header.BigEndian;
                //r.Position(file.AssetTablePos);
                //FileInfos = r.ReadL32EArray((_, b) => new FileInfo(file, _, format, bigEndian), bigEndian);
            }
        }

        #endregion

        // File : RESOURCES
        #region File : RESOURCES

        internal class ResourcesFile
        {
            public bool Success;

            public ResourcesFile(BinaryReader r)
            {
            }
        }

        #endregion

        // File : GENERIC
        #region File : GENERIC

        internal class GenericFile
        {
        }

        #endregion

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            if (stage != ReadStage.File) throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());

            // try-bundle
            var bundleFile = new BundleFile(r);
            if (bundleFile.Success)
            {
                //var table = bundleFile.CreateTable(r);
                return Task.CompletedTask;
            }

            // try-asset
            r.Position(0);
            var assetsFile = new AssetsFile(r);
            if (assetsFile.Success)
            {
                var table = assetsFile.CreateTable(r);
                return Task.CompletedTask;
            }

            // try-resources
            r.Position(0);
            var resourcesFile = new ResourcesFile(r);
            if (resourcesFile.Success)
            {
                return Task.CompletedTask;
            }

            //var files = multiSource.Files = new List<FileMetadata>();
            //var pak = (P4kFile)(source.Tag = new P4kFile(r.BaseStream) { Key = Key });
            //foreach (ZipEntry entry in pak)
            //{
            //    var metadata = new FileMetadata
            //    {
            //        Path = entry.Name.Replace('\\', '/'),
            //        Crypted = entry.IsCrypted,
            //        PackedSize = entry.CompressedSize,
            //        FileSize = entry.Size,
            //        Tag = entry,
            //    };
            //    files.Add(metadata);
            //}
            return Task.CompletedTask;
        }

        public override Task WriteAsync(BinaryPakFile source, BinaryWriter w, WriteStage stage)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();

            //source.UseBinaryReader = false;
            //var files = multiSource.Files;
            //var pak = (P4kFile)(source.Tag = new P4kFile(w.BaseStream) { Key = Key });
            //pak.BeginUpdate();
            //foreach (var file in files)
            //{
            //    var entry = (ZipEntry)(file.Tag = new ZipEntry(Path.GetFileName(file.Path)));
            //    pak.Add(entry);
            //    source.PakBinary.WriteDataAsync(source, w, file, null, null);
            //}
            //pak.CommitUpdate();
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            //var pak = (P4kFile)source.Tag;
            //var entry = (ZipEntry)file.Tag;
            //try
            //{
            //    using var input = pak.GetInputStream(entry);
            //    if (!input.CanRead) { Log($"Unable to read stream for file: {file.Path}"); exception?.Invoke(file, $"Unable to read stream for file: {file.Path}"); return Task.FromResult(System.IO.Stream.Null); }
            //    var s = new MemoryStream();
            //    input.CopyTo(s);
            //    s.Position = 0;
            //    return Task.FromResult((Stream)s);
            //}
            //catch (Exception e) { Log($"{file.Path} - Exception: {e.Message}"); exception?.Invoke(file, $"{file.Path} - Exception: {e.Message}"); return Task.FromResult(System.IO.Stream.Null); }
            return null;
        }

        public override Task WriteDataAsync(BinaryPakFile source, BinaryWriter w, FileMetadata file, Stream data, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            //var pak = (P4kFile)source.Tag;
            //var entry = (ZipEntry)file.Tag;
            //try
            //{
            //    using var s = pak.GetInputStream(entry);
            //    data.CopyTo(s);
            //}
            //catch (Exception e) { exception?.Invoke(file, $"Exception: {e.Message}"); }
            return Task.CompletedTask;
        }
    }
}