using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Id.Formats
{
    public unsafe class PakBinary_Wad : PakBinary<PakBinary_Wad>
    {
        // Headers
        #region WAD

        const uint WAD_MAGIC = 0x32444157; //: WAD2

        [StructLayout(LayoutKind.Sequential)]
        struct WAD_Header
        {
            public static (string, int) Struct = ("<I2i", sizeof(WAD_Header));
            public uint Magic;
            public int LumpCount;
            public int LumpOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct WAD_Lump
        {
            public static (string, int) Struct = ("<3i4b16s", sizeof(WAD_Lump));
            public int Offset;
            public int PackedSize;
            public int FileSize;
            public byte Type;
            public byte Compression;
            public byte Padding0;
            public byte Padding1;
            public fixed byte Path[16];
        }

//#define TYP_LUMPY		64				// 64 + grab command number
//#define TYP_PALETTE		64
//#define TYP_QTEX		65
//#define TYP_QPIC		66
//#define TYP_SOUND		67
//#define TYP_MIPTEX		68

        #endregion

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            // read file
            var header = r.ReadS<WAD_Header>();
            if (header.Magic != WAD_MAGIC) throw new FormatException("BAD MAGIC");
            r.Seek(header.LumpOffset);
            source.Files = r.ReadSArray<WAD_Lump>(header.LumpCount).Select(s => new FileSource
            {
                Path = $"{UnsafeX.FixedAString(s.Path, 16).Replace('\\', '/')}.tex",
                Hash = s.Type,
                Offset = s.Offset,
                PackedSize = s.PackedSize,
                FileSize = s.FileSize,
                Compressed = s.Compression,
            }).ToArray();
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            r.Seek(file.Offset);
            return Task.FromResult<Stream>(new MemoryStream(r.ReadBytes((int)file.FileSize)));
        }
    }
}