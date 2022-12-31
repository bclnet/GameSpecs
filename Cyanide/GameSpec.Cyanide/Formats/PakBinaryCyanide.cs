using GameSpec.Formats;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Cyanide.Formats
{
    public unsafe class PakBinaryCyanide : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryCyanide();
        PakBinaryCyanide() { }

        // Headers
        #region Headers

        const uint CPK_MAGIC = 0x01439855;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct CPK_Header
        {
            public uint NumFiles;               // Number of files
            public fixed byte Root[512];        // Root name
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct CPK_HeaderFile
        {
            public uint FileSize;               // File size
            public ulong Offset;                // File position
            public fixed byte FileName[512];    // File name
        }

        #endregion

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, ReadStage stage)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();
            if (stage != ReadStage.File) throw new ArgumentOutOfRangeException(nameof(stage), stage.ToString());

            var magic = source.Magic = r.ReadUInt32();
            if (magic != CPK_MAGIC) throw new FormatException($"Unknown File Type {magic}");
            var header = r.ReadT<CPK_Header>(sizeof(CPK_Header));
            var headerFiles = r.ReadTArray<CPK_HeaderFile>(sizeof(CPK_HeaderFile), (int)header.NumFiles);
            var files = multiSource.Files = new FileMetadata[header.NumFiles];
            UnsafeX.ReadZASCII(header.Root, 512);
            for (var i = 0; i < files.Count; i++)
            {
                var headerFile = headerFiles[i];
                files[i] = new FileMetadata
                {
                    Path = UnsafeX.ReadZASCII(headerFile.FileName, 512).Replace('\\', '/'),
                    FileSize = headerFile.FileSize,
                    Position = (long)headerFile.Offset,
                };
            }
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            r.Position(file.Position);
            return Task.FromResult((Stream)new MemoryStream(r.ReadBytes((int)file.FileSize)));
        }
    }
}