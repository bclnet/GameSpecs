using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class FURNRecord : Record, IHaveEDID, IHaveMODL
    {
        public override string ToString() => $"FURN: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // Model
        public STRVField FULL; // Furniture Name
        public FMIDField<SCPTRecord> SCRI; // Script (optional)
        public IN32Field MNAM; // Active marker flags, required. A bit field with a bit value of 1 indicating that the matching marker position in the NIF file is active.

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "MODL": MODL = new MODLGroup(r, dataSize); return true;
                case "MODB": MODL.MODBField(r, dataSize); return true;
                case "MODT": MODL.MODTField(r, dataSize); return true;
                case "FULL": FULL = r.ReadSTRV(dataSize); return true;
                case "SCRI": SCRI = new FMIDField<SCPTRecord>(r, dataSize); return true;
                case "MNAM": MNAM = r.ReadS2<IN32Field>(dataSize); return true;
                default: return false;
            }
        }
    }
}