using GameSpec.AC.Formats.Props;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class HeritageGroupCG : IGetExplorerInfo
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
        public readonly Dictionary<int, SexCG> Genders;

        public HeritageGroupCG(BinaryReader r)
        {
            Name = r.ReadString();
            IconImage = r.ReadUInt32();
            SetupID = r.ReadUInt32();
            EnvironmentSetupID = r.ReadUInt32();
            AttributeCredits = r.ReadUInt32();
            SkillCredits = r.ReadUInt32();
            PrimaryStartAreas = r.ReadC32Array<int>(sizeof(int));
            SecondaryStartAreas = r.ReadC32Array<int>(sizeof(int));
            Skills = r.ReadC32Array(x => new SkillCG(x));
            Templates = r.ReadC32Array(x => new TemplateCG(x));
            r.Skip(1); // 0x01 byte here. Not sure what/why, so skip it!
            Genders = r.ReadC32Many<int, SexCG>(sizeof(int), x => new SexCG(x));
        }

        //: Entity.HeritageGroupCG
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Name: {Name}"),
                new ExplorerInfoNode($"Icon: {IconImage:X8}", clickable: true),
                new ExplorerInfoNode($"Setup: {SetupID:X8}", clickable: true),
                new ExplorerInfoNode($"Environment: {EnvironmentSetupID:X8}", clickable: true),
                new ExplorerInfoNode($"Attribute Credits: {AttributeCredits}"),
                new ExplorerInfoNode($"Skill Credits: {SkillCredits}"),
                new ExplorerInfoNode($"Primary Start Areas: {string.Join(",", PrimaryStartAreas)}"),
                new ExplorerInfoNode($"Secondary Start Areas: {string.Join(",", SecondaryStartAreas)}"),
                new ExplorerInfoNode("Skills", items: Skills.Select(x => {
                    var items = (x as IGetExplorerInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("Skill: ", "");
                    items.RemoveAt(0);
                    return new ExplorerInfoNode(name, items: items);
                })),
                new ExplorerInfoNode("Templates", items: Templates.Select(x => {
                    var items = (x as IGetExplorerInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("Name: ", "");
                    items.RemoveAt(0);
                    return new ExplorerInfoNode(name, items: items);
                })),
                new ExplorerInfoNode("Genders", items: Genders.Select(x => {
                    var name = $"{(Gender)x.Key}";
                    var items = (x.Value as IGetExplorerInfo).GetInfoNodes();
                    items.RemoveAt(0);
                    return new ExplorerInfoNode(name, items: items);
                })),
            };
            return nodes;
        }
    }
}
