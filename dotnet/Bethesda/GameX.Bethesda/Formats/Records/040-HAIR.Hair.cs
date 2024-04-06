using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class HAIRRecord : Record, IHaveEDID, IHaveMODL
    {
        public override string ToString() => $"HAIR: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public STRVField FULL;
        public MODLGroup MODL { get; set; }
        public FILEField ICON;
        public BYTEField DATA; // Playable, Not Male, Not Female, Fixed

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "FULL": FULL = r.ReadSTRV(dataSize); return true;
                case "MODL": MODL = new MODLGroup(r, dataSize); return true;
                case "MODB": MODL.MODBField(r, dataSize); return true;
                case "ICON": ICON = r.ReadFILE(dataSize); return true;
                case "DATA": DATA = r.ReadS2<BYTEField>(dataSize); return true;
                default: return false;
            }
        }
    }
}