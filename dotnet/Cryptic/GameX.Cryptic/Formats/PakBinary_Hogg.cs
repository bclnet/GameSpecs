using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameX.Cryptic.Formats
{
    // https://github.com/nohbdy/libhogg
    public unsafe class PakBinary_Hogg : PakBinary<PakBinary_Hogg>
    {
        // Headers
        #region Headers

        const uint MAGIC = 0xDEADF00D; // DEADF00D

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Header
        {
            public uint Magic;                      // DEADF00D
            public ushort Version;                  // Version == 0x0400
            public ushort OperationJournalSection;  // Size of the operation journal section == 0x000A
            public uint FileEntrySection;           // Size of the file entry section
            public uint AttributeEntrySection;      // Size of the attribute entry section
            public uint DataFileNumber;             // 
            public uint FileJournalSection;         // Size of the file journal section == 0x000A
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FileJournalHeader
        {
            public uint Unknown1;                   // Unknown
            public uint Size;                      // Size
            public uint Size2;                      // Size
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FileEntry
        {
            public long Offset;                     // offset to the file's data
            public int FileSize;                    // size of the filedata within the archive
            public uint Timestamp;                  // 32-bit timestamp (seconds since 1970)
            public uint Checksum;                   // checksum
            public uint Unknown4;                   // Unknown
            public ushort Unknown5;                 // Unknown
            public ushort Unknown6;                 // Unknown
            public int Id;                          // Id
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AttributeEntry
        {
            public int PathId;                      // Data ID of the file's path
            public int ExcerptId;                   // Data ID of a data excerpt, or -1 if there is none
            public uint UncompressedSize;           // Size of the data after decompression, or 0 if the file is not compressed
            public uint Flags;                      // Flags
        }

        #endregion

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            // read header
            var header = r.ReadT<Header>(sizeof(Header));
            if (header.Magic != MAGIC) throw new FormatException("BAD MAGIC");
            if (header.Version < 10 || header.Version > 11) throw new FormatException("BAD Version");
            if (header.OperationJournalSection > 1024) throw new FormatException("BAD Journal");
            if (header.FileEntrySection != header.AttributeEntrySection << 1) throw new FormatException("data entry / compression info section size mismatch");
            var numFiles = (int)(header.AttributeEntrySection >> 4);

            // skip journals
            r.Skip(header.OperationJournalSection);
            var fileJournalPosition = r.BaseStream.Position;
            r.Skip(header.FileJournalSection);

            // read files
            var fileEntries = r.ReadTArray<FileEntry>(sizeof(FileEntry), numFiles);
            var attributeEntries = r.ReadTArray<AttributeEntry>(sizeof(AttributeEntry), numFiles);
            var files = new FileSource[numFiles];
            for (var i = 0; i < files.Length; i++)
            {
                ref FileEntry s = ref fileEntries[i];
                ref AttributeEntry a = ref attributeEntries[i];
                files[i] = new FileSource
                {
                    Id = s.Id,
                    Offset = s.Offset,
                    FileSize = s.FileSize,
                    PackedSize = a.UncompressedSize,
                    Compressed = a.UncompressedSize > 0 ? 1 : 0,
                };
            }

            // read "Datalist" file
            var dataListFile = files[0];
            if (dataListFile.Id != 0 || dataListFile.FileSize == -1) throw new FormatException("BAD DataList");
            var fileAttribs = new Dictionary<int, byte[]>();
            using (var r2 = new BinaryReader(ReadData(source, r, dataListFile).Result))
            {
                if (r2.ReadUInt32() != 0) throw new FormatException("BAD DataList");
                var count = r2.ReadInt32();
                for (var i = 0; i < count; i++) fileAttribs.Add(i, r2.ReadBytes((int)r2.ReadUInt32()));
            }

            // read file journal
            r.Seek(fileJournalPosition);
            var fileJournalHeader = r.ReadT<FileJournalHeader>(sizeof(FileJournalHeader));
            var endPosition = r.BaseStream.Position + fileJournalHeader.Size;
            while (r.BaseStream.Position < endPosition)
            {
                var action = r.ReadByte();
                var targetId = r.ReadInt32();
                switch (action)
                {
                    case 1: fileAttribs[targetId] = r.ReadBytes((int)r.ReadUInt32()); break;
                    case 2: fileAttribs.Remove(targetId); break;
                }
            }

            // assign file path
            for (var i = 0; i < files.Length; i++)
            {
                var file = files[i];
                file.Path = Encoding.ASCII.GetString(fileAttribs[attributeEntries[i].PathId][..^1]);
                if (file.Path.EndsWith(".hogg", StringComparison.OrdinalIgnoreCase)) file.Pak = new SubPakFile(source, file, file.Path);
            }

            // remove filesize of -1 and file 0
            source.Files = files.Where(x => x.FileSize != -1 && x.Id != 0).ToList();
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            r.Seek(file.Offset);
            return Task.FromResult((Stream)new MemoryStream(file.Compressed != 0
                ? r.DecompressZlib((int)file.PackedSize, (int)file.FileSize)
                : r.ReadBytes((int)file.FileSize)));
        }
    }
}
