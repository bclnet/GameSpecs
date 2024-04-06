using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class SNDGRecord : Record
    {
        public enum SNDGType : uint
        {
            LeftFoot = 0,
            RightFoot = 1,
            SwimLeft = 2,
            SwimRight = 3,
            Moan = 4,
            Roar = 5,
            Scream = 6,
            Land = 7,
        }

        public override string ToString() => $"SNDG: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public IN32Field DATA; // Sound Type Data
        public STRVField SNAM; // Sound ID
        public STRVField? CNAM; // Creature name (optional)

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            if (format == BethesdaFormat.TES3)
                switch (type)
                {
                    case "NAME": EDID = r.ReadSTRV(dataSize); return true;
                    case "DATA": DATA = r.ReadS2<IN32Field>(dataSize); return true;
                    case "SNAM": SNAM = r.ReadSTRV(dataSize); return true;
                    case "CNAM": CNAM = r.ReadSTRV(dataSize); return true;
                    default: return false;
                }
            return false;
        }
    }
}