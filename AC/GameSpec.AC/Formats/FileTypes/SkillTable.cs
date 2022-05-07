using GameSpec.AC.Formats.Entity;
using GameSpec.AC.Formats.Props;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.FileTypes
{
    [PakFileType(PakFileType.SkillTable)]
    public class SkillTable : FileType, IGetExplorerInfo
    {
        public const uint FILE_ID = 0x0E000004;

        // Key is the SkillId
        public Dictionary<uint, SkillBase> SkillBaseHash;

        public SkillTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            SkillBaseHash = r.ReadL16Many<uint, SkillBase>(sizeof(uint), x => new SkillBase(x), offset: 2);
        }

        //: FileTypes.SkillTable
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(SkillTable)}: {Id:X8}", items: SkillBaseHash.OrderBy(i => i.Key).Where(x => !string.IsNullOrEmpty(x.Value.Name)).Select(
                    x => new ExplorerInfoNode($"{x.Key}: {x.Value.Name}", items: (x.Value as IGetExplorerInfo).GetInfoNodes(tag: tag))
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
