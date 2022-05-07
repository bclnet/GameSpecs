using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameSpec.AC.Formats.Entity
{
    public class Contract : IGetExplorerInfo
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
            ContractName = r.ReadL16String(Encoding.Default); r.AlignBoundary();

            Description = r.ReadL16String(Encoding.Default); r.AlignBoundary();
            DescriptionProgress = r.ReadL16String(Encoding.Default); r.AlignBoundary();

            NameNPCStart = r.ReadL16String(Encoding.Default); r.AlignBoundary();
            NameNPCEnd = r.ReadL16String(Encoding.Default); r.AlignBoundary();

            QuestflagStamped = r.ReadL16String(Encoding.Default); r.AlignBoundary();
            QuestflagStarted = r.ReadL16String(Encoding.Default); r.AlignBoundary();
            QuestflagFinished = r.ReadL16String(Encoding.Default); r.AlignBoundary();
            QuestflagProgress = r.ReadL16String(Encoding.Default); r.AlignBoundary();
            QuestflagTimer = r.ReadL16String(Encoding.Default); r.AlignBoundary();
            QuestflagRepeatTime = r.ReadL16String(Encoding.Default); r.AlignBoundary();

            LocationNPCStart = new Position(r);
            LocationNPCEnd = new Position(r);
            LocationQuestArea = new Position(r);
        }

        //: Entity.Contract
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"ContractId: {ContractId}"),
                new ExplorerInfoNode($"ContractName: {ContractName}"),
                new ExplorerInfoNode($"Version: {Version}"),
                new ExplorerInfoNode($"Description: {Description}"),
                new ExplorerInfoNode($"DescriptionProgress: {DescriptionProgress}"),
                new ExplorerInfoNode($"NameNPCStart: {NameNPCStart}"),
                new ExplorerInfoNode($"NameNPCEnd: {NameNPCEnd}"),
                new ExplorerInfoNode($"QuestflagStamped: {QuestflagStamped}"),
                new ExplorerInfoNode($"QuestflagStarted: {QuestflagStarted}"),
                new ExplorerInfoNode($"QuestflagFinished: {QuestflagFinished}"),
                new ExplorerInfoNode($"QuestflagProgress: {QuestflagProgress}"),
                new ExplorerInfoNode($"QuestflagTimer: {QuestflagTimer}"),
                new ExplorerInfoNode($"QuestflagRepeatTime: {QuestflagRepeatTime}"),
                new ExplorerInfoNode("LocationNPCStart", items: (LocationNPCStart as IGetExplorerInfo).GetInfoNodes(tag: tag)),
                new ExplorerInfoNode("LocationNPCEnd", items: (LocationNPCEnd as IGetExplorerInfo).GetInfoNodes(tag: tag)),
                new ExplorerInfoNode("LocationQuestArea", items: (LocationQuestArea as IGetExplorerInfo).GetInfoNodes(tag: tag)),
            };
            return nodes;
        }
    }
}
