using GameX.Formats;
using OpenStack.Graphics.DirectX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameX.Red.Formats
{
    // https://witcher.fandom.com/wiki/File_format
    // https://witcher.fandom.com/wiki/Extracting_the_original_files
    // https://witcher.fandom.com/wiki/Extracting_The_Witcher_2_files
    // https://github.com/JLouis-B/RedTools
    // https://github.com/yole/Gibbed.RED
    // https://forums.cdprojektred.com/index.php?threads/tw2-tw3-redkit-tool-3d-models-converter.1043/page-2
    public unsafe class PakBinary_Red : PakBinary<PakBinary_Red>
    {
        //class SubPakFile : BinaryPakFile
        //{
        //    public SubPakFile(FamilyGame game, IFileSystem fileSystem, string filePath, object tag = null) : base(game, fileSystem, filePath, Instance, tag) => Open();
        //}

        // Headers : KEY/BIF (Witcher)
        #region Headers : KEY/BIF

        // https://witcher.fandom.com/wiki/KEY_BIF_V1.1_format

        const uint KEY_MAGIC = 0x2059454b;
        const uint KEY_VERSION = 0x312e3156;

        const uint BIFF_MAGIC = 0x46464942;
        const uint BIFF_VERSION = 0x312e3156;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct KEY_Header
        {
            public static (string, int) Struct = ("<8i32c", sizeof(KEY_Header));
            public uint Version;            // Version ("V1.1")
            public uint NumFiles;           // Number of entries in FILETABLE
            public uint NumKeys;            // Number of entries in KEYTABLE.
            public uint NotUsed01;          // Not used
            public uint FilesOffset;        // Offset to FILETABLE (0x440000).
            public uint KeysOffset;         // Offset to KEYTABLE.
            public uint BuildYear;          // Build year (less 1900).
            public uint BuildDay;           // Build day
            public fixed byte NotUsed02[32]; // Not used
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct KEY_HeaderFile
        {
            public static (string, int) Struct = ("<3i", sizeof(KEY_HeaderFile));
            public uint FileSize;           // BIF Filesize
            public uint FileNameOffset;     // Offset To BIF name
            public uint FileNameSize;       // Size of BIF name
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct KEY_HeaderKey
        {
            public static (string, int) Struct = ("<16sH2I", sizeof(KEY_HeaderKey));
            public fixed byte Name[0x10];   // Null-padded string Resource Name (sans extension).
            public ushort ResourceType;     // Resource Type
            public uint ResourceId;         // Resource ID
            public uint Flags;              // Flags (BIF index is in this value, (flags & 0xFFF00000) >> 20). The rest appears to define 'fixed' index.
            public uint Id => (Flags & 0xFFF00000) >> 20; // BIF index
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct BIFF_Header
        {
            public static (string, int) Struct = ("<4I", sizeof(BIFF_Header));
            public uint Version;            // Version ("V1.1")
            public uint NumFiles;           // Resource Count
            public uint NotUsed01;          // Not used
            public uint FilesOffset;        // Offset to RESOURCETABLE (0x14000000).
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct BIFF_HeaderFile
        {
            public static (string, int) Struct = ("<4I2H", sizeof(BIFF_HeaderFile));
            public uint FileId;             // File ID
            public uint Flags;              // Flags (BIF index is now in this value, (flags & 0xFFF00000) >> 20). The rest appears to define 'fixed' index.
            public uint FileOffset;         // Offset to File Data.
            public uint FileSize;           // Size of File Data.
            public ushort FileType;         // File Type
            public ushort NotUsed01;        // Not used
            public uint Id => (Flags & 0xFFF00000) >> 20; // BIF index
        }

        static readonly Dictionary<int, string> BIFF_FileTypes = new Dictionary<int, string> {
            {0x0000, "res"}, // Misc. GFF resources
            {0x0001, "bmp"}, // Microsoft Windows Bitmap
            {0x0002, "mve"},
            {0x0003, "tga"}, // Targa Graphics Format
            {0x0004, "wav"}, // Wave

            {0x0006, "plt"}, // Bioware Packed Layer Texture
            {0x0007, "ini"}, // Windows INI
            {0x0008, "mp3"}, // MP3
            {0x0009, "mpg"}, // MPEG
            {0x000A, "txt"}, // Text file
            {0x000B, "xml"},

            {0x07D0, "plh"},
            {0x07D1, "tex"},
            {0x07D2, "mdl"}, // Model
            {0x07D3, "thg"},

            {0x07D5, "fnt"}, // Font

            {0x07D7, "lua"}, // Lua script source code
            {0x07D8, "slt"},
            {0x07D9, "nss"}, // NWScript source code
            {0x07DA, "ncs"}, // NWScript bytecode
            {0x07DB, "mod"}, // Module
            {0x07DC, "are"}, // Area (GFF)
            {0x07DD, "set"}, // Tileset (unused in KOTOR?)
            {0x07DE, "ifo"}, // Module information
            {0x07DF, "bic"}, // Character sheet (unused)
            {0x07E0, "wok"}, // Walk-mesh
            {0x07E1, "2da"}, // 2-dimensional array
            {0x07E2, "tlk"}, // conversation file

            {0x07E6, "txi"}, // Texture information
            {0x07E7, "git"}, // Dynamic area information, game instance file, all area and objects that are scriptable
            {0x07E8, "bti"},
            {0x07E9, "uti"}, // item blueprint
            {0x07EA, "btc"},
            {0x07EB, "utc"}, // Creature blueprint

            {0x07ED, "dlg"}, // Dialogue
            {0x07EE, "itp"}, // tile blueprint pallet file
            {0x07EF, "btt"},
            {0x07F0, "utt"}, // trigger blueprint
            {0x07F1, "dds"}, // compressed texture file
            {0x07F2, "bts"},
            {0x07F3, "uts"}, // sound blueprint
            {0x07F4, "ltr"}, // letter combo probability info
            {0x07F5, "gff"}, // Generic File Format
            {0x07F6, "fac"}, // faction file
            {0x07F7, "bte"},
            {0x07F8, "ute"}, // encounter blueprint
            {0x07F9, "btd"},
            {0x07FA, "utd"}, // door blueprint
            {0x07FB, "btp"},
            {0x07FC, "utp"}, // placeable object blueprint
            {0x07FD, "dft"}, // default values file (text-ini)
            {0x07FE, "gic"}, // game instance comments
            {0x07FF, "gui"}, // GUI definition (GFF)
            {0x0800, "css"},
            {0x0801, "ccs"},
            {0x0802, "btm"},
            {0x0803, "utm"}, // store merchant blueprint
            {0x0804, "dwk"}, // door walkmesh
            {0x0805, "pwk"}, // placeable object walkmesh
            {0x0806, "btg"},

            {0x0808, "jrl"}, // Journal
            {0x0809, "sav"}, // Saved game (ERF)
            {0x080A, "utw"}, // waypoint blueprint
            {0x080B, "4pc"},
            {0x080C, "ssf"}, // sound set file

            {0x080F, "bik"}, // movie file (bik format)
            {0x0810, "ndb"}, // script debugger file
            {0x0811, "ptm"}, // plot manager/plot instance
            {0x0812, "ptt"}, // plot wizard blueprint
            {0x0813, "ncm"},
            {0x0814, "mfx"},
            {0x0815, "mat"},
            {0x0816, "mdb"}, // not the standard MDB, multiple file formats present despite same type
            {0x0817, "say"},
            {0x0818, "ttf"}, // standard .ttf font files
            {0x0819, "ttc"},
            {0x081A, "cut"}, // cutscene? (GFF)
            {0x081B, "ka"},  // karma file (XML)
            {0x081C, "jpg"}, // jpg image
            {0x081D, "ico"}, // standard windows .ico files
            {0x081E, "ogg"}, // ogg vorbis sound file
            {0x081F, "spt"},
            {0x0820, "spw"},
            {0x0821, "wfx"}, // woot effect class (XML)
            {0x0822, "ugm"}, // 2082 ?? [textures00.bif]
            {0x0823, "qdb"}, // quest database (GFF v3.38)
            {0x0824, "qst"}, // quest (GFF)
            {0x0825, "npc"}, // spawn point? (GFF)
            {0x0826, "spn"},
            {0x0827, "utx"},
            {0x0828, "mmd"},
            {0x0829, "smm"},
            {0x082A, "uta"}, // uta (GFF)
            {0x082B, "mde"},
            {0x082C, "mdv"},
            {0x082D, "mda"},
            {0x082E, "mba"},
            {0x082F, "oct"},
            {0x0830, "bfx"},
            {0x0831, "pdb"},
            {0x0832, "TheWitcherSave"},
            {0x0833, "pvs"},
            {0x0834, "cfx"},
            {0x0835, "luc"}, // compiled lua script

            {0x0837, "prb"},
            {0x0838, "cam"},
            {0x0839, "vds"},
            {0x083A, "bin"},
            {0x083B, "wob"},
            {0x083C, "api"},
            {0x083D, "properties"},
            {0x083E, "png"},

            {0x270B, "big"},

            {0x270D, "erf"}, // Encapsulated Resource Format
            {0x270E, "bif"},
            {0x270F, "key"},
        };

        #endregion

        // Headers : DZIP (Witcher 2)
        #region Headers : DZIP

        const uint DZIP_MAGIC = 0x50495a44;
        const uint DZIP_VERSION = 0x50495a44;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct DZIP_Header
        {
            public static (string, int) Struct = ("<3I2Q", sizeof(DZIP_Header));
            public uint Version;            //
            public uint NumFiles;           //
            public uint Unk02;              //
            public ulong FilesPosition;     //
            public ulong Hash;              //
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct DZIP_HeaderFile
        {
            public static (string, int) Struct = ("<4Q", sizeof(DZIP_HeaderFile));
            public ulong Timestamp;         // 
            public ulong FileSize;          // 
            public ulong Position;          // 
            public ulong PackedSize;        // 
            public DateTime Time => DateTime.FromFileTime((long)Timestamp);
        }

        #endregion

        // Headers : BUNDLE (Witcher 3)
        #region Headers : BUNDLE

        const uint BUNDLE_MAGIC = 0x41544f50; const uint BUNDLE_MAGIC2 = 0x30374f54; // POTATO70

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct BUNDLE_Header
        {
            public static (string, int) Struct = ("<3I", sizeof(BUNDLE_Header));
            public uint FileSize;           // 
            public uint NotUsed01;          // 
            public uint DataPosition;       // 
            public int NumFiles => (int)(DataPosition / sizeof(BUNDLE_HeaderFile));
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct BUNDLE_HeaderFile
        {
            public static (string, int) Struct = ("<256s16s4IQ16c2I", sizeof(BUNDLE_Header));
            public fixed byte Name[0x100];  //
            public fixed byte Hash[16];     //
            public uint NotUsed01;          //
            public uint FileSize;           //
            public uint PackedSize;         //
            public uint FilePosition;       //
            public ulong Timestamp;         //
            public fixed byte NotUsed02[16]; //
            public uint NotUsed03;          //
            public uint Compressed;         //
        }

        #endregion

        // Headers : CACHE:Texture (Witcher 3)
        #region Headers : CACHE:Texture
        // https://github.com/hhrhhr/Lua-utils-for-Witcher-3

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct CACHE_TEX_Header
        {
            public static (string, int) Struct = ("<5I", sizeof(CACHE_TEX_Header));
            public uint NumFiles;           // number of textures
            public uint NamesSize;          // names
            public uint ChunksSize;         // relative offsets of packed chunks (* 4 bytes)
            public uint Unk1;               // const 1415070536
            public uint Unk2;               // const 6
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct CACHE_TEX_HeaderFile
        {
            public static (string, int) Struct = ("<6I4H4I2cH", sizeof(CACHE_TEX_Header));
            public uint Id;                 // 01:crc or id??
            public uint FileNameOffset;     // 02:filename, start offset in block2
            public uint StartOffset;        // 03:* 4096 = start offset, first chunk
            public uint PackedSize;         // 04:packed size (all cunks)
            public uint FileSize;           // 05:unpacked size
            public uint Bpp;                // 06:Bpp // bpp? always 16
            public ushort Width;            // 07:width
            public ushort Height;           // 08:height
            public ushort NumMips;          // 09:mips
            public ushort Caps;               // 10:1/6/N, single, cubemaps, arrays
            public uint ChunkStartIndex;    // 11:offset in block1, second packed chunk
            public uint T12;                // 12:the number of remaining packed chunks
            public uint T13;                // 13:????
            public uint T14;                // 14:????
            public byte Format;             // 15:0-????, 7-DXT1, 8-DXT5, 10-????, 13-DXT3, 14-ATI1, 15-????, 253-RGBA
            public byte T16;                // 16:3-cubemaps, 4-texture, 0-???
            public ushort T17;              // 17:0/1 ???
            public bool HasSkip => Caps == 6 && (Format == 253 || Format == 0);
            public bool HasCubemap => (T16 == 3 || T16 == 0) && Caps == 6;
        }

        #endregion

        // Headers : CACHE:CS3W (Witcher 3)
        #region Headers : CACHE:CS3W

        const uint CS3W_MAGIC = 0x57335343;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct CACHE_CS3W_Header
        {
            public static (string, int) Struct = ("<5Q", sizeof(CACHE_CS3W_Header));
            public ulong NotUsed01;         // Not used
            public ulong InfoPosition;      // Offset to Info
            public ulong NumFiles;          // Number of entries in FILETABLE
            public ulong NameOffset;        // Offset to Names
            public ulong NameSize;          // Size of to Names
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct CACHE_CS3W_HeaderV1
        {
            public static (string, int) Struct = ("<Q4I", sizeof(CACHE_CS3W_HeaderV1));
            public ulong NotUsed01;         // Not used
            public uint InfoPosition;       // Offset to Info
            public uint NumFiles;           // Number of entries in FILETABLE
            public uint NameOffset;         // Offset to Names
            public uint NameSize;           // Size of to Names

            public CACHE_CS3W_Header ToHeader() => new CACHE_CS3W_Header
            {
                NotUsed01 = NotUsed01,
                InfoPosition = InfoPosition,
                NumFiles = NumFiles,
                NameOffset = NameOffset,
                NameSize = NameSize,
            };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct CS3W_HeaderFileV1
        {
            public static (string, int) Struct = ("<3I", sizeof(CS3W_HeaderFileV1));
            public uint NamePosition;       // 
            public uint Position;           // 
            public uint Size;               // 
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct CS3W_HeaderFileV2
        {
            public static (string, int) Struct = ("<3Q", sizeof(CS3W_HeaderFileV2));
            public ulong NamePosition;      // 
            public ulong Position;          // 
            public ulong Size;              // 
        }

        #endregion

        // Headers : RDAR (Cyberpunk 2077)
        #region Headers : RDAR

        const uint RDAR_MAGIC = 0x52414452; // RDAR

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct RDAR_Header
        {
            public static (string, int) Struct = ("<I4Q", sizeof(RDAR_Header));
            public uint Version;
            public ulong TableOffset;
            public ulong TableSize;
            public ulong Unk3;
            public ulong FileSize;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct RDAR_HeaderTable
        {
            public static (string, int) Struct = ("<2IQ3I", sizeof(RDAR_HeaderTable));
            public uint Num;
            public uint Size;
            public ulong Checksum;
            public uint Table1Count;
            public uint Table2Count;
            public uint Table3Count;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct RDAR_HeaderFile
        {
            public static (string, int) Struct = ("<Qq5I20c", sizeof(RDAR_HeaderFile));
            public ulong NameHash64;
            public long DateTime;
            public uint FileFlags;
            public uint FirstDataSector;
            public uint NextDataSector;
            public uint FirstUnkIndex;
            public uint NextUnkIndex;
            public fixed byte SHA1Hash[0x14];
            public DateTime DateTimeConvert => System.DateTime.FromFileTime(DateTime);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct RDAR_HeaderOffset
        {
            public static (string, int) Struct = ("<Q2I", sizeof(RDAR_HeaderOffset));
            public ulong Offset;
            public uint PhysicalSize;
            public uint VirtualSize;
        }

        #endregion

        // https://zenhax.com/viewtopic.php?t=3954
        // https://forums.cdprojektred.com/index.php?threads/modding-the-witcher-3-a-collection-of-tools-you-need.64557/
        // https://github.com/rfuzzo/CP77Tools
        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            FileSource[] files; List<FileSource> files2;
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(source.PakPath);
            var extension = Path.GetExtension(source.PakPath);
            var magic = source.Magic = r.ReadUInt32();
            // KEY
            switch (magic)
            {
                case KEY_MAGIC: // Signature("KEY ")
                    {
                        var header = r.ReadS<KEY_Header>();
                        if (header.Version != KEY_VERSION) throw new FormatException("BAD MAGIC");
                        source.Version = header.Version;
                        source.Files = files = new FileSource[header.NumFiles];

                        // parts
                        r.Seek(header.FilesOffset);
                        var headerFiles = r.ReadTArray<KEY_HeaderFile>(sizeof(KEY_HeaderFile), (int)header.NumFiles).Select(x => { r.Seek(x.FileNameOffset); return (file: x, path: r.ReadFString((int)x.FileNameSize)); }).ToArray();
                        r.Seek(header.KeysOffset);
                        var headerKeys = r.ReadTArray<KEY_HeaderKey>(sizeof(KEY_HeaderKey), (int)header.NumKeys).ToDictionary(x => (x.Id, x.ResourceId), x => UnsafeX.FixedAString(x.Name, 0x10));

                        // combine
                        var sourceName = source.Name;
                        var voiceKey = sourceName[0] == 'M' && char.IsNumber(sourceName[1]);
                        var subPathFormat = Path.Combine(Path.GetDirectoryName(source.PakPath), !voiceKey ? "{0}" : "voices\\{0}");
                        for (var i = 0; i < header.NumFiles; i++)
                        {
                            var (file, path) = headerFiles[i];
                            var subPath = string.Format(subPathFormat, path);
                            if (!File.Exists(subPath)) continue;
                            files[i] = new FileSource
                            {
                                Path = path,
                                FileSize = file.FileSize,
                                Pak = new SubPakFile(source, null, subPath, (headerKeys, (uint)i)),
                            };
                        }
                    }
                    return Task.CompletedTask;
                // BIFF
                case BIFF_MAGIC: // Signature("BIFF")
                    {
                        if (source.Tag == null) throw new FormatException("BIFF files can only be processed through KEY files");
                        var (keys, bifId) = ((Dictionary<(uint, uint), string> keys, uint bifId))source.Tag;
                        var header = r.ReadS<BIFF_Header>();
                        if (header.Version != BIFF_VERSION) throw new FormatException("BAD MAGIC");
                        source.Version = header.Version;
                        source.Files = files2 = new List<FileSource>();

                        // files
                        var fileTypes = BIFF_FileTypes;
                        r.Seek(header.FilesOffset);
                        var headerFiles = r.ReadTArray<BIFF_HeaderFile>(sizeof(BIFF_HeaderFile), (int)header.NumFiles);
                        for (var i = 0; i < header.NumFiles; i++)
                        {
                            var headerFile = headerFiles[i];
                            // Curiously the last resource entry of djinni.bif seem to be missing
                            if (headerFile.FileId > i) continue;
                            var path = $"{(keys.TryGetValue((bifId, headerFile.FileId), out var key) ? key : $"{i}")}{(BIFF_FileTypes.TryGetValue(headerFile.FileType, out var z) ? $".{z}" : string.Empty)}".Replace('\\', '/');
                            files2.Add(new FileSource
                            {
                                Path = path,
                                FileSize = headerFile.FileSize,
                                Offset = headerFile.FileOffset,
                            });
                        }
                    }
                    return Task.CompletedTask;
                // DZIP
                case DZIP_MAGIC: // Signature("DZIP")
                    {
                        var header = r.ReadS<DZIP_Header>();
                        if (header.Version < 2) throw new FormatException("unsupported version");
                        source.Version = DZIP_VERSION;
                        source.Files = files = new FileSource[header.NumFiles];
                        var cryptKey = source.Tag as ulong?;
                        r.Seek((long)header.FilesPosition);
                        var hash = 0x00000000FFFFFFFFUL;
                        for (var i = 0; i < header.NumFiles; i++)
                        {
                            string path;
                            if (cryptKey == null) path = r.ReadL16YEncoding();
                            else
                            {
                                var pathBytes = r.ReadBytes(r.ReadUInt16());
                                for (var j = 0; j < pathBytes.Length; j++) { pathBytes[j] ^= (byte)(cryptKey >> j % 8); cryptKey *= 0x00000100000001B3UL; }
                                var nameBytesLength = pathBytes.Length - 1;
                                path = Encoding.ASCII.GetString(pathBytes, 0, pathBytes[nameBytesLength] == 0 ? nameBytesLength : pathBytes.Length);
                            }
                            var headerFile = r.ReadS<DZIP_HeaderFile>();
                            files[i] = new FileSource
                            {
                                Path = path.Replace('\\', '/'),
                                Compressed = 1,
                                PackedSize = (long)headerFile.PackedSize,
                                FileSize = (long)headerFile.FileSize,
                                Offset = (long)headerFile.Position,
                            };
                            // build digest
                            if (!string.IsNullOrEmpty(path))
                            {
                                for (var j = 0; j < path.Length; j++) { hash ^= (byte)path[j]; hash *= 0x00000100000001B3UL; }
                                hash ^= (ulong)path.Length;
                                hash *= 0x00000100000001B3UL;
                            }
                            hash ^= headerFile.Timestamp;
                            hash *= 0x00000100000001B3UL;
                            hash ^= headerFile.FileSize;
                            hash *= 0x00000100000001B3UL;
                            hash ^= headerFile.Position;
                            hash *= 0x00000100000001B3UL;
                            hash ^= headerFile.PackedSize;
                            hash *= 0x00000100000001B3UL;
                        }
                        if (hash != header.Hash) throw new FormatException("bad entry table hash (wrong cdkey for DLC archives?)");
                    }
                    return Task.CompletedTask;
                // BUNDLE
                case BUNDLE_MAGIC: // Signature("POTATO70")
                    {
                        if (r.ReadUInt32() != BUNDLE_MAGIC2) throw new FormatException("BAD MAGIC");
                        var header = r.ReadS<BUNDLE_Header>();
                        source.Version = BUNDLE_MAGIC;
                        source.Files = files = new FileSource[header.NumFiles];

                        // files
                        r.Seek(0x20);
                        var headerFiles = r.ReadSArray<BUNDLE_HeaderFile>(header.NumFiles);
                        for (var i = 0; i < header.NumFiles; i++)
                        {
                            var headerFile = headerFiles[i];
                            files[i] = new FileSource
                            {
                                Path = UnsafeX.FixedAString(headerFile.Name, 0x100).Replace('\\', '/'),
                                Compressed = (int)headerFile.Compressed,
                                FileSize = headerFile.FileSize,
                                PackedSize = headerFile.PackedSize,
                                Offset = headerFile.FilePosition,
                            };
                        }
                    }
                    return Task.CompletedTask;
                // RDAR (.archive)
                case RDAR_MAGIC: // Signature("RDAR")
                    {
                        var hashLookup = CP77.HashLookup;
                        var header = r.ReadS<RDAR_Header>();
                        if (header.Version != 12) throw new FormatException("unsupported version");
                        source.Version = RDAR_MAGIC;
                        r.Seek((long)header.TableOffset);
                        var headerTable = r.ReadS<RDAR_HeaderTable>();
                        var headerFiles = r.ReadSArray<RDAR_HeaderFile>((int)headerTable.Table1Count);
                        var headerOffsets = r.ReadSArray<RDAR_HeaderOffset>((int)headerTable.Table2Count);
                        //var headerHashs = r.ReadTArray<ulong>(sizeof(ulong), (int)headerTable.Table3Count);
                        source.Files = files2 = new List<FileSource>();
                        var nameHashs = new HashSet<ulong>();
                        for (var i = 0; i < headerFiles.Length; i++)
                        {
                            var headerFile = headerFiles[i];
                            var hash = headerFile.NameHash64;
                            if (nameHashs.Contains(hash)) { Console.WriteLine($"File already added in Archive {source.PakPath}: hash {hash}, idx {i}"); continue; }
                            nameHashs.Add(hash);
                            files2.Add(new FileSource
                            {
                                Path = hashLookup.TryGetValue(hash, out var z) ? z.Replace('\\', '/') : $"{hash:X2}.bin",
                                Tag = (headerFile.FirstDataSector, headerFile.NextDataSector, headerOffsets),
                            });
                        }
                    }
                    return Task.CompletedTask;
                default:
                    // CACHE
                    if (extension == ".cache")
                    {
                        if (fileNameWithoutExtension == "texture")
                        {
                            source.Version = 'T';
                            r.Seek(r.BaseStream.Length - 20);
                            var header = r.ReadS<CACHE_TEX_Header>();
                            Assert(header.Unk1 == 1415070536);
                            Assert(header.Unk2 == 6);
                            source.Files = files = new FileSource[header.NumFiles];
                            var offset = 20 + 12 + (header.NumFiles * 52) + header.NamesSize + (header.ChunksSize * 4);
                            r.Seek(r.BaseStream.Length - offset);

                            // check block
                            var headerChunks = r.ReadTArray<uint>(sizeof(uint), (int)header.ChunksSize);
                            source.Tag = headerChunks;

                            // name block
                            var filePaths = new string[header.NumFiles];
                            for (var i = 0; i < header.NumFiles; i++) filePaths[i] = r.ReadZAString(1000);

                            // file block
                            var headerFiles = r.ReadSArray<CACHE_TEX_HeaderFile>((int)header.NumFiles);
                            for (var i = 0; i < header.NumFiles; i++)
                                files[i] = new FileSource
                                {
                                    Path = filePaths[i].Replace('\\', '/'),
                                    Offset = headerFiles[i].StartOffset,
                                    PackedSize = headerFiles[i].PackedSize,
                                    FileSize = headerFiles[i].FileSize,
                                    Tag = headerFiles[i],
                                };
                        }
                        else if (magic == CS3W_MAGIC)
                        {
                            var version = r.ReadUInt32();
                            var header = version >= 2
                                ? r.ReadS<CACHE_CS3W_Header>()
                                : r.ReadS<CACHE_CS3W_HeaderV1>().ToHeader();
                            r.Seek((long)header.NameOffset);
                            var name = r.ReadFString((int)header.NameSize);
                        }
                        else
                        {
                            r.Seek(0);
                            var packedSize = r.ReadUInt32();
                            if (packedSize == 0) packedSize = r.ReadUInt32() << 4;
                            var fileSize = r.ReadUInt32();
                            var type = r.ReadByte();
                        }
                    }
                    else throw new FormatException($"Unknown File Type {magic}");
                    return Task.CompletedTask;
            }
        }

        public enum FORMAT : uint
        {
            RGB = 0,
            RGBA = RGB,

            // DX9 formats
            DXT1 = 1,
            DXT1a = 2,      // DXT1 with binary alpha.
            DXT3 = 3,
            DXT5 = 4,
            DXT5n = 5,      // Compressed HILO: R = 1, G = y, B = 0, A = x

            // DX10 formats
            BC1 = DXT1,
            BC1a = DXT1a,
            BC2 = DXT3,
            BC3 = DXT5,
            BC3n = DXT5n,
            BC4 = 6,        // ATI1
            BC5 = 7,        // 3DC, ATI2
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            Stream fileData = null;
            r.Seek(file.Offset);
            switch (source.Version)
            {
                case BIFF_VERSION:
                    {
                        fileData = new MemoryStream(r.ReadBytes((int)file.FileSize));
                    }
                    return Task.FromResult(fileData);
                case DZIP_VERSION:
                    {
                        var blocks = (int)((file.FileSize + 0xFFFF) >> 16);
                        var positions = new long[blocks + 1];
                        for (var i = 0; i < positions.Length; i++) positions[i] = file.Offset + r.ReadUInt32();
                        positions[blocks] = file.Offset + file.PackedSize;
                        var buffer = new byte[0x10000];
                        var bytesLeft = file.PackedSize;
                        var s = new MemoryStream();
                        {
                            for (var i = 0; i < blocks; i++)
                            {
                                r.Seek(positions[i]);
                                var bytesRead = r.DecompressLzf((int)(positions[i + 1] - positions[i + 0]), buffer);
                                if (i + 1 < blocks && bytesRead != buffer.Length) throw new FormatException();
                                s.Write(buffer, 0, bytesRead);
                                bytesLeft -= bytesRead;
                            }
                            if (s.Length != file.FileSize) throw new FormatException();
                            s.Position = 0;
                            fileData = s;
                        }
                    }
                    return Task.FromResult(fileData);
                case BUNDLE_MAGIC:
                    {
                        var newFileSize = file.FileSize;
                        switch (file.Compressed)
                        {
                            case 0: break; // no compression
                            case 1: fileData = new MemoryStream(r.DecompressZlib2((int)file.PackedSize, (int)newFileSize)); break;
                            case 2: fileData = new MemoryStream(r.DecompressSnappy((int)file.PackedSize, (int)newFileSize)); break;
                            case 3: fileData = new MemoryStream(r.DecompressDoboz((int)file.PackedSize, (int)newFileSize)); break;
                            case 4: case 5: fileData = new MemoryStream(r.DecompressLz4((int)file.PackedSize, (int)newFileSize)); break;
                            default: throw new ArgumentOutOfRangeException(nameof(file.Compressed), file.Compressed.ToString());
                        }
                    }
                    return Task.FromResult(fileData);
                case RDAR_MAGIC:
                    {
                        var (startIndex, nextIndex, offsets) = ((uint, uint, RDAR_HeaderOffset[]))file.Tag;
                        fileData = new MemoryStream();
                        for (var i = startIndex; i < nextIndex; i++)
                        {
                            var offset = offsets[i];
                            r.Seek((long)offset.Offset);
                            var buf = offset.PhysicalSize == offset.VirtualSize
                                ? r.ReadBytes((int)offset.PhysicalSize)
                                : r.DecompressOodle((int)offset.PhysicalSize, (int)offset.VirtualSize);
                            fileData.Write(buf, 0, buf.Length);
                        }
                    }
                    return Task.FromResult(fileData);
                case 'T':
                    {
                        var headerChunks = (uint[])source.Tag;
                        var tex = (CACHE_TEX_HeaderFile)file.Tag;
                        var fmt = tex.Format switch
                        {
                            7 => FORMAT.DXT1,
                            8 => FORMAT.DXT5,
                            10 => FORMAT.DXT5,
                            13 => FORMAT.DXT3,
                            14 => FORMAT.BC4,
                            15 => FORMAT.DXT5,
                            253 => FORMAT.RGB,
                            0 => FORMAT.RGB,
                            _ => throw new ArgumentOutOfRangeException(nameof(tex.Format), tex.Format.ToString()),
                        };

                        // skip tons of env probes??
                        if (tex.HasSkip) throw new FormatException("SKIP");

                        var cubemap = tex.HasCubemap;

                        // mark as texture arrays
                        var depth = tex.Caps > 1 && tex.T16 == 4 ? tex.Caps : 0U;

                        // TODO: check this
                        if (tex.T16 == 3 && tex.Format == 253) tex.Bpp = 32; // 32 bit

                        //if (tex.T16 == 0 && tex.Format == 0) tex.Bpp = 16; // 16 bit

                        var s = new MemoryStream();
                        {
                            // write header
                            var w = new BinaryWriter(s);
                            var ddsHeader = new DDS_HEADER
                            {
                                dwSize = DDS_HEADER.SizeOf,
                                dwFlags = DDSD.HEADER_FLAGS_TEXTURE | (tex.NumMips > 1 ? DDSD.MIPMAPCOUNT : 0),
                                dwHeight = tex.Height,
                                dwWidth = tex.Width,
                                dwMipMapCount = tex.NumMips,
                                dwCaps = DDSCAPS.TEXTURE,
                            };
                            ddsHeader.ddspf.dwSize = DDS_PIXELFORMAT.SizeOf;
                            var dxt10 = new DDS_HEADER_DXT10
                            {
                                dxgiFormat = DXGI_FORMAT.UNKNOWN,
                                resourceDimension = D3D10_RESOURCE_DIMENSION.UNKNOWN,
                                miscFlag = 0,
                                arraySize = 0,
                                miscFlags2 = 0,
                            };

                            // depth
                            if (depth > 0)
                            {
                                ddsHeader.dwFlags |= DDSD.DEPTH;
                                ddsHeader.dwDepth = depth;
                                ddsHeader.dwCaps2 = DDSCAPS2.VOLUME;
                                dxt10.resourceDimension = D3D10_RESOURCE_DIMENSION.TEXTURE3D;
                            }
                            else dxt10.resourceDimension = D3D10_RESOURCE_DIMENSION.TEXTURE2D;

                            // cubemap
                            if (cubemap)
                            {
                                ddsHeader.dwCaps |= DDSCAPS.COMPLEX;
                                ddsHeader.dwCaps2 |= DDSCAPS2.CUBEMAP | DDSCAPS2.CUBEMAP_ALLFACES;
                                dxt10.resourceDimension = D3D10_RESOURCE_DIMENSION.TEXTURE2D;
                                dxt10.arraySize = 6;
                            }

                            // ddspf
                            if (fmt == FORMAT.RGBA)
                            {
                                ddsHeader.dwFlags |= DDSD.PITCH;
                                var pitch = (tex.Bpp + 7) / 8 * tex.Width;
                                ddsHeader.dwPitchOrLinearSize = (pitch + 3) / 4 * 4;
                                if (tex.Bpp == 8)
                                {
                                    ddsHeader.ddspf.dwFlags = DDPF.RGB | DDPF.ALPHAPIXELS;
                                    ddsHeader.ddspf.dwRGBBitCount = 8;
                                    ddsHeader.ddspf.dwRBitMask = 0x0f00;
                                    ddsHeader.ddspf.dwGBitMask = 0x00f0;
                                    ddsHeader.ddspf.dwBBitMask = 0x000f;
                                    ddsHeader.ddspf.dwABitMask = 0xf000;
                                }
                                else if (tex.Bpp == 16)
                                {
                                    ddsHeader.ddspf.dwFlags = DDPF.RGB | DDPF.ALPHAPIXELS;
                                    ddsHeader.ddspf.dwRGBBitCount = 16;
                                    ddsHeader.ddspf.dwRBitMask = 0x03f000;
                                    ddsHeader.ddspf.dwGBitMask = 0x000fc0;
                                    ddsHeader.ddspf.dwBBitMask = 0x00003f;
                                    ddsHeader.ddspf.dwABitMask = 0xfc0000;
                                }
                                else if (tex.Bpp == 32)
                                {
                                    ddsHeader.ddspf.dwFlags = DDPF.RGB | DDPF.ALPHAPIXELS;
                                    ddsHeader.ddspf.dwRGBBitCount = 32;
                                    ddsHeader.ddspf.dwRBitMask = 0x00ff0000;
                                    ddsHeader.ddspf.dwGBitMask = 0x0000ff00;
                                    ddsHeader.ddspf.dwBBitMask = 0x000000ff;
                                    ddsHeader.ddspf.dwABitMask = 0xff000000;
                                }
                            }
                            else
                            {
                                var bs = new[] { 8U, 8U, 16U, 16U, 16U, 8U, 16U };
                                ddsHeader.dwPitchOrLinearSize = (uint)(bs[(int)fmt] * ((tex.Width + 3) / 4) * ((tex.Height + 3) / 4));
                                ddsHeader.dwFlags = DDSD.LINEARSIZE;
                                ddsHeader.ddspf.dwFlags = DDPF.FOURCC;
                                if (fmt == FORMAT.DXT1 || fmt == FORMAT.DXT1a) ddsHeader.ddspf.dwFourCC = FourCC.DXT1;
                                else if (fmt == FORMAT.DXT3) ddsHeader.ddspf.dwFourCC = FourCC.DXT3;
                                else if (fmt == FORMAT.DXT5) ddsHeader.ddspf.dwFourCC = FourCC.DXT5;
                                else if (fmt == FORMAT.DXT5n) ddsHeader.ddspf.dwFourCC = FourCC.DXT5;
                                else if (fmt == FORMAT.BC4) ddsHeader.ddspf.dwFourCC = FourCC.ATI1;
                                else if (fmt == FORMAT.DXT3) ddsHeader.ddspf.dwFourCC = FourCC.ATI2;
                            }
                            w.Write(DDS_HEADER.MAGIC);
                            w.WriteT(ddsHeader, sizeof(DDS_HEADER));

                            void Read(long position)
                            {
                                r.Seek(position);
                                var packedSize = r.ReadUInt32();
                                var fileSize = r.ReadUInt32();
                                var part = r.ReadByte();
                                w.Write(r.DecompressZlib((int)packedSize, -1));
                            }

                            // write primary
                            var offset = tex.StartOffset * 4096;
                            Read(offset);

                            // write mips
                            for (var i = 0; i < tex.NumMips; i++) Read(offset + headerChunks[tex.ChunkStartIndex + i]);
                            s.Position = 0;
                            fileData = s;
                        }
                    }
                    return Task.FromResult(fileData);
                default: throw new ArgumentOutOfRangeException(nameof(source.Version), $"{source.Version}");
            }
        }
    }
}