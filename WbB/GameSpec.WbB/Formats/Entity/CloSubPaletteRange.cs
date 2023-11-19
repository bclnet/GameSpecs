using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class CloSubPaletteRange : IGetMetadataInfo
    {
        public readonly uint Offset;
        public readonly uint NumColors;

        public CloSubPaletteRange(BinaryReader r)
        {
            Offset = r.ReadUInt32();
            NumColors = r.ReadUInt32();
        }

        //: Entity.ClothingSubPaletteRange
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Offset: {Offset}"),
                new MetadataInfo($"NumColors: {NumColors}"),
            };
            return nodes;
        }

        //: Entity.ClothingSubPaletteRange
        public override string ToString() => $"Offset: {Offset}, NumColors: {NumColors}";
    }
}
