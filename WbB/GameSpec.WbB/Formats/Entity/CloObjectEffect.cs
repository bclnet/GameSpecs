using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class CloObjectEffect : IHaveMetaInfo
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
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Index: {Index}"),
                new MetaInfo($"Model ID: {ModelId:X8}", clickable: true),
                new MetaInfo($"Texture Effects", items: CloTextureEffects.Select(x=> new MetaInfo($"{x}", clickable: true))),
            };
            return nodes;
        }
    }
}
