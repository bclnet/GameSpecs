using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static GameX.Black.Formats.Binary_Frm;

namespace GameX.Black.Formats
{
    // Fallout 2
    public unsafe class PakBinary_Dat : PakBinary<PakBinary_Dat>
    {
        // Header : F1
        #region Header : F1
        // https://falloutmods.fandom.com/wiki/DAT_file_format

        const uint F1_HEADER_FILEID = 0x000000001;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F1_Header
        {
            //public static string Map = "B4B4B4B4";
            public static (string, int) Struct = (">4I", sizeof(F1_Header));
            public uint DirectoryCount; // DirectoryCount
            public uint Unknown1; // Usually 0x0A (0x5E for master.dat). Must not be less than 1 or Fallout will crash instantly with a memory read error. Possibly some kind of memory buffer size.
            public uint Unknown2; // Always 0.
            public uint Unknown3; // Could be some kind of checksum, but Fallout seems to work fine with any value.
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F1_Directory
        {
            //public static string Map = "B4B4B4B4";
            public static (string, int) Struct = (">4I", sizeof(F1_Directory));
            public uint FileCount; // Number of files in the directory.
            public uint Unknown1; // Similar to (Unknown1), the default value seems to be 0x0A and Fallout works with most positive non-zero values.
            public uint Unknown2; // Seems to always be 0x10.
            public uint Unknown3; // See (Unknown3).
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F1_File
        {
            //public static string Map = "B4B4B4B4";
            public static (string, int) Struct = (">4I", sizeof(F1_File));
            public uint Attributes; // 0x20 means plain-text, 0x40 - compressed with LZSS.
            public uint Offset; // Position in the file (from the beginning of the DAT file), where the file contets start.
            public uint Size; // Original (uncompressed) file size.
            public uint PackedSize; // Size of the compressed file in dat. If file is not compressed, PackedSize is 0.
        }

        #endregion

        // Header : F2
        #region Header : F2
        // https://falloutmods.fandom.com/wiki/DAT_file_format

        const uint F2_HEADER_FILEID = 0x000000011;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F2_Header
        {
            public static (string, int) Struct = ("<2I", sizeof(F2_Header));
            public uint TreeSize;               // Size of DirTree in bytes
            public uint DataSize;               // Full size of the archive in bytes
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F2_File
        {
            public static (string, int) Struct = ("<B3I", sizeof(F2_File));
            public byte Type;               // 1 = Compressed 0 = Decompressed
            public uint RealSize;           // Size of the file without compression.
            public uint PackedSize;         // Size of the compressed file.
            public uint Offset;             // Address/Location of the file.
        }

        #endregion

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var gameId = source.Game.Id;

            // Fallout
            if (gameId == "Fallout")
            {
                source.Magic = F1_HEADER_FILEID;
                var header = r.ReadS<F1_Header>();
                var directoryPaths = new string[header.DirectoryCount];
                for (var i = 0; i < header.DirectoryCount; i++)
                    directoryPaths[i] = r.ReadL8Encoding().Replace('\\', '/');
                // Create file metadatas
                var files = new List<FileSource>(); source.Files = files;
                for (var i = 0; i < header.DirectoryCount; i++)
                {
                    var directory = r.ReadS<F1_Directory>();
                    var directoryPath = directoryPaths[i] != "." ? directoryPaths[i] + "/" : string.Empty;
                    for (var j = 0; j < directory.FileCount; j++)
                    {
                        var path = directoryPath + r.ReadL8Encoding().Replace('\\', '/');
                        var file = r.ReadS<F1_File>();
                        files.Add(new FileSource
                        {
                            Path = path,
                            Compressed = (int)file.Attributes & 0x40,
                            Offset = file.Offset,
                            FileSize = file.Size,
                            PackedSize = file.PackedSize,
                        });
                    }
                }
            }

            // Fallout2
            else if (gameId == "Fallout2")
            {
                source.Magic = F2_HEADER_FILEID;
                r.Seek(r.BaseStream.Length - sizeof(F2_Header));
                var header = r.ReadS<F2_Header>();
                if (header.DataSize != r.BaseStream.Length) throw new InvalidOperationException("File is not a valid bsa archive.");
                r.Seek(header.DataSize - header.TreeSize - sizeof(F2_Header));

                // Create file metadatas
                var files = new FileSource[r.ReadInt32()]; source.Files = files;
                for (var i = 0; i < files.Length; i++)
                {
                    var path = r.ReadL32Encoding().Replace('\\', '/');
                    var file = r.ReadS<F2_File>();
                    files[i] = new FileSource
                    {
                        Path = path,
                        Compressed = file.Type,
                        FileSize = file.RealSize,
                        PackedSize = file.PackedSize,
                        Offset = file.Offset,
                    };
                }
            }
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            var magic = source.Magic;
            // F1
            if (magic == F1_HEADER_FILEID)
            {
                r.Seek(file.Offset);
                Stream fileData = new MemoryStream(file.Compressed == 0
                    ? r.ReadBytes((int)file.PackedSize)
                    : r.DecompressLzss((int)file.PackedSize, (int)file.FileSize));
                return Task.FromResult(fileData);
            }
            // F2
            else if (magic == F2_HEADER_FILEID)
            {
                r.Seek(file.Offset);
                Stream fileData = new MemoryStream(r.Peek(z => z.ReadUInt16()) == 0xda78
                    ? r.DecompressZlib((int)file.PackedSize, -1)
                    : r.ReadBytes((int)file.PackedSize));
                return Task.FromResult(fileData);
            }
            else throw new InvalidOperationException("BAD MAGIC");
        }
    }
}