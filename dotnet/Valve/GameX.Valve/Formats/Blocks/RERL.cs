using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.Valve.Formats.Blocks
{
    /// <summary>
    /// "RERL" block. ResourceExtRefList_t.
    /// </summary>
    public class RERL : Block
    {
        public class RERLInfo
        {
            /// <summary>
            /// Gets or sets the resource id.
            /// </summary>
            public ulong Id { get; set; }

            /// <summary>
            /// Gets or sets the resource name.
            /// </summary>
            public string Name { get; set; }

            public void WriteText(IndentedTextWriter w)
            {
                w.WriteLine("ResourceReferenceInfo_t {"); w.Indent++;
                w.WriteLine($"uint64 m_nId = 0x{Id:X16}");
                w.WriteLine($"CResourceString m_pResourceName = \"{Name}\"");
                w.Indent--; w.WriteLine("}");
            }
        }

        public IList<RERLInfo> RERLInfos { get; private set; } = new List<RERLInfo>();

        public string this[ulong id] => RERLInfos.FirstOrDefault(c => c.Id == id)?.Name;

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            r.Seek(Offset);
            var offset = r.ReadUInt32();
            var size = r.ReadUInt32();
            if (size == 0) return;

            r.Skip(offset - 8); // 8 is 2 uint32s we just read
            for (var i = 0; i < size; i++)
            {
                var id = r.ReadUInt64();
                var previousPosition = r.BaseStream.Position;
                // jump to string: offset is counted from current position, so we will need to add 8 to position later
                r.BaseStream.Position += r.ReadInt64();
                RERLInfos.Add(new RERLInfo { Id = id, Name = r.ReadZUTF8() });
                r.BaseStream.Position = previousPosition + 8; // 8 is to account for string offset
            }
        }

        public override void WriteText(IndentedTextWriter w)
        {
            w.WriteLine("ResourceExtRefList_t {"); w.Indent++;
            w.WriteLine($"Struct m_resourceRefInfoList[{RERLInfos.Count}] = ["); w.Indent++;
            foreach (var refInfo in RERLInfos) refInfo.WriteText(w);
            w.Indent--; w.WriteLine("]");
            w.Indent--; w.WriteLine("}");
        }
    }
}
