using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class SkillFormula : IHaveMetaInfo
    {
        public readonly uint W;
        public readonly uint X;
        public readonly uint Y;
        public readonly uint Z;
        public readonly uint Attr1;
        public readonly uint Attr2;

        public SkillFormula() { }
        public SkillFormula(PropertyAttribute attr1, PropertyAttribute attr2, uint divisor)
        {
            X = 1;
            Z = divisor;
            Attr1 = (uint)attr1;
            Attr2 = (uint)attr2;
        }
        public SkillFormula(BinaryReader r)
        {
            W = r.ReadUInt32();
            X = r.ReadUInt32();
            Y = r.ReadUInt32();
            Z = r.ReadUInt32();
            Attr1 = r.ReadUInt32();
            Attr2 = r.ReadUInt32();
        }

        //: Entity.SkillFormula
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Attr1: {(PropertyAttribute)Attr1}"),
                new MetaInfo($"Attr2: {(PropertyAttribute)Attr2}"),
                new MetaInfo($"W: {W}"),
                new MetaInfo($"X: {X}"),
                new MetaInfo($"Y: {Y}"),
                new MetaInfo($"Z (divisor): {Z}"),
            };
            return nodes;
        }
    }
}
