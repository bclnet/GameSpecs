using GameSpec.WbB.Formats.Entity;
using GameSpec.WbB.Formats.Props;
using GameSpec.Meta;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.FileTypes
{
    [PakFileType(PakFileType.SkillTable)]
    public class SkillTable : FileType, IHaveMetaInfo
    {
        public const uint FILE_ID = 0x0E000004;

        // Key is the SkillId
        public IDictionary<uint, SkillBase> SkillBaseHash;

        public SkillTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            SkillBaseHash = r.Skip(2).ReadL16TMany<uint, SkillBase>(sizeof(uint), x => new SkillBase(x));
        }

        //: FileTypes.SkillTable
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(SkillTable)}: {Id:X8}", items: SkillBaseHash.OrderBy(i => i.Key).Where(x => !string.IsNullOrEmpty(x.Value.Name)).Select(
                    x => new MetaInfo($"{x.Key}: {x.Value.Name}", items: (x.Value as IHaveMetaInfo).GetInfoNodes(tag: tag))
                ))
            };
            return nodes;
        }

        public void AddRetiredSkills()
        {
            SkillBaseHash.Add((int)Skill.Axe, new SkillBase(new SkillFormula(PropertyAttribute.Strength, PropertyAttribute.Coordination, 3)));
            SkillBaseHash.Add((int)Skill.Bow, new SkillBase(new SkillFormula(PropertyAttribute.Coordination, PropertyAttribute.Undef, 2)));
            SkillBaseHash.Add((int)Skill.Crossbow, new SkillBase(new SkillFormula(PropertyAttribute.Coordination, PropertyAttribute.Undef, 2)));
            SkillBaseHash.Add((int)Skill.Dagger, new SkillBase(new SkillFormula(PropertyAttribute.Quickness, PropertyAttribute.Coordination, 3)));
            SkillBaseHash.Add((int)Skill.Mace, new SkillBase(new SkillFormula(PropertyAttribute.Strength, PropertyAttribute.Coordination, 3)));
            SkillBaseHash.Add((int)Skill.Spear, new SkillBase(new SkillFormula(PropertyAttribute.Strength, PropertyAttribute.Coordination, 3)));
            SkillBaseHash.Add((int)Skill.Staff, new SkillBase(new SkillFormula(PropertyAttribute.Strength, PropertyAttribute.Coordination, 3)));
            SkillBaseHash.Add((int)Skill.Sword, new SkillBase(new SkillFormula(PropertyAttribute.Strength, PropertyAttribute.Coordination, 3)));
            SkillBaseHash.Add((int)Skill.ThrownWeapon, new SkillBase(new SkillFormula(PropertyAttribute.Coordination, PropertyAttribute.Undef, 2)));
            SkillBaseHash.Add((int)Skill.UnarmedCombat, new SkillBase(new SkillFormula(PropertyAttribute.Strength, PropertyAttribute.Coordination, 3)));
        }
    }
}
