using GameSpec.Algorithms;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameSpec.Valve.Formats.Blocks
{
    public class DATAPanorama : DATA
    {
        public class NameEntry
        {
            public string Name { get; set; }
            public uint Unknown1 { get; set; }
            public uint Unknown2 { get; set; }
        }

        public List<NameEntry> Names { get; } = new List<NameEntry>();

        public byte[] Data { get; private set; }
        public uint Crc32 { get; private set; }

        public override void Read(BinaryPak parent, BinaryReader r)
        {
            r.Seek(Offset);
            Crc32 = r.ReadUInt32();
            var size = r.ReadUInt16();
            for (var i = 0; i < size; i++)
                Names.Add(new NameEntry
                {
                    Name = r.ReadZUTF8(),
                    Unknown1 = r.ReadUInt32(),
                    Unknown2 = r.ReadUInt32(),
                });
            var headerSize = r.BaseStream.Position - Offset;
            Data = r.ReadBytes((int)Size - (int)headerSize);
            if (Crc32Digest.Compute(Data) != Crc32) throw new InvalidDataException("CRC32 mismatch for read data.");
        }

        public override string ToString() => Encoding.UTF8.GetString(Data);
    }
}
