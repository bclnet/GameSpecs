using System.Collections.Generic;
using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class CLMTRecord : Record, IHaveEDID, IHaveMODL
    {
        public struct WLSTField
        {
            public FormId<WTHRRecord> Weather;
            public int Chance;

            public WLSTField(BinaryReader r, int dataSize)
            {
                Weather = new FormId<WTHRRecord>(r.ReadUInt32());
                Chance = r.ReadInt32();
            }
        }

        public struct TNAMField
        {
            public byte Sunrise_Begin;
            public byte Sunrise_End;
            public byte Sunset_Begin;
            public byte Sunset_End;
            public byte Volatility;
            public byte MoonsPhaseLength;

            public TNAMField(BinaryReader r, int dataSize)
            {
                Sunrise_Begin = r.ReadByte();
                Sunrise_End = r.ReadByte();
                Sunset_Begin = r.ReadByte();
                Sunset_End = r.ReadByte();
                Volatility = r.ReadByte();
                MoonsPhaseLength = r.ReadByte();
            }
        }

        public override string ToString() => $"CLMT: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // Model
        public FILEField FNAM; // Sun Texture
        public FILEField GNAM; // Sun Glare Texture
        public List<WLSTField> WLSTs = new List<WLSTField>(); // Climate
        public TNAMField TNAM; // Timing

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "MODL": MODL = new MODLGroup(r, dataSize); return true;
                case "MODB": MODL.MODBField(r, dataSize); return true;
                case "FNAM": FNAM = r.ReadFILE(dataSize); return true;
                case "GNAM": GNAM = r.ReadFILE(dataSize); return true;
                case "WLST": for (var i = 0; i < dataSize >> 3; i++) WLSTs.Add(new WLSTField(r, dataSize)); return true;
                case "TNAM": TNAM = new TNAMField(r, dataSize); return true;
                default: return false;
            }
        }
    }
}