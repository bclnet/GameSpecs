using GameSpec.WbB.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class CellStruct : IGetMetadataInfo
    {
        public readonly CVertexArray VertexArray;
        public readonly Dictionary<ushort, Polygon> Polygons;
        public readonly ushort[] Portals;
        public readonly BspTree CellBSP;
        public readonly Dictionary<ushort, Polygon> PhysicsPolygons;
        public readonly BspTree PhysicsBSP;
        public readonly BspTree DrawingBSP;

        public CellStruct(BinaryReader r)
        {
            var numPolygons = r.ReadUInt32();
            var numPhysicsPolygons = r.ReadUInt32();
            var numPortals = r.ReadUInt32();
            VertexArray = new CVertexArray(r);
            Polygons = r.ReadTMany<ushort, Polygon>(sizeof(ushort), x => new Polygon(x), (int)numPolygons);
            Portals = r.ReadTArray<ushort>(sizeof(ushort), (int)numPortals); r.Align();
            CellBSP = new BspTree(r, BSPType.Cell);
            PhysicsPolygons = r.ReadTMany<ushort, Polygon>(sizeof(ushort), x => new Polygon(x), (int)numPhysicsPolygons);
            PhysicsBSP = new BspTree(r, BSPType.Physics);
            var hasDrawingBSP = r.ReadUInt32();
            if (hasDrawingBSP != 0) DrawingBSP = new BspTree(r, BSPType.Drawing);
            r.Align();
        }

        //: Entity.CellStruct
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"VertexArray", items: (VertexArray as IGetMetadataInfo).GetInfoNodes()),
                new MetadataInfo($"Polygons", items: Polygons.Select(x => new MetadataInfo($"{x.Key}", items: (x.Value as IGetMetadataInfo).GetInfoNodes()))),
                new MetadataInfo($"Portals", items: Portals.Select(x => new MetadataInfo($"{x:X8}"))),
                new MetadataInfo($"CellBSP", items: (CellBSP as IGetMetadataInfo).GetInfoNodes(tag: BSPType.Cell).First().Items),
                new MetadataInfo($"PhysicsPolygons", items: PhysicsPolygons.Select(x => new MetadataInfo($"{x.Key}", items: (x.Value as IGetMetadataInfo).GetInfoNodes()))),
                new MetadataInfo($"PhysicsBSP", items: (PhysicsBSP as IGetMetadataInfo).GetInfoNodes(tag: BSPType.Physics).First().Items),
                DrawingBSP != null ? new MetadataInfo($"DrawingBSP", items: (DrawingBSP as IGetMetadataInfo).GetInfoNodes(tag: BSPType.Drawing).First().Items) : null,
            };
            return nodes;
        }
    }
}
