using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Bioware.Formats
{
    public unsafe class PakBinary_Myp : PakBinary<PakBinary_Myp>
    {
        // Headers : MYP
        #region Headers : KEY/BIF

        const uint MYP_MAGIC = 0x0050594d;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct MYP_Header
        {
            public uint Magic;              // "MYP\0"
            public uint Version;            // Version
            public uint Bom;                // Byte order marker
            public ulong TableOffset;       // Number of entries in FILETABLE
            public uint TableCapacity;      // Number of files
            public uint TotalFiles;         // Number of entries in FILETABLE
            public uint Unk1;               //
            public uint Unk2;               //

            public void Verify()
            {
                if (Magic != MYP_MAGIC) throw new FormatException("Not a .tor file (Wrong file header)");
                if (Version != 5 && Version != 6) throw new FormatException($"Only versions 5 and 6 are supported, file has {Version}");
                if (Bom != 0xfd23ec43) throw new FormatException("Unexpected byte order");
                if (TableOffset == 0) throw new FormatException("File is empty");
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct MYP_HeaderFile
        {
            public ulong Offset;            //
            public uint HeaderSize;         //
            public uint PackedSize;         //
            public uint FileSize;           //
            public ulong Digest;            //
            public uint Crc;                //
            public ushort Compressed;       //
        }

        #endregion

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var files = source.Files = new List<FileSource>();
            var hashLookup = TOR.HashLookup;

            var header = r.ReadT<MYP_Header>(sizeof(MYP_Header));
            header.Verify();
            source.Version = header.Version;

            var tableOffset = (long)header.TableOffset;
            while (tableOffset != 0)
            {
                r.Seek(tableOffset);

                var numFiles = r.ReadInt32();
                if (numFiles == 0) break;
                tableOffset = r.ReadInt64();

                var headerFiles = r.ReadTArray<MYP_HeaderFile>(sizeof(MYP_HeaderFile), numFiles);
                for (var i = 0; i < headerFiles.Length; i++)
                {
                    var headerFile = headerFiles[i];
                    if (headerFile.Offset == 0) continue;
                    var hash = headerFile.Digest;
                    var path = hashLookup.TryGetValue(hash, out var z) ? z.Replace('\\', '/') : $"{hash:X2}.bin";
                    files.Add(new FileSource
                    {
                        Id = i,
                        Path = path.StartsWith('/') ? path[1..] : path,
                        FileSize = headerFile.FileSize,
                        PackedSize = headerFile.PackedSize,
                        Offset = (long)(headerFile.Offset + headerFile.HeaderSize),
                        Hash = hash,
                        Compressed = headerFile.Compressed
                    });
                }
            }
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            if (file.FileSize == 0) return Task.FromResult(System.IO.Stream.Null);
            r.Seek(file.Offset);
            return Task.FromResult((Stream)new MemoryStream(file.Compressed switch
            {
                0 => r.ReadBytes((int)file.PackedSize),
                _ => source.Version switch
                {
                    6 => r.DecompressZstd((int)file.PackedSize, (int)file.FileSize),
                    _ => r.DecompressZlib((int)file.PackedSize, (int)file.FileSize),
                }
            }));
        }
    }
}