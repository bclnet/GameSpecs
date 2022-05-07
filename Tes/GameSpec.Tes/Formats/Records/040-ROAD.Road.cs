using System.IO;

namespace GameSpec.Tes.Formats.Records
{
    public class ROADRecord : Record
    {
        public override string ToString() => $"ROAD:";
        public PGRDRecord.PGRPField[] PGRPs { get; set; }
        public UNKNField PGRR { get; set; }

        public override bool CreateField(BinaryReader r, TesFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "PGRP":
                    PGRPs = new PGRDRecord.PGRPField[dataSize >> 4];
                    for (var i = 0; i < PGRPs.Length; i++) PGRPs[i] = new PGRDRecord.PGRPField(r, dataSize); return true;
                case "PGRR": PGRR = r.ReadUNKN(dataSize); return true;
                default: return false;
            }
        }
    }
}