using System.Collections.Generic;
using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    public class REDISpecialDependencies : REDIAbstract
    {
        public class SpecialDependency
        {
            public string String { get; set; }
            public string CompilerIdentifier { get; set; }
            public uint Fingerprint { get; set; }
            public uint UserData { get; set; }

            public void WriteText(IndentedTextWriter w)
            {
                w.WriteLine("ResourceSpecialDependency_t {"); w.Indent++;
                w.WriteLine($"CResourceString m_String = \"{String}\"");
                w.WriteLine($"CResourceString m_CompilerIdentifier = \"{CompilerIdentifier}\"");
                w.WriteLine($"uint32 m_nFingerprint = 0x{Fingerprint:X8}");
                w.WriteLine($"uint32 m_nUserData = 0x{UserData:X8}");
                w.Indent--; w.WriteLine("}");
            }
        }

        public List<SpecialDependency> List { get; } = new List<SpecialDependency>();

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            r.Seek(Offset);
            for (var i = 0; i < Size; i++)
                List.Add(new SpecialDependency
                {
                    String = r.ReadO32UTF8(),
                    CompilerIdentifier = r.ReadO32UTF8(),
                    Fingerprint = r.ReadUInt32(),
                    UserData = r.ReadUInt32()
                });
        }

        public override void WriteText(IndentedTextWriter w)
        {
            w.WriteLine($"Struct m_SpecialDependencies[{List.Count}] = ["); w.Indent++;
            foreach (var dep in List) dep.WriteText(w);
            w.Indent--; w.WriteLine("]");
        }
    }
}
