using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Monolith.Formats
{
    public unsafe class PakBinary_Lith : PakBinary<PakBinary_Lith>
    {
        // Headers
        #region X_Headers

        const uint CRLF = 0x0d0a;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct X_FileMainHeader
        {
            public ushort CrLf1;            // \r\n
            public fixed byte FileType[60]; // FileType
            public ushort CrLf2;            // \r\n
            public fixed byte UserTitle[60];// FileType
            public ushort CrLf3;            // \r\n
            public byte Eof1;               // Eof
            public uint FileFormatVersion;  // The file format version number only 1 is possible here right now
            public uint RootDirPos;         // Position of the root directory structure in the file
            public uint RootDirSize;        // Size of root directory
            public uint RootDirTime;        // Time Root dir was last updated
            public uint NextWritePos;       // Position of first directory in the file
            public uint Time;               // Time resource file was last updated
            public uint LargestKeyAry;      // Size of the largest key array in the resource file
            public uint LargestDirNameSize; // Size of the largest directory name in the resource file (including 0 terminator)
            public uint LargestRezNameSize; // Size of the largest resource name in the resource file (including 0 terminator)
            public uint LargestCommentSize; // Size of the largest comment in the resource file (including 0 terminator)
            public byte IsSorted;           // If 0 then data is not sorted if 1 then it is sorted
        }

        enum FileDirEntryType
        {
            ResourceEntry = 0,
            DirectoryEntry = 1
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct X_FileDirEntryDirHeader
        {
            public uint Pos;                // File positon of dir entry
            public uint Size;               // Size of directory data
            public uint Time;               // Last time anything in directory was modified
            //public string Name;           // Name of this directory
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct X_FileDirEntryRezHeader
        {
            public uint Pos;                // File positon of dir entry
            public uint Size;               // Size of directory data
            public uint Time;               // Last time this resource was modified
            public uint ID;                 // Resource ID number
            public uint Type;               // Type of resource this is
            public uint NumKeys;            // The number of keys to read in for this resource
            //public string Name;           // The name of this resource
            //public string Comment;        // The comment data for this resource
            //public int[] Keys;            // The key values for this resource
        }

        struct X_FileDirEntryHeader
        {
            uint Type;
            X_FileDirEntryRezHeader Rez;
            X_FileDirEntryDirHeader Dir;
        }

        #endregion

        // Headers
        #region Headers

        const uint MAGIC = 0x5241544c;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct Header
        {
            public const int SizeOf = 0x30;
            public uint Magic;                      // Magic
            public byte Version;                    // Version
            fixed byte Unknown1[3];                 // Unknown
            public int StringsLength;               // DescriptionLength
            public int DirectoryCount;              // Directorys
            public int FileCount;                   // Files
            fixed byte Unknown2[0x1c];              // Unknown
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct HeaderFile
        {
            public int StringAt;                    // StringAt
            public uint Position;                   // Position
            public int Unknown1;                    // Unknown
            public int FileSize;                    // FileSize
            fixed byte Unknown2[0x10];              // Unknown
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct HeaderDirectory
        {
            public int StringAt;                    // StringAt
            public int NextChild;                   // NextChild
            public int NextBrother;                 // NextBrother
            public int Count;                       // Count
        }

        class ArchBase
        {
            public ArchDirectory Parent;
            public string Path;
        }

        class ArchFile : ArchBase
        {
            public int FileSize;
            public uint Position;
        }

        class ArchDirectory : ArchBase
        {
            public List<ArchBase> Files;
            public int Count;
        }

        #endregion

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            // read file
            var header = r.ReadT<Header>(sizeof(Header));
            if (header.Magic != MAGIC) throw new FormatException("BAD MAGIC");
            if (header.Version != 3) throw new FormatException("BAD VERSION");
            source.Version = header.Version;
            var files = new ArchFile[header.FileCount];
            var directories = new ArchDirectory[header.DirectoryCount];
            var strings = r.ReadBytes(header.StringsLength);
            fixed (byte* stringsB = strings)
            {
                // files
                var headerFiles = r.ReadTArray<HeaderFile>(sizeof(HeaderFile), header.FileCount);
                for (var i = 0; i < headerFiles.Length; i++)
                {
                    var headerFile = headerFiles[i];
                    files[i] = new ArchFile
                    {
                        Path = new string((sbyte*)&stringsB[headerFile.StringAt]),
                        FileSize = headerFile.FileSize,
                        Position = headerFile.Position,
                    };
                }

                // directories
                var headerDirectories = r.ReadTArray<HeaderDirectory>(sizeof(HeaderDirectory), header.DirectoryCount);
                for (var i = 0; i < headerDirectories.Length; i++)
                {
                    var headerDirectory = headerDirectories[i];
                    directories[i] = new ArchDirectory
                    {
                        Files = new List<ArchBase>(),
                        Path = new string((sbyte*)&stringsB[headerDirectory.StringAt]).Replace('\\', '/'),
                        Count = headerDirectory.Count,
                    };
                }
            }

            // build tree
            int fileIndex = 0, directoryIdx = 0;
            while (directoryIdx < directories.Length)
            {
                var directory = directories[directoryIdx];
                directory.Files.Clear();
                var countIdx = 0;
                while (true)
                {
                    if (countIdx >= directory.Count)
                    {
                        if (directory.Parent != null) directory.Parent.Files.Add(directory);
                        directoryIdx++;
                        break;
                    }
                    var file = files[fileIndex++];
                    file.Parent = directory;
                    directory.Files.Add(file);
                    countIdx++;
                }
            }

            source.Files = directories.SelectMany(x => x.Files.Cast<ArchFile>(), (a, b) => new FileSource
            {
                Path = $"{a.Path}/{b.Path}",
                FileSize = b.FileSize,
                Offset = b.Position,
            }).ToArray();

            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            Stream fileData;
            r.Seek(file.Offset);
            fileData = new MemoryStream(r.ReadBytes((int)file.FileSize));
            return Task.FromResult(fileData);
        }
    }
}