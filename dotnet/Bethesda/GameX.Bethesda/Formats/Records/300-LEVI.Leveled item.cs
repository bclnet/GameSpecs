using System.Collections.Generic;
using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class LEVIRecord : Record
    {
        public override string ToString() => $"LEVI: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public IN32Field DATA; // List data - 1 = Calc from all levels <= PC level, 2 = Calc for each item
        public BYTEField NNAM; // Chance None?
        public IN32Field INDX; // Number of items in list
        public List<STRVField> INAMs = new List<STRVField>(); // ID string of list item
        public List<IN16Field> INTVs = new List<IN16Field>(); // PC level for previous INAM
        // The CNAM/INTV can occur many times in pairs

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            if (format == BethesdaFormat.TES3)
                switch (type)
                {
                    case "NAME": EDID = r.ReadSTRV(dataSize); return true;
                    case "DATA": DATA = r.ReadS2<IN32Field>(dataSize); return true;
                    case "NNAM": NNAM = r.ReadS2<BYTEField>(dataSize); return true;
                    case "INDX": INDX = r.ReadS2<IN32Field>(dataSize); return true;
                    case "INAM": INAMs.Add(r.ReadSTRV(dataSize)); return true;
                    case "INTV": INTVs.Add(r.ReadS2<IN16Field>(dataSize)); return true;
                    default: return false;
                }
            return false;
        }
    }
}