using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class NameFilterLanguageData : IGetMetadataInfo
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
            CompoundLetterGroups = r.ReadL32Array(x => x.ReadCU32String());
        }

        //: Entity.NameFilterLanguageData
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"MaximumVowelsInARow: {MaximumVowelsInARow}"),
                new MetadataInfo($"FirstNCharactersMustHaveAVowel: {FirstNCharactersMustHaveAVowel}"),
                new MetadataInfo($"VowelContainingSubstringLength: {VowelContainingSubstringLength}"),
                new MetadataInfo($"ExtraAllowedCharacters: {ExtraAllowedCharacters}"),
                new MetadataInfo($"Unknown: {Unknown}"),
                new MetadataInfo($"CompoundLetterGrounds", items: CompoundLetterGroups.Select(x => new MetadataInfo($"{x}"))),
            };
            return nodes;
        }
    }
}
