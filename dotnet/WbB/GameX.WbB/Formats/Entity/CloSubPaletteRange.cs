using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class CloSubPaletteRange : IHaveMetaInfo
    {
        public readonly uint Offset;
        public readonly uint NumColors;

        public CloSubPaletteRange(BinaryReader r)
        {
            Offset = r.ReadUInt32();
            NumColors = r.ReadUInt32();
        }

        //: Entity.ClothingSubPaletteRange
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Offset: {Offset}"),
                new MetaInfo($"NumColors: {NumColors}"),
            };
            return nodes;
        }

        //: Entity.ClothingSubPaletteRange
        public override string ToString() => $"Offset: {Offset}, NumColors: {NumColors}";
    }
}
