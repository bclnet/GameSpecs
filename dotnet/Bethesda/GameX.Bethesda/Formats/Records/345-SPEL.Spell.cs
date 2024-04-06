using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.Bethesda.Formats.Records
{
    public class SPELRecord : Record
    {
        // TESX
        public struct SPITField
        {
            public override string ToString() => $"{Type}";
            // TES3: 0 = Spell, 1 = Ability, 2 = Blight, 3 = Disease, 4 = Curse, 5 = Power
            // TES4: 0 = Spell, 1 = Disease, 2 = Power, 3 = Lesser Power, 4 = Ability, 5 = Poison
            public uint Type; 
            public int SpellCost;
            public uint Flags; // 0x0001 = AutoCalc, 0x0002 = PC Start, 0x0004 = Always Succeeds
            // TES4
            public int SpellLevel;

            public SPITField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                Type = r.ReadUInt32();
                SpellCost = r.ReadInt32();
                SpellLevel = format != BethesdaFormat.TES3 ? r.ReadInt32() : 0;
                Flags = r.ReadUInt32();
            }
        }

        public override string ToString() => $"SPEL: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public STRVField FULL; // Spell name
        public SPITField SPIT; // Spell data
        public List<ENCHRecord.EFITField> EFITs = new List<ENCHRecord.EFITField>(); // Effect Data
        // TES4
        public List<ENCHRecord.SCITField> SCITs = new List<ENCHRecord.SCITField>(); // Script effect data

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID":
                case "NAME": EDID = r.ReadSTRV(dataSize); return true;
                case "FULL": if (SCITs.Count == 0) FULL = r.ReadSTRV(dataSize); else SCITs.Last().FULLField(r, dataSize); return true;
                case "FNAM": FULL = r.ReadSTRV(dataSize); return true;
                case "SPIT":
                case "SPDT": SPIT = new SPITField(r, dataSize, format); return true;
                case "EFID": r.Skip(dataSize); return true;
                case "EFIT":
                case "ENAM": EFITs.Add(new ENCHRecord.EFITField(r, dataSize, format)); return true;
                case "SCIT": SCITs.Add(new ENCHRecord.SCITField(r, dataSize)); return true;
                default: return false;
            }
        }
    }
}