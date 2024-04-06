using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    public class REDIExtraStringData : REDIAbstract
    {
        public class EditStringData
        {
            public string Name { get; set; }
            public string Value { get; set; }

            public void WriteText(IndentedTextWriter w)
            {
                w.WriteLine("ResourceEditStringData_t {"); w.Indent++;
                w.WriteLine($"CResourceString m_Name = \"{Name}\"");
                var lines = Value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                if (lines.Length > 1)
                {
                    w.Indent++;
                    w.Write("CResourceString m_String = \"");
                    foreach (var line in lines) w.WriteLine(line);
                    w.WriteLine("\"");
                    w.Indent--;
                }
                else w.WriteLine($"CResourceString m_String = \"{Value}\"");
                w.Indent--; w.WriteLine("}");
            }
        }

        public List<EditStringData> List { get; } = new List<EditStringData>();

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            r.Seek(Offset);
            for (var i = 0; i < Size; i++) List.Add(new EditStringData
            {
                Name = r.ReadO32UTF8(),
                Value = r.ReadO32UTF8()
            });
        }

        public override void WriteText(IndentedTextWriter w)
        {
            w.WriteLine($"Struct m_ExtraStringData[{List.Count}] = ["); w.Indent++;
            foreach (var dep in List) dep.WriteText(w);
            w.Indent--; w.WriteLine("]");
        }
    }
}
