using GameX.Meta;
using GameX.WbB.Formats.Entity;
using GameX.WbB.Formats.Props;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using GameX.Formats;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// These are client_portal.dat files starting with 0x01. 
    /// These are used both on their own for some pre-populated structures in the world (trees, buildings, etc) or make up SetupModel (0x02) objects.
    /// </summary>
    [PakFileType(PakFileType.GraphicsObject)]
    public class GfxObj : FileType, IHaveMetaInfo
    {
        public readonly GfxObjFlags Flags;
        public readonly uint[] Surfaces; // also referred to as m_rgSurfaces in the client
        public readonly CVertexArray VertexArray;
        public readonly IDictionary<ushort, Polygon> PhysicsPolygons;
        public readonly BspTree PhysicsBSP;
        public readonly Vector3 SortCenter;
        public readonly IDictionary<ushort, Polygon> Polygons;
        public readonly BspTree DrawingBSP;
        public readonly uint DIDDegrade;

        public GfxObj(BinaryReader r)
        {
            Id = r.ReadUInt32();
            Flags = (GfxObjFlags)r.ReadUInt32();
            Surfaces = r.ReadC32TArray<uint>(sizeof(uint));
            VertexArray = new CVertexArray(r);
            // Has Physics 
            if ((Flags & GfxObjFlags.HasPhysics) != 0)
            {
                PhysicsPolygons = r.ReadC32TMany<ushort, Polygon>(sizeof(ushort), x => new Polygon(x));
                PhysicsBSP = new BspTree(r, BSPType.Physics);
            }
            SortCenter = r.ReadVector3();
            // Has Drawing 
            if ((Flags & GfxObjFlags.HasDrawing) != 0)
            {
                Polygons = r.ReadC32TMany<ushort, Polygon>(sizeof(ushort), x => new Polygon(x));
                DrawingBSP = new BspTree(r, BSPType.Drawing);
            }
            if ((Flags & GfxObjFlags.HasDIDDegrade) != 0) DIDDegrade = r.ReadUInt32();
        }

        //: FileTypes.GfxObj
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Model", Name = "Model", Value = this }),
                new MetaInfo($"{nameof(GfxObj)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo($"Surfaces", items: Surfaces.Select(x => new MetaInfo($"{x:X8}", clickable: true))),
                    new MetaInfo($"VertexArray", items: (VertexArray as IHaveMetaInfo).GetInfoNodes(resource, file)),
                    Flags.HasFlag(GfxObjFlags.HasPhysics) ? new MetaInfo($"PhysicsPolygons", items: PhysicsPolygons.Select(x => new MetaInfo($"{x.Key}", items: (x.Value as IHaveMetaInfo).GetInfoNodes()))) : null,
                    Flags.HasFlag(GfxObjFlags.HasPhysics) ? new MetaInfo($"PhysicsBSP", items: (PhysicsBSP as IHaveMetaInfo).GetInfoNodes(tag: BSPType.Physics).First().Items) : null,
                    new MetaInfo($"SortCenter: {SortCenter}"),
                    Flags.HasFlag(GfxObjFlags.HasDrawing) ? new MetaInfo($"Polygons", items: Polygons.Select(x => new MetaInfo($"{x.Key}", items: (x.Value as IHaveMetaInfo).GetInfoNodes()))) : null,
                    Flags.HasFlag(GfxObjFlags.HasDrawing) ? new MetaInfo($"DrawingBSP", items: (DrawingBSP as IHaveMetaInfo).GetInfoNodes(tag: BSPType.Drawing).First().Items) : null,
                    Flags.HasFlag(GfxObjFlags.HasDIDDegrade) ? new MetaInfo($"DIDDegrade: {DIDDegrade:X8}", clickable: true) : null,
                })
            };
            return nodes;
        }
    }
}
