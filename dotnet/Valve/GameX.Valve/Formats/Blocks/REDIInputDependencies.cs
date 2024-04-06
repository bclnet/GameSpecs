using System.Collections.Generic;
using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    public class REDIInputDependencies : REDIAbstract
    {
        public class InputDependency
        {
            public string ContentRelativeFilename { get; set; }
            public string ContentSearchPath { get; set; }
            public uint FileCRC { get; set; }
            public uint Flags { get; set; }

            public void WriteText(IndentedTextWriter w)
            {
                w.WriteLine("ResourceInputDependency_t {"); w.Indent++;
                w.WriteLine($"CResourceString m_ContentRelativeFilename = \"{ContentRelativeFilename}\"");
                w.WriteLine($"CResourceString m_ContentSearchPath = \"{ContentSearchPath}\"");
                w.WriteLine($"uint32 m_nFileCRC = 0x{FileCRC:X8}");
                w.WriteLine($"uint32 m_nFlags = 0x{Flags:X8}");
                w.Indent--; w.WriteLine("}");
            }
        }

        public List<InputDependency> List { get; } = new List<InputDependency>();

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            r.Seek(Offset);
            for (var i = 0; i < Size; i++)
                List.Add(new InputDependency
                {
                    ContentRelativeFilename = r.ReadO32UTF8(),
                    ContentSearchPath = r.ReadO32UTF8(),
                    FileCRC = r.ReadUInt32(),
                    Flags = r.ReadUInt32()
                });
        }

        public override void WriteText(IndentedTextWriter w)
        {
            w.WriteLine($"Struct m_InputDependencies[{List.Count}] = [");
            WriteList(w);
        }

        protected void WriteList(IndentedTextWriter w)
        {
            w.Indent++;
            foreach (var dep in List) dep.WriteText(w);
            w.Indent--; w.WriteLine("]");
        }
    }
}
