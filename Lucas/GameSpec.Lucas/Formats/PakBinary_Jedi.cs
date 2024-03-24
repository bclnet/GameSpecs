using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static OpenStack.Debug;

namespace GameSpec.Lucas.Formats
{
    public unsafe class PakBinary_Jedi : PakBinary<PakBinary_Jedi>
    {
        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct Lab_Header
        {
            public static (string, int) Struct = ("<4I", sizeof(Lab_Header));
            public uint Magic;              // Always 'LABN'
            public uint Unknown;            // Apparently always 0x10000 for Outlaws
            public uint FileCount;          // File entry count
            public uint FileNameListLength; // Length including null bytes of the filename list/string
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0x1)]
        struct Lab_Entry
        {
            public static (string, int) Struct = ("<4I", sizeof(Lab_Entry));
            public uint NameOffset;         // Offset in the name string
            public uint DataOffset;         // Offset in the archive file
            public uint SizeInBytes;        // Size in bytes of this entry
            public uint TypeId;             // All zeros or a 4CC related to the filename extension
        }

        public override Task Read(BinaryPakFile source, BinaryReader r, object tag)
        {
            const uint MAGIC = 0x4E42414C;

            var files = new List<FileSource>(); source.Files = files;

            var header = r.ReadS<Lab_Header>();
            if (header.Magic != MAGIC) throw new FormatException("BAD MAGIC");

            var fileSize = r.BaseStream.Length;
            for (var i = 0; i < header.FileCount; i++)
            {
                var entry = r.ReadS<Lab_Entry>();
                if (entry.NameOffset >= header.FileNameListLength) { Log("Entry: bad name offset"); continue; }
                else if (entry.DataOffset >= fileSize) { Log("Entry: bad data offset"); continue; }
                else if ((entry.DataOffset + entry.SizeInBytes) > fileSize) { Log("Entry: bad data offset/size"); continue; }

                //std::string entryName  { &labFileNameListPtr[entry.nameOffset] };
                //TableEntry tableEntry { entry.dataOffset, entry.sizeInBytes, entry.typeId };
            }



            return Task.CompletedTask;
        }

        public override Task<Stream> ReadData(BinaryPakFile source, BinaryReader r, FileSource file, FileOption option = default)
        {
            throw new NotImplementedException();
        }
    }
}