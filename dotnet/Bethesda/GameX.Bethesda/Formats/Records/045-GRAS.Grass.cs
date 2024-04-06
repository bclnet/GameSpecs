using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class GRASRecord : Record
    {
        public struct DATAField
        {
            public byte Density;
            public byte MinSlope;
            public byte MaxSlope;
            public ushort UnitFromWaterAmount;
            public uint UnitFromWaterType;
            //Above - At Least,
            //Above - At Most,
            //Below - At Least,
            //Below - At Most,
            //Either - At Least,
            //Either - At Most,
            //Either - At Most Above,
            //Either - At Most Below
            public float PositionRange;
            public float HeightRange;
            public float ColorRange;
            public float WavePeriod;
            public byte Flags;

            public DATAField(BinaryReader r, int dataSize)
            {
                Density = r.ReadByte();
                MinSlope = r.ReadByte();
                MaxSlope = r.ReadByte();
                r.ReadByte();
                UnitFromWaterAmount = r.ReadUInt16();
                r.Skip(2);
                UnitFromWaterType = r.ReadUInt32();
                PositionRange = r.ReadSingle();
                HeightRange = r.ReadSingle();
                ColorRange = r.ReadSingle();
                WavePeriod = r.ReadSingle();
                Flags = r.ReadByte();
                r.Skip(3);
            }
        }

        public override string ToString() => $"GRAS: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL;
        public DATAField DATA;

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "MODL": MODL = new MODLGroup(r, dataSize); return true;
                case "MODB": MODL.MODBField(r, dataSize); return true;
                case "MODT": MODL.MODTField(r, dataSize); return true;
                case "DATA": DATA = new DATAField(r, dataSize); return true;
                default: return false;
            }
        }
    }
}