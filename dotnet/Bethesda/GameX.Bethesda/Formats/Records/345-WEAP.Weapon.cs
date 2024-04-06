using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class WEAPRecord : Record, IHaveEDID, IHaveMODL
    {
        public struct DATAField
        {
            public enum WEAPType
            {
                ShortBladeOneHand = 0, LongBladeOneHand, LongBladeTwoClose, BluntOneHand, BluntTwoClose, BluntTwoWide, SpearTwoWide, AxeOneHand, AxeTwoHand, MarksmanBow, MarksmanCrossbow, MarksmanThrown, Arrow, Bolt,
            }

            public float Weight;
            public int Value;
            public ushort Type;
            public short Health;
            public float Speed;
            public float Reach;
            public short Damage; //: EnchantPts;
            public byte ChopMin;
            public byte ChopMax;
            public byte SlashMin;
            public byte SlashMax;
            public byte ThrustMin;
            public byte ThrustMax;
            public int Flags; // 0 = ?, 1 = Ignore Normal Weapon Resistance?

            public DATAField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                if (format == BethesdaFormat.TES3)
                {
                    Weight = r.ReadSingle();
                    Value = r.ReadInt32();
                    Type = r.ReadUInt16();
                    Health = r.ReadInt16();
                    Speed = r.ReadSingle();
                    Reach = r.ReadSingle();
                    Damage = r.ReadInt16();
                    ChopMin = r.ReadByte();
                    ChopMax = r.ReadByte();
                    SlashMin = r.ReadByte();
                    SlashMax = r.ReadByte();
                    ThrustMin = r.ReadByte();
                    ThrustMax = r.ReadByte();
                    Flags = r.ReadInt32();
                    return;
                }
                Type = (ushort)r.ReadUInt32();
                Speed = r.ReadSingle();
                Reach = r.ReadSingle();
                Flags = r.ReadInt32();
                Value = r.ReadInt32();
                Health = (short)r.ReadInt32();
                Weight = r.ReadSingle();
                Damage = r.ReadInt16();
                ChopMin = ChopMax = SlashMin = SlashMax = ThrustMin = ThrustMax = 0;
            }
        }

        public override string ToString() => $"WEAP: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // Model
        public STRVField FULL; // Item Name
        public DATAField DATA; // Weapon Data
        public FILEField ICON; // Male Icon (optional)
        public FMIDField<ENCHRecord> ENAM; // Enchantment ID
        public FMIDField<SCPTRecord> SCRI; // Script (optional)
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
                case "WPDT": DATA = new DATAField(r, dataSize, format); return true;
                case "ICON":
                case "ITEX": ICON = r.ReadFILE(dataSize); return true;
                case "ENAM": ENAM = new FMIDField<ENCHRecord>(r, dataSize); return true;
                case "SCRI": SCRI = new FMIDField<SCPTRecord>(r, dataSize); return true;
                case "ANAM": ANAM = r.ReadS2<IN16Field>(dataSize); return true;
                default: return false;
            }
        }
    }
}