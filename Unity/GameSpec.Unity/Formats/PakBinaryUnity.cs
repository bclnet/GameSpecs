using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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

        // Header : UNITY3
        #region Header : UNITY3

        // Default header data
        const uint MW_BSAHEADER_FILEID = 0x00000100; // Magic for Morrowind BSA

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct U3_Header
        {
            public fixed byte Signature[13];            // 0-terminated; UnityWeb or UnityRaw
            public uint FileVersion;                    // big-endian; 3 : Unity 3.5 and 4;
            public fixed byte MinPlayerVersion[24];     // 0-terminated; 3.x.x -> Unity 3.x.x/4.x.x; 5.x.x
            public fixed byte FileEngineVersion[64];    // 0-terminated; exact unity engine version

        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct U3_Entry
        {
            public uint Offset;
            public uint Length;
            public string Name;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct U3_HeaderFile
        {
            public uint FileSize;           // File size
            public uint FileOffset;         // File offset relative to data position
            public uint Size => FileSize > 0 ? FileSize & 0x3FFFFFFF : 0; // The size of the file inside the BSA
        }

        #endregion

        // File : ASSETS
        #region File : ASSETS

        internal class AssetsFile
        {
            struct FileInfo
            {
                public long index;                      // 0x00 version < 0x0E : only uint32_t
                public long offs_curFile;               // 0x08, version < 0x16 : only uint32_t
                public uint curFileSize;                // 0x0C
                public uint curFileTypeOrIndex;         // 0x10, starting with version 0x10, this is an index into the type tree
                                                        // inheritedUnityClass : for Unity classes, this is curFileType; for MonoBehaviours, this is 114
                                                        // version < 0x0B : inheritedUnityClass is uint, no scriptIndex exists
                public ushort inheritedUnityClass;      // 0x14, (MonoScript) only version < 0x10
                                                        // scriptIndex : for Unity classes, this is 0xFFFF;
                                                        // for MonoBehaviours, this is an index of the mono class, counted separately for each .assets file
                public ushort scriptIndex;              // 0x16, only version <= 0x10
                public byte unknown1;					// 0x18, only 0x0F <= version <= 0x10 //with alignment always a uint32_t
            }

            struct FileHeader                           // Always big-endian
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
                    var dw00 = MathX.SwapEndian(r.ReadUInt32());
                    var dw04 = MathX.SwapEndian(r.ReadUInt32());
                    Format = MathX.SwapEndian(r.ReadUInt32());
                    var dw0C = MathX.SwapEndian(r.ReadUInt32());
                    if (Format >= 0x16)
                    {
                        Unknown00 = (dw00 << 32) | dw04;
                        // dw0C is padding for format >= 0x16
                        MetadataSize = (long)MathX.SwapEndian(r.ReadUInt64());
                        FileSize = (long)MathX.SwapEndian(r.ReadUInt64());
                        OffsFirstFile = (long)MathX.SwapEndian(r.ReadUInt64());
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
                        else { BigEndian = r.ReadByte() != 0; Unknown = r.ReadBytes(3); }
                    }
                }
            };

            struct FileDependency
            {
                // version < 6 : no bufferedPath
                // version < 5 : no bufferedPath, guid, type
                public string BufferedPath; // for buffered (type=1)
                public object Guid;
                public int Type;
                public string AssetPath; // path to the .assets file
            }

            class TypeField_0D
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

            class Type_0D //everything big endian
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
                    ClassId = MathX.SwapEndianIf(r.ReadInt32(), bigEndian);
                    Unknown16_1 = version >= 16 ? r.ReadByte() : (byte)0;
                    ScriptIndex = version >= 17 ? MathX.SwapEndianIf(r.ReadUInt16(), bigEndian) : (ushort)0xffff;
                    if (ClassId < 0 || ClassId == 0x72 || ClassId == 0x7C90B5B3 || ((short)ScriptIndex) >= 0) ScriptIDHash = r.ReadGuid(); // MonoBehaviour
                    TypeHash = r.ReadGuid();
                    if (!hasTypeTree) return;

                    // has tree type
                    var dwVariableCount = (int)MathX.SwapEndianIf(r.ReadUInt32(), bigEndian);
                    var dwStringTableLen = (int)MathX.SwapEndianIf(r.ReadUInt32(), bigEndian);
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
                        if (!secondaryTypeTree)
                        {
                            var depListLen = (int)MathX.SwapEndianIf(r.ReadUInt32(), bigEndian);
                            Deps = depListLen >= 0 ? r.ReadTArray(_ => MathX.SwapEndianIf(r.ReadUInt32(), bigEndian), depListLen) : new uint[0];
                        }
                        else Headers = r.ReadZASCIIList().ToArray();
                    }
                }
            }

            class TypeField_07 // everything big endian
            {
                public string Type; //null-terminated
                public string Name; //null-terminated
                public uint Size;
                public uint Index;
                public uint ArrayFlag;
                public uint Flags1;
                public uint Flags2; // Flag 0x4000 : align to 4 bytes after this field.
                public uint ChildrenCount;
                public TypeField_07[] Children;

                public TypeField_07(bool hasTypeTree, BinaryReader r, uint version, bool bigEndian)
                {
                    Type = r.ReadZASCII(256);
                    Name = r.ReadZASCII(256);
                    Size = MathX.SwapEndianIf(r.ReadUInt32(), bigEndian);
                    if (version == 2) r.Skip(4);
                    else if (version == 3) Index = unchecked((uint)-1);
                    else Index = MathX.SwapEndianIf(r.ReadUInt32(), bigEndian);
                    ArrayFlag = MathX.SwapEndianIf(r.ReadUInt32(), bigEndian);
                    Flags1 = MathX.SwapEndianIf(r.ReadUInt32(), bigEndian);
                    Flags2 = version == 3 ? unchecked((uint)-1) : MathX.SwapEndianIf(r.ReadUInt32(), bigEndian);
                    if (hasTypeTree)
                    {

                    }
                }
            }

            struct Type_07
            {
                public int ClassId; // big endian
                public TypeField_07 Base;

                public Type_07(bool hasTypeTree, BinaryReader r, uint version, bool bigEndian)
                {
                    ClassId = MathX.SwapEndianIf(r.ReadInt32(), bigEndian);
                    Base = new TypeField_07(hasTypeTree, r, version, bigEndian);
                }
            }

            class TypeTree
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
                        Platform = MathX.SwapEndianIf(r.ReadUInt32(), bigEndian);
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

                    if (version >= 0x0D) HasTypeTree = r.ReadByte() != 0; // Unity 5
                    FieldCount = (int)MathX.SwapEndianIf(r.ReadUInt32(), bigEndian);
                    if (FieldCount > 0)
                    {
                        if (version < 0x0D) Types_Unity4 = r.ReadTArray(_ => new Type_07(HasTypeTree, r, version, bigEndian), FieldCount);
                        else Types_Unity5 = r.ReadTArray(_ => new Type_0D(HasTypeTree, r, version, bigEndian), FieldCount);
                    }
                    // actually belongs to the asset file info tree
                    dwUnknown = version < 0x0E ? MathX.SwapEndianIf(r.ReadUInt32(), bigEndian) : 0;
                }
            }


            FileHeader Header;
            TypeTree Tree;
            //PreloadList preloadTable;
            //FileDependencyList dependencies;
            //int SecondaryTypeCount;           // format >= 0x14
            //Type_0D[] SecondaryTypeList;        // format >= 0x14
            //string UnknownString;               // format >= 5; seemingly always empty
            long AssetTablePos;
            int AssetCount;

            public AssetsFile(BinaryReader r) //: was:AssetsFile(IAssetsReader *pReader)
            {
                Header = new FileHeader(r);
                // simple validity check
                if (Header.Format == 0 || Header.Format > 0x40) throw new FormatException("Bad Header");
                if (Header.Format < 9) r.Position(Header.FileSize - Header.MetadataSize + 1);
                Tree = new TypeTree(r, Header.Format, Header.BigEndian);
                if (Tree.UnityVersion[0] < '0' || Tree.UnityVersion[0] > '9') throw new FormatException("Bad Version");
                AssetTablePos = r.Position();

                //    {
                //        var tmpFileList = new AssetFileList();
                //        tmpFileList.sizeFiles = (long)MathX.SwapEndian(header.BigEndian, r.ReadUInt32());
                //        AssetCount = tmpFileList.sizeFiles;

                //        if (header.Format >= 0x0E && AssetCount > 0) r.Align(3);
                //        r.Skip(tmpFileList.GetSizeBytes(header.Format));
                //    }
                //    preloadTable = header.Format >= 0x0B ? new PreloadTable(r, header.Format, header.BigEndian) : PreloadTable.Empty;
                //    dependencies = new dependencies(filePos, pReader, this->header.format, this->header.endianness == 1);
                //    SecondaryTypeList_Read(this, filePos, pReader, this->header.format, this->header.endianness == 1);
                //    UnknownString_Read(this, filePos, pReader, this->header.format);
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
                public ulong DirectorySize;
                public uint Flags;
                public string Name;
            }

            public string Signature;                    // UnityFS, UnityRaw, UnityWeb or UnityArchive
            public uint FileVersion;                    // 3, 6, 7
            public string MinPlayerVersion;             // 5.x.x
            public string FileEngineVersion;            // exact unity engine version
            public ulong FileSize;
            public FileMetadata[] Files;
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
            // Assets
            public ulong ChecksumLow;
            public ulong ChecksumHigh;
            public Directory[] Directories;

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
                FileVersion = Signature == "UnityArchive" ? 6 : MathX.SwapEndian(r.ReadUInt32());
                MinPlayerVersion = r.ReadZASCII(24);
                FileEngineVersion = r.ReadZASCII(64);
                if (FileVersion >= 6)
                {
                    FileSize = MathX.SwapEndian(r.ReadUInt64());
                    CompressedSize = MathX.SwapEndian(r.ReadUInt32());
                    DecompressedSize = MathX.SwapEndian(r.ReadUInt32());
                    Flags = (HeaderFlag)MathX.SwapEndian(r.ReadUInt32());
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
                            switch (compressionType)
                            {
                                case 1: ds = new MemoryStream(r.DecompressLzma((int)CompressedSize, (int)DecompressedSize)); break;
                                case 2: case 3: ds = new MemoryStream(r.DecompressLz4((int)CompressedSize, (int)DecompressedSize)); break;
                            }
                    }
                    var dataOffset = GetU6DataOffset();
                    var dr = new BinaryReader(ds);
                    ChecksumLow = dr.ReadUInt64();
                    ChecksumHigh = dr.ReadUInt64();
                    Files = new FileMetadata[MathX.SwapEndian(dr.ReadUInt32())];
                    for (var i = 0; i < Files.Length; i++)
                    {
                        Files[i] = new FileMetadata
                        {
                            Position = dataOffset,
                            FileSize = MathX.SwapEndian(dr.ReadUInt32()),
                            PackedSize = MathX.SwapEndian(dr.ReadUInt32()),
                            Digest = MathX.SwapEndian(dr.ReadUInt16()), //: flags
                        };
                        dataOffset += Files[i].PackedSize;
                    }
                    Directories = new Directory[MathX.SwapEndian(dr.ReadUInt32())];
                    for (var i = 0; i < Directories.Length; i++)
                        Directories[i] = new Directory
                        {
                            Offset = MathX.SwapEndian(dr.ReadUInt64()),
                            DirectorySize = MathX.SwapEndian(dr.ReadUInt64()),
                            Flags = MathX.SwapEndian(dr.ReadUInt32()),
                            Name = dr.ReadZASCII(400),
                        };
                }
                else if (FileVersion >= 3)
                {
                    MinimumStreamedBytes = MathX.SwapEndian(r.ReadUInt32());
                    DataOffs = MathX.SwapEndian(r.ReadUInt32());
                    NumberOfAssetsToDownload = MathX.SwapEndian(r.ReadUInt32());
                    Files = new FileMetadata[MathX.SwapEndian(r.ReadUInt32())];
                    for (var i = 0; i < Files.Length; i++)
                        Files[i] = new FileMetadata
                        {
                            PackedSize = MathX.SwapEndian(r.ReadUInt32()),
                            FileSize = MathX.SwapEndian(r.ReadUInt32()),
                        };
                    if (FileVersion >= 2) FileSize = MathX.SwapEndian(r.ReadUInt32());
                    if (FileVersion >= 3) Unknown2 = MathX.SwapEndian(r.ReadUInt32());
                    Unknown3 = r.ReadByte();
                    if (Signature == "UnityRaw")
                    {
                        // read assets - compressed bundles only have an uncompressed header
                        r.Position(DataOffs);
                        Directories = new Directory[MathX.SwapEndian(r.ReadUInt32())];
                        for (var i = 0; i < Files.Length; i++)
                            Directories[i] = new Directory
                            {
                                Name = r.ReadZASCII(400),
                                Offset = MathX.SwapEndian(r.ReadUInt32()),
                                DirectorySize = MathX.SwapEndian(r.ReadUInt32()),
                            };
                    }
                }
                else throw new FormatException($"Invalid File Version {FileVersion}");
            }
        }

        #endregion

        // File : RESOURCES
        #region File : RESOURCES

        internal class ResourcesFile
        {
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

            //var headerA = new BundleFile(r);
            var headerB = new AssetsFile(r);


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