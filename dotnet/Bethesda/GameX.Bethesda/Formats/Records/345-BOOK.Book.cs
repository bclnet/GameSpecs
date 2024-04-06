using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class BOOKRecord : Record, IHaveEDID, IHaveMODL
    {
        public struct DATAField
        {
            public byte Flags; //: Scroll - (1 is scroll, 0 not)
            public byte Teaches; //: SkillId - (-1 is no skill)
            public int Value;
            public float Weight;
            //
            public int EnchantPts;

            public DATAField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                if (format == BethesdaFormat.TES3)
                {
                    Weight = r.ReadSingle();
                    Value = r.ReadInt32();
                    Flags = (byte)r.ReadInt32();
                    Teaches = (byte)r.ReadInt32();
                    EnchantPts = r.ReadInt32();
                    return;
                }
                Flags = r.ReadByte();
                Teaches = r.ReadByte();
                Value = r.ReadInt32();
                Weight = r.ReadSingle();
                EnchantPts = 0;
            }
        }

        public override string ToString() => $"BOOK: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // Model (optional)
        public STRVField FULL; // Item Name
        public DATAField DATA; // Book Data
        public STRVField DESC; // Book Text
        public FILEField ICON; // Inventory Icon (optional)
        public FMIDField<SCPTRecord> SCRI; // Script Name (optional)
        public FMIDField<ENCHRecord> ENAM; // Enchantment FormId (optional)
        // TES4
        public IN16Field? ANAM; // Enchantment points (optional)

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
                case "BKDT": DATA = new DATAField(r, dataSize, format); return true;
                case "ICON":
                case "ITEX": ICON = r.ReadFILE(dataSize); return true;
                case "SCRI": SCRI = new FMIDField<SCPTRecord>(r, dataSize); return true;
                case "DESC":
                case "TEXT": DESC = r.ReadSTRV(dataSize); return true;
                case "ENAM": ENAM = new FMIDField<ENCHRecord>(r, dataSize); return true;
                case "ANAM": ANAM = r.ReadS2<IN16Field>(dataSize); return true;
                default: return false;
            }
        }
    }
}
