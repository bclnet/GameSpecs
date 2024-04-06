using System;
using System.Collections.Generic;
using System.IO;
using static System.IO.Polyfill;

namespace GameX.Bethesda.Formats.Records
{
    public class CREARecord : Record, IHaveEDID, IHaveMODL
    {
        [Flags]
        public enum CREAFlags : uint
        {
            Biped = 0x0001,
            Respawn = 0x0002,
            WeaponAndShield = 0x0004,
            None = 0x0008,
            Swims = 0x0010,
            Flies = 0x0020,
            Walks = 0x0040,
            DefaultFlags = 0x0048,
            Essential = 0x0080,
            SkeletonBlood = 0x0400,
            MetalBlood = 0x0800
        }

        public struct NPDTField
        {
            public int Type; // 0 = Creature, 1 = Daedra, 2 = Undead, 3 = Humanoid
            public int Level;
            public int Strength;
            public int Intelligence;
            public int Willpower;
            public int Agility;
            public int Speed;
            public int Endurance;
            public int Personality;
            public int Luck;
            public int Health;
            public int SpellPts;
            public int Fatigue;
            public int Soul;
            public int Combat;
            public int Magic;
            public int Stealth;
            public int AttackMin1;
            public int AttackMax1;
            public int AttackMin2;
            public int AttackMax2;
            public int AttackMin3;
            public int AttackMax3;
            public int Gold;

            public NPDTField(BinaryReader r, int dataSize)
            {
                Type = r.ReadInt32();
                Level = r.ReadInt32();
                Strength = r.ReadInt32();
                Intelligence = r.ReadInt32();
                Willpower = r.ReadInt32();
                Agility = r.ReadInt32();
                Speed = r.ReadInt32();
                Endurance = r.ReadInt32();
                Personality = r.ReadInt32();
                Luck = r.ReadInt32();
                Health = r.ReadInt32();
                SpellPts = r.ReadInt32();
                Fatigue = r.ReadInt32();
                Soul = r.ReadInt32();
                Combat = r.ReadInt32();
                Magic = r.ReadInt32();
                Stealth = r.ReadInt32();
                AttackMin1 = r.ReadInt32();
                AttackMax1 = r.ReadInt32();
                AttackMin2 = r.ReadInt32();
                AttackMax2 = r.ReadInt32();
                AttackMin3 = r.ReadInt32();
                AttackMax3 = r.ReadInt32();
                Gold = r.ReadInt32();
            }
        }

        public struct AIDTField
        {
            public enum AIFlags : uint
            {
                Weapon = 0x00001,
                Armor = 0x00002,
                Clothing = 0x00004,
                Books = 0x00008,
                Ingrediant = 0x00010,
                Picks = 0x00020,
                Probes = 0x00040,
                Lights = 0x00080,
                Apparatus = 0x00100,
                Repair = 0x00200,
                Misc = 0x00400,
                Spells = 0x00800,
                MagicItems = 0x01000,
                Potions = 0x02000,
                Training = 0x04000,
                Spellmaking = 0x08000,
                Enchanting = 0x10000,
                RepairItem = 0x20000
            }

            public byte Hello;
            public byte Unknown1;
            public byte Fight;
            public byte Flee;
            public byte Alarm;
            public byte Unknown2;
            public byte Unknown3;
            public byte Unknown4;
            public uint Flags;

            public AIDTField(BinaryReader r, int dataSize)
            {
                Hello = r.ReadByte();
                Unknown1 = r.ReadByte();
                Fight = r.ReadByte();
                Flee = r.ReadByte();
                Alarm = r.ReadByte();
                Unknown2 = r.ReadByte();
                Unknown3 = r.ReadByte();
                Unknown4 = r.ReadByte();
                Flags = r.ReadUInt32();
            }
        }

        public struct AI_WField
        {
            public short Distance;
            public short Duration;
            public byte TimeOfDay;
            public byte[] Idle;
            public byte Unknown;

            public AI_WField(BinaryReader r, int dataSize)
            {
                Distance = r.ReadInt16();
                Duration = r.ReadInt16();
                TimeOfDay = r.ReadByte();
                Idle = r.ReadBytes(8);
                Unknown = r.ReadByte();
            }
        }

        public struct AI_TField
        {
            public float X;
            public float Y;
            public float Z;
            public float Unknown;

            public AI_TField(BinaryReader r, int dataSize)
            {
                X = r.ReadSingle();
                Y = r.ReadSingle();
                Z = r.ReadSingle();
                Unknown = r.ReadSingle();
            }
        }

        public struct AI_FField
        {
            public float X;
            public float Y;
            public float Z;
            public short Duration;
            public string Id;
            public short Unknown;

            public AI_FField(BinaryReader r, int dataSize)
            {
                X = r.ReadSingle();
                Y = r.ReadSingle();
                Z = r.ReadSingle();
                Duration = r.ReadInt16();
                Id = r.ReadZString(32);
                Unknown = r.ReadInt16();
            }
        }

        public struct AI_AField
        {
            public string Name;
            public byte Unknown;

            public AI_AField(BinaryReader r, int dataSize)
            {
                Name = r.ReadZString(32);
                Unknown = r.ReadByte();
            }
        }

        public override string ToString() => $"CREA: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public MODLGroup MODL { get; set; } // NIF Model
        public STRVField FNAM; // Creature name
        public NPDTField NPDT; // Creature data
        public IN32Field FLAG; // Creature Flags
        public FMIDField<SCPTRecord> SCRI; // Script
        public CNTOField NPCO; // Item record
        public AIDTField AIDT; // AI data
        public AI_WField AI_W; // AI Wander
        public AI_TField? AI_T; // AI Travel
        public AI_FField? AI_F; // AI Follow
        public AI_FField? AI_E; // AI Escort
        public AI_AField? AI_A; // AI Activate
        public FLTVField? XSCL; // Scale (optional), Only present if the scale is not 1.0
        public STRVField? CNAM;
        public List<STRVField> NPCSs = new List<STRVField>();

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            if (format == BethesdaFormat.TES3)
                switch (type)
                {
                    case "NAME": EDID = r.ReadSTRV(dataSize); return true;
                    case "MODL": MODL = new MODLGroup(r, dataSize); return true;
                    case "FNAM": FNAM = r.ReadSTRV(dataSize); return true;
                    case "NPDT": NPDT = new NPDTField(r, dataSize); return true;
                    case "FLAG": FLAG = r.ReadS2<IN32Field>(dataSize); return true;
                    case "SCRI": SCRI = new FMIDField<SCPTRecord>(r, dataSize); return true;
                    case "NPCO": NPCO = new CNTOField(r, dataSize, format); return true;
                    case "AIDT": AIDT = new AIDTField(r, dataSize); return true;
                    case "AI_W": AI_W = new AI_WField(r, dataSize); return true;
                    case "AI_T": AI_T = new AI_TField(r, dataSize); return true;
                    case "AI_F": AI_F = new AI_FField(r, dataSize); return true;
                    case "AI_E": AI_E = new AI_FField(r, dataSize); return true;
                    case "AI_A": AI_A = new AI_AField(r, dataSize); return true;
                    case "XSCL": XSCL = r.ReadS2<FLTVField>(dataSize); return true;
                    case "CNAM": CNAM = r.ReadSTRV(dataSize); return true;
                    case "NPCS": NPCSs.Add(r.ReadSTRV_ZPad(dataSize)); return true;
                    default: return false;
                }
            return false;
        }
    }
}