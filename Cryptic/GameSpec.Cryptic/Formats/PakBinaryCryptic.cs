using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Cryptic.Formats
{
    // https://github.com/nohbdy/libhogg
    public unsafe class PakBinaryCryptic : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryCryptic();

        // Headers
        #region Headers

        const uint MAGIC = 0xDEADF00D; // DEADF00D
        const int HOGG_STRINGTABLE_OFFSET = 0x41C;
        const int HOGG_DATAENTRIES_OFFSET = 0x10018;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Header
        {
            public uint Magic;                      // DEADF00D
            public int Unknown;                     // flags?  == 0x0400000A
            public int DataEntrySectionSize;        // Size (in bytes) of the data entry section
            public int CompressionInfoSectionSize;  // Size (in bytes) of the compression info section size
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DataTableEntry
        {
            public const int SizeOf = 9;
            public byte Unknown;                    // Always 1?
            public int Id;                          // Data ID
            public int DataLength;                  // Length, in bytes, of the data in this entry
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DataEntry
        {
            public long FileOffset;                 // offset to the file's data
            public int SizeOnDisk;                  // size of the filedata within the archive
            public uint Timestamp;                  // 32-bit timestamp (seconds since 1970)
            public uint Unknown10;                  // hash?
            public int Unknown14;                   // 0
            public int Unknown18;                   // 0xFFFE
            public uint Index;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct CompressionInfo
        {
            public int FilePathId;                  // Data ID of the file's path
            public int ExcerptId;                   // Data ID of a data excerpt, or -1 if there is none
            public int UncompressedSize;            // Size of the data after decompression, or 0 if the file is not compressed
            public int UnknownC;                    // 0
        }

        #endregion

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            if (stage != ReadStage.File) throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());

            // read file
            var header = r.ReadT<Header>(sizeof(Header));
            if (header.Magic != MAGIC) throw new FormatException("BAD MAGIC");
            if (header.DataEntrySectionSize != header.CompressionInfoSectionSize * 2) throw new FormatException("data entry / compression info section size mismatch");
            var numFiles = header.CompressionInfoSectionSize >> 4; // compression_info_section_size = 16 * num_files
            var files = new FileMetadata[numFiles];

            // Create FileInfo structures from the data entries within the hogg
            r.Seek(HOGG_DATAENTRIES_OFFSET);
            var dataEntries = r.ReadTArray<DataEntry>(sizeof(DataEntry), numFiles);
            for (var i = 0; i < files.Length; i++)
            {
                ref DataEntry s = ref dataEntries[i];
                files[i] = new FileMetadata
                {
                    Id = (int)s.Index,
                    Position = s.FileOffset,
                    FileSize = s.SizeOnDisk,
                };
            }

            // Set the decompressed size of FileInfos which are compressed
            var compressionInfos = r.ReadTArray<CompressionInfo>(sizeof(CompressionInfo), numFiles);
            for (var i = 0; i < files.Length; i++)
            {
                ref CompressionInfo s = ref compressionInfos[i];
                var f = files[i];
                f.PackedSize = s.UncompressedSize;
                f.Compressed = s.UncompressedSize > 0 ? 1 : 0;
            }

            // Read DataList file into memory
            using var datalist = ReadDataAsync(source, r, files[0]).Result;
            var r2 = new BinaryReader(datalist);
            r2.Seek(4);
            var dataList = new List<string>(r2.ReadInt32());
            for (var i = 0; i < dataList.Capacity; i++) dataList.Add(r2.ReadLA32String(true));
            r2.Close();

            // Assign paths to the appropriate FileInfos
            r.Seek(HOGG_STRINGTABLE_OFFSET);
            var stringTableSize = r.ReadInt32();
            r.Skip(4);
            var endPosition = r.Position() + stringTableSize;
            while (r.Position() < endPosition)
            {
                var s = r.ReadT<DataTableEntry>(DataTableEntry.SizeOf);
                dataList.Add(s.Unknown == 1 ? r.ReadFAString(s.DataLength, true) : null);
            }

            // Assign all the file's paths and fill in the path->FileInfo map
            for (var i = 0; i < files.Length; i++)
            {
                var filePathId = compressionInfos[i].FilePathId;
                files[i].Path = dataList[filePathId];
            }

            // remove filesize of -1
            multiSource.Files = files.Where(x => x.FileSize != -1).ToList();

            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            r.Seek(file.Position);
            return Task.FromResult((Stream)new MemoryStream(file.Compressed != 0
                ? r.DecompressZlib((int)file.PackedSize, (int)file.FileSize)
                : r.ReadBytes((int)file.FileSize)));
        }
    }
}