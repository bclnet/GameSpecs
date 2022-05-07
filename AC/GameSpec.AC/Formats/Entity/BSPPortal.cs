using GameSpec.AC.Formats.Props;
using GameSpec.Explorer;
using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameSpec.AC.Formats.Entity
{
    public class BSPPortal : BSPNode, IGetExplorerInfo
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
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Type: {Type:X8}"),
                new ExplorerInfoNode($"Splitting Plane: {SplittingPlane}"),
                PosNode != null ? new ExplorerInfoNode("PosNode", items: (PosNode as IGetExplorerInfo).GetInfoNodes(tag: tag)) : null,
                NegNode != null ? new ExplorerInfoNode("NegNode", items: (NegNode as IGetExplorerInfo).GetInfoNodes(tag: tag)) : null,
            };
            if ((BSPType)tag != BSPType.Drawing) return nodes;
            nodes.Add(new ExplorerInfoNode($"Sphere: {Sphere}"));
            nodes.Add(new ExplorerInfoNode($"InPolys: {string.Join(", ", InPolys)}"));
            nodes.Add(new ExplorerInfoNode("InPortals", items: InPortals.Select(x => new ExplorerInfoNode($"{x}"))));
            return nodes;
        }
    }
}
