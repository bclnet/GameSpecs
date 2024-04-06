using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.Bethesda.Formats.Records
{
    public class SGSTRecord : Record, IHaveEDID, IHaveMODL
    {
        public struct DATAField
        {
            public byte Uses;
            public int Value;
            public float Weight;

            public DATAField(BinaryReader r, int dataSize)
            {
                Uses = r.ReadByte();
                Value = r.ReadInt32();
                Weight = r.ReadSingle();
            }
        }

        public override string ToString() => $"SGST: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // Model
        public STRVField FULL; // Item Name
        public DATAField DATA; // Sigil Stone Data
        public FILEField ICON; // Icon
        public FMIDField<SCPTRecord>? SCRI; // Script (optional)
        public List<ENCHRecord.EFITField> EFITs = new List<ENCHRecord.EFITField>(); // Effect Data
        public List<ENCHRecord.SCITField> SCITs = new List<ENCHRecord.SCITField>(); // Script Effect Data

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "MODL": MODL = new MODLGroup(r, dataSize); return true;
                case "MODB": MODL.MODBField(r, dataSize); return true;
                case "MODT": MODL.MODTField(r, dataSize); return true;
                case "FULL": if (SCITs.Count == 0) FULL = r.ReadSTRV(dataSize); else SCITs.Last().FULLField(r, dataSize); return true;
                case "DATA": DATA = new DATAField(r, dataSize); return true;
                case "ICON": ICON = r.ReadFILE(dataSize); return true;
                case "SCRI": SCRI = new FMIDField<SCPTRecord>(r, dataSize); return true;
                case "EFID": r.Skip(dataSize); return true;
                case "EFIT": EFITs.Add(new ENCHRecord.EFITField(r, dataSize, format)); return true;
                case "SCIT": SCITs.Add(new ENCHRecord.SCITField(r, dataSize)); return true;
                default: return false;
            }
        }
    }
}