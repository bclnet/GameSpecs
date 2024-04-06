using System.IO;
using System.Collections.Generic;

namespace GameX.Bethesda.Formats.Records
{
    public class PACKRecord : Record
    {
        public struct PKDTField
        {
            public ushort Flags;
            public byte Type;

            public PKDTField(BinaryReader r, int dataSize)
            {
                Flags = r.ReadUInt16();
                Type = r.ReadByte();
                r.Skip(dataSize - 3); // Unused
            }
        }

        public struct PLDTField
        {
            public int Type;
            public uint Target;
            public int Radius;

            public PLDTField(BinaryReader r, int dataSize)
            {
                Type = r.ReadInt32();
                Target = r.ReadUInt32();
                Radius = r.ReadInt32();
            }
        }

        public struct PSDTField
        {
            public byte Month;
            public byte DayOfWeek;
            public byte Date;
            public sbyte Time;
            public int Duration;

            public PSDTField(BinaryReader r, int dataSize)
            {
                Month = r.ReadByte();
                DayOfWeek = r.ReadByte();
                Date = r.ReadByte();
                Time = (sbyte)r.ReadByte();
                Duration = r.ReadInt32();
            }
        }

        public struct PTDTField
        {
            public int Type;
            public uint Target;
            public int Count;

            public PTDTField(BinaryReader r, int dataSize)
            {
                Type = r.ReadInt32();
                Target = r.ReadUInt32();
                Count = r.ReadInt32();
            }
        }

        public override string ToString() => $"PACK: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public PKDTField PKDT; // General
        public PLDTField PLDT; // Location
        public PSDTField PSDT; // Schedule
        public PTDTField PTDT; // Target
        public List<SCPTRecord.CTDAField> CTDAs = new List<SCPTRecord.CTDAField>(); // Conditions

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "PKDT": PKDT = new PKDTField(r, dataSize); return true;
                case "PLDT": PLDT = new PLDTField(r, dataSize); return true;
                case "PSDT": PSDT = new PSDTField(r, dataSize); return true;
                case "PTDT": PTDT = new PTDTField(r, dataSize); return true;
                case "CTDA":
                case "CTDT": CTDAs.Add(new SCPTRecord.CTDAField(r, dataSize, format)); return true;
                default: return false;
            }
        }
    }
}