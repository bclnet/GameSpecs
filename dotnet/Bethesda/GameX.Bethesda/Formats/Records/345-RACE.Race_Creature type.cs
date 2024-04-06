using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.Bethesda.Formats.Records
{
    public class RACERecord : Record
    {
        // TESX
        public class DATAField
        {
            public enum RaceFlag : uint
            {
                Playable = 0x00000001,
                FaceGenHead = 0x00000002,
                Child = 0x00000004,
                TiltFrontBack = 0x00000008,
                TiltLeftRight = 0x00000010,
                NoShadow = 0x00000020,
                Swims = 0x00000040,
                Flies = 0x00000080,
                Walks = 0x00000100,
                Immobile = 0x00000200,
                NotPushable = 0x00000400,
                NoCombatInWater = 0x00000800,
                NoRotatingToHeadTrack = 0x00001000,
                DontShowBloodSpray = 0x00002000,
                DontShowBloodDecal = 0x00004000,
                UsesHeadTrackAnims = 0x00008000,
                SpellsAlignWMagicNode = 0x00010000,
                UseWorldRaycastsForFootIK = 0x00020000,
                AllowRagdollCollision = 0x00040000,
                RegenHPInCombat = 0x00080000,
                CantOpenDoors = 0x00100000,
                AllowPCDialogue = 0x00200000,
                NoKnockdowns = 0x00400000,
                AllowPickpocket = 0x00800000,
                AlwaysUseProxyController = 0x01000000,
                DontShowWeaponBlood = 0x02000000,
                OverlayHeadPartList = 0x04000000, //{> Only one can be active <}
                OverrideHeadPartList = 0x08000000, //{> Only one can be active <}
                CanPickupItems = 0x10000000,
                AllowMultipleMembraneShaders = 0x20000000,
                CanDualWield = 0x40000000,
                AvoidsRoads = 0x80000000,
            }

            public struct SkillBoost
            {
                public byte SkillId;
                public sbyte Bonus;

                public SkillBoost(BinaryReader r, int dataSize, BethesdaFormat format)
                {
                    if (format == BethesdaFormat.TES3)
                    {
                        SkillId = (byte)r.ReadInt32();
                        Bonus = (sbyte)r.ReadInt32();
                        return;
                    }
                    SkillId = r.ReadByte();
                    Bonus = r.ReadSByte();
                }
            }

            public struct RaceStats
            {
                public float Height;
                public float Weight;
                // Attributes;
                public byte Strength;
                public byte Intelligence;
                public byte Willpower;
                public byte Agility;
                public byte Speed;
                public byte Endurance;
                public byte Personality;
                public byte Luck;
            }

            public SkillBoost[] SkillBoosts = new SkillBoost[7]; // Skill Boosts
            public RaceStats Male = new RaceStats();
            public RaceStats Female = new RaceStats();
            public uint Flags; // 1 = Playable 2 = Beast Race

            public DATAField(BinaryReader r, int dataSize, BethesdaFormat format)
            {
                if (format == BethesdaFormat.TES3)
                {
                    for (var i = 0; i < SkillBoosts.Length; i++) SkillBoosts[i] = new SkillBoost(r, 8, format);
                    Male.Strength = (byte)r.ReadInt32(); Female.Strength = (byte)r.ReadInt32();
                    Male.Intelligence = (byte)r.ReadInt32(); Female.Intelligence = (byte)r.ReadInt32();
                    Male.Willpower = (byte)r.ReadInt32(); Female.Willpower = (byte)r.ReadInt32();
                    Male.Agility = (byte)r.ReadInt32(); Female.Agility = (byte)r.ReadInt32();
                    Male.Speed = (byte)r.ReadInt32(); Female.Speed = (byte)r.ReadInt32();
                    Male.Endurance = (byte)r.ReadInt32(); Female.Endurance = (byte)r.ReadInt32();
                    Male.Personality = (byte)r.ReadInt32(); Female.Personality = (byte)r.ReadInt32();
                    Male.Luck = (byte)r.ReadInt32(); Female.Luck = (byte)r.ReadInt32();
                    Male.Height = r.ReadSingle(); Female.Height = r.ReadSingle();
                    Male.Weight = r.ReadSingle(); Female.Weight = r.ReadSingle();
                    Flags = r.ReadUInt32();
                    return;
                }
                for (var i = 0; i < SkillBoosts.Length; i++) SkillBoosts[i] = new SkillBoost(r, 2, format);
                r.ReadInt16(); // padding
                Male.Height = r.ReadSingle(); Female.Height = r.ReadSingle();
                Male.Weight = r.ReadSingle(); Female.Weight = r.ReadSingle();
                Flags = r.ReadUInt32();
            }

            public void ATTRField(BinaryReader r, int dataSize)
            {
                Male.Strength = r.ReadByte();
                Male.Intelligence = r.ReadByte();
                Male.Willpower = r.ReadByte();
                Male.Agility = r.ReadByte();
                Male.Speed = r.ReadByte();
                Male.Endurance = r.ReadByte();
                Male.Personality = r.ReadByte();
                Male.Luck = r.ReadByte();
                Female.Strength = r.ReadByte();
                Female.Intelligence = r.ReadByte();
                Female.Willpower = r.ReadByte();
                Female.Agility = r.ReadByte();
                Female.Speed = r.ReadByte();
                Female.Endurance = r.ReadByte();
                Female.Personality = r.ReadByte();
                Female.Luck = r.ReadByte();
            }
        }

        // TES4
        public class FacePartGroup
        {
            public enum Indx : uint
            {
                Head, Ear_Male, Ear_Female, Mouth, Teeth_Lower, Teeth_Upper, Tongue, Eye_Left, Eye_Right,
            }

            public UI32Field INDX;
            public MODLGroup MODL;
            public FILEField ICON;
        }

        public class BodyPartGroup
        {
            public enum Indx : uint
            {
                UpperBody, LowerBody, Hand, Foot, Tail
            }

            public UI32Field INDX;
            public FILEField ICON;
        }

        public class BodyGroup
        {
            public FILEField MODL;
            public FLTVField MODB;
            public List<BodyPartGroup> BodyParts = new List<BodyPartGroup>();
        }

        public override string ToString() => $"RACE: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public STRVField FULL; // Race name
        public STRVField DESC; // Race description
        public List<STRVField> SPLOs = new List<STRVField>(); // NPCs: Special power/ability name
        // TESX
        public DATAField DATA; // RADT:DATA/ATTR: Race data/Base Attributes
        // TES4
        public FMID2Field<RACERecord> VNAM; // Voice
        public FMID2Field<HAIRRecord> DNAM; // Default Hair
        public BYTEField CNAM; // Default Hair Color
        public FLTVField PNAM; // FaceGen - Main clamp
        public FLTVField UNAM; // FaceGen - Face clamp
        public UNKNField XNAM; // Unknown
        //
        public List<FMIDField<HAIRRecord>> HNAMs = new List<FMIDField<HAIRRecord>>();
        public List<FMIDField<EYESRecord>> ENAMs = new List<FMIDField<EYESRecord>>();
        public BYTVField FGGS; // FaceGen Geometry-Symmetric
        public BYTVField FGGA; // FaceGen Geometry-Asymmetric
        public BYTVField FGTS; // FaceGen Texture-Symmetric
        public UNKNField SNAM; // Unknown

        // Parts
        public List<FacePartGroup> FaceParts = new List<FacePartGroup>();
        public BodyGroup[] Bodys = new[] { new BodyGroup(), new BodyGroup() };
        sbyte _nameState;
        sbyte _genderState;

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            if (format == BethesdaFormat.TES3)
                switch (type)
                {
                    case "NAME": EDID = r.ReadSTRV(dataSize); return true;
                    case "FNAM": FULL = r.ReadSTRV(dataSize); return true;
                    case "RADT": DATA = new DATAField(r, dataSize, format); return true;
                    case "NPCS": SPLOs.Add(r.ReadSTRV(dataSize)); return true;
                    case "DESC": DESC = r.ReadSTRV(dataSize); return true;
                    default: return false;
                }
            if (format == BethesdaFormat.TES4)
            {
                switch (_nameState)
                {
                    case 0:
                        switch (type)
                        {
                            case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                            case "FULL": FULL = r.ReadSTRV(dataSize); return true;
                            case "DESC": DESC = r.ReadSTRV(dataSize); return true;
                            case "DATA": DATA = new DATAField(r, dataSize, format); return true;
                            case "SPLO": SPLOs.Add(r.ReadSTRV(dataSize)); return true;
                            case "VNAM": VNAM = new FMID2Field<RACERecord>(r, dataSize); return true;
                            case "DNAM": DNAM = new FMID2Field<HAIRRecord>(r, dataSize); return true;
                            case "CNAM": CNAM = r.ReadS2<BYTEField>(dataSize); return true;
                            case "PNAM": PNAM = r.ReadS2<FLTVField>(dataSize); return true;
                            case "UNAM": UNAM = r.ReadS2<FLTVField>(dataSize); return true;
                            case "XNAM": XNAM = r.ReadUNKN(dataSize); return true;
                            case "ATTR": DATA.ATTRField(r, dataSize); return true;
                            case "NAM0": _nameState++; return true;
                            default: return false;
                        }
                    case 1: // Face Data
                        switch (type)
                        {
                            case "INDX": FaceParts.Add(new FacePartGroup { INDX = r.ReadS2<UI32Field>(dataSize) }); return true;
                            case "MODL": FaceParts.Last().MODL = new MODLGroup(r, dataSize); return true;
                            case "ICON": FaceParts.Last().ICON = r.ReadFILE(dataSize); return true;
                            case "MODB": FaceParts.Last().MODL.MODBField(r, dataSize); return true;
                            case "NAM1": _nameState++; return true;
                            default: return false;
                        }
                    case 2: // Body Data
                        switch (type)
                        {
                            case "MNAM": _genderState = 0; return true;
                            case "FNAM": _genderState = 1; return true;
                            case "MODL": Bodys[_genderState].MODL = r.ReadFILE(dataSize); return true;
                            case "MODB": Bodys[_genderState].MODB = r.ReadS2<FLTVField>(dataSize); return true;
                            case "INDX": Bodys[_genderState].BodyParts.Add(new BodyPartGroup { INDX = r.ReadS2<UI32Field>(dataSize) }); return true;
                            case "ICON": Bodys[_genderState].BodyParts.Last().ICON = r.ReadFILE(dataSize); return true;
                            case "HNAM": _nameState++; break;
                            default: return false;
                        }
                        goto case 3;
                    case 3: // Postamble
                        switch (type)
                        {
                            case "HNAM": for (var i = 0; i < dataSize >> 2; i++) HNAMs.Add(new FMIDField<HAIRRecord>(r, 4)); return true;
                            case "ENAM": for (var i = 0; i < dataSize >> 2; i++) ENAMs.Add(new FMIDField<EYESRecord>(r, 4)); return true;
                            case "FGGS": FGGS = r.ReadBYTV(dataSize); return true;
                            case "FGGA": FGGA = r.ReadBYTV(dataSize); return true;
                            case "FGTS": FGTS = r.ReadBYTV(dataSize); return true;
                            case "SNAM": SNAM = r.ReadUNKN(dataSize); return true;
                            default: return false;
                        }
                    default: return false;
                }
            }
            return false;
        }
    }
}
