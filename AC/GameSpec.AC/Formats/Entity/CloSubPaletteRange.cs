using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class CloSubPaletteRange : IGetExplorerInfo
    {
        public readonly uint Offset;
        public readonly uint NumColors;

        public CloSubPaletteRange(BinaryReader r)
        {
            Offset = r.ReadUInt32();
            NumColors = r.ReadUInt32();
        }

        //: Entity.ClothingSubPaletteRange
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Offset: {Offset}"),
                new ExplorerInfoNode($"NumColors: {NumColors}"),
            };
            return nodes;
        }

        //: Entity.ClothingSubPaletteRange
        public override string ToString() => $"Offset: {Offset}, NumColors: {NumColors}";
    }
}
