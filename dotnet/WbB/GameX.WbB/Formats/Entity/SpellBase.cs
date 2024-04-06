using GameX.WbB.Formats.FileTypes;
using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class SpellBase : IHaveMetaInfo
    {
        public readonly string Name;
        public readonly string Desc;
        public readonly MagicSchool School;
        public readonly uint Icon;
        public readonly SpellCategory Category; // All related levels of the same spell. Same category spells will not stack. (Strength Self I & Strength Self II)
        public readonly uint Bitfield;
        public readonly uint BaseMana; // Mana Cost
        public readonly float BaseRangeConstant;
        public readonly float BaseRangeMod;
        public readonly uint Power; // Used to determine which spell in the catgory is the strongest.
        public readonly float SpellEconomyMod; // A legacy of a bygone era
        public readonly uint FormulaVersion;
        public readonly float ComponentLoss; // Burn rate
        public readonly SpellType MetaSpellType;
        public readonly uint MetaSpellId; // Just the spell id again

        // Only on EnchantmentSpell/FellowshipEnchantmentSpells
        public readonly double Duration;
        public readonly float DegradeModifier; // Unknown what this does
        public readonly float DegradeLimit;  // Unknown what this does

        public readonly double PortalLifetime; // Only for PortalSummon_SpellType

        public readonly uint[] Formula; // UInt Values correspond to the SpellComponentsTable

        public readonly uint CasterEffect;  // effect that playes on the caster of the casted spell (e.g. for buffs, protects, etc)
        public readonly uint TargetEffect; // effect that playes on the target of the casted spell (e.g. for debuffs, vulns, etc)
        public readonly uint FizzleEffect; // is always zero. All spells have the same fizzle effect.
        public readonly double RecoveryInterval; // is always zero
        public readonly float RecoveryAmount; // is always zero
        public readonly uint DisplayOrder; // for soring in the spell list in the client UI
        public readonly uint NonComponentTargetType; // Unknown what this does
        public readonly uint ManaMod; // Additional mana cost per target (e.g. "Incantation of Acid Bane" Mana Cost = 80 + 14 per target)

        public SpellBase() { }
        public SpellBase(uint power, double duration, float degradeModifier, float degradeLimit)
        {
            Power = power;
            Duration = duration;
            DegradeModifier = degradeModifier;
            DegradeLimit = degradeLimit;
        }
        public SpellBase(BinaryReader r)
        {
            Name = r.ReadL16StringObfuscated(); r.Align();
            Desc = r.ReadL16StringObfuscated(); r.Align();
            School = (MagicSchool)r.ReadUInt32();
            Icon = r.ReadUInt32();
            Category = (SpellCategory)r.ReadUInt32();
            Bitfield = r.ReadUInt32();
            BaseMana = r.ReadUInt32();
            BaseRangeConstant = r.ReadSingle();
            BaseRangeMod = r.ReadSingle();
            Power = r.ReadUInt32();
            SpellEconomyMod = r.ReadSingle();
            FormulaVersion = r.ReadUInt32();
            ComponentLoss = r.ReadSingle();
            MetaSpellType = (SpellType)r.ReadUInt32();
            MetaSpellId = r.ReadUInt32();
            switch (MetaSpellType)
            {
                case SpellType.Enchantment:
                case SpellType.FellowEnchantment:
                    Duration = r.ReadDouble();
                    DegradeModifier = r.ReadSingle();
                    DegradeLimit = r.ReadSingle();
                    break;
                case SpellType.PortalSummon: PortalLifetime = r.ReadDouble(); break;
            }

            // Components : Load them first, then decrypt them. More efficient to hash all at once.
            var rawComps = r.ReadTArray<uint>(sizeof(uint), 8);

            // Get the decryped component values
            Formula = DecryptFormula(rawComps, Name, Desc);

            CasterEffect = r.ReadUInt32();
            TargetEffect = r.ReadUInt32();
            FizzleEffect = r.ReadUInt32();
            RecoveryInterval = r.ReadDouble();
            RecoveryAmount = r.ReadSingle();
            DisplayOrder = r.ReadUInt32();
            NonComponentTargetType = r.ReadUInt32();
            ManaMod = r.ReadUInt32();
        }

        const uint HIGHEST_COMP_ID = 198; // "Essence of Kemeroi", for Void Spells -- not actually ever in game!

        /// <summary>
        /// Does the math based on the crypto keys (name and description) for the spell formula.
        /// </summary>
        static uint[] DecryptFormula(uint[] rawComps, string name, string desc)
        {
            // uint testDescHash = ComputeHash(" – 200");
            uint nameHash = SpellTable.ComputeHash(name);
            uint descHash = SpellTable.ComputeHash(desc);
            var key = (nameHash % 0x12107680) + (descHash % 0xBEADCF45);

            var comps = new uint[rawComps.Length];
            for (var i = 0; i < rawComps.Length; i++)
            {
                var comp = rawComps[i] - key;
                // This seems to correct issues with certain spells with extended characters.
                if (comp > HIGHEST_COMP_ID) comp &= 0xFF; // highest comp ID is 198 - "Essence of Kemeroi", for Void Spells
                comps[i] = comp;
            }
            return comps;
        }

        string _spellWords;

        /// <summary>
        /// Not technically part of this function, but saves numerous looks later.
        /// </summary>
        public string GetSpellWords(SpellComponentTable comps)
        {
            if (_spellWords != null) return _spellWords;
            _spellWords = SpellComponentTable.GetSpellWords(comps, Formula);
            return _spellWords;
        }

        //: Entity.SpellBase
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var componentTable = DatabaseManager.Portal.SpellComponentTable;
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Name: {Name}"),
                new MetaInfo($"Description: {Desc}"),
                new MetaInfo($"School: {School}"),
                new MetaInfo($"Icon: {Icon:X8}", clickable: true),
                new MetaInfo($"Category: {Category}"),
                new MetaInfo($"Flags: {(SpellFlags)Bitfield}"),
                new MetaInfo($"BaseMana: {BaseMana}"),
                new MetaInfo($"BaseRangeConstant: {BaseRangeConstant}"),
                new MetaInfo($"BaseRangeMod: {BaseRangeMod}"),
                new MetaInfo($"Power: {Power}"),
                new MetaInfo($"SpellEconomyMod: {SpellEconomyMod}"),
                new MetaInfo($"FormulaVersion: {FormulaVersion}"),
                new MetaInfo($"ComponentLoss: {ComponentLoss}"),
                new MetaInfo($"MetaSpellType: {MetaSpellType}"),
                new MetaInfo($"MetaSpellId: {MetaSpellId}"),
                new MetaInfo($"Duration: {Duration}"),
                new MetaInfo($"DegradeModifier: {DegradeModifier}"),
                new MetaInfo($"DegradeLimit: {DegradeLimit}"),
                new MetaInfo("Formula", items: Formula.Select(x => new MetaInfo($"{x}: {componentTable.SpellComponents[x].Name}"))),
                new MetaInfo($"CasterEffect: {(PlayScript)CasterEffect}"),
                new MetaInfo($"TargetEffect: {(PlayScript)TargetEffect}"),
                new MetaInfo($"FizzleEffect: {(PlayScript)FizzleEffect}"),
                new MetaInfo($"RecoveryInterval: {RecoveryInterval}"),
                new MetaInfo($"RecoveryAmount: {RecoveryAmount}"),
                new MetaInfo($"DisplayOrder: {DisplayOrder}"),
                new MetaInfo($"NonComponentTargetType: {(ItemType)NonComponentTargetType}"),
                new MetaInfo($"ManaMod: {ManaMod}"),
            };
            return nodes;
        }
    }
}
