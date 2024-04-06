using System.Collections.Generic;
using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public unsafe class TES4Record : Record
    {
        public struct HEDRField
        {
            public static (string, int) Struct = ("<fiI", sizeof(HEDRField));
            public float Version;
            public int NumRecords; // Number of records and groups (not including TES4 record itself).
            public uint NextObjectId; // Next available object ID.
        }

        public HEDRField HEDR;
        public STRVField? CNAM; // author (Optional)
        public STRVField? SNAM; // description (Optional)
        public List<STRVField> MASTs; // master
        public List<INTVField> DATAs; // fileSize
        public UNKNField? ONAM; // overrides (Optional)
        public IN32Field INTV; // unknown
        public IN32Field? INCC; // unknown (Optional)
        // TES5
        public UNKNField? TNAM; // overrides (Optional)

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "HEDR": HEDR = r.ReadS2<HEDRField>(dataSize); return true;
                case "OFST": r.Skip(dataSize); return true;
                case "DELE": r.Skip(dataSize); return true;
                case "CNAM": CNAM = r.ReadSTRV(dataSize); return true;
                case "SNAM": SNAM = r.ReadSTRV(dataSize); return true;
                case "MAST": if (MASTs == null) MASTs = new List<STRVField>(); MASTs.Add(r.ReadSTRV(dataSize)); return true;
                case "DATA": if (DATAs == null) DATAs = new List<INTVField>(); DATAs.Add(r.ReadINTV(dataSize)); return true;
                case "ONAM": ONAM = r.ReadUNKN(dataSize); return true;
                case "INTV": INTV = r.ReadS2<IN32Field>(dataSize); return true;
                case "INCC": INCC = r.ReadS2<IN32Field>(dataSize); return true;
                // TES5
                case "TNAM": TNAM = r.ReadUNKN(dataSize); return true;
                default: return false;
            }
        }
    }
}