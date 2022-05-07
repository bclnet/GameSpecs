using GameSpec.AC.Formats.Props;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class CellStruct : IGetExplorerInfo
    {
        public readonly CVertexArray VertexArray;
        public readonly Dictionary<ushort, Polygon> Polygons;
        public readonly ushort[] Portals;
        public readonly BSPTree CellBSP;
        public readonly Dictionary<ushort, Polygon> PhysicsPolygons;
        public readonly BSPTree PhysicsBSP;
        public readonly BSPTree DrawingBSP;

        public CellStruct(BinaryReader r)
        {
            var numPolygons = r.ReadUInt32();
            var numPhysicsPolygons = r.ReadUInt32();
            var numPortals = r.ReadUInt32();
            VertexArray = new CVertexArray(r);
            Polygons = r.ReadTMany<ushort, Polygon>(sizeof(ushort), x => new Polygon(x), (int)numPolygons);
            Portals = r.ReadTArray<ushort>(sizeof(ushort), (int)numPortals); r.AlignBoundary();
            CellBSP = new BSPTree(r, BSPType.Cell);
            PhysicsPolygons = r.ReadTMany<ushort, Polygon>(sizeof(ushort), x => new Polygon(x), (int)numPhysicsPolygons);
            PhysicsBSP = new BSPTree(r, BSPType.Physics);
            var hasDrawingBSP = r.ReadUInt32();
            if (hasDrawingBSP != 0) DrawingBSP = new BSPTree(r, BSPType.Drawing);
            r.AlignBoundary();
        }

        //: Entity.CellStruct
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"VertexArray", items: (VertexArray as IGetExplorerInfo).GetInfoNodes()),
                new ExplorerInfoNode($"Polygons", items: Polygons.Select(x => new ExplorerInfoNode($"{x.Key}", items: (x.Value as IGetExplorerInfo).GetInfoNodes()))),
                new ExplorerInfoNode($"Portals", items: Portals.Select(x => new ExplorerInfoNode($"{x:X8}"))),
                new ExplorerInfoNode($"CellBSP", items: (CellBSP as IGetExplorerInfo).GetInfoNodes(tag: BSPType.Cell).First().Items),
                new ExplorerInfoNode($"PhysicsPolygons", items: PhysicsPolygons.Select(x => new ExplorerInfoNode($"{x.Key}", items: (x.Value as IGetExplorerInfo).GetInfoNodes()))),
                new ExplorerInfoNode($"PhysicsBSP", items: (PhysicsBSP as IGetExplorerInfo).GetInfoNodes(tag: BSPType.Physics).First().Items),
                DrawingBSP != null ? new ExplorerInfoNode($"DrawingBSP", items: (DrawingBSP as IGetExplorerInfo).GetInfoNodes(tag: BSPType.Drawing).First().Items) : null,
            };
            return nodes;
        }
    }
}
