using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Id.Formats
{
    public unsafe class PakBinaryId : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryId();
        PakBinaryId() { }

        // Headers
        // https://github.com/Rupan/HLLib/blob/master/HLLib/WADFile.h
        #region WAD

        const uint WAD_MAGIC = 0x33444157; //: WAD3

        [StructLayout(LayoutKind.Sequential)]
        struct WAD_Header
        {
            public uint Signature;
            public uint LumpCount;
            public uint LumpOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct WAD_Lump
        {
            public uint Offset;
            public uint DiskLength;
            public uint Length;
            public byte Type;
            public byte Compression;
            public byte Padding0;
            public byte Padding1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string Name;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct WAD_LumpInfo
        {
            public uint Width;
            public uint Height;
            public uint PaletteSize;
        }

        #endregion

        public override Task ReadAsync(BinaryPakFile source, BinaryReader r, object tag)
        {
            if (!(source is BinaryPakManyFile multiSource)) throw new NotSupportedException();

            // read file
            switch (Path.GetExtension(source.FilePath).ToLowerInvariant())
            {
                case ".wad":
                    {
                        var header = r.ReadT<WAD_Header>(sizeof(WAD_Header));
                        if (header.Signature != WAD_MAGIC) throw new FormatException("BAD MAGIC");
                        break;
                    }
            }

            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileSource file, DataOption option = 0, Action<FileSource, string> exception = null)
        {
            Stream fileData;
            r.Seek(file.Position);
            fileData = new MemoryStream(r.ReadBytes((int)file.FileSize));
            return Task.FromResult(fileData);
        }
    }
}