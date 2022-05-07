using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class AnimationFrame : IGetExplorerInfo
    {
        public readonly Frame[] Frames;
        public readonly AnimationHook[] Hooks;

        public AnimationFrame(BinaryReader r, uint numParts)
        {
            Frames = r.ReadTArray(x => new Frame(r), (int)numParts);
            Hooks = r.ReadL32Array(AnimationHook.Factory);
        }

        //: Entity.AnimationFrame
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Frames", items: Frames.Select(x => new ExplorerInfoNode($"{x}"))),
                ExplorerInfoNode.WrapWithGroup(Hooks, "Hooks", Hooks.Select(x => new ExplorerInfoNode($"HookType: {x.HookType}", items: (AnimationHook.Factory(x) as IGetExplorerInfo).GetInfoNodes(tag: tag)))),
            };
            return nodes;
        }
    }
}
