using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class CellStruct : IHaveMetaInfo
    {
        public readonly CVertexArray VertexArray;
        public readonly IDictionary<ushort, Polygon> Polygons;
        public readonly ushort[] Portals;
        public readonly BspTree CellBSP;
        public readonly IDictionary<ushort, Polygon> PhysicsPolygons;
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
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"VertexArray", items: (VertexArray as IHaveMetaInfo).GetInfoNodes()),
                new MetaInfo($"Polygons", items: Polygons.Select(x => new MetaInfo($"{x.Key}", items: (x.Value as IHaveMetaInfo).GetInfoNodes()))),
                new MetaInfo($"Portals", items: Portals.Select(x => new MetaInfo($"{x:X8}"))),
                new MetaInfo($"CellBSP", items: (CellBSP as IHaveMetaInfo).GetInfoNodes(tag: BSPType.Cell).First().Items),
                new MetaInfo($"PhysicsPolygons", items: PhysicsPolygons.Select(x => new MetaInfo($"{x.Key}", items: (x.Value as IHaveMetaInfo).GetInfoNodes()))),
                new MetaInfo($"PhysicsBSP", items: (PhysicsBSP as IHaveMetaInfo).GetInfoNodes(tag: BSPType.Physics).First().Items),
                DrawingBSP != null ? new MetaInfo($"DrawingBSP", items: (DrawingBSP as IHaveMetaInfo).GetInfoNodes(tag: BSPType.Drawing).First().Items) : null,
            };
            return nodes;
        }
    }
}
