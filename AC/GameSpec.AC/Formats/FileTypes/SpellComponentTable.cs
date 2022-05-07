using GameSpec.AC.Formats.Entity;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.FileTypes
{
    [PakFileType(PakFileType.SpellComponentTable)]
    public class SpellComponentTable : FileType, IGetExplorerInfo
    {
        public enum Type
        {
            Scarab = 1,
            Herb = 2,
            Powder = 3,
            Potion = 4,
            Talisman = 5,
            Taper = 6,
            PotionPea = 7,
            TalismanPea = 5,
            TaperPea = 7
        }

        public const uint FILE_ID = 0x0E00000F;

        public readonly Dictionary<uint, SpellComponentBase> SpellComponents;

        public SpellComponentTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            var numComps = r.ReadUInt16(); r.AlignBoundary(); // Should be 163 or 0xA3
            SpellComponents = r.ReadTMany<uint, SpellComponentBase>(sizeof(uint), x => new SpellComponentBase(r), numComps);
        }

        //: FileTypes.SpellComponentTable
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(SpellComponentTable)}: {Id:X8}", items: SpellComponents.Select(
                    x => new ExplorerInfoNode($"{x.Key} - {x.Value.Name}", items: (x.Value as IGetExplorerInfo).GetInfoNodes(tag: tag))
                ))
            };
            return nodes;
        }

        public static string GetSpellWords(SpellComponentTable comps, uint[] formula)
        {
            var firstSpellWord = string.Empty;
            var secondSpellWord = string.Empty;
            var thirdSpellWord = string.Empty;
            if (formula == null) return string.Empty;
            // Locate the herb component in the Spell formula
            for (var i = 0; i < formula.Length; i++) if (comps.SpellComponents[formula[i]].Type == (uint)Type.Herb) firstSpellWord = comps.SpellComponents[formula[i]].Text;
            // Locate the powder component in the Spell formula
            for (var i = 0; i < formula.Length; i++) if (comps.SpellComponents[formula[i]].Type == (uint)Type.Powder) secondSpellWord = comps.SpellComponents[formula[i]].Text;
            // Locate the potion component in the Spell formula
            for (var i = 0; i < formula.Length; i++) if (comps.SpellComponents[formula[i]].Type == (uint)Type.Potion) thirdSpellWord = comps.SpellComponents[formula[i]].Text;
            // We need to make sure our second spell word, if any, is capitalized
            // Some spell words have no "secondSpellWord", so we're basically making sure the third word is capitalized.
            var secondSpellWordSet = secondSpellWord + thirdSpellWord.ToLowerInvariant();
            if (secondSpellWordSet != string.Empty) { var firstLetter = secondSpellWordSet.Substring(0, 1).ToUpperInvariant(); secondSpellWordSet = firstLetter + secondSpellWordSet.Substring(1); }
            var result = $"{firstSpellWord} {secondSpellWordSet}".Trim();
            return result;
        }
    }
}
