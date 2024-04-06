using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.IO.Polyfill;

namespace GameX.Bethesda.Formats.Records
{
    public class INFORecord : Record
    {
        // TES3
        public struct DATA3Field
        {
            public int Unknown1;
            public int Disposition;
            public byte Rank; // (0-10)
            public byte Gender; // 0xFF = None, 0x00 = Male, 0x01 = Female
            public byte PCRank; // (0-10)
            public byte Unknown2;

            public DATA3Field(BinaryReader r, int dataSize)
            {
                Unknown1 = r.ReadInt32();
                Disposition = r.ReadInt32();
                Rank = r.ReadByte();
                Gender = r.ReadByte();
                PCRank = r.ReadByte();
                Unknown2 = r.ReadByte();
            }
        }

        public class TES3Group
        {
            public STRVField NNAM; // Next info ID (form a linked list of INFOs for the DIAL). First INFO has an empty PNAM, last has an empty NNAM.
            public DATA3Field DATA; // Info data
            public STRVField ONAM; // Actor
            public STRVField RNAM; // Race
            public STRVField CNAM; // Class
            public STRVField FNAM; // Faction 
            public STRVField ANAM; // Cell
            public STRVField DNAM; // PC Faction
            public STRVField NAME; // The info response string (512 max)
            public FILEField SNAM; // Sound
            public BYTEField QSTN; // Journal Name
            public BYTEField QSTF; // Journal Finished
            public BYTEField QSTR; // Journal Restart
            public SCPTRecord.CTDAField SCVR; // String for the function/variable choice
            public UNKNField INTV; //
            public UNKNField FLTV; // The function/variable result for the previous SCVR
            public STRVField BNAM; // Result text (not compiled)
        }

        // TES4
        public struct DATA4Field
        {
            public byte Type;
            public byte NextSpeaker;
            public byte Flags;

            public DATA4Field(BinaryReader r, int dataSize)
            {
                Type = r.ReadByte();
                NextSpeaker = r.ReadByte();
                Flags = dataSize == 3 ? r.ReadByte() : (byte)0;
            }
        }

        public class TRDTField
        {
            public uint EmotionType;
            public int EmotionValue;
            public byte ResponseNumber;
            public string ResponseText;
            public string ActorNotes;

            public TRDTField(BinaryReader r, int dataSize)
            {
                EmotionType = r.ReadUInt32();
                EmotionValue = r.ReadInt32();
                r.Skip(4); // Unused
                ResponseNumber = r.ReadByte();
                r.Skip(3); // Unused
            }
            public void NAM1Field(BinaryReader r, int dataSize) => ResponseText = r.ReadYEncoding(dataSize);
            public void NAM2Field(BinaryReader r, int dataSize) => ActorNotes = r.ReadYEncoding(dataSize);
        }

        public class TES4Group
        {
            public DATA4Field DATA; // Info data
            public FMIDField<QUSTRecord> QSTI; // Quest
            public FMIDField<DIALRecord> TPIC; // Topic
            public List<FMIDField<DIALRecord>> NAMEs = new List<FMIDField<DIALRecord>>(); // Topics
            public List<TRDTField> TRDTs = new List<TRDTField>(); // Responses
            public List<SCPTRecord.CTDAField> CTDAs = new List<SCPTRecord.CTDAField>(); // Conditions
            public List<FMIDField<DIALRecord>> TCLTs = new List<FMIDField<DIALRecord>>(); // Choices
            public List<FMIDField<DIALRecord>> TCLFs = new List<FMIDField<DIALRecord>>(); // Link From Topics
            public SCPTRecord.SCHRField SCHR; // Script Data
            public BYTVField SCDA; // Compiled Script
            public STRVField SCTX; // Script Source
            public List<FMIDField<Record>> SCROs = new List<FMIDField<Record>>(); // Global variable reference
        }

        public override string ToString() => $"INFO: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID - Info name string (unique sequence of #'s), ID
        public FMIDField<INFORecord> PNAM; // Previous info ID
        public TES3Group TES3 = new TES3Group();
        public TES4Group TES4 = new TES4Group();

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            if (format == BethesdaFormat.TES3)
                switch (type)
                {
                    case "INAM": EDID = r.ReadSTRV(dataSize); DIALRecord.LastRecord?.INFOs.Add(this); return true;
                    case "PNAM": PNAM = new FMIDField<INFORecord>(r, dataSize); return true;
                    case "NNAM": TES3.NNAM = r.ReadSTRV(dataSize); return true;
                    case "DATA": TES3.DATA = new DATA3Field(r, dataSize); return true;
                    case "ONAM": TES3.ONAM = r.ReadSTRV(dataSize); return true;
                    case "RNAM": TES3.RNAM = r.ReadSTRV(dataSize); return true;
                    case "CNAM": TES3.CNAM = r.ReadSTRV(dataSize); return true;
                    case "FNAM": TES3.FNAM = r.ReadSTRV(dataSize); return true;
                    case "ANAM": TES3.ANAM = r.ReadSTRV(dataSize); return true;
                    case "DNAM": TES3.DNAM = r.ReadSTRV(dataSize); return true;
                    case "NAME": TES3.NAME = r.ReadSTRV(dataSize); return true;
                    case "SNAM": TES3.SNAM = r.ReadFILE(dataSize); return true;
                    case "QSTN": TES3.QSTN = r.ReadT<BYTEField>(dataSize); return true;
                    case "QSTF": TES3.QSTF = r.ReadT<BYTEField>(dataSize); return true;
                    case "QSTR": TES3.QSTR = r.ReadT<BYTEField>(dataSize); return true;
                    case "SCVR": TES3.SCVR = new SCPTRecord.CTDAField(r, dataSize, format); return true;
                    case "INTV": TES3.INTV = r.ReadUNKN(dataSize); return true;
                    case "FLTV": TES3.FLTV = r.ReadUNKN(dataSize); return true;
                    case "BNAM": TES3.BNAM = r.ReadSTRV(dataSize); return true;
                    default: return false;
                }
            switch (type)
            {
                case "DATA": TES4.DATA = new DATA4Field(r, dataSize); return true;
                case "QSTI": TES4.QSTI = new FMIDField<QUSTRecord>(r, dataSize); return true;
                case "TPIC": TES4.TPIC = new FMIDField<DIALRecord>(r, dataSize); return true;
                case "NAME": TES4.NAMEs.Add(new FMIDField<DIALRecord>(r, dataSize)); return true;
                case "TRDT": TES4.TRDTs.Add(new TRDTField(r, dataSize)); return true;
                case "NAM1": TES4.TRDTs.Last().NAM1Field(r, dataSize); return true;
                case "NAM2": TES4.TRDTs.Last().NAM2Field(r, dataSize); return true;
                case "CTDA":
                case "CTDT": TES4.CTDAs.Add(new SCPTRecord.CTDAField(r, dataSize, format)); return true;
                case "TCLT": TES4.TCLTs.Add(new FMIDField<DIALRecord>(r, dataSize)); return true;
                case "TCLF": TES4.TCLFs.Add(new FMIDField<DIALRecord>(r, dataSize)); return true;
                case "SCHR":
                case "SCHD": TES4.SCHR = new SCPTRecord.SCHRField(r, dataSize); return true;
                case "SCDA": TES4.SCDA = r.ReadBYTV(dataSize); return true;
                case "SCTX": TES4.SCTX = r.ReadSTRV(dataSize); return true;
                case "SCRO": TES4.SCROs.Add(new FMIDField<Record>(r, dataSize)); return true;
                default: return false;
            }
        }
    }
}