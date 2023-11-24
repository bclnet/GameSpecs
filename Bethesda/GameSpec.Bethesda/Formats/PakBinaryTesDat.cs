using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Bethesda.Formats
{
    // Fallout 2
    public unsafe class PakBinaryBethesdaDat : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryBethesdaDat();

        // Header : F1
        #region Header : F1
        // https://falloutmods.fandom.com/wiki/DAT_file_format

        const uint F1_HEADER_FILEID = 0x000000001;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F1_Header
        {
            public static string Endian = "B4B4B4B4";
            public uint DirectoryCount;
            public uint Unknown1;
            public uint Unknown2;
            public uint Unknown3;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F1_ContentBlock
        {
            public static string Endian = "B4B4B4B4";
            public uint FileCount;
            public uint Unknown1;
            public uint Unknown2;
            public uint Unknown3;
        }

        #endregion

        // Header : F2
        #region Header : F2
        // https://falloutmods.fandom.com/wiki/DAT_file_format

        const uint F2_HEADER_FILEID = 0x000000011;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct F2_Header
        {
            public uint TreeSize;               // Size of DirTree in bytes
            public uint DataSize;               // Full size of the archive in bytes
        }

        #endregion

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, object tag)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            var gameId = source.Game.Id;

            if (!string.Equals(Path.GetExtension(source.FilePath), ".dat", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("BAD MAGIC");

            if (gameId == "Fallout")
            {
                source.Magic = F1_HEADER_FILEID;
                var header = r.ReadTE<F1_Header>(sizeof(F1_Header), F1_Header.Endian);
                var directoryNames = new string[header.DirectoryCount];
                for (var i = 0; i < header.DirectoryCount; i++) directoryNames[i] = r.ReadL8Encoding().Replace('\\', '/'); // directory name block
                // Create file metadatas
                var files = new List<FileMetadata>(); multiSource.Files = files;
                for (var i = 0; i < header.DirectoryCount; i++)
                {
                    var contentBlock = r.ReadTE<F1_ContentBlock>(sizeof(F1_ContentBlock), F1_ContentBlock.Endian); // directory content block
                    var directoryPrefix = directoryNames[i] != "." ? directoryNames[i] + "\\" : string.Empty;
                    for (var j = 0; j < contentBlock.FileCount; j++)
                        files.Add(new FileMetadata
                        {
                            Path = directoryPrefix + r.ReadL8Encoding().Replace('\\', '/'),
                            Compressed = (int)r.ReadUInt32E() & 0x40,
                            Position = r.ReadUInt32E(),
                            FileSize = r.ReadUInt32E(),
                            PackedSize = r.ReadUInt32E(),
                        });
                }
            }
            else if (gameId == "Fallout2")
            {
                source.Magic = F2_HEADER_FILEID;
                r.Seek(r.BaseStream.Length - sizeof(F2_Header));
                var header = r.ReadT<F2_Header>(sizeof(F2_Header));
                if (header.DataSize != r.BaseStream.Length) throw new InvalidOperationException("File is not a valid bsa archive.");
                r.Seek(header.DataSize - header.TreeSize - sizeof(F2_Header));

                // Create file metadatas
                var files = new FileMetadata[r.ReadInt32()]; multiSource.Files = files;
                for (var i = 0; i < files.Length; i++)
                    files[i] = new FileMetadata
                    {
                        Path = r.ReadL32Encoding().TrimStart('\\').Replace('\\', '/'),
                        Compressed = r.ReadByte(),
                        FileSize = r.ReadUInt32(),
                        PackedSize = r.ReadUInt32(),
                        Position = r.ReadUInt32(),
                    };
            }
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            var magic = source.Magic;
            // F1
            if (magic == F1_HEADER_FILEID)
            {
                r.Seek(file.Position);
                Stream fileData = new MemoryStream(file.Compressed == 0
                    ? r.ReadBytes((int)file.PackedSize)
                    : r.DecompressLzss((int)file.PackedSize, (int)file.FileSize));
                return Task.FromResult(fileData);
            }
            // F2
            else if (magic == F2_HEADER_FILEID)
            {
                r.Seek(file.Position);
                Stream fileData = new MemoryStream(r.Peek(z => z.ReadUInt16()) == 0xda78
                    ? r.DecompressZlib((int)file.PackedSize, -1)
                    : r.ReadBytes((int)file.PackedSize));
                return Task.FromResult(fileData);
            }
            else throw new InvalidOperationException("BAD MAGIC");
        }
    }
}