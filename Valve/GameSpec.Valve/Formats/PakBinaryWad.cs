using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Valve.Formats
{
    // https://github.com/Rupan/HLLib/blob/master/HLLib/WADFile.h
    public unsafe class PakBinaryWad : PakBinary
    {
        public static readonly PakBinary Instance = new PakBinaryWad();
        PakBinaryWad() { }

        // Headers
        #region WAD

        const uint WAD_MAGIC = 0x33444157; //: WAD3

        [StructLayout(LayoutKind.Sequential), DebuggerDisplay("Header:{LumpCount}")]
        struct WAD_Header
        {
            public uint Signature;
            public uint LumpCount;
            public uint LumpOffset;
        }

        [StructLayout(LayoutKind.Sequential), DebuggerDisplay("Lump:{Name}")]
        struct WAD_Lump
        {
            public const int SizeOf = 32;
            public uint Offset;
            public uint DiskSize;
            public uint Size;
            public byte Type;
            public byte Compression;
            public ushort Padding;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)] public string Name;
        }

        [StructLayout(LayoutKind.Sequential), DebuggerDisplay("LumpInfo:{Width}x{Height}")]
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
            var files = multiSource.Files = new List<FileMetadata>();

            // read file
            var header = r.ReadT<WAD_Header>(sizeof(WAD_Header));
            if (header.Signature != WAD_MAGIC) throw new FormatException("BAD MAGIC");
            r.Seek(header.LumpOffset);
            var lumps = r.ReadTArrayEach<WAD_Lump>(WAD_Lump.SizeOf, (int)header.LumpCount);
            foreach (var lump in lumps)
                files.Add(new FileMetadata
                {
                    Path = lump.Type switch
                    {
                        0x40 => $"{lump.Name}.tex2",
                        0x42 => $"{lump.Name}.pic",
                        0x43 => $"{lump.Name}.tex",
                        0x46 => $"{lump.Name}.fnt",
                        _ => $"{lump.Name}.{lump.Type:x}"
                    },
                    Position = lump.Offset,
                    Compressed = lump.Compression,
                    FileSize = lump.DiskSize,
                    PackedSize = lump.Size,
                });
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadDataAsync(BinaryPakFile source, BinaryReader r, FileMetadata file, DataOption option = 0, Action<FileMetadata, string> exception = null)
        {
            Stream fileData;
            r.Seek(file.Position);
            fileData = new MemoryStream(file.Compressed == 0
                ? r.ReadBytes((int)file.FileSize)
                : throw new NotSupportedException());
            return Task.FromResult(fileData);
        }
    }
}