using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.WbB.Formats
{
    public unsafe class PakBinary_AC : PakBinary<PakBinary_AC>
    {
        #region Headers

        const uint DAT_HEADER_OFFSET = 0x140;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Header
        {
            public uint FileType;
            public uint BlockSize;
            public uint FileSize;
            [MarshalAs(UnmanagedType.U4)] public PakType DataSet;
            public uint DataSubset;

            public uint FreeHead;
            public uint FreeTail;
            public uint FreeCount;
            public uint BTree;

            public uint NewLRU;
            public uint OldLRU;
            public uint UseLRU; // UseLRU != 0

            public uint MasterMapId;

            public uint EnginePackVersion;
            public uint GamePackVersion;

            public fixed byte VersionMajor[16];
            public uint VersionMinor;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DirectoryHeader
        {
            public const int SizeOf = (sizeof(uint) * 0x3E) + sizeof(uint) + (File.SizeOf * 0x3D);
            public fixed uint Branches[0x3E];
            public uint EntryCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3D)] public File[] Entries;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct File
        {
            public const int SizeOf = sizeof(uint) * 6;
            public uint BitFlags; // not-used
            public uint ObjectId;
            public uint FileOffset;
            public uint FileSize;
            public uint Date; // not-used
            public uint Iteration; // not-used
        }

        class Directory
        {
            public readonly DirectoryHeader Header;
            public readonly List<Directory> Directories = new List<Directory>();

            public Directory(BinaryReader r, long offset, int blockSize)
            {
                Header = ReadT<DirectoryHeader>(r, offset, DirectoryHeader.SizeOf, blockSize);
                if (Header.Branches[0] != 0) for (var i = 0; i < Header.EntryCount + 1; i++) Directories.Add(new Directory(r, Header.Branches[i], blockSize));
            }

            public void AddFiles(BinaryReader r, PakType pakType, IList<FileSource> files, string path, int blockSize)
            {
                //var did = 0; Directories.ForEach(d => d.AddFiles(pakType, files, Path.Combine(path, did++.ToString()), blockSize));
                Directories.ForEach(d => d.AddFiles(r, pakType, files, path, blockSize));
                for (var i = 0; i < Header.EntryCount; i++)
                {
                    var entry = Header.Entries[i];
                    var file = new FileSource
                    {
                        Id = (int)entry.ObjectId,
                        Offset = entry.FileOffset,
                        FileSize = entry.FileSize,
                        Hash = (ulong)blockSize,
                        Tag = entry,
                    };
                    file.Path = Path.Combine(path, WbBPakFile.GetPath(file, r, pakType, out var type));
                    file.ExtraArgs = (pakType, type);
                    files.Add(file);
                }
            }
        }

        #endregion

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var files = source.Files = new List<FileSource>();
            r.Seek(DAT_HEADER_OFFSET);
            var header = r.ReadT<Header>(sizeof(Header));
            var directory = new Directory(r, header.BTree, (int)header.BlockSize);
            directory.AddFiles(r, header.DataSet, files, string.Empty, (int)header.BlockSize);
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
            => Task.FromResult((Stream)new MemoryStream(ReadBytes(r, file.Offset, (int)file.FileSize, (int)file.Hash)));

        static T ReadT<T>(BinaryReader r, long offset, int size, int blockSize) where T : struct
            => UnsafeX.MarshalT<T>(ReadBytes(r, offset, size, blockSize));

        static byte[] ReadBytes(BinaryReader r, long offset, int size, int blockSize)
        {
            int read;
            var buffer = new byte[size];
            r.Seek(offset);
            // Dat "file" is broken up into sectors that are not neccessarily congruous. Next address is stored in first four bytes of each sector.
            var nextAddress = (long)r.ReadUInt32();
            var bufferOffset = 0;

            while (size > 0)
                if (size >= blockSize)
                {
                    read = r.Read(buffer, bufferOffset, blockSize - 4); // Read in our sector into the buffer[]
                    bufferOffset += read; // Adjust this so we know where in our buffer[] the next sector gets appended to
                    size -= read; // Decrease this by the amount of data we just read into buffer[] so we know how much more to go
                    r.Seek(nextAddress); // Move the file pointer to the start of the next sector we read above.
                    nextAddress = r.ReadUInt32(); // Get the start location of the next sector.
                }
                else { r.Read(buffer, bufferOffset, size); return buffer; }
            return buffer;
        }
    }
}