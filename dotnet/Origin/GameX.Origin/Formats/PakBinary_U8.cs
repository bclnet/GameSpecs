using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Origin.Formats
{
    public unsafe class PakBinary_U8 : PakBinary<PakBinary_U8>
    {
        public static (FileOption, Func<BinaryReader, FileSource, PakFile, Task<object>>) ObjectFactoryFactory(FileSource source, FamilyGame game)
            => source.Path.ToLowerInvariant() switch
            {
                _ => Path.GetExtension(source.Path).ToLowerInvariant() switch
                {
                    _ => (0, null),
                }
            };

        // Headers
        #region Headers
        // https://wiki.ultimacodex.com/wiki/Ultima_VIII_internal_formats

        const uint MAGIC = 0x00001a1aU;

        //[StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        //struct FLX_Header
        //{
        //    public static (string, int) Struct = ("<81cb5I", sizeof(FLX_Header));
        //    public fixed byte Title[81];
        //    public byte ControlZ;
        //    public uint Reserved1;
        //    public uint Count;
        //    public uint Version;
        //    public uint TotalSize;
        //    public uint Checksum;
        //    public fixed byte Reserved2[26];
        //}

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct FLX_Header
        {
            public static (string, int) Struct = ("<803I", sizeof(FLX_Header));
            public fixed byte Title[80];
            public uint Magic;
            public uint Count;
            public uint Version;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct FLX_Record
        {
            public static (string, int) Struct = ("<2I", sizeof(FLX_Record));
            public uint Offset;
            public uint Size;
        }

        #endregion

        static (string name, string ext)[] NameToExts = {
            ("damage", ".dmg"),
            ("dtable", ".dtb"),
            ("usecode", ".scr"),
            ("font", ".fnt"),
            ("glob", ".glb"),
            ("gump", ".gmp"),
            ("shape", ".shp"),
            ("sound", ".snd"),
            ("music", ".mus")
        };

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            var fileName = Path.GetFileName(source.PakPath).ToLowerInvariant();
            var nameToExt = NameToExts.FirstOrDefault(x => fileName.Contains(x.name));
            var ext = nameToExt.ext ?? ".dat";

            // read header
            var header = r.ReadS<FLX_Header>();
            //var title = UnsafeX.ReadZASCII(header.Title, 80);
            r.Skip(9 * sizeof(int));
            if (header.Magic != MAGIC) throw new FormatException("BAD MAGIC");

            // read files
            var id = 0;
            source.Files = r.ReadSArray<FLX_Record>((int)header.Count)
                .Where(s => s.Offset > 0 && s.Size > 0)
                .Select(s => new FileSource
                {
                    Path = $"file_{id++:x}{ext}",
                    Id = id,
                    Offset = s.Offset,
                    FileSize = s.Size,
                }).ToList();
            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            r.Seek(file.Offset);
            return Task.FromResult((Stream)new MemoryStream(r.ReadBytes((int)file.FileSize)));
        }
    }
}