using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class LOCKRecord : Record, IHaveEDID, IHaveMODL
    {
        public struct LKDTField
        {
            public float Weight;
            public int Value;
            public float Quality;
            public int Uses;

            public LKDTField(BinaryReader r, int dataSize)
            {
                Weight = r.ReadSingle();
                Value = r.ReadInt32();
                Quality = r.ReadSingle();
                Uses = r.ReadInt32();
            }
        }

        public override string ToString() => $"LOCK: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // Model Name
        public STRVField FNAM; // Item Name
        public LKDTField LKDT; // Lock Data
        public FILEField ICON; // Inventory Icon
        public FMIDField<SCPTRecord> SCRI; // Script Name

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            if (format == BethesdaFormat.TES3)
                switch (type)
                {
                    case "NAME": EDID = r.ReadSTRV(dataSize); return true;
                    case "MODL": MODL = new MODLGroup(r, dataSize); return true;
                    case "FNAM": FNAM = r.ReadSTRV(dataSize); return true;
                    case "LKDT": LKDT = new LKDTField(r, dataSize); return true;
                    case "ITEX": ICON = r.ReadFILE(dataSize); return true;
                    case "SCRI": SCRI = new FMIDField<SCPTRecord>(r, dataSize); return true;
                    default: return false;
                }
            return false;
        }
    }
}