using GameX.Formats;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static GameX.Util;

namespace GameX.Capcom.Formats
{
    public unsafe class PakBinary_Kpka : PakBinary<PakBinary_Kpka>
    {
        // Header
        #region Header

        const uint K_MAGIC = 0x414b504b;

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct K_Header
        {
            public byte MajorVersion;
            public byte MinorVersion;
            public short Feature;
            public int NumFiles;
            public uint Hash;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct K_FileV2
        {
            public long Offset;
            public long FileSize;
            public ulong HashName;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct K_FileV4
        {
            public ulong HashName;
            public long Offset;
            public long PackedSize;
            public long FileSize;
            public long Flag;
            public ulong Checksum;
        }

        #endregion

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var magic = r.ReadUInt32();
            if (magic != K_MAGIC) throw new FormatException("BAD MAGIC");

            // get hashlookup
            var hashLookup = source.Game.Resource != null ? RE.GetHashLookup($"{source.Game.Resource}.list") : null;

            // get header
            var header = r.ReadT<K_Header>(sizeof(K_Header));
            if (header.MajorVersion != 2 && header.MajorVersion != 4 || header.MinorVersion != 0) throw new FormatException("BAD VERSION");

            // decrypt table
            var tr = r;
            if (header.Feature == 8)
            {
                var entrySize = header.MajorVersion == 2 ? sizeof(K_FileV2) : sizeof(K_FileV4);
                var table = r.ReadBytes(header.NumFiles * entrySize);
                var key = r.ReadBytes(128);
                tr = new BinaryReader(DecryptTable(table, DecryptKey(key)));
            }

            // get files
            if (header.MajorVersion == 2)
            {
                source.Files = tr.ReadTArray<K_FileV2>(sizeof(K_FileV2), header.NumFiles)
                    .Select(x => new FileSource
                    {
                        Path = hashLookup != null && hashLookup.TryGetValue(x.HashName, out var z)
                            ? z.Replace('\\', '/')
                            : $"_unknown/{x.HashName:x16}{GetExtension(r, x.Offset, 0)}",
                        Offset = x.Offset,
                        FileSize = x.FileSize,
                    }).ToArray();
            }
            else if (header.MajorVersion == 4)
            {
                int compressed;
                source.Files = tr.ReadTArray<K_FileV4>(sizeof(K_FileV4), header.NumFiles)
                    .Select(x => new FileSource
                    {
                        Compressed = compressed = GetCompressed(x.Flag),
                        Path = hashLookup != null && hashLookup.TryGetValue(x.HashName, out var z)
                            ? z.Replace('\\', '/')
                            : $"_unknown/{x.HashName:x16}{GetExtension(r, x.Offset, compressed)}",
                        Offset = x.Offset,
                        PackedSize = x.PackedSize,
                        FileSize = x.FileSize,
                    }).ToArray();
            }
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            r.Seek(file.Offset);
            return Task.FromResult<Stream>(new MemoryStream(Decompress(r, file.Compressed, (int)file.PackedSize, (int)file.FileSize)));
        }

        static byte[] Decompress(BinaryReader r, int compressed, int length, int newLength = 0)
            => compressed == 0 ? r.ReadBytes(length)
            : compressed == 'Z' ? r.DecompressZlib(length, newLength, noHeader: true)
            : compressed == 'S' ? r.DecompressZstd(length, newLength)
            : throw new ArgumentOutOfRangeException(nameof(compressed));

        static int GetCompressed(long f)
            => (f & 0xF) == 1 ? f >> 16 > 0 ? 0 : 'Z'
            : (f & 0xF) == 2 ? f >> 16 > 0 ? 0 : 'S'
            : 0;

        static string GetExtension(BinaryReader r, long offset, int compressed)
        {
            r.Seek(offset);
            return _guessExtension(Decompress(r, compressed, 150));
        }

        static readonly BigInteger Modulus = new BigInteger(new byte[] {
            0x7D, 0x0B, 0xF8, 0xC1, 0x7C, 0x23, 0xFD, 0x3B, 0xD4, 0x75, 0x16, 0xD2, 0x33, 0x21, 0xD8, 0x10,
            0x71, 0xF9, 0x7C, 0xD1, 0x34, 0x93, 0xBA, 0x77, 0x26, 0xFC, 0xAB, 0x2C, 0xEE, 0xDA, 0xD9, 0x1C,
            0x89, 0xE7, 0x29, 0x7B, 0xDD, 0x8A, 0xAE, 0x50, 0x39, 0xB6, 0x01, 0x6D, 0x21, 0x89, 0x5D, 0xA5,
            0xA1, 0x3E, 0xA2, 0xC0, 0x8C, 0x93, 0x13, 0x36, 0x65, 0xEB, 0xE8, 0xDF, 0x06, 0x17, 0x67, 0x96,
            0x06, 0x2B, 0xAC, 0x23, 0xED, 0x8C, 0xB7, 0x8B, 0x90, 0xAD, 0xEA, 0x71, 0xC4, 0x40, 0x44, 0x9D,
            0x1C, 0x7B, 0xBA, 0xC4, 0xB6, 0x2D, 0xD6, 0xD2, 0x4B, 0x62, 0xD6, 0x26, 0xFC, 0x74, 0x20, 0x07,
            0xEC, 0xE3, 0x59, 0x9A, 0xE6, 0xAF, 0xB9, 0xA8, 0x35, 0x8B, 0xE0, 0xE8, 0xD3, 0xCD, 0x45, 0x65,
            0xB0, 0x91, 0xC4, 0x95, 0x1B, 0xF3, 0x23, 0x1E, 0xC6, 0x71, 0xCF, 0x3E, 0x35, 0x2D, 0x6B, 0xE3,
            0x00
        });

        static readonly BigInteger Exponent = new BigInteger(new byte[] {
            0x01, 0x00, 0x01, 0x00
        });

        static byte[] DecryptKey(byte[] key)
        {
            Array.Resize(ref key, 129);
            return BigInteger.ModPow(new BigInteger(key), Exponent, Modulus).ToByteArray();
        }

        static Stream DecryptTable(byte[] buf, byte[] key)
        {
            if (key.Length == 0) return new MemoryStream(buf);
            for (var i = 0; i < buf.Length; i++)
                buf[i] ^= (byte)(i + key[i % 32] * key[i % 29]);
            return new MemoryStream(buf);
        }
    }
}