using System.Collections.Generic;
using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    public class REDIArgumentDependencies : REDIAbstract
    {
        public class ArgumentDependency
        {
            public string ParameterName { get; set; }
            public string ParameterType { get; set; }
            public uint Fingerprint { get; set; }
            public uint FingerprintDefault { get; set; }

            public void WriteText(IndentedTextWriter w)
            {
                w.WriteLine("ResourceArgumentDependency_t {"); w.Indent++;
                w.WriteLine($"CResourceString m_ParameterName = \"{ParameterName}\"");
                w.WriteLine($"CResourceString m_ParameterType = \"{ParameterType}\"");
                w.WriteLine($"uint32 m_nFingerprint = 0x{Fingerprint:X8}");
                w.WriteLine($"uint32 m_nFingerprintDefault = 0x{FingerprintDefault:X8}");
                w.Indent--; w.WriteLine("}");
            }
        }

        public List<ArgumentDependency> List { get; } = new List<ArgumentDependency>();

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            r.Seek(Offset);
            for (var i = 0; i < Size; i++)
                List.Add(new ArgumentDependency
                {
                    ParameterName = r.ReadO32UTF8(),
                    ParameterType = r.ReadO32UTF8(),
                    Fingerprint = r.ReadUInt32(),
                    FingerprintDefault = r.ReadUInt32()
                });
        }

        public override void WriteText(IndentedTextWriter w)
        {
            w.WriteLine($"Struct m_ArgumentDependencies[{List.Count}] = ["); w.Indent++;
            foreach (var dep in List) dep.WriteText(w);
            w.Indent--; w.WriteLine("]");
        }
    }
}
