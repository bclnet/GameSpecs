using System;
using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class SOUNRecord : Record, IHaveEDID
    {
        [Flags]
        public enum SOUNFlags : ushort
        {
            RandomFrequencyShift = 0x0001,
            PlayAtRandom = 0x0002,
            EnvironmentIgnored = 0x0004,
            RandomLocation = 0x0008,
            Loop = 0x0010,
            MenuSound = 0x0020,
            _2D = 0x0040,
            _360LFE = 0x0080,
        }

        // TESX
        public class DATAField
        {
            public byte Volume; // (0=0.00, 255=1.00)
            public byte MinRange; // Minimum attenuation distance
            public byte MaxRange; // Maximum attenuation distance
            // Bethesda4
            public sbyte FrequencyAdjustment; // Frequency adjustment %
            public ushort Flags; // Flags
            public ushort StaticAttenuation; // Static Attenuation (db)
            public byte StopTime; // Stop time
            public byte StartTime; // Start time

            public DATAField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                Volume = format == BethesdaFormat.TES3 ? r.ReadByte() : (byte)0;
                MinRange = r.ReadByte();
                MaxRange = r.ReadByte();
                if (format == BethesdaFormat.TES3)
                    return;
                FrequencyAdjustment = r.ReadSByte();
                r.ReadByte(); // Unused
                Flags = r.ReadUInt16();
                r.ReadUInt16(); // Unused
                if (dataSize == 8)
                    return;
                StaticAttenuation = r.ReadUInt16();
                StopTime = r.ReadByte();
                StartTime = r.ReadByte();
            }
        }

        public override string ToString() => $"SOUN: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public FILEField FNAM; // Sound Filename (relative to Sounds\)
        public DATAField DATA; // Sound Data

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID":
                case "NAME": EDID = r.ReadSTRV(dataSize); return true;
                case "FNAM": FNAM = r.ReadFILE(dataSize); return true;
                case "SNDX": DATA = new DATAField(r, dataSize, format); return true;
                case "SNDD": DATA = new DATAField(r, dataSize, format); return true;
                case "DATA": DATA = new DATAField(r, dataSize, format); return true;
                default: return false;
            }
        }
    }
}