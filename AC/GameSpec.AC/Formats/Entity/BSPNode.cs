using GameSpec.AC.Formats.Props;
using GameSpec.Explorer;
using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameSpec.AC.Formats.Entity
{
    public class BSPNode : IGetExplorerInfo
    {
        // These constants are actually strings in the dat file
        const uint PORT = 1347375700; // 0x504F5254
        const uint LEAF = 1279607110; // 0x4C454146
        const uint BPnn = 1112567406; // 0x42506E6E
        const uint BPIn = 1112557934; // 0x4250496E
        const uint BpIN = 1114655054; // 0x4270494E
        const uint BpnN = 1114664526; // 0x42706E4E
        const uint BPIN = 1112557902; // 0x4250494E
        const uint BPnN = 1112567374; // 0x42506E4E

        public string Type;
        public Plane SplittingPlane;
        public BSPNode PosNode;
        public BSPNode NegNode;
        public Sphere Sphere;
        public ushort[] InPolys; // List of PolygonIds

        protected BSPNode() { }
        public BSPNode(BinaryReader r, BSPType treeType)
        {
            Type = Encoding.ASCII.GetString(r.ReadBytes(4)).Reverse();
            switch (Type)
            {
                // These types will unpack the data completely, in their own classes
                case "PORT":
                case "LEAF": throw new Exception();
            }
            SplittingPlane = new Plane(r);
            switch (Type)
            {
                case "BPnn":
                case "BPIn": PosNode = Factory(r, treeType); break;
                case "BpIN":
                case "BpnN": NegNode = Factory(r, treeType); break;
                case "BPIN":
                case "BPnN": PosNode = Factory(r, treeType); NegNode = Factory(r, treeType); break;
            }
            if (treeType == BSPType.Cell) return;
            Sphere = new Sphere(r);
            if (treeType == BSPType.Physics) return;
            InPolys = r.ReadL32Array<ushort>(sizeof(ushort));
        }

        //: Entity.BSPNode
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Type: {Type:X8}"),
                new ExplorerInfoNode($"Splitting Plane: {SplittingPlane}"),
                PosNode != null ? new ExplorerInfoNode("PosNode", items: (PosNode as IGetExplorerInfo).GetInfoNodes(tag: tag)) : null,
                NegNode != null ? new ExplorerInfoNode("NegNode", items: (NegNode as IGetExplorerInfo).GetInfoNodes(tag: tag)) : null,
            };
            if ((BSPType)tag == BSPType.Cell) return nodes;
            nodes.Add(new ExplorerInfoNode($"Sphere: {Sphere}"));
            if ((BSPType)tag == BSPType.Physics) return nodes;
            nodes.Add(new ExplorerInfoNode($"InPolys: {string.Join(", ", InPolys)}"));
            return nodes;
        }

        public static BSPNode Factory(BinaryReader r, BSPType treeType)
        {
            // We peek forward to get the type, then revert our position.
            var type = Encoding.ASCII.GetString(r.ReadBytes(4)).Reverse();
            r.BaseStream.Position -= 4;
            switch (type)
            {
                case "PORT": return new BSPPortal(r, treeType);
                case "LEAF": return new BSPLeaf(r, treeType);
                case "BPnn":
                case "BPIn":
                case "BpIN":
                case "BpnN":
                case "BPIN":
                case "BPnN":
                default: return new BSPNode(r, treeType);
            }
        }
    }
}
