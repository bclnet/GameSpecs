using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class SceneType : IGetMetadataInfo
    {
        public uint StbIndex;
        public uint[] Scenes;

        public SceneType(BinaryReader r)
        {
            StbIndex = r.ReadUInt32();
            Scenes = r.ReadL32Array<uint>(sizeof(uint));
        }

        //: Entity.SceneType
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"SceneTableIdx: {StbIndex}"),
                new MetadataInfo("Scenes", items: Scenes.Select(x => new MetadataInfo($"{x:X8}", clickable: true))),
            };
            return nodes;
        }
    }
}
