using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class ACTIRecord : Record, IHaveEDID, IHaveMODL
    {
        public override string ToString() => $"ACTI: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // Model Name
        public STRVField FULL; // Item Name
        public FMIDField<SCPTRecord> SCRI; // Script (Optional)
        // TES4
        public FMIDField<SOUNRecord> SNAM; // Sound (Optional)

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID":
                case "NAME": EDID = r.ReadSTRV(dataSize); return true;
                case "MODL": MODL = new MODLGroup(r, dataSize); return true;
                case "MODB": MODL.MODBField(r, dataSize); return true;
                case "MODT": MODL.MODTField(r, dataSize); return true;
                case "FULL":
                case "FNAM": FULL = r.ReadSTRV(dataSize); return true;
                case "SCRI": SCRI = new FMIDField<SCPTRecord>(r, dataSize); return true;
                case "SNAM": SNAM = new FMIDField<SOUNRecord>(r, dataSize); return true;
                default: return false;
            }
        }
    }
}