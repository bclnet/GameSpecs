using GameX.WbB.Formats.FileTypes;

namespace GameX.WbB
{
    public class DatabasePortal : Database
    {
        public DatabasePortal(PakFile pakFile) : base(pakFile)
        {
            BadData = GetFile<BadData>(BadData.FILE_ID);
            ChatPoseTable = GetFile<ChatPoseTable>(ChatPoseTable.FILE_ID);
            CharGen = GetFile<CharGen>(CharGen.FILE_ID);
            ContractTable = GetFile<ContractTable>(ContractTable.FILE_ID);
            GeneratorTable = GetFile<GeneratorTable>(GeneratorTable.FILE_ID);
            NameFilterTable = GetFile<NameFilterTable>(NameFilterTable.FILE_ID);
            RegionDesc = GetFile<RegionDesc>(RegionDesc.FILE_ID);
            SecondaryAttributeTable = GetFile<SecondaryAttributeTable>(SecondaryAttributeTable.FILE_ID);
            SkillTable = GetFile<SkillTable>(SkillTable.FILE_ID);
            SpellComponentTable = GetFile<SpellComponentTable>(SpellComponentTable.FILE_ID);
            SpellTable = GetFile<SpellTable>(SpellTable.FILE_ID);
            TabooTable = GetFile<TabooTable>(TabooTable.FILE_ID);
            XpTable = GetFile<XpTable>(XpTable.FILE_ID);
        }

        public BadData BadData { get; }
        public ChatPoseTable ChatPoseTable { get; }
        public CharGen CharGen { get; }
        public ContractTable ContractTable { get; }
        public GeneratorTable GeneratorTable { get; }
        public NameFilterTable NameFilterTable { get; }
        public RegionDesc RegionDesc { get; }
        public SecondaryAttributeTable SecondaryAttributeTable { get; }
        public SkillTable SkillTable { get; }
        public SpellComponentTable SpellComponentTable { get; }
        public SpellTable SpellTable { get; }
        public TabooTable TabooTable { get; }
        public XpTable XpTable { get; }
    }
}
