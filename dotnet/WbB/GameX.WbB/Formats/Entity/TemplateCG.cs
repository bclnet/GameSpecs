using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class TemplateCG : IHaveMetaInfo
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
            NormalSkillsList = r.ReadC32TArray<Skill>(sizeof(uint));
            PrimarySkillsList = r.ReadC32TArray<Skill>(sizeof(uint));
        }

        //: Entity.TemplateCG
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Name: {Name}"),
                new MetaInfo($"Icon: {IconImage:X8}", clickable: true),
                new MetaInfo($"Title: {Title}"),
                new MetaInfo($"Strength: {Strength}"),
                new MetaInfo($"Endurance: {Endurance}"),
                new MetaInfo($"Coordination: {Coordination}"),
                new MetaInfo($"Quickness: {Quickness}"),
                new MetaInfo($"Focus: {Focus}"),
                new MetaInfo($"Self: {Self}"),
                NormalSkillsList.Length > 0 ? new MetaInfo("Normal Skills", items: NormalSkillsList.Select(x => new MetaInfo($"{x}"))) : null,
                PrimarySkillsList.Length > 0 ? new MetaInfo("Primary Skills", items: PrimarySkillsList.Select(x => new MetaInfo($"{x}"))) : null,
            };
            return nodes;
        }
    }
}
