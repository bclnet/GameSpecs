using GameSpec.AC.Formats.Props;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class SkillFormula : IGetExplorerInfo
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
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Attr1: {(PropertyAttribute)Attr1}"),
                new ExplorerInfoNode($"Attr2: {(PropertyAttribute)Attr2}"),
                new ExplorerInfoNode($"W: {W}"),
                new ExplorerInfoNode($"X: {X}"),
                new ExplorerInfoNode($"Y: {Y}"),
                new ExplorerInfoNode($"Z (divisor): {Z}"),
            };
            return nodes;
        }
    }
}
