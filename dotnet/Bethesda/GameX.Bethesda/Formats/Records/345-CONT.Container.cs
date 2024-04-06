using System.Collections.Generic;
using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class CONTRecord : Record, IHaveEDID, IHaveMODL
    {
        // TESX
        public class DATAField
        {
            public byte Flags; // flags 0x0001 = Organic, 0x0002 = Respawns, organic only, 0x0008 = Default, unknown
            public float Weight;

            public DATAField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                if (format == BethesdaFormat.TES3)
                {
                    Weight = r.ReadSingle();
                    return;
                }
                Flags = r.ReadByte();
                Weight = r.ReadSingle();
            }
            public void FLAGField(BinaryReader r, int dataSize) => Flags = (byte)r.ReadUInt32();
        }

        public override string ToString() => $"CONT: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // Model
        public STRVField FULL; // Container Name
        public DATAField DATA; // Container Data
        public FMIDField<SCPTRecord>? SCRI;
        public List<CNTOField> CNTOs = new List<CNTOField>();
        // TES4
        public FMIDField<SOUNRecord> SNAM; // Open sound
        public FMIDField<SOUNRecord> QNAM; // Close sound

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
                case "CNDT": DATA = new DATAField(r, dataSize, format); return true;
                case "FLAG": DATA.FLAGField(r, dataSize); return true;
                case "CNTO":
                case "NPCO": CNTOs.Add(new CNTOField(r, dataSize, format)); return true;
                case "SCRI": SCRI = new FMIDField<SCPTRecord>(r, dataSize); return true;
                case "SNAM": SNAM = new FMIDField<SOUNRecord>(r, dataSize); return true;
                case "QNAM": QNAM = new FMIDField<SOUNRecord>(r, dataSize); return true;
                default: return false;
            }
        }
    }
}