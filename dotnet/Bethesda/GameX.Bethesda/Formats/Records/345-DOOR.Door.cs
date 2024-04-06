using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class DOORRecord : Record, IHaveEDID, IHaveMODL
    {
        public override string ToString() => $"DOOR: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public STRVField FULL; // Door name
        public MODLGroup MODL { get; set; } // NIF model filename
        public FMIDField<SCPTRecord>? SCRI; // Script (optional)
        public FMIDField<SOUNRecord> SNAM; // Open Sound
        public FMIDField<SOUNRecord> ANAM; // Close Sound
        // TES4
        public FMIDField<SOUNRecord> BNAM; // Loop Sound
        public BYTEField FNAM; // Flags
        public FMIDField<Record> TNAM; // Random teleport destination

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID":
                case "NAME": EDID = r.ReadSTRV(dataSize); return true;
                case "FULL": FULL = r.ReadSTRV(dataSize); return true;
                case "FNAM": if (format != BethesdaFormat.TES3) FNAM = r.ReadT<BYTEField>(dataSize); else FULL = r.ReadSTRV(dataSize); return true;
                case "MODL": MODL = new MODLGroup(r, dataSize); return true;
                case "MODB": MODL.MODBField(r, dataSize); return true;
                case "MODT": MODL.MODTField(r, dataSize); return true;
                case "SCRI": SCRI = new FMIDField<SCPTRecord>(r, dataSize); return true;
                case "SNAM": SNAM = new FMIDField<SOUNRecord>(r, dataSize); return true;
                case "ANAM": ANAM = new FMIDField<SOUNRecord>(r, dataSize); return true;
                case "BNAM": ANAM = new FMIDField<SOUNRecord>(r, dataSize); return true;
                case "TNAM": TNAM = new FMIDField<Record>(r, dataSize); return true;
                default: return false;
            }
        }
    }
}