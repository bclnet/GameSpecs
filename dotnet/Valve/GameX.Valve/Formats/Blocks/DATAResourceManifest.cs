using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/ResourceManifest
    public class DATAResourceManifest : DATABinaryNTRO
    {
        public List<List<string>> Resources { get; private set; }

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            r.Seek(Offset);
            if (parent.ContainsBlockType<NTRO>())
            {
                var ntro = new DATABinaryNTRO { StructName = "ResourceManifest_t", Offset = Offset, Size = Size };
                ntro.Read(parent, r);
                Resources = new List<List<string>> { new List<string>(ntro.Data.Get<string[]>("m_ResourceFileNameList")) };
                return;
            }

            var version = r.ReadInt32();
            if (version != 8) throw new ArgumentOutOfRangeException(nameof(version), $"Unknown version {version}");

            Resources = new List<List<string>>();
            var blockCount = r.ReadInt32();
            for (var block = 0; block < blockCount; block++)
            {
                var strings = new List<string>();
                var originalOffset = r.BaseStream.Position;
                var offset = r.ReadInt32();
                var count = r.ReadInt32();
                r.Seek(originalOffset + offset);
                for (var i = 0; i < count; i++)
                {
                    var returnOffset = r.BaseStream.Position;
                    var stringOffset = r.ReadInt32();
                    r.Seek(returnOffset + stringOffset);
                    strings.Add(r.ReadZUTF8());
                    r.Seek(returnOffset + 4);
                }
                r.Seek(originalOffset + 8);
                Resources.Add(strings);
            }
        }

        public override string ToString()
        {
            using var w = new IndentedTextWriter();
            foreach (var block in Resources)
            {
                foreach (var entry in block) w.WriteLine(entry);
                w.WriteLine();
            }
            return w.ToString();
        }
    }
}
