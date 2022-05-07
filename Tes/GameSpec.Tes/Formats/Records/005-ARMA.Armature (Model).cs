using System.IO;

namespace GameSpec.Tes.Formats.Records
{
    public class ARMARecord : Record
    {
        public override string ToString() => $"ARMA: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID

        public override bool CreateField(BinaryReader r, TesFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                default: return false;
            }
        }
    }
}