using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace GameX.Bethesda.Formats.Records
{
    public class PGRDRecord : Record
    {
        public struct DATAField
        {
            public int X;
            public int Y;
            public short Granularity;
            public short PointCount;

            public DATAField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                if (format != BethesdaFormat.TES3)
                {
                    X = Y = Granularity = 0;
                    PointCount = r.ReadInt16();
                    return;
                }
                X = r.ReadInt32();
                Y = r.ReadInt32();
                Granularity = r.ReadInt16();
                PointCount = r.ReadInt16();
            }
        }

        public struct PGRPField
        {
            public Vector3 Point;
            public byte Connections;

            public PGRPField(BinaryReader r, int dataSize)
            {
                Point = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                Connections = r.ReadByte();
                r.Skip(3); // Unused
            }
        }

        public struct PGRRField
        {
            public short StartPointId;
            public short EndPointId;

            public PGRRField(BinaryReader r, int dataSize)
            {
                StartPointId = r.ReadInt16();
                EndPointId = r.ReadInt16();
            }
        }

        public struct PGRIField
        {
            public short PointId;
            public Vector3 ForeignNode;

            public PGRIField(BinaryReader r, int dataSize)
            {
                PointId = r.ReadInt16();
                r.Skip(2); // Unused (can merge back)
                ForeignNode = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            }
        }

        public struct PGRLField
        {
            public FormId<REFRRecord> Reference;
            public short[] PointIds;

            public PGRLField(BinaryReader r, int dataSize)
            {
                Reference = new FormId<REFRRecord>(r.ReadUInt32());
                PointIds = new short[(dataSize - 4) >> 2];
                for (var i = 0; i < PointIds.Length; i++)
                {
                    PointIds[i] = r.ReadInt16();
                    r.Skip(2); // Unused (can merge back)
                }
            }
        }

        public override string ToString() => $"PGRD: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public DATAField DATA; // Number of nodes
        public PGRPField[] PGRPs;
        public UNKNField PGRC;
        public UNKNField PGAG;
        public PGRRField[] PGRRs; // Point-to-Point Connections
        public List<PGRLField> PGRLs; // Point-to-Reference Mappings
        public PGRIField[] PGRIs; // Inter-Cell Connections

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID":
                case "NAME": EDID = r.ReadSTRV(dataSize); return true;
                case "DATA": DATA = new DATAField(r, dataSize, format); return true;
                case "PGRP":
                    PGRPs = new PGRPField[dataSize >> 4];
                    for (var i = 0; i < PGRPs.Length; i++) PGRPs[i] = new PGRPField(r, 16); return true;
                case "PGRC": PGRC = r.ReadUNKN(dataSize); return true;
                case "PGAG": PGAG = r.ReadUNKN(dataSize); return true;
                case "PGRR":
                    PGRRs = new PGRRField[dataSize >> 2];
                    for (var i = 0; i < PGRRs.Length; i++) PGRRs[i] = new PGRRField(r, 4); r.Skip(dataSize % 4); return true;
                case "PGRL": if (PGRLs == null) PGRLs = new List<PGRLField>(); PGRLs.Add(new PGRLField(r, dataSize)); return true;
                case "PGRI":
                    PGRIs = new PGRIField[dataSize >> 4];
                    for (var i = 0; i < PGRIs.Length; i++) PGRIs[i] = new PGRIField(r, 16); return true;
                default: return false;
            }
        }
    }
}