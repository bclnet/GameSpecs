using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Bioware.Formats
{
    public unsafe class PakBinary_Aurora : PakBinary<PakBinary_Aurora>
    {
        // https://nwn2.fandom.com/wiki/File_formats

        // Headers : KEY/BIF
        #region Headers : KEY/BIF

        const uint KEY_MAGIC = 0x2059454b;
        const uint KEY_VERSION = 0x20203156;

        const uint BIFF_MAGIC = 0x46464942;
        const uint BIFF_VERSION = 0x20203156;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct KEY_Header
        {
            public uint Version;            // Version ("V1  ")
            public uint NumFiles;           // Number of entries in FILETABLE
            public uint NumKeys;            // Number of entries in KEYTABLE.
            public uint FilesOffset;        // Offset to FILETABLE (0x440000).
            public uint KeysOffset;         // Offset to KEYTABLE.
            public uint BuildYear;          // Build year (less 1900).
            public uint BuildDay;           // Build day
            public fixed byte NotUsed02[32]; // Not used
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct KEY_HeaderFile
        {
            public uint FileSize;           // BIF Filesize
            public uint FileNameOffset;     // Offset To BIF name
            public ushort FileNameSize;     // Size of BIF name
            public ushort Drives;           // A number that represents which drives the BIF file is located in
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct KEY_HeaderFileName
        {
            public fixed byte Name[0x10];   // Null-padded string Resource Name (sans extension).
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct KEY_HeaderKey
        {
            public fixed byte Name[0x10];   // Null-padded string Resource Name (sans extension).
            public ushort ResourceType;     // Resource Type
            public uint Id;                 // Resource ID
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct BIFF_Header
        {
            public uint Version;            // Version ("V1  ")
            public uint NumFiles;           // File Count
            public uint NotUsed01;          // Not used
            public uint FilesOffset;        // Offset to FILETABLE
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct BIFF_HeaderFile
        {
            public uint FileId;             // File ID
            public uint Offset;             // Offset to File Data.
            public uint FileSize;           // Size of File Data.
            public uint FileType;           // File Type
            public uint Id => (FileId & 0xFFF00000) >> 20; // BIF index
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

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            FileSource[] files; List<FileSource> files2;

            // KEY
            var magic = source.Magic = r.ReadUInt32();
            if (magic == KEY_MAGIC) // Signature("KEY ")
            {
                var header = r.ReadT<KEY_Header>(sizeof(KEY_Header));
                if (header.Version != KEY_VERSION) throw new FormatException("BAD MAGIC");
                source.Version = header.Version;
                source.Files = files = new FileSource[header.NumFiles];

                // parts
                r.Seek(header.FilesOffset);
                var headerFiles = r.ReadTArray<KEY_HeaderFile>(sizeof(KEY_HeaderFile), (int)header.NumFiles).Select(x =>
                {
                    r.Seek(x.FileNameOffset);
                    return (file: x, path: r.ReadFString(x.FileNameSize - 1));
                }).ToArray();
                r.Seek(header.KeysOffset);
                var headerKeys = r.ReadTArray<KEY_HeaderKey>(sizeof(KEY_HeaderKey), (int)header.NumKeys).ToDictionary(x => x.Id, x => UnsafeX.FixedAString(x.Name, 0x10));

                // combine
                var subPathFormat = Path.Combine(Path.GetDirectoryName(source.PakPath), "{0}");
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
            // BIFF
            else if (magic == BIFF_MAGIC) // Signature("BIFF")
            {
                if (source.Tag == null) throw new FormatException("BIFF files can only be processed through KEY files");
                var (keys, bifId) = ((Dictionary<uint, string> keys, uint bifId))source.Tag;
                var header = r.ReadT<BIFF_Header>(sizeof(BIFF_Header));
                if (header.Version != BIFF_VERSION) throw new FormatException("BAD MAGIC");
                source.Version = header.Version;
                source.Files = files2 = new List<FileSource>();

                // files
                r.Seek(header.FilesOffset);
                var headerFiles = r.ReadTArray<BIFF_HeaderFile>(sizeof(BIFF_HeaderFile), (int)header.NumFiles);
                for (var i = 0; i < headerFiles.Length; i++)
                {
                    var headerFile = headerFiles[i];
                    if (headerFile.Id > i) continue;
                    var path = $"{(keys.TryGetValue(headerFile.Id, out var key) ? key : $"{i}")}{(BIFF_FileTypes.TryGetValue((int)headerFile.FileType, out var z) ? $".{z}" : string.Empty)}".Replace('\\', '/');
                    files2.Add(new FileSource
                    {
                        Id = (int)headerFile.Id,
                        Path = path,
                        FileSize = headerFile.FileSize,
                        Offset = headerFile.Offset,
                    });
                }
            }
            else throw new FormatException($"Unknown File Type {magic}");
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            Stream fileData;
            r.Seek(file.Offset);
            if (source.Version == BIFF_VERSION) fileData = new MemoryStream(r.ReadBytes((int)file.FileSize));
            else throw new ArgumentOutOfRangeException(nameof(source.Version), $"{source.Version}");
            return Task.FromResult(fileData);
        }
    }
}