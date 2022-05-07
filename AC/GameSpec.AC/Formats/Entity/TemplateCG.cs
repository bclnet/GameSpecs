using GameSpec.AC.Formats.Props;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class TemplateCG : IGetExplorerInfo
    {
        public string Name;
        public uint IconImage;
        public CharacterTitle Title;
        // Attributes
        public uint Strength;
        public uint Endurance;
        public uint Coordination;
        public uint Quickness;
        public uint Focus;
        public uint Self;
        public Skill[] NormalSkillsList;
        public Skill[] PrimarySkillsList;

        public TemplateCG(BinaryReader r)
        {
            Name = r.ReadString();
            IconImage = r.ReadUInt32();
            Title = (CharacterTitle)r.ReadUInt32();
            // Attributes
            Strength = r.ReadUInt32();
            Endurance = r.ReadUInt32();
            Coordination = r.ReadUInt32();
            Quickness = r.ReadUInt32();
            Focus = r.ReadUInt32();
            Self = r.ReadUInt32();
            NormalSkillsList = r.ReadC32Array<Skill>(sizeof(uint));
            PrimarySkillsList = r.ReadC32Array<Skill>(sizeof(uint));
        }

        //: Entity.TemplateCG
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Name: {Name}"),
                new ExplorerInfoNode($"Icon: {IconImage:X8}", clickable: true),
                new ExplorerInfoNode($"Title: {Title}"),
                new ExplorerInfoNode($"Strength: {Strength}"),
                new ExplorerInfoNode($"Endurance: {Endurance}"),
                new ExplorerInfoNode($"Coordination: {Coordination}"),
                new ExplorerInfoNode($"Quickness: {Quickness}"),
                new ExplorerInfoNode($"Focus: {Focus}"),
                new ExplorerInfoNode($"Self: {Self}"),
                NormalSkillsList.Length > 0 ? new ExplorerInfoNode("Normal Skills", items: NormalSkillsList.Select(x => new ExplorerInfoNode($"{x}"))) : null,
                PrimarySkillsList.Length > 0 ? new ExplorerInfoNode("Primary Skills", items: PrimarySkillsList.Select(x => new ExplorerInfoNode($"{x}"))) : null,
            };
            return nodes;
        }
    }
}
