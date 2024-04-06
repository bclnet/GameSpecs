using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class HeritageGroupCG : IHaveMetaInfo
    {
        public readonly string Name;
        public readonly uint IconImage;
        public readonly uint SetupID; // Basic character model
        public readonly uint EnvironmentSetupID; // This is the background environment during Character Creation
        public readonly uint AttributeCredits;
        public readonly uint SkillCredits;
        public readonly int[] PrimaryStartAreas;
        public readonly int[] SecondaryStartAreas;
        public readonly SkillCG[] Skills;
        public readonly TemplateCG[] Templates;
        public readonly IDictionary<int, SexCG> Genders;

        public HeritageGroupCG(BinaryReader r)
        {
            Name = r.ReadString();
            IconImage = r.ReadUInt32();
            SetupID = r.ReadUInt32();
            EnvironmentSetupID = r.ReadUInt32();
            AttributeCredits = r.ReadUInt32();
            SkillCredits = r.ReadUInt32();
            PrimaryStartAreas = r.ReadC32TArray<int>(sizeof(int));
            SecondaryStartAreas = r.ReadC32TArray<int>(sizeof(int));
            Skills = r.ReadC32FArray(x => new SkillCG(x));
            Templates = r.ReadC32FArray(x => new TemplateCG(x));
            r.Skip(1); // 0x01 byte here. Not sure what/why, so skip it!
            Genders = r.ReadC32TMany<int, SexCG>(sizeof(int), x => new SexCG(x));
        }

        //: Entity.HeritageGroupCG
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Name: {Name}"),
                new MetaInfo($"Icon: {IconImage:X8}", clickable: true),
                new MetaInfo($"Setup: {SetupID:X8}", clickable: true),
                new MetaInfo($"Environment: {EnvironmentSetupID:X8}", clickable: true),
                new MetaInfo($"Attribute Credits: {AttributeCredits}"),
                new MetaInfo($"Skill Credits: {SkillCredits}"),
                new MetaInfo($"Primary Start Areas: {string.Join(",", PrimaryStartAreas)}"),
                new MetaInfo($"Secondary Start Areas: {string.Join(",", SecondaryStartAreas)}"),
                new MetaInfo("Skills", items: Skills.Select(x => {
                    var items = (x as IHaveMetaInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("Skill: ", "");
                    items.RemoveAt(0);
                    return new MetaInfo(name, items: items);
                })),
                new MetaInfo("Templates", items: Templates.Select(x => {
                    var items = (x as IHaveMetaInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("Name: ", "");
                    items.RemoveAt(0);
                    return new MetaInfo(name, items: items);
                })),
                new MetaInfo("Genders", items: Genders.Select(x => {
                    var name = $"{(Gender)x.Key}";
                    var items = (x.Value as IHaveMetaInfo).GetInfoNodes();
                    items.RemoveAt(0);
                    return new MetaInfo(name, items: items);
                })),
            };
            return nodes;
        }
    }
}
