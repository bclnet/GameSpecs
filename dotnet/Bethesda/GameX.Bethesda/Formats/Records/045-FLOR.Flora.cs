using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class FLORRecord : Record, IHaveEDID, IHaveMODL
    {
        public override string ToString() => $"FLOR: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // Model
        public STRVField FULL; // Plant Name
        public FMIDField<SCPTRecord> SCRI; // Script (optional)
        public FMIDField<INGRRecord> PFIG; // The ingredient the plant produces (optional)
        public BYTVField PFPC; // Spring, Summer, Fall, Winter Ingredient Production (byte)

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
                case "PFIG": PFIG = new FMIDField<INGRRecord>(r, dataSize); return true;
                case "PFPC": PFPC = r.ReadBYTV(dataSize); return true;
                default: return false;
            }
        }
    }
}