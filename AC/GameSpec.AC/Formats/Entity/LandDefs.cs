using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class LandDefs : IGetExplorerInfo
    {
        public readonly int NumBlockLength;
        public readonly int NumBlockWidth;
        public readonly float SquareLength;
        public readonly int LBlockLength;
        public readonly int VertexPerCell;
        public readonly float MaxObjHeight;
        public readonly float SkyHeight;
        public readonly float RoadWidth;
        public readonly float[] LandHeightTable;

        public LandDefs(BinaryReader r)
        {
            NumBlockLength = r.ReadInt32();
            NumBlockWidth = r.ReadInt32();
            SquareLength = r.ReadSingle();
            LBlockLength = r.ReadInt32();
            VertexPerCell = r.ReadInt32();
            MaxObjHeight = r.ReadSingle();
            SkyHeight = r.ReadSingle();
            RoadWidth = r.ReadSingle();
            LandHeightTable = r.ReadTArray(x => x.ReadSingle(), 256);
        }

        //: Entity.LandDefs
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"NumBlockLength: {NumBlockLength}"),
                new ExplorerInfoNode($"NumBlockWidth: {NumBlockWidth}"),
                new ExplorerInfoNode($"SquareLength: {SquareLength}"),
                new ExplorerInfoNode($"LBlockLength: {LBlockLength}"),
                new ExplorerInfoNode($"VertexPerCell: {VertexPerCell}"),
                new ExplorerInfoNode($"MaxObjHeight: {MaxObjHeight}"),
                new ExplorerInfoNode($"SkyHeight: {SkyHeight}"),
                new ExplorerInfoNode($"RoadWidth: {RoadWidth}"),
                new ExplorerInfoNode("LandHeightTable", items: LandHeightTable.Select((x, i) => new ExplorerInfoNode($"{i}: {x}"))),
            };
            return nodes;
        }
    }
}
