using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class LandDefs : IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"NumBlockLength: {NumBlockLength}"),
                new MetadataInfo($"NumBlockWidth: {NumBlockWidth}"),
                new MetadataInfo($"SquareLength: {SquareLength}"),
                new MetadataInfo($"LBlockLength: {LBlockLength}"),
                new MetadataInfo($"VertexPerCell: {VertexPerCell}"),
                new MetadataInfo($"MaxObjHeight: {MaxObjHeight}"),
                new MetadataInfo($"SkyHeight: {SkyHeight}"),
                new MetadataInfo($"RoadWidth: {RoadWidth}"),
                new MetadataInfo("LandHeightTable", items: LandHeightTable.Select((x, i) => new MetadataInfo($"{i}: {x}"))),
            };
            return nodes;
        }
    }
}
