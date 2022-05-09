using GameSpec.AC.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameSpec.AC.Formats.Entity
{
    public class BSPPortal : BSPNode, IGetMetadataInfo
    {
        public readonly PortalPoly[] InPortals;

        public BSPPortal(BinaryReader r, BSPType treeType) : base()
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
                InPortals = r.ReadTArray(x => new PortalPoly(x), (int)numPortals);
            }
        }

        //: Entity.BSPPortal
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Type: {Type:X8}"),
                new MetadataInfo($"Splitting Plane: {SplittingPlane}"),
                PosNode != null ? new MetadataInfo("PosNode", items: (PosNode as IGetMetadataInfo).GetInfoNodes(tag: tag)) : null,
                NegNode != null ? new MetadataInfo("NegNode", items: (NegNode as IGetMetadataInfo).GetInfoNodes(tag: tag)) : null,
            };
            if ((BSPType)tag != BSPType.Drawing) return nodes;
            nodes.Add(new MetadataInfo($"Sphere: {Sphere}"));
            nodes.Add(new MetadataInfo($"InPolys: {string.Join(", ", InPolys)}"));
            nodes.Add(new MetadataInfo("InPortals", items: InPortals.Select(x => new MetadataInfo($"{x}"))));
            return nodes;
        }
    }
}
