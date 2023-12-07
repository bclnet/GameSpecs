using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class AnimationFrame : IGetMetadataInfo
    {
        public readonly Frame[] Frames;
        public readonly AnimationHook[] Hooks;

        public AnimationFrame(BinaryReader r, uint numParts)
        {
            Frames = r.ReadTArray(x => new Frame(r), (int)numParts);
            Hooks = r.ReadL32Array(AnimationHook.Factory);
        }

        //: Entity.AnimationFrame
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Frames", items: Frames.Select(x => new MetadataInfo($"{x}"))),
                MetadataInfo.WrapWithGroup(Hooks, "Hooks", Hooks.Select(x => new MetadataInfo($"HookType: {x.HookType}", items: (AnimationHook.Factory(x) as IGetMetadataInfo).GetInfoNodes(tag: tag)))),
            };
            return nodes;
        }
    }
}
