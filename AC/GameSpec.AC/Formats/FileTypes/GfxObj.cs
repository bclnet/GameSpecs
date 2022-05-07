using GameSpec.Explorer;
using GameSpec.AC.Formats.Entity;
using GameSpec.AC.Formats.Props;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using GameSpec.Formats;

namespace GameSpec.AC.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x01. 
    /// These are used both on their own for some pre-populated structures in the world (trees, buildings, etc) or make up SetupModel (0x02) objects.
    /// </summary>
    [PakFileType(PakFileType.GraphicsObject)]
    public class GfxObj : FileType, IGetExplorerInfo
    {
        public readonly GfxObjFlags Flags;
        public readonly uint[] Surfaces; // also referred to as m_rgSurfaces in the client
        public readonly CVertexArray VertexArray;
        public readonly Dictionary<ushort, Polygon> PhysicsPolygons;
        public readonly BSPTree PhysicsBSP;
        public readonly Vector3 SortCenter;
        public readonly Dictionary<ushort, Polygon> Polygons;
        public readonly BSPTree DrawingBSP;
        public readonly uint DIDDegrade;

        public GfxObj(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Flags = (GfxObjFlags)r.ReadUInt32();
            Surfaces = r.ReadC32Array<uint>(sizeof(uint));
            VertexArray = new CVertexArray(r);
            // Has Physics 
            if ((Flags & GfxObjFlags.HasPhysics) != 0)
            {
                PhysicsPolygons = r.ReadC32Many<ushort, Polygon>(sizeof(ushort), x => new Polygon(x));
                PhysicsBSP = new BSPTree(r, BSPType.Physics);
            }
            SortCenter = r.ReadVector3();
            // Has Drawing 
            if ((Flags & GfxObjFlags.HasDrawing) != 0)
            {
                Polygons = r.ReadC32Many<ushort, Polygon>(sizeof(ushort), x => new Polygon(x));
                DrawingBSP = new BSPTree(r, BSPType.Drawing);
            }
            if ((Flags & GfxObjFlags.HasDIDDegrade) != 0) DIDDegrade = r.ReadUInt32();
        }

        //: FileTypes.GfxObj
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode(null, new ExplorerContentTab { Type = "Model", Name = "Model", Value = this }),
                new ExplorerInfoNode($"{nameof(GfxObj)}: {Id:X8}", items: new List<ExplorerInfoNode> {
                    new ExplorerInfoNode($"Surfaces", items: Surfaces.Select(x => new ExplorerInfoNode($"{x:X8}", clickable: true))),
                    new ExplorerInfoNode($"VertexArray", items: (VertexArray as IGetExplorerInfo).GetInfoNodes(resource, file)),
                    Flags.HasFlag(GfxObjFlags.HasPhysics) ? new ExplorerInfoNode($"PhysicsPolygons", items: PhysicsPolygons.Select(x => new ExplorerInfoNode($"{x.Key}", items: (x.Value as IGetExplorerInfo).GetInfoNodes()))) : null,
                    Flags.HasFlag(GfxObjFlags.HasPhysics) ? new ExplorerInfoNode($"PhysicsBSP", items: (PhysicsBSP as IGetExplorerInfo).GetInfoNodes(tag: BSPType.Physics).First().Items) : null,
                    new ExplorerInfoNode($"SortCenter: {SortCenter}"),
                    Flags.HasFlag(GfxObjFlags.HasDrawing) ? new ExplorerInfoNode($"Polygons", items: Polygons.Select(x => new ExplorerInfoNode($"{x.Key}", items: (x.Value as IGetExplorerInfo).GetInfoNodes()))) : null,
                    Flags.HasFlag(GfxObjFlags.HasDrawing) ? new ExplorerInfoNode($"DrawingBSP", items: (DrawingBSP as IGetExplorerInfo).GetInfoNodes(tag: BSPType.Drawing).First().Items) : null,
                    Flags.HasFlag(GfxObjFlags.HasDIDDegrade) ? new ExplorerInfoNode($"DIDDegrade: {DIDDegrade:X8}", clickable: true) : null,
                })
            };
            return nodes;
        }
    }
}
