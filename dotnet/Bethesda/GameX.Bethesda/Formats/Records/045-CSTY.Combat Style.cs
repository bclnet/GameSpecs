using System.IO;

namespace GameX.Bethesda.Formats.Records
{
    public class CSTYRecord : Record
    {
        public class CSTDField
        {
            public byte DodgePercentChance;
            public byte LeftRightPercentChance;
            public float DodgeLeftRightTimer_Min;
            public float DodgeLeftRightTimer_Max;
            public float DodgeForwardTimer_Min;
            public float DodgeForwardTimer_Max;
            public float DodgeBackTimer_Min;
            public float DodgeBackTimer_Max;
            public float IdleTimer_Min;
            public float IdleTimer_Max;
            public byte BlockPercentChance;
            public byte AttackPercentChance;
            public float RecoilStaggerBonusToAttack;
            public float UnconsciousBonusToAttack;
            public float HandToHandBonusToAttack;
            public byte PowerAttackPercentChance;
            public float RecoilStaggerBonusToPower;
            public float UnconsciousBonusToPowerAttack;
            public byte PowerAttack_Normal;
            public byte PowerAttack_Forward;
            public byte PowerAttack_Back;
            public byte PowerAttack_Left;
            public byte PowerAttack_Right;
            public float HoldTimer_Min;
            public float HoldTimer_Max;
            public byte Flags1;
            public byte AcrobaticDodgePercentChance;
            public float RangeMult_Optimal;
            public float RangeMult_Max;
            public float SwitchDistance_Melee;
            public float SwitchDistance_Ranged;
            public float BuffStandoffDistance;
            public float RangedStandoffDistance;
            public float GroupStandoffDistance;
            public byte RushingAttackPercentChance;
            public float RushingAttackDistanceMult;
            public uint Flags2;

            public CSTDField(BinaryReader r, int dataSize)
            {
                //if (dataSize != 124 && dataSize != 120 && dataSize != 112 && dataSize != 104 && dataSize != 92 && dataSize != 84)
                //    DodgePercentChance = 0;
                DodgePercentChance = r.ReadByte();
                LeftRightPercentChance = r.ReadByte();
                r.Skip(2); // Unused
                DodgeLeftRightTimer_Min = r.ReadSingle();
                DodgeLeftRightTimer_Max = r.ReadSingle();
                DodgeForwardTimer_Min = r.ReadSingle();
                DodgeForwardTimer_Max = r.ReadSingle();
                DodgeBackTimer_Min = r.ReadSingle();
                DodgeBackTimer_Max = r.ReadSingle();
                IdleTimer_Min = r.ReadSingle();
                IdleTimer_Max = r.ReadSingle();
                BlockPercentChance = r.ReadByte();
                AttackPercentChance = r.ReadByte();
                r.Skip(2); // Unused
                RecoilStaggerBonusToAttack = r.ReadSingle();
                UnconsciousBonusToAttack = r.ReadSingle();
                HandToHandBonusToAttack = r.ReadSingle();
                PowerAttackPercentChance = r.ReadByte();
                r.Skip(3); // Unused
                RecoilStaggerBonusToPower = r.ReadSingle();
                UnconsciousBonusToPowerAttack = r.ReadSingle();
                PowerAttack_Normal = r.ReadByte();
                PowerAttack_Forward = r.ReadByte();
                PowerAttack_Back = r.ReadByte();
                PowerAttack_Left = r.ReadByte();
                PowerAttack_Right = r.ReadByte();
                r.Skip(3); // Unused
                HoldTimer_Min = r.ReadSingle();
                HoldTimer_Max = r.ReadSingle();
                Flags1 = r.ReadByte();
                AcrobaticDodgePercentChance = r.ReadByte();
                r.Skip(2); // Unused
                if (dataSize == 84) return; RangeMult_Optimal = r.ReadSingle();
                RangeMult_Max = r.ReadSingle();
                if (dataSize == 92) return; SwitchDistance_Melee = r.ReadSingle();
                SwitchDistance_Ranged = r.ReadSingle();
                BuffStandoffDistance = r.ReadSingle();
                if (dataSize == 104) return; RangedStandoffDistance = r.ReadSingle();
                GroupStandoffDistance = r.ReadSingle();
                if (dataSize == 112) return; RushingAttackPercentChance = r.ReadByte();
                r.Skip(3); // Unused
                RushingAttackDistanceMult = r.ReadSingle();
                if (dataSize == 120) return; Flags2 = r.ReadUInt32();
            }
        }

        public struct CSADField
        {
            public float DodgeFatigueModMult;
            public float DodgeFatigueModBase;
            public float EncumbSpeedModBase;
            public float EncumbSpeedModMult;
            public float DodgeWhileUnderAttackMult;
            public float DodgeNotUnderAttackMult;
            public float DodgeBackWhileUnderAttackMult;
            public float DodgeBackNotUnderAttackMult;
            public float DodgeForwardWhileAttackingMult;
            public float DodgeForwardNotAttackingMult;
            public float BlockSkillModifierMult;
            public float BlockSkillModifierBase;
            public float BlockWhileUnderAttackMult;
            public float BlockNotUnderAttackMult;
            public float AttackSkillModifierMult;
            public float AttackSkillModifierBase;
            public float AttackWhileUnderAttackMult;
            public float AttackNotUnderAttackMult;
            public float AttackDuringBlockMult;
            public float PowerAttFatigueModBase;
            public float PowerAttFatigueModMult;

            public CSADField(BinaryReader r, int dataSize)
            {
                DodgeFatigueModMult = r.ReadSingle();
                DodgeFatigueModBase = r.ReadSingle();
                EncumbSpeedModBase = r.ReadSingle();
                EncumbSpeedModMult = r.ReadSingle();
                DodgeWhileUnderAttackMult = r.ReadSingle();
                DodgeNotUnderAttackMult = r.ReadSingle();
                DodgeBackWhileUnderAttackMult = r.ReadSingle();
                DodgeBackNotUnderAttackMult = r.ReadSingle();
                DodgeForwardWhileAttackingMult = r.ReadSingle();
                DodgeForwardNotAttackingMult = r.ReadSingle();
                BlockSkillModifierMult = r.ReadSingle();
                BlockSkillModifierBase = r.ReadSingle();
                BlockWhileUnderAttackMult = r.ReadSingle();
                BlockNotUnderAttackMult = r.ReadSingle();
                AttackSkillModifierMult = r.ReadSingle();
                AttackSkillModifierBase = r.ReadSingle();
                AttackWhileUnderAttackMult = r.ReadSingle();
                AttackNotUnderAttackMult = r.ReadSingle();
                AttackDuringBlockMult = r.ReadSingle();
                PowerAttFatigueModBase = r.ReadSingle();
                PowerAttFatigueModMult = r.ReadSingle();
            }
        }

        public override string ToString() => $"CSTY: {EDID.Value}";
        public STRVField EDID { get; set; } // Editor ID
        public CSTDField CSTD; // Standard
        public CSADField CSAD; // Advanced

        public override bool CreateField(BinaryReader r, BethesdaFormat format, string type, int dataSize)
        {
            switch (type)
            {
                case "EDID": EDID = r.ReadSTRV(dataSize); return true;
                case "CSTD": CSTD = new CSTDField(r, dataSize); return true;
                case "CSAD": CSAD = new CSADField(r, dataSize); return true;
                default: return false;
            }
        }
    }
}