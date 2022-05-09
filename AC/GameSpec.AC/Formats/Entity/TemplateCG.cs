using GameSpec.AC.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class TemplateCG : IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Name: {Name}"),
                new MetadataInfo($"Icon: {IconImage:X8}", clickable: true),
                new MetadataInfo($"Title: {Title}"),
                new MetadataInfo($"Strength: {Strength}"),
                new MetadataInfo($"Endurance: {Endurance}"),
                new MetadataInfo($"Coordination: {Coordination}"),
                new MetadataInfo($"Quickness: {Quickness}"),
                new MetadataInfo($"Focus: {Focus}"),
                new MetadataInfo($"Self: {Self}"),
                NormalSkillsList.Length > 0 ? new MetadataInfo("Normal Skills", items: NormalSkillsList.Select(x => new MetadataInfo($"{x}"))) : null,
                PrimarySkillsList.Length > 0 ? new MetadataInfo("Primary Skills", items: PrimarySkillsList.Select(x => new MetadataInfo($"{x}"))) : null,
            };
            return nodes;
        }
    }
}
