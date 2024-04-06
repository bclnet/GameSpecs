using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class NameFilterLanguageData : IHaveMetaInfo
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
            CompoundLetterGroups = r.ReadL32FArray(x => x.ReadCU32String());
        }

        //: Entity.NameFilterLanguageData
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"MaximumVowelsInARow: {MaximumVowelsInARow}"),
                new MetaInfo($"FirstNCharactersMustHaveAVowel: {FirstNCharactersMustHaveAVowel}"),
                new MetaInfo($"VowelContainingSubstringLength: {VowelContainingSubstringLength}"),
                new MetaInfo($"ExtraAllowedCharacters: {ExtraAllowedCharacters}"),
                new MetaInfo($"Unknown: {Unknown}"),
                new MetaInfo($"CompoundLetterGrounds", items: CompoundLetterGroups.Select(x => new MetaInfo($"{x}"))),
            };
            return nodes;
        }
    }
}
