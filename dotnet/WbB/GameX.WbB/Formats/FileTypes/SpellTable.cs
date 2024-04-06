using GameX.WbB.Formats.Entity;
using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameX.WbB.Formats.FileTypes
{
    [PakFileType(PakFileType.SpellTable)]
    public class SpellTable : FileType, IHaveMetaInfo
    {
        public const uint FILE_ID = 0x0E00000E;

        public readonly IDictionary<uint, SpellBase> Spells;
        /// <summary>
        /// the key uint refers to the SpellSetID, set in PropInt.EquipmentSetId
        /// </summary>
        public readonly IDictionary<uint, SpellSet> SpellSet;

        public SpellTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Spells = r.Skip(2).ReadL16TMany<uint, SpellBase>(sizeof(uint), x => new SpellBase(x));
            SpellSet = r.Skip(2).ReadL16TMany<uint, SpellSet>(sizeof(uint), x => new SpellSet(x));
        }

        //: FileTypes.SpellTable
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(SpellTable)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo("Spells", items: Spells.Select(x => new MetaInfo($"{x.Key}: {x.Value.Name}", items: (x.Value as IHaveMetaInfo).GetInfoNodes(tag: tag)))),
                    new MetaInfo("Spell Sets", items: SpellSet.OrderBy(i => i.Key).Select(x => new MetaInfo($"{x.Key}: {(EquipmentSet)x.Key}", items: (x.Value as IHaveMetaInfo).GetInfoNodes(tag: tag)))),
                })
            };
            return nodes;
        }

        /// <summary>
        /// Generates a hash based on the string. Used to decrypt spell formulas and calculate taper rotation for players.
        /// </summary>
        public static uint ComputeHash(string strToHash)
        {
            var result = 0L;
            if (strToHash.Length > 0)
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var str = Encoding.GetEncoding(1252).GetBytes(strToHash);
                foreach (sbyte c in str)
                {
                    result = c + (result << 4);
                    if ((result & 0xF0000000) != 0) result = (result ^ ((result & 0xF0000000) >> 24)) & 0x0FFFFFFF;
                }
            }
            return (uint)result;
        }

        const uint LOWEST_TAPER_ID = 63; // This is the lowest id in the SpellComponentTable of a taper (Red Taper)

        /// <summary>
        /// Returns the correct spell formula, which is hashed from a player's account name
        /// </summary>
        public static uint[] GetSpellFormula(SpellTable spellTable, uint spellId, string accountName)
        {
            var spell = spellTable.Spells[spellId];
            return spell.FormulaVersion switch
            {
                1 => RandomizeVersion1(spell, accountName),
                2 => RandomizeVersion2(spell, accountName),
                3 => RandomizeVersion3(spell, accountName),
                _ => spell.Formula,
            };
        }

        static uint[] RandomizeVersion1(SpellBase spell, string accountName)
        {
            var comps = new List<uint>(spell.Formula);
            var hasTaper1 = false;
            var hasTaper2 = false;
            var hasTaper3 = false;

            var key = ComputeHash(accountName);
            var seed = key % 0x13D573;

            var scarab = comps[0];
            var herb_index = 1;
            if (comps.Count > 5) { herb_index = 2; hasTaper1 = true; }
            var herb = comps[herb_index];

            var powder_index = herb_index + 1;
            if (comps.Count > 6) { powder_index++; hasTaper2 = true; }
            var powder = comps[powder_index];

            var potion_index = powder_index + 1;
            var potion = comps[potion_index];

            var talisman_index = potion_index + 1;
            if (comps.Count > 7) { talisman_index++; hasTaper3 = true; }
            var talisman = comps[talisman_index];
            if (hasTaper1) comps[1] = (powder + 2 * herb + potion + talisman + scarab) % 0xC + LOWEST_TAPER_ID;
            if (hasTaper2) comps[3] = (scarab + herb + talisman + 2 * (powder + potion)) * (seed / (scarab + (powder + potion))) % 0xC + LOWEST_TAPER_ID;
            if (hasTaper3) comps[6] = (powder + 2 * talisman + potion + herb + scarab) * (seed / (talisman + scarab)) % 0xC + LOWEST_TAPER_ID;
            return comps.ToArray();
        }

        static uint[] RandomizeVersion2(SpellBase spell, string accountName)
        {
            var comps = new List<uint>(spell.Formula);

            var key = ComputeHash(accountName);
            var seed = key % 0x13D573;

            var p1 = comps[0];
            var c = comps[4];
            var x = comps[5];
            var a = comps[7];

            comps[3] = (a + 2 * comps[0] + 2 * c * x + comps[0] + comps[2] + comps[1]) % 0xC + LOWEST_TAPER_ID;
            comps[6] = (a + 2 * p1 * comps[2] + 2 * x + p1 * comps[2] + c) * (seed / (comps[1] * a + 2 * c)) % 0xC + LOWEST_TAPER_ID;

            return comps.ToArray();
        }

        static uint[] RandomizeVersion3(SpellBase spell, string accountName)
        {
            var comps = new List<uint>(spell.Formula);

            var key = ComputeHash(accountName);
            var seed1 = key % 0x13D573;
            var seed2 = key % 0x4AEFD;
            var seed3 = key % 0x96A7F;
            var seed4 = key % 0x100A03;
            var seed5 = key % 0xEB2EF;
            var seed6 = key % 0x121E7D;

            var compHash0 = (seed1 + comps[0]) % 0xC;
            var compHash1 = (seed2 + comps[1]) % 0xC;
            var compHash2 = (seed3 + comps[2]) % 0xC;
            var compHash4 = (seed4 + comps[4]) % 0xC;
            var compHash5 = (seed5 + comps[5]) % 0xC;

            // Some spells don't have the full number of comps. 2697 ("Aerfalle's Touch"), is one example.
            var compHash7 = comps.Count < 8 ? (seed6 + 0) % 0xC : (seed6 + comps[7]) % 0xC;
            comps[3] = (compHash0 + compHash1 + compHash2 + compHash4 + compHash5 + compHash2 * compHash5 + compHash0 * compHash1 + compHash7 * (compHash4 + 1)) % 0xC + LOWEST_TAPER_ID;
            comps[6] = (compHash0 + compHash1 + compHash2 + compHash4 + key % 0x65039 % 0xC + compHash7 * (compHash4 * (compHash0 * compHash1 * compHash2 * compHash5 + 7) + 1) + compHash5 + 4 * compHash0 * compHash1 + compHash0 * compHash1 + 11 * compHash2 * compHash5) % 0xC + LOWEST_TAPER_ID;

            return comps.ToArray();
        }
    }
}
