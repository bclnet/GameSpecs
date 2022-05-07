using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class NameFilterLanguageData : IGetExplorerInfo
    {
        public readonly uint MaximumVowelsInARow; 
        public readonly uint FirstNCharactersMustHaveAVowel;
        public readonly uint VowelContainingSubstringLength;
        public readonly uint ExtraAllowedCharacters;
        public readonly byte Unknown;
        public readonly string[] CompoundLetterGroups;

        public NameFilterLanguageData(BinaryReader r)
        {
            MaximumVowelsInARow = r.ReadUInt32();
            FirstNCharactersMustHaveAVowel = r.ReadUInt32();
            VowelContainingSubstringLength = r.ReadUInt32();
            ExtraAllowedCharacters = r.ReadUInt32();
            Unknown = r.ReadByte();
            CompoundLetterGroups = r.ReadL32Array(x => x.ReadUnicodeString());
        }

        //: Entity.NameFilterLanguageData
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"MaximumVowelsInARow: {MaximumVowelsInARow}"),
                new ExplorerInfoNode($"FirstNCharactersMustHaveAVowel: {FirstNCharactersMustHaveAVowel}"),
                new ExplorerInfoNode($"VowelContainingSubstringLength: {VowelContainingSubstringLength}"),
                new ExplorerInfoNode($"ExtraAllowedCharacters: {ExtraAllowedCharacters}"),
                new ExplorerInfoNode($"Unknown: {Unknown}"),
                new ExplorerInfoNode($"CompoundLetterGrounds", items: CompoundLetterGroups.Select(x => new ExplorerInfoNode($"{x}"))),
            };
            return nodes;
        }
    }
}
