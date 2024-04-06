using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class EYESRecord : Record
    {
        public override string ToString() => $"EYES: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public STRVField FULL;
        public FILEField ICON;
        public BYTEField DATA; // Playable

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "FULL": FULL = r.ReadSTRV(dataSize); return true;
                case "ICON": ICON = r.ReadFILE(dataSize); return true;
                case "DATA": DATA = r.ReadS2<BYTEField>(dataSize); return true;
                default: return false;
            }
        }
    }
}