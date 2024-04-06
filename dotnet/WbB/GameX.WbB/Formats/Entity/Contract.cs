using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameX.WbB.Formats.Entity
{
    public class Contract : IHaveMetaInfo
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
            ContractName = r.ReadL16Encoding(Encoding.Default); r.Align();

            Description = r.ReadL16Encoding(Encoding.Default); r.Align();
            DescriptionProgress = r.ReadL16Encoding(Encoding.Default); r.Align();

            NameNPCStart = r.ReadL16Encoding(Encoding.Default); r.Align();
            NameNPCEnd = r.ReadL16Encoding(Encoding.Default); r.Align();

            QuestflagStamped = r.ReadL16Encoding(Encoding.Default); r.Align();
            QuestflagStarted = r.ReadL16Encoding(Encoding.Default); r.Align();
            QuestflagFinished = r.ReadL16Encoding(Encoding.Default); r.Align();
            QuestflagProgress = r.ReadL16Encoding(Encoding.Default); r.Align();
            QuestflagTimer = r.ReadL16Encoding(Encoding.Default); r.Align();
            QuestflagRepeatTime = r.ReadL16Encoding(Encoding.Default); r.Align();

            LocationNPCStart = new Position(r);
            LocationNPCEnd = new Position(r);
            LocationQuestArea = new Position(r);
        }

        //: Entity.Contract
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"ContractId: {ContractId}"),
                new MetaInfo($"ContractName: {ContractName}"),
                new MetaInfo($"Version: {Version}"),
                new MetaInfo($"Description: {Description}"),
                new MetaInfo($"DescriptionProgress: {DescriptionProgress}"),
                new MetaInfo($"NameNPCStart: {NameNPCStart}"),
                new MetaInfo($"NameNPCEnd: {NameNPCEnd}"),
                new MetaInfo($"QuestflagStamped: {QuestflagStamped}"),
                new MetaInfo($"QuestflagStarted: {QuestflagStarted}"),
                new MetaInfo($"QuestflagFinished: {QuestflagFinished}"),
                new MetaInfo($"QuestflagProgress: {QuestflagProgress}"),
                new MetaInfo($"QuestflagTimer: {QuestflagTimer}"),
                new MetaInfo($"QuestflagRepeatTime: {QuestflagRepeatTime}"),
                new MetaInfo("LocationNPCStart", items: (LocationNPCStart as IHaveMetaInfo).GetInfoNodes(tag: tag)),
                new MetaInfo("LocationNPCEnd", items: (LocationNPCEnd as IHaveMetaInfo).GetInfoNodes(tag: tag)),
                new MetaInfo("LocationQuestArea", items: (LocationQuestArea as IHaveMetaInfo).GetInfoNodes(tag: tag)),
            };
            return nodes;
        }
    }
}
