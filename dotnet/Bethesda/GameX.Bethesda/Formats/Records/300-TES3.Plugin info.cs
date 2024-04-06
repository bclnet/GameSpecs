using System.Collections.Generic;
using System.IO;
using static System.IO.Polyfill;

namespace GameX.Bethesda.Formats.Records
{
    public class TES3Record : Record
    {
        public struct HEDRField
        {
            public float Version;
            public uint FileType;
            public string CompanyName;
            public string FileDescription;
            public uint NumRecords;

            public HEDRField(BinaryReader r, int dataSize)
            {
                Version = r.ReadSingle();
                FileType = r.ReadUInt32();
                CompanyName = r.ReadZString(32);
                FileDescription = r.ReadZString(256);
                NumRecords = r.ReadUInt32();
            }
        }

        public HEDRField HEDR;
        public List<STRVField> MASTs;
        public List<INTVField> DATAs;

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "HEDR": HEDR = new HEDRField(r, dataSize); return true;
                case "MAST": if (MASTs == null) MASTs = new List<STRVField>(); MASTs.Add(r.ReadSTRV(dataSize)); return true;
                case "DATA": if (DATAs == null) DATAs = new List<INTVField>(); DATAs.Add(r.ReadINTV(dataSize)); return true;
                default: return false;
            }
        }
    }
}