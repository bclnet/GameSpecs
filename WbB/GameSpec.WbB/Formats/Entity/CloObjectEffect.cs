using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class CloObjectEffect : IGetMetadataInfo
    {
        public readonly uint Index;
        public readonly uint ModelId;
        public readonly CloTextureEffect[] CloTextureEffects;

        public CloObjectEffect(BinaryReader r)
        {
            Index = r.ReadUInt32();
            ModelId = r.ReadUInt32();
            CloTextureEffects = r.ReadL32Array(x => new CloTextureEffect(x));
        }

        //: Entity.ClothingObjectEffect
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Index: {Index}"),
                new MetadataInfo($"Model ID: {ModelId:X8}", clickable: true),
                new MetadataInfo($"Texture Effects", items: CloTextureEffects.Select(x=> new MetadataInfo($"{x}", clickable: true))),
            };
            return nodes;
        }
    }
}
