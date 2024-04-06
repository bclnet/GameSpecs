using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class ANIORecord : Record, IHaveEDID, IHaveMODL
    {
        public override string ToString() => $"ANIO: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // Model
        public FMIDField<IDLERecord> DATA; // IDLE animation

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "MODL": MODL = new MODLGroup(r, dataSize); return true;
                case "MODB": MODL.MODBField(r, dataSize); return true;
                case "DATA": DATA = new FMIDField<IDLERecord>(r, dataSize); return true;
                default: return false;
            }
        }
    }
}