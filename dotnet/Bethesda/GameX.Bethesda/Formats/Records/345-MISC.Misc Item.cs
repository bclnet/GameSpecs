using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class MISCRecord : Record, IHaveEDID, IHaveMODL
    {
        // TESX
        public struct DATAField
        {
            public float Weight;
            public uint Value;
            public uint Unknown;

            public DATAField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                if (format == BethesdaFormat.TES3)
                {
                    Weight = r.ReadSingle();
                    Value = r.ReadUInt32();
                    Unknown = r.ReadUInt32();
                    return;
                }
                Value = r.ReadUInt32();
                Weight = r.ReadSingle();
                Unknown = 0;
            }
        }

        public override string ToString() => $"MISC: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // Model
        public STRVField FULL; // Item Name
        public DATAField DATA; // Misc Item Data
        public FILEField ICON; // Icon (optional)
        public FMIDField<SCPTRecord> SCRI; // Script FormID (optional)
        // TES3
        public FMIDField<ENCHRecord> ENAM; // enchantment ID

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
                case "DATA":
                case "MCDT": DATA = new DATAField(r, dataSize, format); return true;
                case "ICON":
                case "ITEX": ICON = r.ReadFILE(dataSize); return true;
                case "ENAM": ENAM = new FMIDField<ENCHRecord>(r, dataSize); return true;
                case "SCRI": SCRI = new FMIDField<SCPTRecord>(r, dataSize); return true;
                default: return false;
            }
        }
    }
}