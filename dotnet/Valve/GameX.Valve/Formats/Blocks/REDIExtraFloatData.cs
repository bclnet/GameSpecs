using System.Collections.Generic;
using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    public class REDIExtraFloatData : REDIAbstract
    {
        public class EditFloatData
        {
            public string Name { get; set; }
            public float Value { get; set; }

            public void WriteText(IndentedTextWriter w)
            {
                w.WriteLine("ResourceEditFloatData_t {"); w.Indent++;
                w.WriteLine($"CResourceString m_Name = \"{Name}\"");
                w.WriteLine($"float32 m_flFloat = {Value:F6}");
                w.Indent--; w.WriteLine("}");
            }
        }

        public List<EditFloatData> List { get; } = new List<EditFloatData>();

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            r.Seek(Offset);
            for (var i = 0; i < Size; i++) List.Add(new EditFloatData
            {
                Name = r.ReadO32UTF8(),
                Value = r.ReadSingle()
            });
        }

        public override void WriteText(IndentedTextWriter w)
        {
            w.WriteLine($"Struct m_ExtraFloatData[{List.Count}] = ["); w.Indent++;
            foreach (var dep in List) dep.WriteText(w);
            w.Indent--; w.WriteLine("]");
        }
    }
}
