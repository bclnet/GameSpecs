using System.Collections.Generic;
using System.IO;

namespace GameSpec.Tes.Formats.Records
{
    public class LVSPRecord : Record
    {
        public override string ToString() => $"LVSP: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public BYTEField LVLD; // Chance
        public BYTEField LVLF; // Flags
        public List<LVLIRecord.LVLOField> LVLOs = new List<LVLIRecord.LVLOField>(); // Number of items in list

        public override bool CreateField(BinaryReader r, TesFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "LVLD": LVLD = r.ReadT<BYTEField>(dataSize); return true;
                case "LVLF": LVLF = r.ReadT<BYTEField>(dataSize); return true;
                case "LVLO": LVLOs.Add(new LVLIRecord.LVLOField(r, dataSize)); return true;
                default: return false;
            }
        }
    }
}