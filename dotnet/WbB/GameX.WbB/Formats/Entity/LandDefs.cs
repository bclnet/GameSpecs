using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class LandDefs : IHaveMetaInfo
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
            LandHeightTable = r.ReadFArray(x => x.ReadSingle(), 256);
        }

        //: Entity.LandDefs
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"NumBlockLength: {NumBlockLength}"),
                new MetaInfo($"NumBlockWidth: {NumBlockWidth}"),
                new MetaInfo($"SquareLength: {SquareLength}"),
                new MetaInfo($"LBlockLength: {LBlockLength}"),
                new MetaInfo($"VertexPerCell: {VertexPerCell}"),
                new MetaInfo($"MaxObjHeight: {MaxObjHeight}"),
                new MetaInfo($"SkyHeight: {SkyHeight}"),
                new MetaInfo($"RoadWidth: {RoadWidth}"),
                new MetaInfo("LandHeightTable", items: LandHeightTable.Select((x, i) => new MetaInfo($"{i}: {x}"))),
            };
            return nodes;
        }
    }
}
