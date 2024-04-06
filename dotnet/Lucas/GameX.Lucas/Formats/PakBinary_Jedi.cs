using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GameX.Lucas.Formats
{
    public unsafe class PakBinary_Jedi : PakBinary<PakBinary_Jedi>
    {
        #region GOB

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct GOB_Header
        {
            public static (string, int) Struct = ("<2I", sizeof(GOB_Header));
            public uint Magic;              // Always 'GOB '
            public uint EntryOffset;        // Offset to GOB_Entry
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct GOB_Entry
        {
            public static (string, int) Struct = ("<2I13s", sizeof(GOB_Entry));
            public uint Offset;             // Offset in the archive file
            public uint FileSize;           // Size in bytes of this entry
            public fixed byte Path[13];     // File name
        }

        #endregion

        #region LFD

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct LFD_Entry
        {
            public static (string, int) Struct = ("<I8sI", sizeof(LFD_Entry));
            public uint Type;
            public fixed byte Name[8];
            public uint Size;
        }

        #endregion

        #region LAB

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct LAB_Header
        {
            public static (string, int) Struct = ("<4I", sizeof(LAB_Header));
            public uint Magic;              // Always 'LABN'
            public uint Version;            // Apparently always 0x10000 for Outlaws
            public uint FileCount;          // File entry count
            public uint NameTableLength;    // Length including null bytes of the filename list/string
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct LAB_Entry
        {
            public static (string, int) Struct = ("<4I", sizeof(LAB_Entry));
            public uint NameOffset;         // Offset in the name string
            public uint Offset;             // Offset in the archive file
            public uint FileSize;           // Size in bytes of this entry
            public uint FourCC;             // All zeros or a 4CC related to the filename extension
        }

        #endregion

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            const uint GOB_MAGIC = 0x0a424f47;
            const uint LFD_MAGIC = 0x50414d52;
            const uint LAB_MAGIC = 0x4e42414c;

            switch (Path.GetExtension(source.Name).ToLowerInvariant())
            {
                case ".gob":
                    {
                        var header = r.ReadS<GOB_Header>();
                        if (header.Magic != GOB_MAGIC) throw new FormatException("BAD MAGIC");

                        r.Seek(header.EntryOffset);
                        var entries = r.ReadL32SArray<GOB_Entry>();
                        source.Files = entries.Select(s => new FileSource
                        {
                            Path = UnsafeX.FixedAString(s.Path, 13),
                            Offset = s.Offset,
                            FileSize = s.FileSize,
                        }).ToArray();
                        return Task.CompletedTask;
                    }
                case ".lfd":
                    {
                        var header = r.ReadS<LFD_Entry>();
                        if (header.Type != LFD_MAGIC) throw new FormatException("BAD MAGIC");
                        else if (UnsafeX.FixedAString(header.Name, 8) != "resource") throw new FormatException("BAD NAME");
                        else if (header.Size % 16 != 0) throw new FormatException("BAD SIZE");
                        var entries = r.ReadSArray<LFD_Entry>((int)header.Size / 16);
                        var offset = header.Size + 16;
                        source.Files = entries.Select(s => new FileSource
                        {
                            Path = UnsafeX.FixedAString(s.Name, 8),
                            Offset = (offset += s.Size + 16) - s.Size,
                            FileSize = s.Size,
                        }).ToArray();
                        return Task.CompletedTask;
                    }
                case ".lab":
                    {
                        var header = r.ReadS<LAB_Header>();
                        if (header.Magic != LAB_MAGIC) throw new FormatException("BAD MAGIC");
                        else if (header.Version != 0x10000) throw new FormatException("BAD VERSION");

                        var entries = r.ReadSArray<LAB_Entry>((int)header.FileCount);
                        var paths = r.ReadCStringArray((int)header.FileCount);
                        source.Files = entries.Select((s, i) => new FileSource
                        {
                            Path = paths[i],
                            Offset = s.Offset,
                            FileSize = s.FileSize,
                        }).ToArray();
                        return Task.CompletedTask;
                    }
                default: throw new FormatException();
            }
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            r.Seek(file.Offset);
            return Task.FromResult((Stream)new MemoryStream(r.ReadBytes((int)file.FileSize)));
        }
    }
}