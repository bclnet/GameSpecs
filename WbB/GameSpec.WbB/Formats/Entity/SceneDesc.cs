using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class SceneDesc : IGetMetadataInfo
    {
        public readonly SceneType[] SceneTypes;

        public SceneDesc(BinaryReader r)
            => SceneTypes = r.ReadL32Array(x => new SceneType(x));

        //: Entity.SceneDesc
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo("SceneTypes", items: SceneTypes.Select((x, i) => new MetadataInfo($"{i}", items: (x as IGetMetadataInfo).GetInfoNodes()))),
            };
            return nodes;
        }
    }
}
