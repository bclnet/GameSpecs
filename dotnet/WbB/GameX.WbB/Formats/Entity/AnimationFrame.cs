using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class AnimationFrame : IHaveMetaInfo
    {
        public readonly Frame[] Frames;
        public readonly AnimationHook[] Hooks;

        public AnimationFrame(BinaryReader r, uint numParts)
        {
            Frames = r.ReadFArray(x => new Frame(r), (int)numParts);
            Hooks = r.ReadL32FArray(AnimationHook.Factory);
        }

        //: Entity.AnimationFrame
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Frames", items: Frames.Select(x => new MetaInfo($"{x}"))),
                MetaInfo.WrapWithGroup(Hooks, "Hooks", Hooks.Select(x => new MetaInfo($"HookType: {x.HookType}", items: (AnimationHook.Factory(x) as IHaveMetaInfo).GetInfoNodes(tag: tag)))),
            };
            return nodes;
        }
    }
}
