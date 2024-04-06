using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static OpenStack.Debug;

namespace GameX.Bethesda.Formats.Records
{
    public unsafe class WRLDRecord : Record
    {
        public struct MNAMField
        {
            public static (string, int) Struct = ($"<2i4h", sizeof(MNAMField));
            public Int2 UsableDimensions;
            // Cell Coordinates
            public short NWCell_X;
            public short NWCell_Y;
            public short SECell_X;
            public short SECell_Y;
        }

        public struct NAM0Field
        {
            public static (string, int) Struct = ("<2f2f", sizeof(NAM0Field));
            public Vector2 Min;
            public Vector2 Max;

            public NAM0Field(BinaryReader r, int dataSize)
            {
                Min = new Vector2(r.ReadSingle(), r.ReadSingle());
                Max = Vector2.Zero;
            }
            public void NAM9Field(BinaryReader r, int dataSize) => Max = new Vector2(r.ReadSingle(), r.ReadSingle());
        }

        // TES5
        public struct RNAMField
        {
            public struct Reference
            {
                public FormId32<REFRRecord> Ref;
                public short X;
                public short Y;
            }
            public short GridX;
            public short GridY;
            public Reference[] GridReferences;

            public RNAMField(BinaryReader r, int dataSize)
            {
                GridX = r.ReadInt16();
                GridY = r.ReadInt16();
                var referenceCount = r.ReadUInt32();
                var referenceSize = dataSize - 8;
                Assert(referenceSize >> 3 == referenceCount);
                GridReferences = r.ReadTArray<Reference>(referenceSize, referenceSize >> 3);
            }
        }

        public override string ToString() => $"WRLD: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public STRVField FULL;
        public FMIDField<WRLDRecord>? WNAM; // Parent Worldspace
        public FMIDField<CLMTRecord>? CNAM; // Climate
        public FMIDField<WATRRecord>? NAM2; // Water
        public FILEField? ICON; // Icon
        public MNAMField? MNAM; // Map Data
        public BYTEField? DATA; // Flags
        public NAM0Field NAM0; // Object Bounds
        public UI32Field? SNAM; // Music
        // TES5
        public List<RNAMField> RNAMs = new List<RNAMField>(); // Large References

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "FULL": FULL = r.ReadSTRV(dataSize); return true;
                case "WNAM": WNAM = new FMIDField<WRLDRecord>(r, dataSize); return true;
                case "CNAM": CNAM = new FMIDField<CLMTRecord>(r, dataSize); return true;
                case "NAM2": NAM2 = new FMIDField<WATRRecord>(r, dataSize); return true;
                case "ICON": ICON = r.ReadFILE(dataSize); return true;
                case "MNAM": MNAM = r.ReadS2<MNAMField>(dataSize); return true;
                case "DATA": DATA = r.ReadS2<BYTEField>(dataSize); return true;
                case "NAM0": NAM0 = new NAM0Field(r, dataSize); return true;
                case "NAM9": NAM0.NAM9Field(r, dataSize); return true;
                case "SNAM": SNAM = r.ReadS2<UI32Field>(dataSize); return true;
                case "OFST": r.Skip(dataSize); return true;
                // TES5
                case "RNAM": RNAMs.Add(new RNAMField(r, dataSize)); return true;
                default: return false;
            }
        }
    }
}