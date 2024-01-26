using GameSpec.Formats;
using GameSpec.Origin.Formats.UO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameSpec.Origin.Formats
{
    public unsafe class PakBinary_UO : PakBinary<PakBinary_UO>
    {
        // Headers
        #region Headers

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct IdxFile
        {
            public static (string, int) Struct = ("<3I", sizeof(IdxFile));
            public int Offset;
            public int FileSize;
            public int Tag;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct UopHeader
        {
            public static (string, int) Struct = ("<3I", sizeof(UopHeader));
            public int Magic;
            public long VersionAndSignature;
            public long NextBlock;
            public int BlockCapacity;
            public int Count;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct UopRecord
        {
            public static (string, int) Struct = ("<3I", sizeof(UopRecord));
            public long Offset;
            public int HeaderLength;
            public int CompressedLength;
            public int DecompressedLength;
            public ulong Hash;
            public uint Reserved;
            public short Flag;
            public readonly int FileSize => Flag == 1 ? CompressedLength : DecompressedLength;
        }

        #endregion

        static Binary_Verdata VerData;
        bool Idx;
        PakFile Data;

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var verData = VerData ??= source.Contains("verdata.mul") ? source.LoadFileObject<Binary_Verdata>("verdata.mul").Result : Binary_Verdata.Empty;
            Idx = !source.PakPath.EndsWith(".uop");
            if (Idx) ReadIdx(verData, source, r, tag);
            else ReadUop(source, r, tag);
            return Task.CompletedTask;
        }

        #region Idx/Mul

        Task ReadIdx(Binary_Verdata verData, BinaryPakFile source, BinaryReader r, object tag)
        {
            var pair = source.PakPath.ToLowerInvariant() switch
            {
                "anim.idx" => ("anim.mul", 0x40000, 6),
                "anim2.idx" => ("anim2.mul", 0x10000, -1),
                "anim3.idx" => ("anim3.mul", 0x20000, -1),
                "anim4.idx" => ("anim4.mul", 0x20000, -1),
                "anim5.idx" => ("anim5.mul", 0x20000, -1),
                "artidx.mul" => ("art.mul", 0x10000, -1),
                "gumpidx.mul" => ("Gumpart.mul", 0x10000, 12),
                "multi.mul" => ("Multi.mul", 0x4000, 14),
                "skills.mul" => ("Skills.mul", 55, -1),
                "soundidx.mul" => ("sound.mul", 0x1000, -1),
                "texidx.mul" => ("texmaps.mul", 0x4000, -1),
                _ => (null, 0, -1),
            };
            source.PakPath = pair.Item1;
            var count = (int)(r.BaseStream.Length / 12);
            var id = 0;
            List<FileSource> files;
            source.Files = files = r.ReadSArray<IdxFile>(IdxFile.Struct, count).Select(s => new FileSource
            {
                Id = id,
                Path = $"file{id++:x5}.dat",
                Offset = s.Offset,
                FileSize = s.FileSize,
                Tag = s.Tag,
            }).ToList();

            // apply patch
            if (verData.Patches.TryGetValue(pair.Item3, out var patches))
                foreach (var patch in patches.Where(s => s.Index > 0 && s.Index < files.Count))
                {
                    var entry = files[patch.Index];
                    entry.Offset = patch.Offset;
                    entry.FileSize = patch.FileSize; // | (1 << 31);
                    entry.Tag = patch.Tag;
                }
            return Task.CompletedTask;
        }

        #endregion

        #region Uop

        const int UOP_MAGIC = 0x50594D;

        Task ReadUop(BinaryPakFile source, BinaryReader r, object tag)
        {
            var header = r.ReadS<UopHeader>(UopHeader.Struct);
            if (header.Magic != UOP_MAGIC) throw new FormatException("BAD MAGIC");
            var uopPattern = Path.GetFileNameWithoutExtension(source.PakPath).ToLowerInvariant();
            var pair = source.PakPath switch
            {
                "artLegacyMUL.uop" => (".tga", 0x10000, false),
                "gumpartLegacyMUL.uop" => (".tga", 0xFFFF, true),
                "soundLegacyMUL.uop" => (".dat", 0xFFF, false),
                _ => (null, 0, false),
            };
            var extension = pair.Item1;
            var length = pair.Item2;
            var hasExtra = pair.Item3;

            // add files
            FileSource[] files;
            source.Files = files = new FileSource[length];
            for (var i = 0; i < files.Length; i++)
                files[i] = new FileSource
                {
                    Id = i,
                    Path = $"file{i:x5}.dat",
                    Offset = -1,
                };

            // find hashes
            var hashes = new Dictionary<ulong, int>();
            for (var i = 0; i < length; i++)
            {
                var entryName = $"build/{uopPattern}/{i:D8}{extension}";
                var hash = CreateHash(entryName);
                if (!hashes.ContainsKey(hash)) hashes.Add(hash, i);
            }

            // walk blocks
            var nextBlock = header.NextBlock;
            r.Seek(nextBlock);
            do
            {
                var filesCount = r.ReadInt32();
                nextBlock = r.ReadInt64();
                for (var i = 0; i < filesCount; i++)
                {
                    var record = r.ReadS<UopRecord>(UopRecord.Struct);
                    if (record.Offset == 0) continue;
                    if (hashes.TryGetValue(record.Hash, out var idx))
                    {
                        if (idx < 0 || idx > files.Length) throw new IndexOutOfRangeException("hashes dictionary and files collection have different count of entries!");
                        var file = files[idx];
                        file.Offset = (int)(record.Offset + record.HeaderLength);
                        file.FileSize = record.FileSize;
                        if (!hasExtra) continue;
                        r.Peek(x =>
                        {
                            r.Seek(file.Offset);
                            var extra = r.ReadBytes(8);
                            var extra1 = (ushort)((extra[3] << 24) | (extra[2] << 16) | (extra[1] << 8) | extra[0]);
                            var extra2 = (ushort)((extra[7] << 24) | (extra[6] << 16) | (extra[5] << 8) | extra[4]);
                            file.Offset += 8;
                            file.Tag = extra1 << 16 | extra2;
                        });
                    }
                }
            } while (r.BaseStream.Seek(nextBlock, SeekOrigin.Begin) != 0);
            return Task.CompletedTask;
        }

        #endregion

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            r.Seek(file.Offset);
            return Task.FromResult((Stream)new MemoryStream(r.ReadBytes((int)file.FileSize)));
        }

        static ulong CreateHash(string s)
        {
            uint eax = 0, ecx, edx, ebx, esi, edi;
            //eax = ecx = edx = ebx = esi = edi = 0;
            ebx = edi = esi = (uint)s.Length + 0xDEADBEEF;
            int i;
            for (i = 0; i + 12 < s.Length; i += 12)
            {
                edi = (uint)((s[i + 7] << 24) | (s[i + 6] << 16) | (s[i + 5] << 8) | s[i + 4]) + edi;
                esi = (uint)((s[i + 11] << 24) | (s[i + 10] << 16) | (s[i + 9] << 8) | s[i + 8]) + esi;
                edx = (uint)((s[i + 3] << 24) | (s[i + 2] << 16) | (s[i + 1] << 8) | s[i]) - esi;
                edx = (edx + ebx) ^ (esi >> 28) ^ (esi << 4); esi += edi;
                edi = (edi - edx) ^ (edx >> 26) ^ (edx << 6); edx += esi;
                esi = (esi - edi) ^ (edi >> 24) ^ (edi << 8); edi += edx;
                ebx = (edx - esi) ^ (esi >> 16) ^ (esi << 16); esi += edi;
                edi = (edi - ebx) ^ (ebx >> 13) ^ (ebx << 19); ebx += esi;
                esi = (esi - edi) ^ (edi >> 28) ^ (edi << 4); edi += ebx;
            }
            if (s.Length - i > 0)
            {
                switch (s.Length - i)
                {
                    case 12: esi += (uint)s[i + 11] << 24; goto case 11;
                    case 11: esi += (uint)s[i + 10] << 16; goto case 10;
                    case 10: esi += (uint)s[i + 9] << 8; goto case 9;
                    case 9: esi += s[i + 8]; goto case 8;
                    case 8: edi += (uint)s[i + 7] << 24; goto case 7;
                    case 7: edi += (uint)s[i + 6] << 16; goto case 6;
                    case 6: edi += (uint)s[i + 5] << 8; goto case 5;
                    case 5: edi += s[i + 4]; goto case 4;
                    case 4: ebx += (uint)s[i + 3] << 24; goto case 3;
                    case 3: ebx += (uint)s[i + 2] << 16; goto case 2;
                    case 2: ebx += (uint)s[i + 1] << 8; goto case 1;
                    case 1: ebx += s[i]; break;
                }
                esi = (esi ^ edi) - ((edi >> 18) ^ (edi << 14));
                ecx = (esi ^ ebx) - ((esi >> 21) ^ (esi << 11));
                edi = (edi ^ ecx) - ((ecx >> 7) ^ (ecx << 25));
                esi = (esi ^ edi) - ((edi >> 16) ^ (edi << 16));
                edx = (esi ^ ecx) - ((esi >> 28) ^ (esi << 4));
                edi = (edi ^ edx) - ((edx >> 18) ^ (edx << 14));
                eax = (esi ^ edi) - ((edi >> 8) ^ (edi << 24));
                return ((ulong)edi << 32) | eax;
            }
            return ((ulong)esi << 32) | eax;
        }
    }
}