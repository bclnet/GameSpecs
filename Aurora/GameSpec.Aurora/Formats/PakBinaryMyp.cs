using GameSpec.Aurora.Resource;
using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Aurora.Formats
{
    public unsafe class PakBinaryMyp : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryMyp();
        PakBinaryMyp() { }

        // Headers : MYP
        #region Headers : KEY/BIF

        const uint MYP_MAGIC = 0x0050594d;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct MYP_Header
        {
            public uint Magic;              // "MYP\0"
            public uint Unk1;               //
            public uint Unk2;               //
            public ulong TableOffset;       // Number of entries in FILETABLE
            public uint NumFiles;           // Number of files
            public uint NumFilesPerTable;   // Number of entries in FILETABLE
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct MYP_HeaderFile
        {
            public ulong Offset;            //
            public uint HeaderSize;         //
            public uint PackedSize;         //
            public uint FileSize;           //
            public ulong Digest;            //
            public uint crc;                //
            public ushort Compressed;       //
        }

        #endregion

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            if (!(source is BinaryPakManyFile multiSource))
                throw new NotSupportedException();
            if (stage != ReadStage.File)
                throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());

            List<FileMetadata> files;
            var hashLookup = TOR.HashLookup;
            var header = r.ReadT<MYP_Header>(sizeof(MYP_Header));
            if (header.Magic != MYP_MAGIC) throw new FormatException("BAD MAGIC");
            multiSource.Files = files = new List<FileMetadata>();
            var nextTableOffset = (long)header.TableOffset;
            while (nextTableOffset != 0)
            {
                r.Seek(nextTableOffset);
                var numHeaderFiles = r.ReadInt32();
                if (numHeaderFiles == 0) break;
                nextTableOffset = r.ReadInt64();
                var headerFiles = r.ReadTArray<MYP_HeaderFile>(sizeof(MYP_HeaderFile), numHeaderFiles);
                for (var i = 0; i < headerFiles.Length; i++)
                {
                    var headerFile = headerFiles[i];
                    if (headerFile.Offset == 0) continue;
                    var hash = headerFile.Digest;
                    var path = hashLookup.TryGetValue(hash, out var z) ? z.Replace('\\', '/') : $"{hash:X2}.bin";
                    files.Add(new FileMetadata
                    {
                        Id = (int)i,
                        Path = path.StartsWith('/') ? path[1..] : path,
                        FileSize = headerFile.FileSize,
                        PackedSize = headerFile.PackedSize,
                        Position = (long)headerFile.Offset,
                        Digest = hash,
                        Compressed = headerFile.Compressed
                    });
                }
            }

            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            if (file.FileSize == 0) return Task.FromResult(System.IO.Stream.Null);
            r.Position(file.Position);
            return Task.FromResult((Stream)new MemoryStream(file.Compressed != 0
                ? r.DecompressZlib((int)file.PackedSize, (int)file.FileSize)
                : r.ReadBytes((int)file.PackedSize)));
        }
    }
}