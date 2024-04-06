using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class ACHRRecord : Record
    {
        public override string ToString() => $"ACHR: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public FMIDField<Record> NAME; // Base
        public REFRRecord.DATAField DATA; // Position/Rotation
        public FMIDField<CELLRecord>? XPCI; // Unused (optional)
        public BYTVField? XLOD; // Distant LOD Data (optional)
        public REFRRecord.XESPField? XESP; // Enable Parent (optional)
        public FMIDField<REFRRecord>? XMRC; // Merchant container (optional)
        public FMIDField<ACRERecord>? XHRS; // Horse (optional)
        public FLTVField? XSCL; // Scale (optional)
        public BYTVField? XRGD; // Ragdoll Data (optional)

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "NAME": NAME = new FMIDField<Record>(r, dataSize); return true;
                case "DATA": DATA = new REFRRecord.DATAField(r, dataSize); return true;
                case "XPCI": XPCI = new FMIDField<CELLRecord>(r, dataSize); return true;
                case "FULL": XPCI.Value.AddName(r.ReadFString(dataSize)); return true;
                case "XLOD": XLOD = r.ReadBYTV(dataSize); return true;
                case "XESP": XESP = new REFRRecord.XESPField(r, dataSize); return true;
                case "XMRC": XMRC = new FMIDField<REFRRecord>(r, dataSize); return true;
                case "XHRS": XHRS = new FMIDField<ACRERecord>(r, dataSize); return true;
                case "XSCL": XSCL = r.ReadS2<FLTVField>(dataSize); return true;
                case "XRGD": XRGD = r.ReadBYTV(dataSize); return true;
                default: return false;
            }
        }
    }
}