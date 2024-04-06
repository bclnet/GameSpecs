using GameX.Formats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Valve.Formats
{
    // https://github.com/Rupan/HLLib/blob/master/HLLib/WADFile.h
    public unsafe class PakBinary_Wad : PakBinary<PakBinary_Wad>
    {
        // Headers
        #region WAD

        const uint WAD_MAGIC = 0x33444157; //: WAD3

        [StructLayout(LayoutKind.Sequential), DebuggerDisplay("Header:{LumpCount}")]
        struct WAD_Header
        {
            public static (string, int) Struct = ("<3I", sizeof(WAD_Header));
            public uint Signature;
            public uint LumpCount;
            public uint LumpOffset;
        }

        [StructLayout(LayoutKind.Sequential), DebuggerDisplay("Lump:{Name}")]
        struct WAD_Lump
        {
            public static (string, int) Struct = ("<3I2cH16s", 32);
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
            public static (string, int) Struct = ("<3I", 32);
            public uint Width;
            public uint Height;
            public uint PaletteSize;
        }

        #endregion

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var files = source.Files = new List<FileSource>();

            // read file
            var header = r.ReadS<WAD_Header>();
            if (header.Signature != WAD_MAGIC) throw new FormatException("BAD MAGIC");
            r.Seek(header.LumpOffset);
            var lumps = r.ReadTEach<WAD_Lump>(WAD_Lump.Struct.Item2, (int)header.LumpCount);
            foreach (var lump in lumps)
                files.Add(new FileSource
                {
                    Path = lump.Type switch
                    {
                        0x40 => $"{lump.Name}.tex2",
                        0x42 => $"{lump.Name}.pic",
                        0x43 => $"{lump.Name}.tex",
                        0x46 => $"{lump.Name}.fnt",
                        _ => $"{lump.Name}.{lump.Type:x}"
                    },
                    Offset = lump.Offset,
                    Compressed = lump.Compression,
                    FileSize = lump.DiskSize,
                    PackedSize = lump.Size,
                });
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            Stream fileData;
            r.Seek(file.Offset);
            fileData = new MemoryStream(file.Compressed == 0
                ? r.ReadBytes((int)file.FileSize)
                : throw new NotSupportedException());
            return Task.FromResult(fileData);
        }
    }
}