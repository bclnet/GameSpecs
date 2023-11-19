using GameSpec.WbB.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class BspTree : IGetMetadataInfo
    {
        public readonly BspNode RootNode;

        public BspTree(BinaryReader r, BSPType treeType)
            => RootNode = BspNode.Factory(r, treeType);

        //: Entity.BSPTree
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Root", items: (RootNode as IGetMetadataInfo).GetInfoNodes(tag: tag)),
            };
            return nodes;
        }
    }
}
