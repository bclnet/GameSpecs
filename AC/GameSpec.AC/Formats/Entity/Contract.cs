using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameSpec.AC.Formats.Entity
{
    public class Contract : IGetMetadataInfo
    {
        public readonly uint Version;
        public readonly uint ContractId;
        public readonly string ContractName;

        public readonly string Description;
        public readonly string DescriptionProgress;

        public readonly string NameNPCStart;
        public readonly string NameNPCEnd;

        public readonly string QuestflagStamped;
        public readonly string QuestflagStarted;
        public readonly string QuestflagFinished;
        public readonly string QuestflagProgress;
        public readonly string QuestflagTimer;
        public readonly string QuestflagRepeatTime;

        public readonly Position LocationNPCStart;
        public readonly Position LocationNPCEnd;
        public readonly Position LocationQuestArea;

        public Contract(BinaryReader r)
        {
            Version = r.ReadUInt32();
            ContractId = r.ReadUInt32();
            ContractName = r.ReadL16String(Encoding.Default); r.Align();

            Description = r.ReadL16String(Encoding.Default); r.Align();
            DescriptionProgress = r.ReadL16String(Encoding.Default); r.Align();

            NameNPCStart = r.ReadL16String(Encoding.Default); r.Align();
            NameNPCEnd = r.ReadL16String(Encoding.Default); r.Align();

            QuestflagStamped = r.ReadL16String(Encoding.Default); r.Align();
            QuestflagStarted = r.ReadL16String(Encoding.Default); r.Align();
            QuestflagFinished = r.ReadL16String(Encoding.Default); r.Align();
            QuestflagProgress = r.ReadL16String(Encoding.Default); r.Align();
            QuestflagTimer = r.ReadL16String(Encoding.Default); r.Align();
            QuestflagRepeatTime = r.ReadL16String(Encoding.Default); r.Align();

            LocationNPCStart = new Position(r);
            LocationNPCEnd = new Position(r);
            LocationQuestArea = new Position(r);
        }

        //: Entity.Contract
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"ContractId: {ContractId}"),
                new MetadataInfo($"ContractName: {ContractName}"),
                new MetadataInfo($"Version: {Version}"),
                new MetadataInfo($"Description: {Description}"),
                new MetadataInfo($"DescriptionProgress: {DescriptionProgress}"),
                new MetadataInfo($"NameNPCStart: {NameNPCStart}"),
                new MetadataInfo($"NameNPCEnd: {NameNPCEnd}"),
                new MetadataInfo($"QuestflagStamped: {QuestflagStamped}"),
                new MetadataInfo($"QuestflagStarted: {QuestflagStarted}"),
                new MetadataInfo($"QuestflagFinished: {QuestflagFinished}"),
                new MetadataInfo($"QuestflagProgress: {QuestflagProgress}"),
                new MetadataInfo($"QuestflagTimer: {QuestflagTimer}"),
                new MetadataInfo($"QuestflagRepeatTime: {QuestflagRepeatTime}"),
                new MetadataInfo("LocationNPCStart", items: (LocationNPCStart as IGetMetadataInfo).GetInfoNodes(tag: tag)),
                new MetadataInfo("LocationNPCEnd", items: (LocationNPCEnd as IGetMetadataInfo).GetInfoNodes(tag: tag)),
                new MetadataInfo("LocationQuestArea", items: (LocationQuestArea as IGetMetadataInfo).GetInfoNodes(tag: tag)),
            };
            return nodes;
        }
    }
}
