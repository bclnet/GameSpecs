using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.IO.Polyfill;

namespace GameX.Bethesda.Formats.Records
{
    public class SCPTRecord : Record
    {
        // TESX
        public struct CTDAField
        {
            public enum INFOType : byte
            {
                Nothing = 0, Function, Global, Local, Journal, Item, Dead, NotId, NotFaction, NotClass, NotRace, NotCell, NotLocal
            }

            // TES3: 0 = [=], 1 = [!=], 2 = [>], 3 = [>=], 4 = [<], 5 = [<=]
            // TES4: 0 = [=], 2 = [!=], 4 = [>], 6 = [>=], 8 = [<], 10 = [<=]
            public byte CompareOp;
            // (00-71) - sX = Global/Local/Not Local types, JX = Journal type, IX = Item Type, DX = Dead Type, XX = Not ID Type, FX = Not Faction, CX = Not Class, RX = Not Race, LX = Not Cell
            public string FunctionId;
            // TES3
            public byte Index; // (0-5)
            public byte Type;
            // Except for the function type, this is the ID for the global/local/etc. Is not nessecarily NULL terminated.The function type SCVR sub-record has
            public string Name;
            // TES4
            public float ComparisonValue;
            public int Parameter1; // Parameter #1
            public int Parameter2; // Parameter #2

            public CTDAField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                if (format == BethesdaFormat.TES3)
                {
                    Index = r.ReadByte();
                    Type = r.ReadByte();
                    FunctionId = r.ReadFString(2);
                    CompareOp = (byte)(r.ReadByte() << 1);
                    Name = r.ReadFString(dataSize - 5);
                    ComparisonValue = Parameter1 = Parameter2 = 0;
                    return;
                }
                CompareOp = r.ReadByte();
                r.Skip(3); // Unused
                ComparisonValue = r.ReadSingle();
                FunctionId = r.ReadFString(4);
                Parameter1 = r.ReadInt32();
                Parameter2 = r.ReadInt32();
                if (dataSize != 24)
                    r.Skip(4); // Unused
                Index = Type = 0;
                Name = null;
            }
        }

        // TES3
        public class SCHDField
        {
            public override string ToString() => $"{Name}";
            public string Name;
            public int NumShorts;
            public int NumLongs;
            public int NumFloats;
            public int ScriptDataSize;
            public int LocalVarSize;
            public string[] Variables;

            public SCHDField(BinaryReader r, int dataSize)
            {
                Name = r.ReadZString(32);
                NumShorts = r.ReadInt32();
                NumLongs = r.ReadInt32();
                NumFloats = r.ReadInt32();
                ScriptDataSize = r.ReadInt32();
                LocalVarSize = r.ReadInt32();
                // SCVRField
                Variables = null;
            }
            public void SCVRField(BinaryReader r, int dataSize) => Variables = r.ReadZAStringList(dataSize).ToArray();
        }

        // TES4
        public struct SCHRField
        {
            public override string ToString() => $"{RefCount}";
            public uint RefCount;
            public uint CompiledSize;
            public uint VariableCount;
            public uint Type; // 0x000 = Object, 0x001 = Quest, 0x100 = Magic Effect

            public SCHRField(BinaryReader r, int dataSize)
            {
                r.Skip(4); // Unused
                RefCount = r.ReadUInt32();
                CompiledSize = r.ReadUInt32();
                VariableCount = r.ReadUInt32();
                Type = r.ReadUInt32();
                if (dataSize == 20)
                    return;
                r.Skip(dataSize - 20);
            }
        }

        public class SLSDField
        {
            public override string ToString() => $"{Idx}:{VariableName}";
            public uint Idx;
            public uint Type;
            public string VariableName;

            public SLSDField(BinaryReader r, int dataSize)
            {
                Idx = r.ReadUInt32();
                r.ReadUInt32(); // Unknown
                r.ReadUInt32(); // Unknown
                r.ReadUInt32(); // Unknown
                Type = r.ReadUInt32();
                r.ReadUInt32(); // Unknown
                // SCVRField
                VariableName = null;
            }
            public void SCVRField(BinaryReader r, int dataSize) => VariableName = r.ReadYEncoding(dataSize);
        }

        public override string ToString() => $"SCPT: {EDID.Value ?? SCHD.Name}";
        public STRVField EDID { get; set; } // Editor ID
        public BYTVField SCDA; // Compiled Script
        public STRVField SCTX; // Script Source
        // TES3
        public SCHDField SCHD; // Script Data
        // TES4
        public SCHRField SCHR; // Script Data
        public List<SLSDField> SLSDs = new List<SLSDField>(); // Variable data
        public List<SLSDField> SCRVs = new List<SLSDField>(); // Ref variable data (one for each ref declared)
        public List<FMIDField<Record>> SCROs = new List<FMIDField<Record>>(); // Global variable reference

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "SCHD": SCHD = new SCHDField(r, dataSize); return true;
                case "SCVR": if (format != BethesdaFormat.TES3) SLSDs.Last().SCVRField(r, dataSize); else SCHD.SCVRField(r, dataSize); return true;
                case "SCDA":
                case "SCDT": SCDA = r.ReadBYTV(dataSize); return true;
                case "SCTX": SCTX = r.ReadSTRV(dataSize); return true;
                // TES4
                case "SCHR": SCHR = new SCHRField(r, dataSize); return true;
                case "SLSD": SLSDs.Add(new SLSDField(r, dataSize)); return true;
                case "SCRO": SCROs.Add(new FMIDField<Record>(r, dataSize)); return true;
                case "SCRV": var idx = r.ReadUInt32(); SCRVs.Add(SLSDs.Single(x => x.Idx == idx)); return true;
                default: return false;
            }
        }
    }
}