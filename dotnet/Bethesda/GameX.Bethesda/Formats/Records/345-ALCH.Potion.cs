using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.Bethesda.Formats.Records
{
    public class ALCHRecord : Record, IHaveEDID, IHaveMODL
    {
        // TESX
        public class DATAField
        {
            public float Weight;
            public int Value;
            public int Flags; //: AutoCalc

            public DATAField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                Weight = r.ReadSingle();
                if (format == BethesdaFormat.TES3)
                {
                    Value = r.ReadInt32();
                    Flags = r.ReadInt32();
                }
            }

            public void ENITField(BinaryReader r, int dataSize)
            {
                Value = r.ReadInt32();
                Flags = r.ReadByte();
                r.Skip(3); // Unknown
            }
        }

        // TES3
        public struct ENAMField
        {
            public short EffectId;
            public byte SkillId; // for skill related effects, -1/0 otherwise
            public byte AttributeId; // for attribute related effects, -1/0 otherwise
            public int Unknown1;
            public int Unknown2;
            public int Duration;
            public int Magnitude;
            public int Unknown4;

            public ENAMField(BinaryReader r, int dataSize)
            {
                EffectId = r.ReadInt16();
                SkillId = r.ReadByte();
                AttributeId = r.ReadByte();
                Unknown1 = r.ReadInt32();
                Unknown2 = r.ReadInt32();
                Duration = r.ReadInt32();
                Magnitude = r.ReadInt32();
                Unknown4 = r.ReadInt32();
            }
        }

        public override string ToString() => $"ALCH: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // Model
        public STRVField FULL; // Item Name
        public DATAField DATA; // Alchemy Data
        public ENAMField? ENAM; // Enchantment
        public FILEField ICON; // Icon
        public FMIDField<SCPTRecord>? SCRI; // Script (optional)
        // TES4
        public List<ENCHRecord.EFITField> EFITs = new List<ENCHRecord.EFITField>(); // Effect Data
        public List<ENCHRecord.SCITField> SCITs = new List<ENCHRecord.SCITField>(); // Script Effect Data

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID":
                case "NAME": EDID = r.ReadSTRV(dataSize); return true;
                case "MODL": MODL = new MODLGroup(r, dataSize); return true;
                case "MODB": MODL.MODBField(r, dataSize); return true;
                case "MODT": MODL.MODTField(r, dataSize); return true;
                case "FULL": if (SCITs.Count == 0) FULL = r.ReadSTRV(dataSize); else SCITs.Last().FULLField(r, dataSize); return true;
                case "FNAM": FULL = r.ReadSTRV(dataSize); return true;
                case "DATA":
                case "ALDT": DATA = new DATAField(r, dataSize, format); return true;
                case "ENAM": ENAM = new ENAMField(r, dataSize); return true;
                case "ICON":
                case "TEXT": ICON = r.ReadFILE(dataSize); return true;
                case "SCRI": SCRI = new FMIDField<SCPTRecord>(r, dataSize); return true;
                //
                case "ENIT": DATA.ENITField(r, dataSize); return true;
                case "EFID": r.Skip(dataSize); return true;
                case "EFIT": EFITs.Add(new ENCHRecord.EFITField(r, dataSize, format)); return true;
                case "SCIT": SCITs.Add(new ENCHRecord.SCITField(r, dataSize)); return true;
                default: return false;
            }
        }
    }
}