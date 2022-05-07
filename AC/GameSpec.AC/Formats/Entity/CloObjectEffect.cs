using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class CloObjectEffect : IGetExplorerInfo
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
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Index: {Index}"),
                new ExplorerInfoNode($"Model ID: {ModelId:X8}", clickable: true),
                new ExplorerInfoNode($"Texture Effects", items: CloTextureEffects.Select(x=> new ExplorerInfoNode($"{x}", clickable: true))),
            };
            return nodes;
        }
    }
}
