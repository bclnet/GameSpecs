using GameSpec.WbB.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameSpec.WbB.Formats.Entity
{
    public class BspLeaf : BspNode, IGetMetadataInfo
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
                InPolys = r.ReadL32Array<ushort>(sizeof(ushort));
            }
        }

        //: Entity.BSPLeaf
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Type: {Type}"),
                new MetadataInfo($"LeafIndex: {LeafIndex}"),
            };
            if ((BSPType)tag == BSPType.Physics)
                nodes.AddRange(new[] {
                    new MetadataInfo($"Solid: {Solid}"),
                    new MetadataInfo($"Sphere: {Sphere}"),
                    new MetadataInfo($"InPolys: {string.Join(", ", InPolys)}"),
                });
            return nodes;
        }
    }
}
