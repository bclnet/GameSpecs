using System.Collections.Generic;
using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class LSCRRecord : Record
    {
        public struct LNAMField
        {
            public FormId<Record> Direct;
            public FormId<WRLDRecord> IndirectWorld;
            public short IndirectGridX;
            public short IndirectGridY;

            public LNAMField(BinaryReader r, int dataSize)
            {
                Direct = new FormId<Record>(r.ReadUInt32());
                //if (dataSize == 0)
                IndirectWorld = new FormId<WRLDRecord>(r.ReadUInt32());
                //if (dataSize == 0)
                IndirectGridX = r.ReadInt16();
                IndirectGridY = r.ReadInt16();
            }
        }

        public override string ToString() => $"LSCR: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public FILEField ICON; // Icon
        public STRVField DESC; // Description
        public List<LNAMField> LNAMs; // LoadForm

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "ICON": ICON = r.ReadFILE(dataSize); return true;
                case "DESC": DESC = r.ReadSTRV(dataSize); return true;
                case "LNAM": if (LNAMs == null) LNAMs = new List<LNAMField>(); LNAMs.Add(new LNAMField(r, dataSize)); return true;
                default: return false;
            }
        }
    }
}