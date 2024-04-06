using System.Collections.Generic;
using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    public class REDIChildResourceList : REDIAbstract
    {
        public class ReferenceInfo
        {
            public ulong Id { get; set; }
            public string ResourceName { get; set; }
            public uint Unknown { get; set; }

            public void WriteText(IndentedTextWriter w)
            {
                w.WriteLine("ResourceReferenceInfo_t {"); w.Indent++;
                w.WriteLine($"uint64 m_nId = 0x{Id:X16}");
                w.WriteLine($"CResourceString m_pResourceName = \"{ResourceName}\"");
                w.Indent--; w.WriteLine("}");
            }
        }

        public List<ReferenceInfo> List { get; } = new List<ReferenceInfo>();

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            r.Seek(Offset);
            for (var i = 0; i < Size; i++)
                List.Add(new ReferenceInfo
                {
                    Id = r.ReadUInt64(),
                    ResourceName = r.ReadO32UTF8(),
                    Unknown = r.ReadUInt32()
                });
        }

        public override void WriteText(IndentedTextWriter w)
        {
            w.WriteLine($"Struct m_ChildResourceList[{List.Count}] = ["); w.Indent++;
            foreach (var dep in List) dep.WriteText(w);
            w.Indent--; w.WriteLine("]");
        }
    }
}
