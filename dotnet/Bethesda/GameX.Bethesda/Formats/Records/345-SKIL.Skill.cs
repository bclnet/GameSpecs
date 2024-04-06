using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class SKILRecord : Record
    {
        // TESX
        public struct DATAField
        {
            public int Action;
            public int Attribute;
            public uint Specialization; // 0 = Combat, 1 = Magic, 2 = Stealth
            public float[] UseValue; // The use types for each skill are hard-coded.

            public DATAField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                Action = format == BethesdaFormat.TES3 ? 0 : r.ReadInt32();
                Attribute = r.ReadInt32();
                Specialization = r.ReadUInt32();
                UseValue = new float[format == BethesdaFormat.TES3 ? 4 : 2];
                for (var i = 0; i < UseValue.Length; i++) UseValue[i] = r.ReadSingle();
            }
        }

        public override string ToString() => $"SKIL: {INDX.Value}:{EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public IN32Field INDX; // Skill ID
        public DATAField DATA; // Skill Data
        public STRVField DESC; // Skill description
        // TES4
        public FILEField ICON; // Icon
        public STRVField ANAM; // Apprentice Text
        public STRVField JNAM; // Journeyman Text
        public STRVField ENAM; // Expert Text
        public STRVField MNAM; // Master Text

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "INDX": INDX = r.ReadT<IN32Field>(dataSize); return true;
                case "DATA":
                case "SKDT": DATA = new DATAField(r, dataSize, format); return true;
                case "DESC": DESC = r.ReadSTRV(dataSize); return true;
                case "ICON": ICON = r.ReadFILE(dataSize); return true;
                case "ANAM": ANAM = r.ReadSTRV(dataSize); return true;
                case "JNAM": JNAM = r.ReadSTRV(dataSize); return true;
                case "ENAM": ENAM = r.ReadSTRV(dataSize); return true;
                case "MNAM": MNAM = r.ReadSTRV(dataSize); return true;
                default: return false;
            }
        }
    }
}