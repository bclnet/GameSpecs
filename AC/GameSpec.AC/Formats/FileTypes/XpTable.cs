using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.FileTypes
{
    /// <summary>
    /// Reads and stores the XP Tables from the client_portal.dat (file 0x0E000018).
    /// </summary>
    [PakFileType(PakFileType.XpTable)]
    public class XpTable : FileType, IGetMetadataInfo
    {
        public const uint FILE_ID = 0x0E000018;

        public readonly uint[] AttributeXpList;
        public readonly uint[] VitalXpList;
        public readonly uint[] TrainedSkillXpList;
        public readonly uint[] SpecializedSkillXpList;
        /// <summary>
        /// The XP needed to reach each level
        /// </summary>
        public readonly ulong[] CharacterLevelXPList;
        /// <summary>
        /// Number of credits gifted at each level
        /// </summary>
        public readonly uint[] CharacterLevelSkillCreditList;

        public XpTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            // The counts for each "Table" are at the top of the file.
            var attributeCount = r.ReadInt32() + 1;
            var vitalCount = r.ReadInt32() + 1;
            var trainedSkillCount = r.ReadInt32() + 1;
            var specializedSkillCount = r.ReadInt32() + 1;
            var levelCount = r.ReadUInt32() + 1;
            AttributeXpList = r.ReadTArray<uint>(sizeof(uint), attributeCount);
            VitalXpList = r.ReadTArray<uint>(sizeof(uint), vitalCount);
            TrainedSkillXpList = r.ReadTArray<uint>(sizeof(uint), trainedSkillCount);
            SpecializedSkillXpList = r.ReadTArray<uint>(sizeof(uint), specializedSkillCount);
            CharacterLevelXPList = r.ReadTArray<ulong>(sizeof(ulong), (int)levelCount);
            CharacterLevelSkillCreditList = r.ReadTArray<uint>(sizeof(uint), (int)levelCount);
        }

        //: FileTypes.XpTable
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{nameof(XpTable)}: {Id:X8}", items: new List<MetadataInfo> {
                    new MetadataInfo("AttributeXpList", items: AttributeXpList.Select((x, i) => new MetadataInfo($"{i}: {x:N0}"))),
                    new MetadataInfo("VitalXpList", items: VitalXpList.Select((x, i) => new MetadataInfo($"{i}: {x:N0}"))),
                    new MetadataInfo("TrainedSkillXpList", items: TrainedSkillXpList.Select((x, i) => new MetadataInfo($"{i}: {x:N0}"))),
                    new MetadataInfo("SpecializedSkillXpList", items: SpecializedSkillXpList.Select((x, i) => new MetadataInfo($"{i}: {x:N0}"))),
                    new MetadataInfo("CharacterLevelXpList", items: CharacterLevelXPList.Select((x, i) => new MetadataInfo($"{i}: {x:N0}"))),
                    new MetadataInfo("CharacterLevelSkillCreditList", items: CharacterLevelSkillCreditList.Select((x, i) => new MetadataInfo($"{i}: {x:N0}"))),
                })
            };
            return nodes;
        }
    }
}
