using GameX.Id.Formats.Q;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Id.Formats
{
    public unsafe class PakBinary_Pak : PakBinary<PakBinary_Pak>
    {
        #region Factories

        public static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => source.Path.ToLowerInvariant() switch
            {
                //"abc" => (0, Binary_Palette.Factory),
                _ => Path.GetExtension(source.Path).ToLowerInvariant() switch
                {
                    ".lmp" => (0, Binary_Lump.Factory),
                    ".bsp" => (0, Binary_Level.Factory),
                    ".mdl" => (0, Binary_Model.Factory),
                    ".spr" => (0, Binary_Sprite.Factory),
                    _ => (0, null),
                }
            };

        #endregion

        // Headers
        #region PAK

        const uint PAK_MAGIC = 0x4b434150; // PACK

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PAK_Header
        {
            public static (string, int) Struct = ("<I2i", sizeof(PAK_Header));
            public uint Magic;
            public int DirOffset;
            public int DirLength;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct PAK_File
        {
            public static (string, int) Struct = ("<56s2i", sizeof(PAK_File));
            public fixed byte Path[56];
            public int Offset;
            public int FileSize;
        }

        #endregion

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            // read file
            var header = r.ReadS<PAK_Header>();
            if (header.Magic != PAK_MAGIC) throw new FormatException("BAD MAGIC");
            var numFiles = header.DirLength / sizeof(PAK_File);
            r.Seek(header.DirOffset);
            string path;
            source.Files = r.ReadSArray<PAK_File>(numFiles).Select(s =>
            {
                var file = new FileSource
                {
                    Path = path = UnsafeX.FixedAString(s.Path, 56).Replace('\\', '/'),
                    Offset = s.Offset,
                    FileSize = s.FileSize,
                };
                if (file.Path.EndsWith(".wad", StringComparison.OrdinalIgnoreCase)) file.Pak = new SubPakFile(source, file, file.Path, instance: PakBinary_Wad.Instance);
                return file;
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