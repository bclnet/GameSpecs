using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.Bethesda.Formats.Records
{
    public class FACTRecord : Record
    {
        // TESX
        public class RNAMGroup
        {
            public override string ToString() => $"{RNAM.Value}:{MNAM.Value}";
            public IN32Field RNAM; // rank
            public STRVField MNAM; // male
            public STRVField FNAM; // female
            public STRVField INAM; // insignia
        }

        // TES3
        public struct FADTField
        {
            public FADTField(BinaryReader r, int dataSize) => r.Skip(dataSize);
        }

        // TES4
        public struct XNAMField
        {
            public override string ToString() => $"{FormId}";
            public int FormId;
            public int Mod;
            public int Combat;

            public XNAMField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                FormId = r.ReadInt32();
                Mod = r.ReadInt32();
                Combat = format > BethesdaFormat.TES4 ? r.ReadInt32() : 0; // 0 - Neutral, 1 - Enemy, 2 - Ally, 3 - Friend
            }
        }

        public override string ToString() => $"FACT: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public STRVField FNAM; // Faction name
        public List<RNAMGroup> RNAMs = new List<RNAMGroup>(); // Rank Name
        public FADTField FADT; // Faction data
        public List<STRVField> ANAMs = new List<STRVField>(); // Faction name
        public List<INTVField> INTVs = new List<INTVField>(); // Faction reaction
        // TES4
        public XNAMField XNAM; // Interfaction Relations
        public INTVField DATA; // Flags (byte, uint32)
        public UI32Field CNAM;

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            if (format == BethesdaFormat.TES3)
                switch (type)
                {
                    case "NAME": EDID = r.ReadSTRV(dataSize); return true;
                    case "FNAM": FNAM = r.ReadSTRV(dataSize); return true;
                    case "RNAM": RNAMs.Add(new RNAMGroup { MNAM = r.ReadSTRV(dataSize) }); return true;
                    case "FADT": FADT = new FADTField(r, dataSize); return true;
                    case "ANAM": ANAMs.Add(r.ReadSTRV(dataSize)); return true;
                    case "INTV": INTVs.Add(r.ReadINTV(dataSize)); return true;
                    default: return false;
                }
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "FULL": FNAM = r.ReadSTRV(dataSize); return true;
                case "XNAM": XNAM = new XNAMField(r, dataSize, format); return true;
                case "DATA": DATA = r.ReadINTV(dataSize); return true;
                case "CNAM": CNAM = r.ReadT<UI32Field>(dataSize); return true;
                case "RNAM": RNAMs.Add(new RNAMGroup { RNAM = r.ReadT<IN32Field>(dataSize) }); return true;
                case "MNAM": RNAMs.Last().MNAM = r.ReadSTRV(dataSize); return true;
                case "FNAM": RNAMs.Last().FNAM = r.ReadSTRV(dataSize); return true;
                case "INAM": RNAMs.Last().INAM = r.ReadSTRV(dataSize); return true;
                default: return false;
            }
        }
    }
}
