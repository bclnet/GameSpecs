using GameX.Formats;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Bethesda.Formats
{
    public unsafe class PakBinary_Bsa : PakBinary<PakBinary_Bsa>
    {
        // Header : TES4
        #region Header : TES4
        // http://en.uesp.net/wiki/Bethesda4Mod:BSA_File_Format

        // Default header data
        const uint OB_BSAHEADER_FILEID = 0x00415342;    // Magic for Oblivion BSA, the literal string "BSA\0".
        const uint OB_BSAHEADER_VERSION = 0x67;         // Version number of an Oblivion BSA
        const uint F3_BSAHEADER_VERSION = 0x68;         // Version number of a Fallout 3 BSA
        const uint SSE_BSAHEADER_VERSION = 0x69;        // Version number of a Skyrim SE BSA

        // Archive flags
        const ushort OB_BSAARCHIVE_PATHNAMES = 0x0001;  // Whether the BSA has names for paths
        const ushort OB_BSAARCHIVE_FILENAMES = 0x0002;  // Whether the BSA has names for files
        const ushort OB_BSAARCHIVE_COMPRESSFILES = 0x0004; // Whether the files are compressed
        const ushort F3_BSAARCHIVE_PREFIXFULLFILENAMES = 0x0100; // Whether the name is prefixed to the data?

        // File flags
        //const ushort OB_BSAFILE_NIF = 0x0001; // Set when the BSA contains NIF files (Meshes)
        //const ushort OB_BSAFILE_DDS = 0x0002; // Set when the BSA contains DDS files (Textures)
        //const ushort OB_BSAFILE_XML = 0x0004; // Set when the BSA contains XML files (Menus)
        //const ushort OB_BSAFILE_WAV = 0x0008; // Set when the BSA contains WAV files (Sounds)
        //const ushort OB_BSAFILE_MP3 = 0x0010; // Set when the BSA contains MP3 files (Voices)
        //const ushort OB_BSAFILE_TXT = 0x0020; // Set when the BSA contains TXT files (Shaders)
        //const ushort OB_BSAFILE_HTML = 0x0020; // Set when the BSA contains HTML files
        //const ushort OB_BSAFILE_BAT = 0x0020; // Set when the BSA contains BAT files
        //const ushort OB_BSAFILE_SCC = 0x0020; // Set when the BSA contains SCC files
        //const ushort OB_BSAFILE_SPT = 0x0040; // Set when the BSA contains SPT files (Trees)
        //const ushort OB_BSAFILE_TEX = 0x0080; // Set when the BSA contains TEX files
        //const ushort OB_BSAFILE_FNT = 0x0080; // Set when the BSA contains FNT files (Fonts)
        //const ushort OB_BSAFILE_CTL = 0x0100; // Set when the BSA contains CTL files (Miscellaneous)

        // Bitmasks for the size field in the header
        const uint OB_BSAFILE_SIZEMASK = 0x3fffffff; // Bit mask with OB_HeaderFile:SizeFlags to get the compression status
        const uint OB_BSAFILE_SIZECOMPRESS = 0xC0000000; // Bit mask with OB_HeaderFile:SizeFlags to get the compression status

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct OB_Header
        {
            public static (string, int) Struct = ("<IIIIIIII", sizeof(OB_Header));
            public uint Version;            // 04
            public uint FolderRecordOffset; // Offset of beginning of folder records
            public uint ArchiveFlags;       // Archive flags
            public uint FolderCount;        // Total number of folder records (OBBSAFolderInfo)
            public uint FileCount;          // Total number of file records (OBBSAFileInfo)
            public uint FolderNameLength;   // Total length of folder names
            public uint FileNameLength;     // Total length of file names
            public uint FileFlags;          // File flags
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct OB_Folder
        {
            public static (string, int) Struct = ("<QII", sizeof(OB_Folder));
            public ulong Hash;              // Hash of the folder name
            public uint FileCount;          // Number of files in folder
            public uint Offset;             // The offset
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct OB_FolderSSE
        {
            public static (string, int) Struct = ("<QIIQ", sizeof(OB_Folder));
            public ulong Hash;              // Hash of the folder name
            public uint FileCount;          // Number of files in folder
            public uint Unk;                // Unknown
            public ulong Offset;            // The offset
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct OB_File
        {
            public static (string, int) Struct = ("<QII", sizeof(OB_Folder));
            public ulong Hash;              // Hash of the filename
            public uint Size;               // Size of the data, possibly with OB_BSAFILE_SIZECOMPRESS set
            public uint Offset;             // Offset to raw file data
        }

        #endregion

        // Header : TES3
        #region Header : TES3
        // http://en.uesp.net/wiki/Bethesda3Mod:BSA_File_Format

        // Default header data
        const uint MW_BSAHEADER_FILEID = 0x00000100; // Magic for Morrowind BSA

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct MW_Header
        {
            public static (string, int) Struct = ("<II", sizeof(MW_Header));
            public uint HashOffset;         // Offset of hash table minus header size (12)
            public uint FileCount;          // Number of files in the archive
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct MW_File
        {
            public static (string, int) Struct = ("<II", sizeof(MW_File));
            public uint FileSize;           // File size
            public uint FileOffset;         // File offset relative to data position
            public readonly uint Size => FileSize > 0 ? FileSize & 0x3FFFFFFF : 0; // The size of the file inside the BSA
        }

        #endregion

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            FileSource[] files;
            var magic = source.Magic = r.ReadUInt32();

            // Oblivion - Skyrim
            if (magic == OB_BSAHEADER_FILEID)
            {
                var header = r.ReadS<OB_Header>();
                if (header.Version != OB_BSAHEADER_VERSION && header.Version != F3_BSAHEADER_VERSION && header.Version != SSE_BSAHEADER_VERSION)
                    throw new FormatException("BAD MAGIC");
                if ((header.ArchiveFlags & OB_BSAARCHIVE_PATHNAMES) == 0 || (header.ArchiveFlags & OB_BSAARCHIVE_FILENAMES) == 0)
                    throw new FormatException("HEADER FLAGS");
                source.Version = header.Version;

                // calculate some useful values
                var compressedToggle = (header.ArchiveFlags & OB_BSAARCHIVE_COMPRESSFILES) > 0;
                if (header.Version == F3_BSAHEADER_VERSION || header.Version == SSE_BSAHEADER_VERSION)
                    source.Tag = (header.ArchiveFlags & F3_BSAARCHIVE_PREFIXFULLFILENAMES) > 0 ? 'Y' : 'N';

                // read-all folders
                var foldersFiles = header.Version == SSE_BSAHEADER_VERSION
                    ? r.ReadTArray<OB_FolderSSE>(sizeof(OB_FolderSSE), (int)header.FolderCount).Select(x => x.FileCount).ToArray()
                    : r.ReadTArray<OB_Folder>(sizeof(OB_Folder), (int)header.FolderCount).Select(x => x.FileCount).ToArray();

                // read-all folder files
                var fileIdx = 0U;
                source.Files = files = new FileSource[header.FileCount];
                for (var i = 0; i < header.FolderCount; i++)
                {
                    var folderName = r.ReadFString(r.ReadByte() - 1).Replace('\\', '/');
                    r.Skip(1);
                    var headerFiles = r.ReadTArray<OB_File>(sizeof(OB_File), (int)foldersFiles[i]);
                    foreach (var headerFile in headerFiles)
                    {
                        var compressed = (headerFile.Size & OB_BSAFILE_SIZECOMPRESS) != 0;
                        var packedSize = compressed ? headerFile.Size ^ OB_BSAFILE_SIZECOMPRESS : headerFile.Size;
                        files[fileIdx++] = new FileSource
                        {
                            Path = folderName,
                            Offset = headerFile.Offset,
                            Compressed = compressed ^ compressedToggle ? 1 : 0,
                            PackedSize = packedSize,
                            FileSize = source.Version == SSE_BSAHEADER_VERSION ? packedSize & OB_BSAFILE_SIZEMASK : packedSize,
                        };
                    };
                }

                // read-all names
                foreach (var file in files) file.Path = $"{file.Path}/{r.ReadCString()}";
            }
            // Morrowind
            else if (magic == MW_BSAHEADER_FILEID)
            {
                var header = r.ReadS<MW_Header>();
                var dataOffset = 12 + header.HashOffset + (8 * header.FileCount);

                // create filesources
                source.Files = files = new FileSource[header.FileCount];
                var headerFiles = r.ReadTArray<MW_File>(sizeof(MW_File), (int)header.FileCount);
                for (var i = 0; i < headerFiles.Length; i++)
                {
                    ref MW_File headerFile = ref headerFiles[i];
                    var size = headerFile.Size;
                    files[i] = new FileSource
                    {
                        Offset = dataOffset + headerFile.FileOffset,
                        FileSize = size,
                        PackedSize = size,
                    };
                }

                // read filename offsets
                var filenameOffsets = r.ReadTArray<uint>(sizeof(uint), (int)header.FileCount); // relative offset in filenames section

                // read filenames
                var filenamesPosition = r.Tell();
                for (var i = 0; i < files.Length; i++)
                {
                    r.Seek(filenamesPosition + filenameOffsets[i]);
                    files[i].Path = r.ReadZAString(1000).Replace('\\', '/');
                }
            }
            else throw new InvalidOperationException("BAD MAGIC");
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            // position
            var fileSize = (int)file.FileSize;
            r.Seek(file.Offset);
            if (source.Tag is char? && ((char?)source.Tag).Value == 'Y')
            {
                var prefixLength = r.ReadByte() + 1;
                if (source.Version == SSE_BSAHEADER_VERSION)
                    fileSize -= prefixLength;
                r.Seek(file.Offset + prefixLength);
            }

            // not compressed
            if (fileSize <= 0 || file.Compressed == 0)
                return Task.FromResult<Stream>(new MemoryStream(r.ReadBytes(fileSize)));

            // compressed
            var newFileSize = (int)r.ReadUInt32(); fileSize -= 4;
            return Task.FromResult<Stream>(source.Version == SSE_BSAHEADER_VERSION
                ? new MemoryStream(r.DecompressLz4(fileSize, newFileSize))
                : new MemoryStream(r.DecompressZlib2(fileSize, newFileSize)));
        }
    }
}