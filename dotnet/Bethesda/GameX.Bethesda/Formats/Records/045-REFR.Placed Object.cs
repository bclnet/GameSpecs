using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace GameX.Bethesda.Formats.Records
{
    public class REFRRecord : Record
    {
        public struct XTELField
        {
            public FormId<REFRRecord> Door;
            public Vector3 Position;
            public Vector3 Rotation;

            public XTELField(BinaryReader r, int dataSize)
            {
                Door = new FormId<REFRRecord>(r.ReadUInt32());
                Position = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                Rotation = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            }
        }

        public struct DATAField
        {
            public Vector3 Position;
            public Vector3 Rotation;

            public DATAField(BinaryReader r, int dataSize)
            {
                Position = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                Rotation = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
            }
        }

        public struct XLOCField
        {
            public override string ToString() => $"{Key}";
            public byte LockLevel;
            public FormId<KEYMRecord> Key;
            public byte Flags;

            public XLOCField(BinaryReader r, int dataSize)
            {
                LockLevel = r.ReadByte();
                r.Skip(3); // Unused
                Key = new FormId<KEYMRecord>(r.ReadUInt32());
                if (dataSize == 16)
                    r.Skip(4); // Unused
                Flags = r.ReadByte();
                r.Skip(3); // Unused
            }
        }

        public struct XESPField
        {
            public override string ToString() => $"{Reference}";
            public FormId<Record> Reference;
            public byte Flags;

            public XESPField(BinaryReader r, int dataSize)
            {
                Reference = new FormId<Record>(r.ReadUInt32());
                Flags = r.ReadByte();
                r.Skip(3); // Unused
            }
        }

        public struct XSEDField
        {
            public override string ToString() => $"{Seed}";
            public byte Seed;

            public XSEDField(BinaryReader r, int dataSize)
            {
                Seed = r.ReadByte();
                if (dataSize == 4)
                    r.Skip(3); // Unused
            }
        }

        public class XMRKGroup
        {
            public override string ToString() => $"{FULL.Value}";
            public BYTEField FNAM; // Map Flags
            public STRVField FULL; // Name
            public BYTEField TNAM; // Type
        }

        public override string ToString() => $"REFR: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public FMIDField<Record> NAME; // Base
        public XTELField? XTEL; // Teleport Destination (optional)
        public DATAField DATA; // Position/Rotation
        public XLOCField? XLOC; // Lock information (optional)
        public List<CELLRecord.XOWNGroup> XOWNs; // Ownership (optional)
        public XESPField? XESP; // Enable Parent (optional)
        public FMIDField<Record>? XTRG; // Target (optional)
        public XSEDField? XSED; // SpeedTree (optional)
        public BYTVField? XLOD; // Distant LOD Data (optional)
        public FLTVField? XCHG; // Charge (optional)
        public FLTVField? XHLT; // Health (optional)
        public FMIDField<CELLRecord>? XPCI; // Unused (optional)
        public IN32Field? XLCM; // Level Modifier (optional)
        public FMIDField<REFRRecord>? XRTM; // Unknown (optional)
        public UI32Field? XACT; // Action Flag (optional)
        public IN32Field? XCNT; // Count (optional)
        public List<XMRKGroup> XMRKs; // Ownership (optional)
        //public bool? ONAM; // Open by Default
        public BYTVField? XRGD; // Ragdoll Data (optional)
        public FLTVField? XSCL; // Scale (optional)
        public BYTEField? XSOL; // Contained Soul (optional)
        int _nextFull;

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "NAME": NAME = new FMIDField<Record>(r, dataSize); return true;
                case "XTEL": XTEL = new XTELField(r, dataSize); return true;
                case "DATA": DATA = new DATAField(r, dataSize); return true;
                case "XLOC": XLOC = new XLOCField(r, dataSize); return true;
                case "XOWN": if (XOWNs == null) XOWNs = new List<CELLRecord.XOWNGroup>(); XOWNs.Add(new CELLRecord.XOWNGroup { XOWN = new FMIDField<Record>(r, dataSize) }); return true;
                case "XRNK": XOWNs.Last().XRNK = r.ReadS2<IN32Field>(dataSize); return true;
                case "XGLB": XOWNs.Last().XGLB = new FMIDField<Record>(r, dataSize); return true;
                case "XESP": XESP = new XESPField(r, dataSize); return true;
                case "XTRG": XTRG = new FMIDField<Record>(r, dataSize); return true;
                case "XSED": XSED = new XSEDField(r, dataSize); return true;
                case "XLOD": XLOD = r.ReadBYTV(dataSize); return true;
                case "XCHG": XCHG = r.ReadS2<FLTVField>(dataSize); return true;
                case "XHLT": XCHG = r.ReadS2<FLTVField>(dataSize); return true;
                case "XPCI": XPCI = new FMIDField<CELLRecord>(r, dataSize); _nextFull = 1; return true;
                case "FULL":
                    if (_nextFull == 1) XPCI.Value.AddName(r.ReadFString(dataSize));
                    else if (_nextFull == 2) XMRKs.Last().FULL = r.ReadSTRV(dataSize);
                    _nextFull = 0;
                    return true;
                case "XLCM": XLCM = r.ReadS2<IN32Field>(dataSize); return true;
                case "XRTM": XRTM = new FMIDField<REFRRecord>(r, dataSize); return true;
                case "XACT": XACT = r.ReadS2<UI32Field>(dataSize); return true;
                case "XCNT": XCNT = r.ReadS2<IN32Field>(dataSize); return true;
                case "XMRK": if (XMRKs == null) XMRKs = new List<XMRKGroup>(); XMRKs.Add(new XMRKGroup()); _nextFull = 2; return true;
                case "FNAM": XMRKs.Last().FNAM = r.ReadS2<BYTEField>(dataSize); return true;
                case "TNAM": XMRKs.Last().TNAM = r.ReadS2<BYTEField>(dataSize); r.ReadByte(); return true;
                case "ONAM": return true;
                case "XRGD": XRGD = r.ReadBYTV(dataSize); return true;
                case "XSCL": XSCL = r.ReadS2<FLTVField>(dataSize); return true;
                case "XSOL": XSOL = r.ReadS2<BYTEField>(dataSize); return true;
                default: return false;
            }
        }
    }
}