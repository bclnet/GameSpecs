using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.Bethesda.Formats.Records
{
    public class ACRERecord : Record
    {
        public override string ToString() => $"GMST: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public FMIDField<Record> NAME; // Base
        public REFRRecord.DATAField DATA; // Position/Rotation
        public List<CELLRecord.XOWNGroup> XOWNs; // Ownership (optional)
        public REFRRecord.XESPField? XESP; // Enable Parent (optional)
        public FLTVField XSCL; // Scale (optional)
        public BYTVField? XRGD; // Ragdoll Data (optional)

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "NAME": NAME = new FMIDField<Record>(r, dataSize); return true;
                case "DATA": DATA = new REFRRecord.DATAField(r, dataSize); return true;
                case "XOWN": if (XOWNs == null) XOWNs = new List<CELLRecord.XOWNGroup>(); XOWNs.Add(new CELLRecord.XOWNGroup { XOWN = new FMIDField<Record>(r, dataSize) }); return true;
                case "XRNK": XOWNs.Last().XRNK = r.ReadS2<IN32Field>(dataSize); return true;
                case "XGLB": XOWNs.Last().XGLB = new FMIDField<Record>(r, dataSize); return true;
                case "XESP": XESP = new REFRRecord.XESPField(r, dataSize); return true;
                case "XSCL": XSCL = r.ReadS2<FLTVField>(dataSize); return true;
                case "XRGD": XRGD = r.ReadBYTV(dataSize); return true;
                default: return false;
            }
        }
    }
}