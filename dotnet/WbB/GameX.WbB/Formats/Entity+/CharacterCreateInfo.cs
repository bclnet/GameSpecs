using GameX.WbB.Formats.Props;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class CharacterCreateInfo
    {
        public HeritageGroup Heritage { get; set; }
        public uint Gender { get; set; }

        public Appearance Appearance { get; private set; } = new Appearance();

        public int TemplateOption { get; private set; }

        public uint StrengthAbility { get; set; }
        public uint EnduranceAbility { get; set; }
        public uint CoordinationAbility { get; set; }
        public uint QuicknessAbility { get; set; }
        public uint FocusAbility { get; set; }
        public uint SelfAbility { get; set; }

        public uint CharacterSlot { get; private set; }
        public uint ClassId { get; private set; }

        public List<SkillAdvancementClass> SkillAdvancementClasses = new List<SkillAdvancementClass>();

        public string Name { get; set; }

        public uint StartArea { get; private set; }

        public bool IsAdmin { get; private set; }
        public bool IsSentinel { get; private set; }

        public CharacterCreateInfo(BinaryReader r)
        {
            r.Skip(4); // Unknown constant (1)
            Heritage = (HeritageGroup)r.ReadUInt32();
            Gender = r.ReadUInt32();
            Appearance = new Appearance(r);
            TemplateOption = r.ReadInt32();
            StrengthAbility = r.ReadUInt32();
            EnduranceAbility = r.ReadUInt32();
            CoordinationAbility = r.ReadUInt32();
            QuicknessAbility = r.ReadUInt32();
            FocusAbility = r.ReadUInt32();
            SelfAbility = r.ReadUInt32();
            CharacterSlot = r.ReadUInt32();
            ClassId = r.ReadUInt32();
            SkillAdvancementClasses = r.ReadL32FArray(x => (SkillAdvancementClass)x.ReadUInt32()).ToList();
            Name = r.ReadL16Encoding();
            StartArea = r.ReadUInt32();
            IsAdmin = r.ReadUInt32() == 1;
            IsSentinel = r.ReadUInt32() == 1;
        }
    }
}
