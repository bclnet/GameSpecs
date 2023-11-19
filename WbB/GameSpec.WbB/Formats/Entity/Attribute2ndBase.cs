using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class Attribute2ndBase
    {
        public readonly SkillFormula Formula;

        public Attribute2ndBase(BinaryReader r)
            => Formula = new SkillFormula(r);
    }
}
