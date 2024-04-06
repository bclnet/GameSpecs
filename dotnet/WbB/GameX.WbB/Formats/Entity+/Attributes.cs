using GameX.WbB.Formats.Props;
using System;

namespace GameX.WbB.Formats.Entity
{
    public class AbilityRegenAttribute : Attribute
    {
        public AbilityRegenAttribute(double rate) => Rate = rate;
        public double Rate { get; set; }
    }
    public class AbilityVitalAttribute : Attribute
    {
        public AbilityVitalAttribute(Vital vital) => Vital = vital;
        public Vital Vital { get; }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class CharacterOptions1Attribute : Attribute
    {
        public CharacterOptions1Attribute(CharacterOptions1 option) => Option = option;
        public CharacterOptions1 Option { get; }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class CharacterOptions2Attribute : Attribute
    {
        public CharacterOptions2Attribute(CharacterOptions2 option) => Option = option;
        public CharacterOptions2 Option { get; }
    }
}
