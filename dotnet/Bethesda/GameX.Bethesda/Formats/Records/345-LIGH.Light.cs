using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class LIGHRecord : Record, IHaveEDID, IHaveMODL
    {
        // TESX
        public struct DATAField
        {
            public enum ColorFlags
            {
                Dynamic = 0x0001,
                CanCarry = 0x0002,
                Negative = 0x0004,
                Flicker = 0x0008,
                Fire = 0x0010,
                OffDefault = 0x0020,
                FlickerSlow = 0x0040,
                Pulse = 0x0080,
                PulseSlow = 0x0100
            }

            public float Weight;
            public int Value;
            public int Time;
            public int Radius;
            public ColorRef4 LightColor;
            public int Flags;
            // TES4
            public float FalloffExponent;
            public float FOV;

            public DATAField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                if (format == BethesdaFormat.TES3)
                {
                    Weight = r.ReadSingle();
                    Value = r.ReadInt32();
                    Time = r.ReadInt32();
                    Radius = r.ReadInt32();
                    LightColor = r.ReadS2<ColorRef4>(4);
                    Flags = r.ReadInt32();
                    FalloffExponent = 1;
                    FOV = 90;
                    return;
                }
                Time = r.ReadInt32();
                Radius = r.ReadInt32();
                LightColor = r.ReadS2<ColorRef4>(4);
                Flags = r.ReadInt32();
                if (dataSize == 32) { FalloffExponent = r.ReadSingle(); FOV = r.ReadSingle(); }
                else { FalloffExponent = 1; FOV = 90; }
                Value = r.ReadInt32();
                Weight = r.ReadSingle();
            }
        }

        public override string ToString() => $"LIGH: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // Model
        public STRVField? FULL; // Item Name (optional)
        public DATAField DATA; // Light Data
        public STRVField? SCPT; // Script Name (optional)??
        public FMIDField<SCPTRecord>? SCRI; // Script FormId (optional)
        public FILEField? ICON; // Male Icon (optional)
        public FLTVField FNAM; // Fade Value
        public FMIDField<SOUNRecord> SNAM; // Sound FormId (optional)

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID":
                case "NAME": EDID = r.ReadSTRV(dataSize); return true;
                case "FULL": FULL = r.ReadSTRV(dataSize); return true;
                case "FNAM": if (format != BethesdaFormat.TES3) FNAM = r.ReadS2<FLTVField>(dataSize); else FULL = r.ReadSTRV(dataSize); return true;
                case "DATA":
                case "LHDT": DATA = new DATAField(r, dataSize, format); return true;
                case "SCPT": SCPT = r.ReadSTRV(dataSize); return true;
                case "SCRI": SCRI = new FMIDField<SCPTRecord>(r, dataSize); return true;
                case "ICON":
                case "ITEX": ICON = r.ReadFILE(dataSize); return true;
                case "MODL": MODL = new MODLGroup(r, dataSize); return true;
                case "MODB": MODL.MODBField(r, dataSize); return true;
                case "MODT": MODL.MODTField(r, dataSize); return true;
                case "SNAM": SNAM = new FMIDField<SOUNRecord>(r, dataSize); return true;
                default: return false;
            }
        }
    }
}