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
    public class BspLeaf : BspNode, IHaveMetaInfo
    {
        public readonly int LeafIndex;
        public readonly int Solid;

        public BspLeaf(BinaryReader r, BSPType treeType) : base()
        {
            Type = Encoding.ASCII.GetString(r.ReadBytes(4)).Reverse();
            LeafIndex = r.ReadInt32();
            if (treeType == BSPType.Physics)
            {
                Solid = r.ReadInt32();
                // Note that if Solid is equal to 0, these values will basically be null. Still read them, but they don't mean anything.
                Sphere = new Sphere(r);
                InPolys = r.ReadL32TArray<ushort>(sizeof(ushort));
            }
        }

        //: Entity.BSPLeaf
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Type: {Type}"),
                new MetaInfo($"LeafIndex: {LeafIndex}"),
            };
            if ((BSPType)tag == BSPType.Physics)
                nodes.AddRange(new[] {
                    new MetaInfo($"Solid: {Solid}"),
                    new MetaInfo($"Sphere: {Sphere}"),
                    new MetaInfo($"InPolys: {string.Join(", ", InPolys)}"),
                });
            return nodes;
        }
    }
}
