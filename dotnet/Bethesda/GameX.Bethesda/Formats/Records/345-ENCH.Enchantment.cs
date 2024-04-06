using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.IO.Polyfill;

namespace GameX.Bethesda.Formats.Records
{
    public class ENCHRecord : Record
    {
        // TESX
        public struct ENITField
        {
            // TES3: 0 = Cast Once, 1 = Cast Strikes, 2 = Cast when Used, 3 = Constant Effect
            // TES4: 0 = Scroll, 1 = Staff, 2 = Weapon, 3 = Apparel
            public int Type;
            public int EnchantCost;
            public int ChargeAmount; //: Charge
            public int Flags; //: AutoCalc

            public ENITField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                Type = r.ReadInt32();
                if (format == BethesdaFormat.TES3)
                {
                    EnchantCost = r.ReadInt32();
                    ChargeAmount = r.ReadInt32();
                }
                else
                {
                    ChargeAmount = r.ReadInt32();
                    EnchantCost = r.ReadInt32();
                }
                Flags = r.ReadInt32();
            }
        }

        public class EFITField
        {
            public string EffectId;
            public int Type; //:RangeType - 0 = Self, 1 = Touch, 2 = Target
            public int Area;
            public int Duration;
            public int MagnitudeMin;
            // TES3
            public byte SkillId; // (-1 if NA)
            public byte AttributeId; // (-1 if NA)
            public int MagnitudeMax;
            // TES4
            public int ActorValue;

            public EFITField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                if (format == BethesdaFormat.TES3)
                {
                    EffectId = r.ReadFString(2);
                    SkillId = r.ReadByte();
                    AttributeId = r.ReadByte();
                    Type = r.ReadInt32();
                    Area = r.ReadInt32();
                    Duration = r.ReadInt32();
                    MagnitudeMin = r.ReadInt32();
                    MagnitudeMax = r.ReadInt32();
                    return;
                }
                EffectId = r.ReadFString(4);
                MagnitudeMin = r.ReadInt32();
                Area = r.ReadInt32();
                Duration = r.ReadInt32();
                Type = r.ReadInt32();
                ActorValue = r.ReadInt32();
            }
        }

        // TES4
        public class SCITField
        {
            public string Name;
            public int ScriptFormId;
            public int School; // 0 = Alteration, 1 = Conjuration, 2 = Destruction, 3 = Illusion, 4 = Mysticism, 5 = Restoration
            public string VisualEffect;
            public uint Flags;

            public SCITField(BinaryReader r, int dataSize)
            {
                Name = "Script Effect";
                ScriptFormId = r.ReadInt32();
                if (dataSize == 4)
                    return;
                School = r.ReadInt32();
                VisualEffect = r.ReadFString(4);
                Flags = dataSize > 12 ? r.ReadUInt32() : 0;
            }
            public void FULLField(BinaryReader r, int dataSize) => Name = r.ReadYEncoding(dataSize);
        }

        public override string ToString() => $"ENCH: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public STRVField FULL; // Enchant name
        public ENITField ENIT; // Enchant Data
        public List<EFITField> EFITs = new List<EFITField>(); // Effect Data
        // TES4
        public List<SCITField> SCITs = new List<SCITField>(); // Script effect data

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID":
                case "NAME": EDID = r.ReadSTRV(dataSize); return true;
                case "FULL": if (SCITs.Count == 0) FULL = r.ReadSTRV(dataSize); else SCITs.Last().FULLField(r, dataSize); return true;
                case "ENIT":
                case "ENDT": ENIT = new ENITField(r, dataSize, format); return true;
                case "EFID": r.Skip(dataSize); return true;
                case "EFIT":
                case "ENAM": EFITs.Add(new EFITField(r, dataSize, format)); return true;
                case "SCIT": SCITs.Add(new SCITField(r, dataSize)); return true;
                default: return false;
            }
        }
    }
}
