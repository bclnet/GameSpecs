using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameX.WbB.Formats.Entity
{
    public class BspPortal : BspNode, IHaveMetaInfo
    {
        public readonly PortalPoly[] InPortals;

        public BspPortal(BinaryReader r, BSPType treeType) : base()
        {
            Type = Encoding.ASCII.GetString(r.ReadBytes(4)).Reverse();
            SplittingPlane = new Plane(r);
            PosNode = Factory(r, treeType);
            NegNode = Factory(r, treeType);
            if (treeType == BSPType.Drawing)
            {
                Sphere = new Sphere(r);
                var numPolys = r.ReadUInt32();
                var numPortals = r.ReadUInt32();
                InPolys = r.ReadTArray<ushort>(sizeof(ushort), (int)numPolys);
                InPortals = r.ReadFArray(x => new PortalPoly(x), (int)numPortals);
            }
        }

        //: Entity.BSPPortal
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Type: {Type:X8}"),
                new MetaInfo($"Splitting Plane: {SplittingPlane}"),
                PosNode != null ? new MetaInfo("PosNode", items: (PosNode as IHaveMetaInfo).GetInfoNodes(tag: tag)) : null,
                NegNode != null ? new MetaInfo("NegNode", items: (NegNode as IHaveMetaInfo).GetInfoNodes(tag: tag)) : null,
            };
            if ((BSPType)tag != BSPType.Drawing) return nodes;
            nodes.Add(new MetaInfo($"Sphere: {Sphere}"));
            nodes.Add(new MetaInfo($"InPolys: {string.Join(", ", InPolys)}"));
            nodes.Add(new MetaInfo("InPortals", items: InPortals.Select(x => new MetaInfo($"{x}"))));
            return nodes;
        }
    }
}
