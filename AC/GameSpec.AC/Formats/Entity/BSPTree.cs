using GameSpec.AC.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class BSPTree : IGetMetadataInfo
    {
        public readonly BSPNode RootNode;

        public BSPTree(BinaryReader r, BSPType treeType)
            => RootNode = BSPNode.Factory(r, treeType);

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
