using System.Collections.Generic;
using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    public class REDIAdditionalRelatedFiles : REDIAbstract
    {
        public class AdditionalRelatedFile
        {
            public string ContentRelativeFilename { get; set; }
            public string ContentSearchPath { get; set; }

            public void WriteText(IndentedTextWriter w)
            {
                w.WriteLine("ResourceAdditionalRelatedFile_t {"); w.Indent++;
                w.WriteLine($"CResourceString m_ContentRelativeFilename = \"{ContentRelativeFilename}\"");
                w.WriteLine($"CResourceString m_ContentSearchPath = \"{ContentSearchPath}\"");
                w.Indent--; w.WriteLine("}");
            }
        }

        public List<AdditionalRelatedFile> List { get; } = new List<AdditionalRelatedFile>();

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            r.Seek(Offset);
            for (var i = 0; i < Size; i++)
                List.Add(new AdditionalRelatedFile
                {
                    ContentRelativeFilename = r.ReadO32UTF8(),
                    ContentSearchPath = r.ReadO32UTF8()
                });
        }

        public override void WriteText(IndentedTextWriter w)
        {
            w.WriteLine($"Struct m_AdditionalRelatedFiles[{List.Count}] = ["); w.Indent++;
            foreach (var dep in List) dep.WriteText(w);
            w.Indent--; w.WriteLine("]");
        }
    }
}
