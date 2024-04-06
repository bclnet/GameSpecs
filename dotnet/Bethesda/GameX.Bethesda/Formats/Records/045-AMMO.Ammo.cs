using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class AMMORecord : Record, IHaveEDID, IHaveMODL
    {
        public struct DATAField
        {
            public float Speed;
            public uint Flags;
            public uint Value;
            public float Weight;
            public ushort Damage;

            public DATAField(BinaryReader r, int dataSize)
            {
                Speed = r.ReadSingle();
                Flags = r.ReadUInt32();
                Value = r.ReadUInt32();
                Weight = r.ReadSingle();
                Damage = r.ReadUInt16();
            }
        }

        public override string ToString() => $"AMMO: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // Model
        public STRVField FULL; // Item Name
        public FILEField? ICON; // Male Icon (optional)
        public FMIDField<ENCHRecord>? ENAM; // Enchantment ID (optional)
        public IN16Field? ANAM; // Enchantment points (optional)
        public DATAField DATA; // Ammo Data

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "MODL": MODL = new MODLGroup(r, dataSize); return true;
                case "MODB": MODL.MODBField(r, dataSize); return true;
                case "MODT": MODL.MODTField(r, dataSize); return true;
                case "FULL": FULL = r.ReadSTRV(dataSize); return true;
                case "ICON": ICON = r.ReadFILE(dataSize); return true;
                case "ENAM": ENAM = new FMIDField<ENCHRecord>(r, dataSize); return true;
                case "ANAM": ANAM = r.ReadS2<IN16Field>(dataSize); return true;
                case "DATA": DATA = new DATAField(r, dataSize); return true;
                default: return false;
            }
        }
    }
}