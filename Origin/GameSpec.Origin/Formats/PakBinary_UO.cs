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
            public uint Lookup;
            public uint Length;
            public uint Extra;
        }

        #endregion

        static Binary_Verdata VerData;
        bool Idx;

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var verData = VerData ??= source.Contains("verdata.mul") ? source.LoadFileObject<Binary_Verdata>("verdata.mul").Result : Binary_Verdata.Empty;
            Idx = !source.FilePath.EndsWith(".uop");
            if (Idx) ReadIdx(verData, source, r, tag);
            else ReadUop(source, r, tag);
            return Task.CompletedTask;
        }

        public Task ReadIdx(Binary_Verdata verData, BinaryPakFile source, BinaryReader r, object tag)
        {
            var pair = source.FilePath.ToLowerInvariant() switch
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
            source.OpenPakFile(pair);
            var count = (int)(r.BaseStream.Length / 12);
            var id = 0;
            List<FileSource> files;
            source.Files = files = r.ReadSArray<IdxFile>(IdxFile.Struct, count).Select(s => new FileSource
            {
                Id = id,
                Path = $"file{id++}.dat",
                Offset = s.Lookup,
                FileSize = s.Length,
                Tag = s.Extra,
            }).ToList();

            // apply patch
            if (verData.Patches.TryGetValue(pair.Item3, out var patches))
                foreach (var patch in patches.Where(s => s.Index > 0 && s.Index < files.Count))
                {
                    var entry = files[patch.Index];
                    entry.Offset = patch.Lookup;
                    entry.FileSize = patch.Length | (1 << 31);
                    entry.Tag = patch.Extra;
                }
            return Task.CompletedTask;
        }

        public Task ReadUop(BinaryPakFile source, BinaryReader r, object tag)
        {
            var pair = source.FilePath switch
            {
                "artLegacyMUL.uop" => (".tga", 0x10000, 0),
                "gumpartLegacyMUL.uop" => (".tga", 0xFFFF, 1),
                "soundLegacyMUL.uop" => (".dat", 0xFFF, 0),
                _ => (null, 0, -1),
            };
            return Task.CompletedTask;
        }


        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            r.Seek(file.Offset);
            return Task.FromResult((Stream)new MemoryStream(r.ReadBytes((int)file.FileSize)));
        }
    }
}