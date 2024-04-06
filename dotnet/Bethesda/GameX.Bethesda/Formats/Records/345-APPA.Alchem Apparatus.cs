using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class APPARecord : Record, IHaveEDID, IHaveMODL
    {
        // TESX
        public struct DATAField
        {
            public byte Type; // 0 = Mortar and Pestle, 1 = Albemic, 2 = Calcinator, 3 = Retort
            public float Quality;
            public float Weight;
            public int Value;

            public DATAField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                if (format == BethesdaFormat.TES3)
                {
                    Type = (byte)r.ReadInt32();
                    Quality = r.ReadSingle();
                    Weight = r.ReadSingle();
                    Value = r.ReadInt32();
                    return;
                }
                Type = r.ReadByte();
                Value = r.ReadInt32();
                Weight = r.ReadSingle();
                Quality = r.ReadSingle();
            }
        }

        public override string ToString() => $"APPA: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // Model Name
        public STRVField FULL; // Item Name
        public DATAField DATA; // Alchemy Data
        public FILEField ICON; // Inventory Icon
        public FMIDField<SCPTRecord> SCRI; // Script Name

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
                case "AADT": DATA = new DATAField(r, dataSize, format); return true;
                case "ICON":
                case "ITEX": ICON = r.ReadFILE(dataSize); return true;
                case "SCRI": SCRI = new FMIDField<SCPTRecord>(r, dataSize); return true;
                default: return false;
            }
        }
    }
}