using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using static System.IO.Polyfill;

namespace GameX.Bethesda.Formats.Records
{
    public class REGNRecord : Record, IHaveEDID
    {
        // TESX
        public class RDATField
        {
            public enum REGNType : byte
            {
                Objects = 2, Weather, Map, Landscape, Grass, Sound
            }

            public uint Type;
            public REGNType Flags;
            public byte Priority;
            // groups
            public RDOTField[] RDOTs; // Objects
            public STRVField RDMP; // MapName
            public RDGSField[] RDGSs; // Grasses
            public UI32Field RDMD; // Music Type
            public RDSDField[] RDSDs; // Sounds
            public RDWTField[] RDWTs; // Weather Types

            public RDATField() { }
            public RDATField(BinaryReader r, int dataSize)
            {
                Type = r.ReadUInt32();
                Flags = (REGNType)r.ReadByte();
                Priority = r.ReadByte();
                r.Skip(2); // Unused
            }
        }

        public struct RDOTField
        {
            public override string ToString() => $"{Object}";
            public FormId<Record> Object;
            public ushort ParentIdx;
            public float Density;
            public byte Clustering;
            public byte MinSlope; // (degrees)
            public byte MaxSlope; // (degrees)
            public byte Flags;
            public ushort RadiusWrtParent;
            public ushort Radius;
            public float MinHeight;
            public float MaxHeight;
            public float Sink;
            public float SinkVariance;
            public float SizeVariance;
            public Int3 AngleVariance;
            public ColorRef4 VertexShading; // RGB + Shading radius (0 - 200) %

            public RDOTField(BinaryReader r, int dataSize)
            {
                Object = new FormId<Record>(r.ReadUInt32());
                ParentIdx = r.ReadUInt16();
                r.Skip(2); // Unused
                Density = r.ReadSingle();
                Clustering = r.ReadByte();
                MinSlope = r.ReadByte();
                MaxSlope = r.ReadByte();
                Flags = r.ReadByte();
                RadiusWrtParent = r.ReadUInt16();
                Radius = r.ReadUInt16();
                MinHeight = r.ReadSingle();
                MaxHeight = r.ReadSingle();
                Sink = r.ReadSingle();
                SinkVariance = r.ReadSingle();
                SizeVariance = r.ReadSingle();
                AngleVariance = new Int3(r.ReadUInt16(), r.ReadUInt16(), r.ReadUInt16());
                r.Skip(2); // Unused
                VertexShading = r.ReadS2<ColorRef4>(dataSize);
            }
        }

        public struct RDGSField
        {
            public override string ToString() => $"{Grass}";
            public FormId<GRASRecord> Grass;

            public RDGSField(BinaryReader r, int dataSize)
            {
                Grass = new FormId<GRASRecord>(r.ReadUInt32());
                r.Skip(4); // Unused
            }
        }

        public struct RDSDField
        {
            public override string ToString() => $"{Sound}";
            public FormId<SOUNRecord> Sound;
            public uint Flags;
            public uint Chance;

            public RDSDField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                if (format == BethesdaFormat.TES3)
                {
                    Sound = new FormId<SOUNRecord>(r.ReadZString(32));
                    Flags = 0;
                    Chance = r.ReadByte();
                    return;
                }
                Sound = new FormId<SOUNRecord>(r.ReadUInt32());
                Flags = r.ReadUInt32();
                Chance = r.ReadUInt32(); //: float with TES5
            }
        }

        public struct RDWTField
        {
            public override string ToString() => $"{Weather}";
            public static byte SizeOf(BethesdaFormat format) => format == BethesdaFormat.TES4 ? (byte)8 : (byte)12;
            public FormId<WTHRRecord> Weather;
            public uint Chance;
            public FormId<GLOBRecord> Global;

            public RDWTField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                Weather = new FormId<WTHRRecord>(r.ReadUInt32());
                Chance = r.ReadUInt32();
                Global = format == BethesdaFormat.TES5 ? new FormId<GLOBRecord>(r.ReadUInt32()) : new FormId<GLOBRecord>();
            }
        }

        // TES3
        public struct WEATField
        {
            public byte Clear;
            public byte Cloudy;
            public byte Foggy;
            public byte Overcast;
            public byte Rain;
            public byte Thunder;
            public byte Ash;
            public byte Blight;

            public WEATField(BinaryReader r, int dataSize)
            {
                Clear = r.ReadByte();
                Cloudy = r.ReadByte();
                Foggy = r.ReadByte();
                Overcast = r.ReadByte();
                Rain = r.ReadByte();
                Thunder = r.ReadByte();
                Ash = r.ReadByte();
                Blight = r.ReadByte();
                // v1.3 ESM files add 2 bytes to WEAT subrecords.
                if (dataSize == 10)
                    r.Skip(2);
            }
        }

        // TES4
        public class RPLIField
        {
            public uint EdgeFalloff; // (World Units)
            public Vector2[] Points; // Region Point List Data

            public RPLIField(BinaryReader r, int dataSize) => EdgeFalloff = r.ReadUInt32();
            public void RPLDField(BinaryReader r, int dataSize)
            {
                Points = new Vector2[dataSize >> 3];
                for (var i = 0; i < Points.Length; i++) Points[i] = new Vector2(r.ReadSingle(), r.ReadSingle());
            }
        }

        public override string ToString() => $"REGN: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public STRVField ICON; // Icon / Sleep creature
        public FMIDField<WRLDRecord> WNAM; // Worldspace - Region name
        public CREFField RCLR; // Map Color (COLORREF)
        public List<RDATField> RDATs = new List<RDATField>(); // Region Data Entries / TES3: Sound Record (order determines the sound priority)
        // TES3
        public WEATField? WEAT; // Weather Data
        // TES4
        public List<RPLIField> RPLIs = new List<RPLIField>(); // Region Areas

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID":
                case "NAME": EDID = r.ReadSTRV(dataSize); return true;
                case "WNAM":
                case "FNAM": WNAM = new FMIDField<WRLDRecord>(r, dataSize); return true;
                case "WEAT": WEAT = new WEATField(r, dataSize); return true; //: TES3
                case "ICON":
                case "BNAM": ICON = r.ReadSTRV(dataSize); return true;
                case "RCLR":
                case "CNAM": RCLR = r.ReadS2<CREFField>(dataSize); return true;
                case "SNAM": RDATs.Add(new RDATField { RDSDs = new[] { new RDSDField(r, dataSize, format) } }); return true;
                case "RPLI": RPLIs.Add(new RPLIField(r, dataSize)); return true;
                case "RPLD": RPLIs.Last().RPLDField(r, dataSize); return true;
                case "RDAT": RDATs.Add(new RDATField(r, dataSize)); return true;
                case "RDOT":
                    var rdot = RDATs.Last().RDOTs = new RDOTField[dataSize / 52];
                    for (var i = 0; i < rdot.Length; i++) rdot[i] = new RDOTField(r, dataSize); return true;
                case "RDMP": RDATs.Last().RDMP = r.ReadSTRV(dataSize); return true;
                case "RDGS":
                    var rdgs = RDATs.Last().RDGSs = new RDGSField[dataSize / 8];
                    for (var i = 0; i < rdgs.Length; i++) rdgs[i] = new RDGSField(r, dataSize); return true;
                case "RDMD": RDATs.Last().RDMD = r.ReadS2<UI32Field>(dataSize); return true;
                case "RDSD":
                    var rdsd = RDATs.Last().RDSDs = new RDSDField[dataSize / 12];
                    for (var i = 0; i < rdsd.Length; i++) rdsd[i] = new RDSDField(r, dataSize, format); return true;
                case "RDWT":
                    var rdwt = RDATs.Last().RDWTs = new RDWTField[dataSize / RDWTField.SizeOf(format)];
                    for (var i = 0; i < rdwt.Length; i++) rdwt[i] = new RDWTField(r, dataSize, format); return true;
                default: return false;
            }
        }
    }
}