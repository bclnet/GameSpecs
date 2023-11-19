using GameSpec.WbB.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class SkillFormula : IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Attr1: {(PropertyAttribute)Attr1}"),
                new MetadataInfo($"Attr2: {(PropertyAttribute)Attr2}"),
                new MetadataInfo($"W: {W}"),
                new MetadataInfo($"X: {X}"),
                new MetadataInfo($"Y: {Y}"),
                new MetadataInfo($"Z (divisor): {Z}"),
            };
            return nodes;
        }
    }
}
