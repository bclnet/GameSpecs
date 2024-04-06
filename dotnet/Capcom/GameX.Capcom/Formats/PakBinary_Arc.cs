using GameX.Formats;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static GameX.Util;

namespace GameX.Capcom.Formats
{
    public unsafe class PakBinary_Arc : PakBinary<PakBinary_Arc>
    {
        // Header
        #region Header

        const uint K_MAGIC = 0x00435241;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct K_Header
        {
            public ushort Version;
            public ushort NumFiles;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct K_File
        {
            public fixed byte Path[0x40];
            public uint Compressed;
            public uint PackedSize;
            public uint FileSize;
            public uint Offset;
        }

        #endregion

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var magic = r.ReadUInt32();
            magic &= 0x00FFFFFF;
            if (magic != K_MAGIC) throw new FormatException("BAD MAGIC");

            // get header
            var header = r.ReadT<K_Header>(sizeof(K_Header));

            // get files
            source.Files = r.ReadTArray<K_File>(sizeof(K_File), header.NumFiles)
                .Select(x => new FileSource
                {
                    Path = $"{Encoding.UTF8.GetString(new Span<byte>(x.Path, 0x40)).TrimEnd('\0')}{GetExtension(r, x.Offset)}".Replace('\\', '/'),
                    Compressed = (int)x.Compressed,
                    PackedSize = x.PackedSize,
                    FileSize = x.FileSize,
                    Offset = x.Offset,
                }).ToArray();
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            r.Seek(file.Offset);
            return Task.FromResult<Stream>(new MemoryStream(Decompress(r, (int)file.PackedSize, (int)file.FileSize)));
        }

        static byte[] Decompress(BinaryReader r, int length, int newLength = 0) => r.DecompressZlib(length, newLength);

        static string GetExtension(BinaryReader r, long position)
        {
            r.Seek(position);
            return _guessExtension(Decompress(r, 150));
        }
    }
}