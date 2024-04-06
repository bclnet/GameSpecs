using GameX.Formats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameX.Unity.Formats
{
    /// <summary>
    /// PakBinaryUnity
    /// </summary>
    /// <seealso cref="GameX.Formats.PakBinary" />
    public unsafe class PakBinary_Unity : PakBinary<PakBinary_Unity>
    {
        readonly byte[] Key;

        //public PakBinaryUnity(byte[] key = null) => Key = key;

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

                public FileInfo(BinaryReader r, uint version, bool endian)
                {
                    if (version >= 0x0E) r.Align();
                    Index = version >= 0x0E ? r.ReadUInt64X(endian) : r.ReadUInt32X(endian);
                    OffsCurFile = version >= 0x16 ? (long)r.ReadUInt64X(endian) : r.ReadUInt32X(endian);
                    CurFileSize = r.ReadUInt32X(endian);
                    CurFileTypeOrIndex = r.ReadUInt32X(endian);
                    InheritedUnityClass = version < 0x10 ? r.ReadUInt16X(endian) : (ushort)0;
                    if (version < 0x0B) r.Skip(2);
                    ScriptIndex = version >= 0x0B && version <= 0x10 ? r.ReadUInt16X(endian) : (ushort)0xffff;
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
                    var beginPosition = r.Tell();
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
                        if (Format < 9 && FileSize > MetadataSize) { BigEndian = r.Peek(_ => _.ReadByte(), beginPosition + FileSize - MetadataSize, SeekOrigin.Begin) == 1; Unknown = new byte[3]; }
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

                public FileDependency(BinaryReader r, uint format, bool endian)
                {
                    BufferedPath = format >= 6 ? r.ReadZAString(1000) : null;
                    Guid = format >= 5 ? r.ReadGuid() : Guid.Empty;
                    Type = format >= 5 ? r.ReadUInt32X(endian) : 0U;
                    AssetPath = r.ReadZAString(1000);
                }
            }

            public class TypeField_0D
            {
                public ushort Version;         // 0x00
                public byte Depth;             // 0x02 //specifies the amount of parents
                                               // 0x01 : IsArray
                                               // 0x02 : IsRef
                                               // 0x04 : IsRegistry
                                               // 0x08 : IsArrayOfRefs
                public byte IsArray;           // 0x03 //actually a bool for format <= 0x12, uint8_t since format 0x13
                public uint TypeStringOffset;  // 0x04
                public uint NameStringOffset;  // 0x08
                public uint Size;              // 0x0C //size in bytes; if not static (if it contains an array), set to -1
                public uint Index;             // 0x10
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
                public uint Flags;             // 0x14 
                public byte[] Unknown1;        // 0x18 //since format 0x12

                public TypeField_0D(BinaryReader r, uint format, bool endian)
                {
                    Version = r.ReadUInt16X(endian);
                    Depth = r.ReadByte();
                    var isArrayTemp = r.ReadByte();
                    IsArray = format >= 0x13 ? isArrayTemp : isArrayTemp != 0 ? (byte)1 : (byte)0;
                    TypeStringOffset = r.ReadUInt32X(endian);
                    NameStringOffset = r.ReadUInt32X(endian);
                    Size = r.ReadUInt32X(endian);
                    Index = r.ReadUInt32X(endian);
                    Flags = r.ReadUInt32X(endian);
                    if (format >= 0x12) Unknown1 = r.ReadBytes(8);
                }

                public string GetString(string[] strings)
                {
                    if ((TypeStringOffset & 0x80000000) != 0)
                    {
                        if ((TypeStringOffset & 0x7FFFFFFF) < GlobalTypeTreeStrings.Length - 1) fixed (byte* _ = &GlobalTypeTreeStrings[(byte)(TypeStringOffset & 0x7FFFFFFF)]) return new string((sbyte*)_);
                        return null;
                    }
                    else if (TypeStringOffset < strings.Length - 1) return strings[TypeStringOffset];
                    return null;
                }

                readonly static byte[] GlobalTypeTreeStrings = {
                    0x41, 0x41, 0x42, 0x42, 0x00,
                    0x41, 0x6E, 0x69, 0x6D, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x43, 0x6C, 0x69, 0x70, 0x00,
                    0x41, 0x6E, 0x69, 0x6D, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x43, 0x75, 0x72, 0x76, 0x65, 0x00,
                    0x41, 0x6E, 0x69, 0x6D, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x53, 0x74, 0x61, 0x74, 0x65, 0x00,
                    0x41, 0x72, 0x72, 0x61, 0x79, 0x00,
                    0x42, 0x61, 0x73, 0x65, 0x00, 0x42, 0x69, 0x74, 0x46, 0x69, 0x65, 0x6C, 0x64, 0x00,
                    0x62, 0x69, 0x74, 0x73, 0x65, 0x74, 0x00,
                    0x62, 0x6F, 0x6F, 0x6C, 0x00,
                    0x63, 0x68, 0x61, 0x72, 0x00,
                    0x43, 0x6F, 0x6C, 0x6F, 0x72, 0x52, 0x47, 0x42, 0x41, 0x00,
                    0x43, 0x6F, 0x6D, 0x70, 0x6F, 0x6E, 0x65, 0x6E, 0x74, 0x00,
                    0x64, 0x61, 0x74, 0x61, 0x00,
                    0x64, 0x65, 0x71, 0x75, 0x65, 0x00,
                    0x64, 0x6F, 0x75, 0x62, 0x6C, 0x65, 0x00,
                    0x64, 0x79, 0x6E, 0x61, 0x6D, 0x69, 0x63, 0x5F, 0x61, 0x72, 0x72, 0x61, 0x79, 0x00,
                    0x46, 0x61, 0x73, 0x74, 0x50, 0x72, 0x6F, 0x70, 0x65, 0x72, 0x74, 0x79, 0x4E, 0x61, 0x6D, 0x65, 0x00,
                    0x66, 0x69, 0x72, 0x73, 0x74, 0x00,
                    0x66, 0x6C, 0x6F, 0x61, 0x74, 0x00,
                    0x46, 0x6F, 0x6E, 0x74, 0x00,
                    0x47, 0x61, 0x6D, 0x65, 0x4F, 0x62, 0x6A, 0x65, 0x63, 0x74, 0x00,
                    0x47, 0x65, 0x6E, 0x65, 0x72, 0x69, 0x63, 0x20, 0x4D, 0x6F, 0x6E, 0x6F, 0x00,
                    0x47, 0x72, 0x61, 0x64, 0x69, 0x65, 0x6E, 0x74, 0x4E, 0x45, 0x57, 0x00,
                    0x47, 0x55, 0x49, 0x44, 0x00,
                    0x47, 0x55, 0x49, 0x53, 0x74, 0x79, 0x6C, 0x65, 0x00,
                    0x69, 0x6E, 0x74, 0x00,
                    0x6C, 0x69, 0x73, 0x74, 0x00,
                    0x6C, 0x6F, 0x6E, 0x67, 0x20, 0x6C, 0x6F, 0x6E, 0x67, 0x00,
                    0x6D, 0x61, 0x70, 0x00,
                    0x4D, 0x61, 0x74, 0x72, 0x69, 0x78, 0x34, 0x78, 0x34, 0x66, 0x00,
                    0x4D, 0x64, 0x46, 0x6F, 0x75, 0x72, 0x00,
                    0x4D, 0x6F, 0x6E, 0x6F, 0x42, 0x65, 0x68, 0x61, 0x76, 0x69, 0x6F, 0x75, 0x72, 0x00,
                    0x4D, 0x6F, 0x6E, 0x6F, 0x53, 0x63, 0x72, 0x69, 0x70, 0x74, 0x00,
                    0x6D, 0x5F, 0x42, 0x79, 0x74, 0x65, 0x53, 0x69, 0x7A, 0x65, 0x00,
                    0x6D, 0x5F, 0x43, 0x75, 0x72, 0x76, 0x65, 0x00,
                    0x6D, 0x5F, 0x45, 0x64, 0x69, 0x74, 0x6F, 0x72, 0x43, 0x6C, 0x61, 0x73, 0x73, 0x49, 0x64, 0x65, 0x6E, 0x74, 0x69, 0x66, 0x69, 0x65, 0x72, 0x00,
                    0x6D, 0x5F, 0x45, 0x64, 0x69, 0x74, 0x6F, 0x72, 0x48, 0x69, 0x64, 0x65, 0x46, 0x6C, 0x61, 0x67, 0x73, 0x00,
                    0x6D, 0x5F, 0x45, 0x6E, 0x61, 0x62, 0x6C, 0x65, 0x64, 0x00,
                    0x6D, 0x5F, 0x45, 0x78, 0x74, 0x65, 0x6E, 0x73, 0x69, 0x6F, 0x6E, 0x50, 0x74, 0x72, 0x00,
                    0x6D, 0x5F, 0x47, 0x61, 0x6D, 0x65, 0x4F, 0x62, 0x6A, 0x65, 0x63, 0x74, 0x00,
                    0x6D, 0x5F, 0x49, 0x6E, 0x64, 0x65, 0x78, 0x00,
                    0x6D, 0x5F, 0x49, 0x73, 0x41, 0x72, 0x72, 0x61, 0x79, 0x00,
                    0x6D, 0x5F, 0x49, 0x73, 0x53, 0x74, 0x61, 0x74, 0x69, 0x63, 0x00,
                    0x6D, 0x5F, 0x4D, 0x65, 0x74, 0x61, 0x46, 0x6C, 0x61, 0x67, 0x00,
                    0x6D, 0x5F, 0x4E, 0x61, 0x6D, 0x65, 0x00,
                    0x6D, 0x5F, 0x4F, 0x62, 0x6A, 0x65, 0x63, 0x74, 0x48, 0x69, 0x64, 0x65, 0x46, 0x6C, 0x61, 0x67, 0x73, 0x00,
                    0x6D, 0x5F, 0x50, 0x72, 0x65, 0x66, 0x61, 0x62, 0x49, 0x6E, 0x74, 0x65, 0x72, 0x6E, 0x61, 0x6C, 0x00,
                    0x6D, 0x5F, 0x50, 0x72, 0x65, 0x66, 0x61, 0x62, 0x50, 0x61, 0x72, 0x65, 0x6E, 0x74, 0x4F, 0x62, 0x6A, 0x65, 0x63, 0x74, 0x00,
                    0x6D, 0x5F, 0x53, 0x63, 0x72, 0x69, 0x70, 0x74, 0x00,
                    0x6D, 0x5F, 0x53, 0x74, 0x61, 0x74, 0x69, 0x63, 0x45, 0x64, 0x69, 0x74, 0x6F, 0x72, 0x46, 0x6C, 0x61, 0x67, 0x73, 0x00,
                    0x6D, 0x5F, 0x54, 0x79, 0x70, 0x65, 0x00, 0x6D, 0x5F, 0x56, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x00,
                    0x4F, 0x62, 0x6A, 0x65, 0x63, 0x74, 0x00, 0x70, 0x61, 0x69, 0x72, 0x00,
                    0x50, 0x50, 0x74, 0x72, 0x3C, 0x43, 0x6F, 0x6D, 0x70, 0x6F, 0x6E, 0x65, 0x6E, 0x74, 0x3E, 0x00,
                    0x50, 0x50, 0x74, 0x72, 0x3C, 0x47, 0x61, 0x6D, 0x65, 0x4F, 0x62, 0x6A, 0x65, 0x63, 0x74, 0x3E, 0x00,
                    0x50, 0x50, 0x74, 0x72, 0x3C, 0x4D, 0x61, 0x74, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x3E, 0x00,
                    0x50, 0x50, 0x74, 0x72, 0x3C, 0x4D, 0x6F, 0x6E, 0x6F, 0x42, 0x65, 0x68, 0x61, 0x76, 0x69, 0x6F, 0x75, 0x72, 0x3E, 0x00,
                    0x50, 0x50, 0x74, 0x72, 0x3C, 0x4D, 0x6F, 0x6E, 0x6F, 0x53, 0x63, 0x72, 0x69, 0x70, 0x74, 0x3E, 0x00,
                    0x50, 0x50, 0x74, 0x72, 0x3C, 0x4F, 0x62, 0x6A, 0x65, 0x63, 0x74, 0x3E, 0x00,
                    0x50, 0x50, 0x74, 0x72, 0x3C, 0x50, 0x72, 0x65, 0x66, 0x61, 0x62, 0x3E, 0x00,
                    0x50, 0x50, 0x74, 0x72, 0x3C, 0x53, 0x70, 0x72, 0x69, 0x74, 0x65, 0x3E, 0x00,
                    0x50, 0x50, 0x74, 0x72, 0x3C, 0x54, 0x65, 0x78, 0x74, 0x41, 0x73, 0x73, 0x65, 0x74, 0x3E, 0x00,
                    0x50, 0x50, 0x74, 0x72, 0x3C, 0x54, 0x65, 0x78, 0x74, 0x75, 0x72, 0x65, 0x3E, 0x00,
                    0x50, 0x50, 0x74, 0x72, 0x3C, 0x54, 0x65, 0x78, 0x74, 0x75, 0x72, 0x65, 0x32, 0x44, 0x3E, 0x00,
                    0x50, 0x50, 0x74, 0x72, 0x3C, 0x54, 0x72, 0x61, 0x6E, 0x73, 0x66, 0x6F, 0x72, 0x6D, 0x3E, 0x00,
                    0x50, 0x72, 0x65, 0x66, 0x61, 0x62, 0x00,
                    0x51, 0x75, 0x61, 0x74, 0x65, 0x72, 0x6E, 0x69, 0x6F, 0x6E, 0x66, 0x00,
                    0x52, 0x65, 0x63, 0x74, 0x66, 0x00,
                    0x52, 0x65, 0x63, 0x74, 0x49, 0x6E, 0x74, 0x00,
                    0x52, 0x65, 0x63, 0x74, 0x4F, 0x66, 0x66, 0x73, 0x65, 0x74, 0x00,
                    0x73, 0x65, 0x63, 0x6F, 0x6E, 0x64, 0x00, 0x73, 0x65, 0x74, 0x00,
                    0x73, 0x68, 0x6F, 0x72, 0x74, 0x00,
                    0x73, 0x69, 0x7A, 0x65, 0x00,
                    0x53, 0x49, 0x6E, 0x74, 0x31, 0x36, 0x00,
                    0x53, 0x49, 0x6E, 0x74, 0x33, 0x32, 0x00,
                    0x53, 0x49, 0x6E, 0x74, 0x36, 0x34, 0x00,
                    0x53, 0x49, 0x6E, 0x74, 0x38, 0x00,
                    0x73, 0x74, 0x61, 0x74, 0x69, 0x63, 0x76, 0x65, 0x63, 0x74, 0x6F, 0x72, 0x00,
                    0x73, 0x74, 0x72, 0x69, 0x6E, 0x67, 0x00,
                    0x54, 0x65, 0x78, 0x74, 0x41, 0x73, 0x73, 0x65, 0x74, 0x00,
                    0x54, 0x65, 0x78, 0x74, 0x4D, 0x65, 0x73, 0x68, 0x00,
                    0x54, 0x65, 0x78, 0x74, 0x75, 0x72, 0x65, 0x00, 0x54, 0x65, 0x78, 0x74, 0x75, 0x72, 0x65, 0x32, 0x44, 0x00,
                    0x54, 0x72, 0x61, 0x6E, 0x73, 0x66, 0x6F, 0x72, 0x6D, 0x00,
                    0x54, 0x79, 0x70, 0x65, 0x6C, 0x65, 0x73, 0x73, 0x44, 0x61, 0x74, 0x61, 0x00,
                    0x55, 0x49, 0x6E, 0x74, 0x31, 0x36, 0x00, 0x55, 0x49, 0x6E, 0x74, 0x33, 0x32, 0x00,
                    0x55, 0x49, 0x6E, 0x74, 0x36, 0x34, 0x00, 0x55, 0x49, 0x6E, 0x74, 0x38, 0x00,
                    0x75, 0x6E, 0x73, 0x69, 0x67, 0x6E, 0x65, 0x64, 0x20, 0x69, 0x6E, 0x74, 0x00,
                    0x75, 0x6E, 0x73, 0x69, 0x67, 0x6E, 0x65, 0x64, 0x20, 0x6C, 0x6F, 0x6E, 0x67, 0x20, 0x6C, 0x6F, 0x6E, 0x67, 0x00,
                    0x75, 0x6E, 0x73, 0x69, 0x67, 0x6E, 0x65, 0x64, 0x20, 0x73, 0x68, 0x6F, 0x72, 0x74, 0x00,
                    0x76, 0x65, 0x63, 0x74, 0x6F, 0x72, 0x00,
                    0x56, 0x65, 0x63, 0x74, 0x6F, 0x72, 0x32, 0x66, 0x00,
                    0x56, 0x65, 0x63, 0x74, 0x6F, 0x72, 0x33, 0x66, 0x00,
                    0x56, 0x65, 0x63, 0x74, 0x6F, 0x72, 0x34, 0x66, 0x00,
                    0x6D, 0x5F, 0x53, 0x63, 0x72, 0x69, 0x70, 0x74, 0x69, 0x6E, 0x67, 0x43, 0x6C, 0x61, 0x73, 0x73, 0x49, 0x64, 0x65, 0x6E, 0x74, 0x69, 0x66, 0x69, 0x65, 0x72, 0x00,
                    0x47, 0x72, 0x61, 0x64, 0x69, 0x65, 0x6E, 0x74, 0x00,
                    0x54, 0x79, 0x70, 0x65, 0x2A, 0x00,
                    0x69, 0x6E, 0x74, 0x32, 0x5F, 0x73, 0x74, 0x6F, 0x72, 0x61, 0x67, 0x65, 0x00,
                    0x69, 0x6E, 0x74, 0x33, 0x5F, 0x73, 0x74, 0x6F, 0x72, 0x61, 0x67, 0x65, 0x00,
                    0x42, 0x6F, 0x75, 0x6E, 0x64, 0x73, 0x49, 0x6E, 0x74, 0x00,
                    0x6D, 0x5F, 0x43, 0x6F, 0x72, 0x72, 0x65, 0x73, 0x70, 0x6F, 0x6E, 0x64, 0x69, 0x6E, 0x67, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x4F, 0x62, 0x6A, 0x65, 0x63, 0x74, 0x00,
                    0x6D, 0x5F, 0x50, 0x72, 0x65, 0x66, 0x61, 0x62, 0x49, 0x6E, 0x73, 0x74, 0x61, 0x6E, 0x63, 0x65, 0x00,
                    0x6D, 0x5F, 0x50, 0x72, 0x65, 0x66, 0x61, 0x62, 0x41, 0x73, 0x73, 0x65, 0x74, 0x00,
                    0x46, 0x69, 0x6C, 0x65, 0x53, 0x69, 0x7A, 0x65, 0x00,
                    0x48, 0x61, 0x73, 0x68, 0x31, 0x32, 0x38, 0x00,
                    0x00
                };
            } // 0x18

            public class Type_0D // everything big endian
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

                public Type_0D(bool hasTypeTree, BinaryReader r, uint version, bool endian, bool secondaryTypeTree = false)
                {
                    ClassId = r.ReadInt32X(endian);
                    Unknown16_1 = version >= 16 ? r.ReadByte() : (byte)0;
                    ScriptIndex = version >= 17 ? r.ReadUInt16X(endian) : (ushort)0xffff;
                    if (ClassId < 0 || ClassId == 0x72 || ClassId == 0x7C90B5B3 || ((short)ScriptIndex) >= 0) ScriptIDHash = r.ReadGuid(); // MonoBehaviour
                    TypeHash = r.ReadGuid();
                    if (!hasTypeTree) return;

                    // has tree type
                    var dwVariableCount = (int)r.ReadUInt32X(endian);
                    var dwStringTableLen = (int)r.ReadUInt32X(endian);
                    var variableFieldsLen = dwVariableCount * (version >= 0x12 ? 32 : 24);
                    var typeTreeLen = variableFieldsLen + dwStringTableLen;

                    // read fields
                    var treeBuffer = r.ReadBytes(typeTreeLen);
                    using var tr = new BinaryReader(new MemoryStream());
                    TypeFields = tr.ReadFArray(_ => new TypeField_0D(tr, version, endian), dwVariableCount);

                    // read strings
                    var appendNullTerminator = typeTreeLen == 0 || treeBuffer[typeTreeLen - 1] != 0;
                    tr.Seek(variableFieldsLen);
                    var stringTable = tr.ReadZAStringList();
                    if (appendNullTerminator) stringTable.Add(null);
                    Strings = stringTable.ToArray();

                    // read secondary
                    if (version >= 0x15)
                    {
                        //var depListLen = (int)r.ReadUInt32E(endian); Deps = depListLen >= 0 ? r.ReadTArray(_ => r.ReadUInt32E(endian)), depListLen) : new uint[0];
                        if (!secondaryTypeTree) Deps = r.ReadL32TArray<uint>(4, endian: endian);
                        else Headers = r.ReadZAStringList().ToArray();
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

                public TypeField_07(bool hasTypeTree, BinaryReader r, uint version, bool endian)
                {
                    Type = r.ReadZAString(256);
                    Name = r.ReadZAString(256);
                    Size = r.ReadUInt32X(endian);
                    if (version == 2) r.Skip(4);
                    else if (version == 3) Index = unchecked((uint)-1);
                    else Index = r.ReadUInt32X(endian);
                    ArrayFlag = r.ReadUInt32X(endian);
                    Flags1 = r.ReadUInt32X(endian);
                    Flags2 = version == 3 ? unchecked((uint)-1) : r.ReadUInt32X(endian);
                    if (hasTypeTree) Children = r.ReadL32FArray(_ => new TypeField_07(true, r, version, endian), endian: endian);
                }
            }

            public struct Type_07
            {
                public int ClassId; // big endian
                public TypeField_07 Base;

                public Type_07(bool hasTypeTree, BinaryReader r, uint version, bool endian)
                {
                    ClassId = r.ReadInt32X(endian);
                    Base = new TypeField_07(hasTypeTree, r, version, endian);
                }
            }

            public struct Preload
            {
                public uint FileId;
                public ulong PathId;

                public Preload(BinaryReader r, uint format, bool endian)
                {
                    FileId = r.ReadUInt32X(endian);
                    if (format >= 0x0E) r.Align();
                    PathId = format >= 0x0E ? r.ReadUInt64X(endian) : r.ReadUInt32X(endian);
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

                public TypeTree(BinaryReader r, uint version, bool endian) // Minimum AssetsFile format : 6
                {
                    _fmt = version;
                    HasTypeTree = true;
                    if (version > 6)
                    {
                        UnityVersion = r.ReadZAString(64);
                        if (UnityVersion[0] < '0' || UnityVersion[0] > '9') { FieldCount = 0; return; }
                        Platform = r.ReadUInt32X(endian);
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
                    FieldCount = (int)r.ReadUInt32X(endian);
                    if (FieldCount > 0)
                    {
                        if (version < 0x0D) Types_Unity4 = r.ReadFArray(_ => new Type_07(HasTypeTree, r, version, endian), FieldCount);
                        else Types_Unity5 = r.ReadFArray(_ => new Type_0D(HasTypeTree, r, version, endian), FieldCount);
                    }
                    // actually belongs to the asset file info tree
                    dwUnknown = version < 0x0E ? r.ReadUInt32X(endian) : 0;
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
                Header = new FileHeader(r); var format = Header.Format; var endian = Header.BigEndian;
                // simple validity check
                if (format == 0 || format > 0x40) throw new FormatException("Bad Header");
                if (format < 9) r.Seek(Header.FileSize - Header.MetadataSize + 1);
                Tree = new TypeTree(r, format, endian);
                if (Tree.UnityVersion[0] < '0' || Tree.UnityVersion[0] > '9') throw new FormatException("Bad Version");
                AssetTablePos = r.Tell();
                //
                AssetCount = (int)r.ReadUInt32X(endian);
                if (format >= 0x0E && AssetCount > 0) r.Align();
                r.Skip(FileInfo.GetSize(AssetCount, format));
                //
                Preloads = format >= 0x0B ? r.ReadL32FArray(_ => new Preload(r, format, endian), endian: endian) : new Preload[0];
                Dependencies = r.ReadL32FArray(_ => new FileDependency(r, format, endian), endian: endian);
                SecondaryTypes = format >= 0x14 ? r.ReadL32FArray(_ => new Type_0D(Tree.HasTypeTree, r, format, endian), endian: endian) : new Type_0D[0];
                Unknown = r.ReadZAString();
                // verify
                Success = Verify(r);
            }

            bool Verify(BinaryReader r)
            {
                string errorData = null;
                var format = Header.Format; var endian = Header.BigEndian;
                if (format == 0 || format > 0x40) { errorData = "Invalid file format"; goto _fileFormatError; }
                if (Tree.UnityVersion[0] == 0 || Tree.UnityVersion[0] < '0' || Tree.UnityVersion[0] > '9') { errorData = $"Invalid version string of {Tree.UnityVersion}"; goto _fileFormatError; }
                Log($"INFO: The .assets file was built for Unity {Tree.UnityVersion}.");
                if (format > 0x16 || format < 0x08) Log("WARNING: AssetsTools (for .assets versions 8-22) wasn't tested with this .assets' version, likely parsing or writing the file won't work properly!");

                r.Seek(AssetTablePos);
                var fileInfos = r.ReadL32FArray(_ => new FileInfo(_, format, endian), endian: endian);
                Log($"INFO: The .assets file has {fileInfos.Length} assets (info list : {FileInfo.GetSize(format)} bytes)");
                if (fileInfos.Length > 0)
                {
                    if (Header.MetadataSize < 8) { errorData = "Invalid metadata size"; goto _fileFormatError; }
                    var lastFileInfo = fileInfos[^1];
                    if ((Header.OffsFirstFile + lastFileInfo.OffsCurFile + lastFileInfo.CurFileSize - 1) < Header.MetadataSize) { errorData = "Last asset begins before the header ends"; goto _fileFormatError; };
                    if (r.Peek(_ => _.ReadBytes(1), Header.OffsFirstFile + lastFileInfo.OffsCurFile + lastFileInfo.CurFileSize - 1, SeekOrigin.Begin).Length != 1) { errorData = "File data are cut off"; goto _fileFormatError; }
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

                public FileInfo(AssetsFile file, BinaryReader r, uint version, bool endian) : base(r, version, endian)
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
                    var endian = file.Header.BigEndian;
                    r.Seek(AbsolutePos);
                    var nameSize = (int)r.ReadUInt32X(endian);
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
                var format = file.Header.Format; var endian = file.Header.BigEndian;
                r.Seek(file.AssetTablePos);
                FileInfos = r.ReadL32FArray(_ => new FileInfo(file, _, format, endian), endian: endian);
            }
        }

        internal class AssetType
        {
            enum ValueType
            {
                None,
                Bool,
                Int8,
                UInt8,
                Int16,
                UInt16,
                Int32,
                UInt32,
                Int64,
                UInt64,
                Float,
                Double,
                String,
                Array,
                ByteArray
            }

            class TypeArray
            {
                public int Size;
            }

            class TypeValue
            {
                [StructLayout(LayoutKind.Explicit)]
                public struct UValue
                {
                    [FieldOffset(0)] public TypeArray asArray;
                    [FieldOffset(0)] public byte[] asByteArray;
                    [FieldOffset(0)] public bool asBool;
                    [FieldOffset(0)] public sbyte asInt8;
                    [FieldOffset(0)] public byte asUInt8;
                    [FieldOffset(0)] public short asInt16;
                    [FieldOffset(0)] public ushort asUInt16;
                    [FieldOffset(0)] public int asInt32;
                    [FieldOffset(0)] public uint asUInt32;
                    [FieldOffset(0)] public long asInt64;
                    [FieldOffset(0)] public ulong asUInt64;
                    [FieldOffset(0)] public float asFloat;
                    [FieldOffset(0)] public double asDouble;
                    [FieldOffset(0)] public string asString;
                }

                public const int SizeOf = 8;
                public ValueType Type;
                public UValue Value;

                public TypeValue(ValueType type, object value)
                {
                    Type = type;
                    Set(value);
                }
                public TypeValue(ValueType type, UValue value)
                {
                    Type = type;
                    Value = value;
                }
                void Set(object valueContainer, ValueType contType = ValueType.None)
                {
                    var mismatch = false;
                    Value = new UValue();
                    switch (Type)
                    {
                        case ValueType.None: break;
                        case ValueType.Bool: if (contType >= ValueType.None && contType <= ValueType.UInt64) Value.asBool = (bool)valueContainer; else mismatch = true; break;
                        case ValueType.Int8:
                        case ValueType.UInt8: if (contType >= ValueType.None && contType <= ValueType.UInt64) Value.asInt8 = (sbyte)valueContainer; else mismatch = true; break;
                        case ValueType.Int16:
                        case ValueType.UInt16: if (contType == ValueType.None || (contType >= ValueType.Int16 && contType <= ValueType.UInt64)) Value.asInt16 = (short)valueContainer; else mismatch = true; break;
                        case ValueType.Int32:
                        case ValueType.UInt32: if (contType == ValueType.None || (contType >= ValueType.Int32 && contType <= ValueType.UInt64)) Value.asInt32 = (int)valueContainer; else mismatch = true; break;
                        case ValueType.Int64:
                        case ValueType.UInt64: if (contType == ValueType.None || (contType >= ValueType.Int64 && contType <= ValueType.UInt64)) Value.asInt64 = (long)valueContainer; else mismatch = true; break;
                        case ValueType.Float: if (contType == ValueType.None || contType == ValueType.Float) Value.asFloat = (float)valueContainer; else mismatch = true; break;
                        case ValueType.Double: if (contType == ValueType.None || contType == ValueType.Double) Value.asDouble = (double)valueContainer; else mismatch = true; break;
                        case ValueType.String: if (contType == ValueType.None || contType == ValueType.String) Value.asString = (string)valueContainer; else mismatch = true; break;
                        case ValueType.Array: if (contType == ValueType.None || contType == ValueType.Array) Value.asArray = (TypeArray)valueContainer; else mismatch = true; break;
                        case ValueType.ByteArray: if (contType == ValueType.None || contType == ValueType.ByteArray) Value.asByteArray = (byte[])valueContainer; else mismatch = true; break;
                    }
                    if (mismatch) throw new ArgumentOutOfRangeException(nameof(valueContainer), "TypeValue::Set: Mismatching value type supplied.");
                }
                public TypeArray AsArray() => Type == ValueType.Array ? Value.asArray : null;
                public byte[] AsByteArray() => Type == ValueType.ByteArray ? Value.asByteArray : null;
                public string AsString() => Type == ValueType.String ? Value.asString : null;
                public bool AsBool()
                {
                    switch (Type)
                    {
                        case ValueType.Float:
                        case ValueType.Double:
                        case ValueType.String:
                        case ValueType.ByteArray:
                        case ValueType.Array: return false;
                        default: return Value.asBool;
                    }
                }
                public int AsInt()
                {
                    switch (Type)
                    {
                        case ValueType.Float: return (int)Value.asFloat;
                        case ValueType.Double: return (int)Value.asDouble;
                        case ValueType.String:
                        case ValueType.ByteArray:
                        case ValueType.Array: return 0;
                        case ValueType.Int8: return (int)Value.asInt8;
                        case ValueType.Int16: return (int)Value.asInt16;
                        case ValueType.Int64: return (int)Value.asInt64;
                        default: return Value.asInt32;
                    }
                }
                public uint AsUInt()
                {
                    switch (Type)
                    {
                        case ValueType.Float: return (uint)Value.asFloat;
                        case ValueType.Double: return (uint)Value.asDouble;
                        case ValueType.String:
                        case ValueType.ByteArray:
                        case ValueType.Array: return 0;
                        default: return Value.asUInt32;
                    }
                }
                public long AsInt64()
                {
                    switch (Type)
                    {
                        case ValueType.Float: return (long)Value.asFloat;
                        case ValueType.Double: return (long)Value.asDouble;
                        case ValueType.String:
                        case ValueType.ByteArray:
                        case ValueType.Array: return 0;
                        case ValueType.Int8: return (long)Value.asInt8;
                        case ValueType.Int16: return (long)Value.asInt16;
                        case ValueType.Int32: return (long)Value.asInt32;
                        default: return Value.asInt64;
                    }
                }
                public ulong AsUInt64()
                {
                    switch (Type)
                    {
                        case ValueType.Float: return (ulong)Value.asFloat;
                        case ValueType.Double: return (ulong)Value.asDouble;
                        case ValueType.String:
                        case ValueType.ByteArray:
                        case ValueType.Array: return 0;
                        default: return Value.asUInt64;
                    }
                }
                public float AsFloat()
                {
                    switch (Type)
                    {
                        case ValueType.Float: return Value.asFloat;
                        case ValueType.Double: return (float)Value.asDouble;
                        case ValueType.String:
                        case ValueType.ByteArray:
                        case ValueType.Array: return 0;
                        case ValueType.Int8: return (float)Value.asInt8;
                        case ValueType.Int16: return (float)Value.asInt16;
                        case ValueType.Int32: return (float)Value.asInt32;
                        case ValueType.Int64: return (float)Value.asInt64;
                        default: return (float)Value.asUInt64;
                    }
                }
                public double AsDouble()
                {
                    switch (Type)
                    {
                        case ValueType.Float: return (double)Value.asFloat;
                        case ValueType.Double: return Value.asDouble;
                        case ValueType.String:
                        case ValueType.ByteArray:
                        case ValueType.Array: return 0;
                        case ValueType.Int8: return (double)Value.asInt8;
                        case ValueType.Int16: return (double)Value.asInt16;
                        case ValueType.Int32: return (double)Value.asInt32;
                        case ValueType.Int64: return (double)Value.asInt64;
                        default: return (double)Value.asUInt64;
                    }
                }
            }

            class TypeTemplateField
            {
                public string Name;
                public string Type;
                public ValueType ValueType;
                public bool IsArray;
                public bool Align;
                public bool HasValue;
                public TypeTemplateField[] Children;

                //static int _RecursiveGetValueFieldCount(TypeTemplateField child, BinaryReader r, long maxFilePos, ref long pFilePos, ref int pValueByteLen, ref int pChildListLen, ref int pRawDataLen, ref bool pReadFailed, bool endian)
                //{
                //    var filePos = pFilePos;
                //    var valueByteLen = pValueByteLen;
                //    var childListLen = pChildListLen;
                //    var rawDataLen = pRawDataLen;
                //    if (pReadFailed) return 0;

                //    var ret = 1;
                //    if (child.IsArray && child.Children.Length == 2)
                //    {
                //        valueByteLen += TypeValue.SizeOf;
                //        int arrayLen;
                //        if (child.Children[0].ValueType == ValueType.Int32 || child.Children[0].ValueType == ValueType.UInt32)
                //        {
                //            r.Position(filePos);
                //            arrayLen = (int)r.ReadUInt32E(endian); filePos += 4;
                //            if (string.Equals(child.Type, "TypelessData", StringComparison.OrdinalIgnoreCase))
                //            {
                //                rawDataLen += arrayLen;
                //                filePos += arrayLen;
                //                if (filePos > maxFilePos) pReadFailed = true;
                //            }
                //            else
                //            {
                //                childListLen += TypeValueField.SizeOf * arrayLen;
                //                for (var i = 0; i < arrayLen; i++)
                //                {
                //                    ret += _RecursiveGetValueFieldCount(child.Children[1], r, maxFilePos, ref filePos, ref valueByteLen, ref childListLen, ref rawDataLen, ref pReadFailed, endian);
                //                    if (pReadFailed || filePos > maxFilePos) { pReadFailed = true; break; }
                //                }
                //            }
                //            if (child.Align) filePos = (filePos + 3) & (~3);
                //        }
                //        else Debug.Assert(false);
                //    }
                //    else if (child.ValueType == ValueType.String)
                //    {
                //        r.Position(filePos);
                //        var stringLen = (int)r.ReadUInt32E(endian); filePos += 4;
                //        if ((filePos + stringLen) > maxFilePos) pReadFailed = true;
                //        else
                //        {
                //            filePos += stringLen;
                //            if (child.Align || (child.Children.Length > 0 && child.Children[0].Align)) filePos = (filePos + 3) & (~3);
                //            valueByteLen += TypeValue.SizeOf + stringLen + 1;
                //        }
                //    }
                //    else if (child.Children.Length == 0)
                //    {
                //        switch (child.ValueType)
                //        {
                //            case ValueType.Bool:
                //            case ValueType.Int8:
                //            case ValueType.UInt8: filePos++; break;
                //            case ValueType.Int16:
                //            case ValueType.UInt16: filePos += 2; break;
                //            case ValueType.Int32:
                //            case ValueType.UInt32:
                //            case ValueType.Float: filePos += 4; break;
                //            case ValueType.Int64:
                //            case ValueType.UInt64:
                //            case ValueType.Double: filePos += 8; break;
                //        }
                //        valueByteLen += TypeValue.SizeOf;
                //        if (child.Align) filePos = (filePos + 3) & (~3);
                //        if (filePos > maxFilePos) pReadFailed = true;
                //    }
                //    else
                //    {
                //        childListLen += TypeValueField.SizeOf * child.Children.Length;
                //        for (var i = 0; i < child.Children.Length; i++)
                //            ret += _RecursiveGetValueFieldCount(child.Children[i], r, maxFilePos, ref filePos, ref valueByteLen, ref childListLen, ref rawDataLen, ref pReadFailed, endian);
                //        if (child.Align) filePos = (filePos + 3) & (~3);
                //    }
                //    pRawDataLen = rawDataLen;
                //    pChildListLen = childListLen;
                //    pValueByteLen = valueByteLen;
                //    pFilePos = filePos;
                //    return ret;
                //}

                static void _RecursiveMakeValues(TypeTemplateField template, BinaryReader r, long maxFilePos, List<TypeValueField> valueFields, bool endian)
                {
                    TypeValue curValue;
                    if (template.IsArray)
                    {
                        if (template.Children.Length != 2) Debug.Assert(false);
                        if (template.Children[0].ValueType == ValueType.Int32 || template.Children[0].ValueType == ValueType.UInt32) Debug.Assert(false);
                        var arrayLen = (int)r.ReadUInt32X(endian);
                        if (string.Equals(template.Type, "TypelessData", StringComparison.OrdinalIgnoreCase))
                        {
                            var curRawData = r.ReadBytes(arrayLen);
                            if (r.Tell() <= maxFilePos)
                            {
                                curValue = new TypeValue(ValueType.ByteArray, curRawData);
                                valueFields.Add(new TypeValueField(curValue, template, 0, null));
                            }
                        }
                        else
                        {
                            curValue = new TypeValue(ValueType.Array, new TypeArray { Size = arrayLen });
                            var arrayItemList = new TypeValueField[arrayLen];
                            valueFields.Add(new TypeValueField(curValue, template, arrayLen, arrayItemList));
                            for (var i = 0; i < arrayLen; i++)
                            {
                                arrayItemList[i] = valueFields[^1];
                                _RecursiveMakeValues(template.Children[1], r, maxFilePos, valueFields, endian);
                                if (r.Tell() > maxFilePos) break;
                            }
                        }
                        if (template.Align) r.Align();
                    }
                    else if (template.ValueType == ValueType.String)
                    {
                        var stringLen = (int)r.ReadUInt32X(endian);
                        if ((r.Tell() + stringLen) > maxFilePos) stringLen = (int)(maxFilePos - r.Tell());
                        var bytes = r.ReadBytes(stringLen);
                        curValue = new TypeValue(ValueType.String, Encoding.ASCII.GetString(bytes));
                        valueFields.Add(new TypeValueField(curValue, template, 0, null));
                        if (template.Align || (template.Children.Length > 0 && template.Children[0].Align)) r.Align();
                    }
                    else if (template.Children.Length == 0)
                    {
                        object valueContainer = null;
                        switch (template.ValueType)
                        {
                            case ValueType.Bool: case ValueType.Int8: case ValueType.UInt8: valueContainer = r.ReadByte(); break;
                            case ValueType.Int16: case ValueType.UInt16: valueContainer = r.ReadUInt16X(endian); break;
                            case ValueType.Int32: case ValueType.UInt32: valueContainer = r.ReadUInt32X(endian); break;
                            case ValueType.Float: valueContainer = r.ReadSingleX(endian); break;
                            case ValueType.Int64: case ValueType.UInt64: valueContainer = r.ReadUInt64X(endian); break;
                            case ValueType.Double: valueContainer = r.ReadDoubleX(endian); break;
                            case ValueType.String: break;
                        }
                        if (template.Align) r.Align();
                        if (r.Tell() <= maxFilePos)
                        {
                            curValue = new TypeValue(template.ValueType, valueContainer);
                            valueFields.Add(new TypeValueField(curValue, template, 0, null));
                        }
                    }
                }

                void MakeValue(BinaryReader r, long fileLen, List<TypeValueField> ppValueField, bool endian)
                {
                    //TypeValue newValue = null;
                    //int newValueByteLen = 0; int childListByteLen = 0; int rawDataByteLen = 0;
                    ////Set to true if it goes EOF while reading an array; This allows parsing empty files and having them filled with zeros without risking crashes on invalid files. 
                    //var readFailed = false;
                    var firstPosition = r.Tell();
                    //var position = 0L;
                    //var newChildrenCount = _RecursiveGetValueFieldCount(this, r, firstPosition + fileLen, ref position, ref newValueByteLen, ref childListByteLen, ref rawDataByteLen, ref readFailed, endian);
                    ////ppValueField will be set to pValueFieldMemory so the caller knows which pointer to free
                    //if (readFailed) { ppValueField = null; return; }
                    //void* pValueFieldMemory = malloc((newChildrenCount * sizeof(AssetTypeValueField)) + newValueByteLen + childListByteLen + rawDataByteLen);
                    //if (pValueFieldMemory == NULL)
                    //{
                    //    *ppValueField = NULL;
                    //    return filePos;
                    //}
                    //AssetTypeValueField* pValueFields = (AssetTypeValueField*)pValueFieldMemory;
                    //AssetTypeValue* pCurValue = (AssetTypeValue*)(&((uint8_t*)pValueFieldMemory)[newChildrenCount * sizeof(AssetTypeValueField)]);

                    //AssetTypeValueField** pCurValueList = (AssetTypeValueField**)(&((uint8_t*)pValueFieldMemory)[newChildrenCount * sizeof(AssetTypeValueField) + newValueByteLen]);
                    //uint8_t* pCurRawByte = (uint8_t*)(&((uint8_t*)pValueFieldMemory)[newChildrenCount * sizeof(AssetTypeValueField) + newValueByteLen + childListByteLen]);

                    TypeValueField[] curValueList = null;
                    var valueFields = new List<TypeValueField>();
                    _RecursiveMakeValues(this, r, firstPosition + fileLen, valueFields, endian);
                    //_RecursiveDumpValues(pValueFields, 0);
                    ppValueField = valueFields;
                    return;
                }

                bool From0D(AssetsFile.Type_0D type, uint fieldIndex)
                {
                    if (type.TypeFields.Length <= fieldIndex) return false;
                    var typeField = type.TypeFields[fieldIndex];
                    Type = typeField.GetString(type.Strings);
                    Name = typeField.GetString(type.Strings);
                    ValueType = !string.IsNullOrEmpty(Type) ? GetValueTypeByTypeName(Type) : ValueType.None;
                    IsArray = (typeField.IsArray & 1) != 0;
                    if (IsArray) ValueType = ValueType.Array;
                    Align = (typeField.Flags & 0x4000) != 0;

                    int newChildCount = 0; byte directChildDepth = 0;
                    for (var i = fieldIndex + 1; i < type.TypeFields.Length; i++)
                    {
                        if (type.TypeFields[i].Depth <= typeField.Depth) break;
                        if (directChildDepth == 0) { directChildDepth = type.TypeFields[i].Depth; newChildCount++; }
                        else if (type.TypeFields[i].Depth == directChildDepth) newChildCount++;
                    }
                    HasValue = newChildCount == 0;
                    Array.Resize(ref Children, newChildCount);
                    var childIndex = 0; var ret = true;
                    for (var i = fieldIndex + 1; i < type.TypeFields.Length; i++)
                    {
                        if (type.TypeFields[i].Depth <= typeField.Depth) break;
                        if (type.TypeFields[i].Depth == directChildDepth)
                        {
                            if (!Children[childIndex].From0D(type, i)) ret = false;
                            childIndex++;
                        }
                    }
                    return ret;
                }

                bool FromClassDatabase(ClassDatabaseFile file, ClassDatabaseFile.Type type, int fieldIndex)
                {
                    if (type.Fields.Count <= fieldIndex) return false;
                    var typeField = type.Fields[fieldIndex];
                    IsArray = (typeField.IsArray & 1) != 0;
                    Name = typeField.FieldName.GetString(file);
                    Type = typeField.TypeName.GetString(file);
                    ValueType = !string.IsNullOrEmpty(Type) ? GetValueTypeByTypeName(Type) : ValueType.None;
                    Align = (typeField.Flags2 & 0x4000) != 0;

                    var newChildCount = 0;
                    var directChildDepth = (byte)0;
                    for (var i = fieldIndex + 1; i < type.Fields.Count; i++)
                    {
                        if (type.Fields[i].Depth <= typeField.Depth) break;
                        if (directChildDepth == 0) { directChildDepth = type.Fields[i].Depth; newChildCount++; }
                        else if (type.Fields[i].Depth == directChildDepth) newChildCount++;
                    }
                    HasValue = type.Fields.Count <= fieldIndex + 1 || newChildCount == 0;
                    Array.Resize(ref Children, newChildCount);
                    var childIndex = 0; var ret = true;
                    for (var i = fieldIndex + 1; i < type.Fields.Count; i++)
                    {
                        if (type.Fields[i].Depth <= typeField.Depth) break;
                        if (type.Fields[i].Depth == directChildDepth)
                        {
                            if (!Children[childIndex].FromClassDatabase(file, type, i)) ret = false;
                            childIndex++;
                        }
                    }
                    return ret;
                }

                bool From07(AssetsFile.TypeField_07 typeField)
                {
                    IsArray = typeField.ArrayFlag != 0;
                    Align = (typeField.Flags2 & 0x4000) != 0;
                    Name = typeField.Name;
                    Type = typeField.Type;
                    ValueType = Type.Length != 0 ? GetValueTypeByTypeName(Type) : ValueType.None;
                    HasValue = typeField.Children.Length == 0;
                    Array.Resize(ref Children, typeField.Children.Length);
                    for (var i = 0; i < typeField.Children.Length; i++)
                        if (!Children[i].From07(typeField.Children[i])) return false;
                    return true;
                }

                static ValueType GetValueTypeByTypeName(string type)
                {
                    if (string.Equals(type, "string", StringComparison.OrdinalIgnoreCase)) return ValueType.String;
                    else if (string.Equals(type, "SInt8", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "char", StringComparison.OrdinalIgnoreCase)) return ValueType.Int8;
                    else if (string.Equals(type, "UInt8", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "unsigned char", StringComparison.OrdinalIgnoreCase)) return ValueType.UInt8;
                    else if (string.Equals(type, "SInt16", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "short", StringComparison.OrdinalIgnoreCase)) return ValueType.Int16;
                    else if (string.Equals(type, "UInt16", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "unsigned short", StringComparison.OrdinalIgnoreCase)) return ValueType.UInt16;
                    else if (string.Equals(type, "SInt32", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "int", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "Type*", StringComparison.OrdinalIgnoreCase)) return ValueType.Int32;
                    else if (string.Equals(type, "UInt32", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "unsigned int", StringComparison.OrdinalIgnoreCase)) return ValueType.UInt32;
                    else if (string.Equals(type, "SInt64", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "long", StringComparison.OrdinalIgnoreCase)) return ValueType.Int64;
                    else if (string.Equals(type, "UInt64", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "FileSize", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "unsigned long", StringComparison.OrdinalIgnoreCase)) return ValueType.UInt64;
                    else if (string.Equals(type, "float", StringComparison.OrdinalIgnoreCase)) return ValueType.Float;
                    else if (string.Equals(type, "double", StringComparison.OrdinalIgnoreCase)) return ValueType.Double;
                    else if (string.Equals(type, "bool", StringComparison.OrdinalIgnoreCase)) return ValueType.Bool;
                    else return ValueType.None;
                }
            }

            class TypeValueField
            {
                public const int SizeOf = 0;
                public static TypeValueField Empty = new TypeValueField();

                TypeTemplateField TemplateField;
                TypeValueField[] Children;
                TypeValue Value; //pointer so it may also have no value (NULL)

                public TypeValueField this[string name]
                {
                    get
                    {
                        if (Children.Length >= 0)
                            foreach (var child in Children) if (child.TemplateField != null && child.TemplateField.Name == name) return child;
                        return Empty;
                    }
                }

                public TypeValueField this[int index]
                {
                    get
                    {
                        if (Children.Length == 0 || index >= Children.Length) return Empty;
                        return Children[index];
                    }
                }

                TypeValueField() { }
                public TypeValueField(TypeValue value, TypeTemplateField template, int childrenCount, TypeValueField[] children)
                {
                    Value = value;
                    TemplateField = template;
                    Children = children;
                }

                void Write(BinaryWriter w, bool endian)
                {
                    var doPadding = TemplateField.Align;
                    if (TemplateField.Children.Length == 0 && Value != null && Value.Type != ValueType.ByteArray)
                        switch (TemplateField.ValueType)
                        {
                            case ValueType.Bool:
                            case ValueType.Int8:
                            case ValueType.UInt8: w.Write((byte)(Value.AsInt() & 0xff)); break;
                            case ValueType.Int16:
                            case ValueType.UInt16: w.Write((ushort)MathX.SwapEndian((uint)(Value.AsInt() & 0xffff << 16), endian)); break;
                            case ValueType.Int32:
                            case ValueType.UInt32: w.WriteE((uint)Value.AsInt(), endian); break;
                            case ValueType.Int64:
                            case ValueType.UInt64: w.WriteE((ulong)Value.AsInt64(), endian); break;
                            case ValueType.Float: w.WriteE(Value.AsFloat(), endian); break;
                            case ValueType.Double: w.WriteE(Value.AsDouble(), endian); break;
                        }
                    else if (Value != null && Value.Type == ValueType.String)
                    {
                        var strVal = Encoding.ASCII.GetBytes(Value.AsString() ?? string.Empty);
                        w.WriteE((uint)strVal.Length);
                        w.Write(strVal);
                        if (TemplateField.Children.Length == 1 && TemplateField.Children[0].Align) doPadding = true;
                    }
                    else if (Value != null && (Value.Type == ValueType.Array || Value.Type == ValueType.ByteArray))
                    {
                        if (Value.Type == ValueType.ByteArray)
                        {
                            var bytes = Value.AsByteArray();
                            w.WriteE(bytes.Length, endian);
                            w.Write(bytes);
                        }
                        else
                        {
                            var curArrLen = Value.AsArray().Size;
                            w.WriteE(curArrLen, endian);
                            for (var i = 0; i < curArrLen; i++) Children[i].Write(w, endian);
                        }
                        if (TemplateField.Children.Length == 1 && TemplateField.Children[0].Align) doPadding = true; //For special case: String overwritten with ByteArray value.
                    }
                    else if (Children.Length > 0)
                    {
                        for (var i = 0; i < Children.Length; i++) Children[i].Write(w, endian);
                    }
                    if (doPadding)
                    {
                        var paddingLen = 3 - (((int)(w.Position() & 3) - 1) & 3);
                        //if (paddingLen > 0)
                        //{
                        //    dwValueTmp = 0;
                        //    w.Write(paddingLen, &dwValueTmp);
                        //}
                    }
                }

                int GetByteSize(int filePos)
                {
                    var doPadding = TemplateField.Align;
                    if (TemplateField.Children.Length == 0 && Value != null)
                    {
                        switch (TemplateField.ValueType)
                        {
                            case ValueType.Bool:
                            case ValueType.Int8:
                            case ValueType.UInt8: filePos++; break;
                            case ValueType.Int16:
                            case ValueType.UInt16: filePos += 2; break;
                            case ValueType.Int32:
                            case ValueType.UInt32:
                            case ValueType.Float: filePos += 4; break;
                            case ValueType.Int64:
                            case ValueType.UInt64:
                            case ValueType.Double: filePos += 8; break;
                        }
                    }
                    else if (TemplateField.ValueType == ValueType.String && Value != null)
                    {
                        filePos += 4 + Value.AsString().Length;
                        if (TemplateField.Children.Length > 0 && TemplateField.Children[0].Align) doPadding = true;
                    }
                    else if (TemplateField.IsArray && Value != null)
                    {
                        filePos += 4;
                        if (string.Equals(TemplateField.Type, "TypelessData", StringComparison.OrdinalIgnoreCase)) filePos += Value.AsByteArray().Length;
                        else for (var i = 0; i < Value.AsArray().Size; i++) filePos = Children[i].GetByteSize(filePos);
                    }
                    else if (Children.Length > 0)
                    {
                        for (var i = 0; i < Children.Length; i++) filePos = Children[i].GetByteSize(filePos);
                    }
                    if (doPadding) filePos = (filePos + 3) & (~3);
                    return filePos;
                }
            }
        }

        #endregion

        // File : CLASSDATABASE
        #region File : CLASSDATABASE

        internal class ClassDatabaseFile
        {
            public struct FileString //:was ClassDatabaseFileString
            {
                public static readonly FileString Empty = new FileString { };
                bool FromStringTable;
                public uint StringTableOffset; // Don't trust this offset! GetString makes sure no out-of-bounds offset is used.
                public string String;

                public string GetString(ClassDatabaseFile file)
                {
                    if (!FromStringTable) return String;
                    if (StringTableOffset >= file.Header.StringTableLen) return String.Empty;
                    return file.Strings[StringTableOffset];
                }

                public FileString(BinaryReader r)
                {
                    FromStringTable = true;
                    StringTableOffset = r.ReadUInt32();
                    String = null;
                }
            }

            public struct TypeField //:was ClassDatabaseTypeField
            {
                public FileString TypeName;
                public FileString FieldName;
                public byte Depth;
                public byte IsArray;
                public uint Size;
                public ushort Version;
                public uint Flags2;               // Flag 0x4000 : align to 4 bytes after this field.

                public TypeField(BinaryReader r, int version) // reads version 0,1,2,3
                {
                    TypeName = new FileString(r);
                    FieldName = new FileString(r);
                    Depth = r.ReadByte();
                    IsArray = r.ReadByte();
                    Size = r.ReadUInt32();
                    Version = 1;
                    if (version < 1)
                    {
                        var index = r.ReadUInt32();
                        if ((index & 0x80000000) != 0) Version = r.ReadUInt16();
                    }
                    else if (version >= 3) Version = r.ReadUInt16();
                    Flags2 = r.ReadUInt32();
                }
            }

            public class Type //:was ClassDatabaseType
            {
                public uint ClassId;
                public uint BaseClass;
                public FileString Name;
                public FileString AssemblyFileName; // set if (header.flags & 1)
                public List<TypeField> Fields;

                public Type(BinaryReader r, int version, byte flags)
                {
                    ClassId = r.ReadUInt32();
                    BaseClass = r.ReadUInt32();
                    Name = new FileString(r);
                    AssemblyFileName = (flags & 1) != 0 ? new FileString(r) : FileString.Empty;
                    Fields = new List<TypeField>();
                    var fieldCount = r.ReadUInt32();
                    Fields.Capacity = (int)fieldCount;
                    for (var i = 0; i < fieldCount; i++) Fields.Add(new TypeField(r, version));
                }

                public byte[] MakeTypeHash(ClassDatabaseFile file)
                {
                    using var md5 = MD5.Create();
                    foreach (var field in Fields)
                    {
                        md5.TransformBlock(field.TypeName.GetString(file));
                        md5.TransformBlock(field.FieldName.GetString(file));
                        md5.TransformBlock(BitConverter.GetBytes(field.Size));
                        md5.TransformBlock(BitConverter.GetBytes((uint)field.IsArray));
                        md5.TransformBlock(BitConverter.GetBytes((uint)field.Version));
                        md5.TransformBlock(BitConverter.GetBytes(field.Flags2 & 0x4000));
                    }
                    return md5.ToFinalHash();
                }

                public static byte[] MakeScriptId(string scriptName, string scriptNamespace, string scriptAssembly)
                {
                    using var md5 = MD5.Create();
                    md5.TransformBlock(scriptName);
                    md5.TransformBlock(scriptNamespace);
                    md5.TransformBlock(scriptAssembly);
                    return md5.ToFinalHash();
                }
            }

            public struct FileHeader //:was ClassDatabaseFileHeader
            {
                const uint Header_CLDB = 'c' << 24 | 'l' << 16 | 'd' << 8 | 'b';
                public uint Header;
                public byte FileVersion;
                public byte Flags;                  // 1 : Describes MonoBehaviour classes (contains assembly and full class names, base field is to be ignored)
                public byte CompressionType;        // version 2; 0 = none, 1 = LZ4
                public uint CompressedSize;         // version 2
                public uint UncompressedSize;       // version 2

                public string[] UnityVersions;

                public uint StringTableLen;
                public uint StringTablePos;

                public FileHeader(BinaryReader r)
                {
                    Header = r.ReadUInt32();
                    if (Header != Header_CLDB) throw new FormatException("Bad Header");
                    FileVersion = r.ReadByte();
                    Flags = FileVersion >= 4 ? r.ReadByte() : (byte)0;
                    if (FileVersion >= 2)
                    {
                        CompressionType = r.ReadByte();
                        CompressedSize = r.ReadUInt32();
                        UncompressedSize = r.ReadUInt32();
                    }
                    else
                    {
                        CompressionType = 0;
                        CompressedSize = UncompressedSize = 0;
                    }
                    if (FileVersion == 0) { r.Skip(r.ReadByte()); UnityVersions = new string[0]; }
                    else UnityVersions = r.ReadL8FArray(_ => Encoding.ASCII.GetString(_.ReadBytes(_.ReadByte())));
                    StringTableLen = r.ReadUInt32();
                    StringTablePos = r.ReadUInt32();
                }
            }

            public struct FileRef
            {
                public uint Offset;
                public uint Length;
                public string Name;
            }

            public struct PackageHeader
            {
                public uint Magic;                  // "CLPK"
                public byte FileVersion;            // 0 or 1
                public byte CompressionType;        // Version 1 flags : 0x80 compressed all files in one block; 0x40 string table uncompressed; 0x20 file block uncompressed;
                public uint StringTableOffset;
                public uint StringTableLenUncompressed;
                public uint StringTableLenCompressed;
                public uint FileBlockSize;
                public uint FileCount;
                public List<FileRef> Files;
                public PackageHeader(BinaryReader r)
                {
                    throw new NotImplementedException();
                }
            }

            public class DatabasePackage
            {
                public bool Valid;
                public PackageHeader Header;
                public ClassDatabaseFile[] Files;
                public string[] Strings;

                public DatabasePackage(BinaryReader r)
                {
                    throw new NotImplementedException();
                }
            }

            public bool Valid;
            public bool DontFreeStringTable; // Only for internal use, otherwise this could create a memory leak!
            public FileHeader Header;
            public List<Type> Classes;
            public string[] Strings;

            public ClassDatabaseFile(BinaryReader r)
            {
                Header = new FileHeader(r);
                long compressedFilePos = r.Tell(), postHeaderPos = r.Tell();
                var ds = r.BaseStream;
                if (Header.CompressionType != 0 && Header.CompressionType < 3)
                {
                    try
                    {
                        switch (Header.CompressionType)
                        {
                            case 1: ds = new MemoryStream(r.DecompressLz4((int)Header.CompressedSize, (int)Header.UncompressedSize)); break;
                            case 2: ds = new MemoryStream(r.DecompressLzma((int)Header.CompressedSize - GameX.Formats.Compression.LZMAPropsSize, (int)Header.UncompressedSize)); break;
                        }
                        if (ds.Length != Header.UncompressedSize) return;
                    }
                    catch { return; }
                }
                var version = Header.FileVersion; var flags = Header.Flags;
                Classes = r.ReadL32FArray(_ => new Type(_, version, flags)).ToList();
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
                    Blocks = r.ReadL32FArray(_ => new Block
                    {
                        DecompressedSize = _.ReadUInt32E(),
                        CompressedSize = _.ReadUInt32E(),
                        Flags = _.ReadUInt16E(),
                    }, endian: true);
                    Directories = r.ReadL32FArray(_ => new Directory
                    {
                        Offset = _.ReadUInt64E(),
                        DecompressedSize = _.ReadUInt64E(),
                        Flags = _.ReadUInt32E(),
                        Name = _.ReadZAString(400),
                    }, endian: true);
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
                Signature = r.ReadZAString(13);
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
                MinPlayerVersion = r.ReadZAString(24);
                FileEngineVersion = r.ReadZAString(64);
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
                    Blocks3 = r.ReadL32FArray(_ => new Block
                    {
                        CompressedSize = _.ReadUInt32E(),
                        DecompressedSize = _.ReadUInt32E(),
                    }, endian: true);
                    if (FileVersion >= 2) FileSize = r.ReadUInt32E();
                    if (FileVersion >= 3) Unknown2 = r.ReadUInt32E();
                    Unknown3 = r.ReadByte();
                    //
                    if (Signature.StartsWith("UnityRaw")) // compressed bundles only have an uncompressed header
                    {
                        r.Seek(DataOffs);
                        Directories3 = r.ReadL32FArray(_ => new Directory
                        {
                            Name = _.ReadZAString(400),
                            Offset = _.ReadUInt32E(),
                            DecompressedSize = _.ReadUInt32E(),
                        }, endian: true);
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
                //var format = file.Header.Format; var endian = file.Header.BigEndian;
                //r.Position(file.AssetTablePos);
                //FileInfos = r.ReadL32EArray((_, b) => new FileInfo(file, _, format, endian), endian);
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

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            // try-bundle
            var bundleFile = new BundleFile(r);
            if (bundleFile.Success)
            {
                //var table = bundleFile.CreateTable(r);
                return Task.CompletedTask;
            }

            // try-asset
            r.Seek(0);
            var assetsFile = new AssetsFile(r);
            if (assetsFile.Success)
            {
                var table = assetsFile.CreateTable(r);
                return Task.CompletedTask;
            }

            // try-resources
            r.Seek(0);
            var resourcesFile = new ResourcesFile(r);
            if (resourcesFile.Success)
            {
                return Task.CompletedTask;
            }

            //var files = source.Files = new List<FileMetadata>();
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

        public override Task Write(BinaryPakFile source, BinaryWriter w, object tag)
        {


            //source.UseBinaryReader = false;
            //var files = source.Files;
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

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
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
    }
}