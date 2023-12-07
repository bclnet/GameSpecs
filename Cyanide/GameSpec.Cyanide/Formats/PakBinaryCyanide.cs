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

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, object tag)
        {
            var magic = source.Magic = r.ReadUInt32();
            if (magic != CPK_MAGIC) throw new FormatException("BAD MAGIC");
            var header = r.ReadT<CPK_Header>(sizeof(CPK_Header));
            var headerFiles = r.ReadTArray<CPK_HeaderFile>(sizeof(CPK_HeaderFile), (int)header.NumFiles);
            var files = source.Files = new FileSource[header.NumFiles];
            UnsafeX.ReadZASCII(header.Root, 512);
            for (var i = 0; i < files.Count; i++)
            {
                var headerFile = headerFiles[i];
                files[i] = new FileSource
                {
                    Path = UnsafeX.ReadZASCII(headerFile.FileName, 512).Replace('\\', '/'),
                    FileSize = headerFile.FileSize,
                    Position = (long)headerFile.Offset,
                };
            }
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileSource file, DataOption option = 0, Action<FileSource, string> exception = null)
        {
            r.Seek(file.Position);
            return Task.FromResult((Stream)new MemoryStream(r.ReadBytes((int)file.FileSize)));
        }
    }
}