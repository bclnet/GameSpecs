using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class KEYMRecord : Record, IHaveEDID, IHaveMODL
    {
        public struct DATAField
        {
            public int Value;
            public float Weight;

            public DATAField(BinaryReader r, int dataSize)
            {
                Value = r.ReadInt32();
                Weight = r.ReadSingle();
            }
        }

        public override string ToString() => $"KEYM: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // Model
        public STRVField FULL; // Item Name
        public FMIDField<SCPTRecord> SCRI; // Script (optional)
        public DATAField DATA; // Type of soul contained in the gem
        public FILEField ICON; // Icon (optional)

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
                case "DATA": DATA = new DATAField(r, dataSize); return true;
                case "ICON": ICON = r.ReadFILE(dataSize); return true;
                default: return false;
            }
        }
    }
}