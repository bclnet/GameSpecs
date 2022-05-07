using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class ReplaceObjectHook : AnimationHook, IGetExplorerInfo
    {
        public readonly AnimationPartChange APChange;

        public ReplaceObjectHook(AnimationHook hook) : base(hook) { }
        public ReplaceObjectHook(BinaryReader r) : base(r)
            => APChange = new AnimationPartChange(r, r.ReadUInt16());

        //: Entity.ReplaceObjectHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is ReplaceObjectHook s)
            {
                nodes.AddRange((s.APChange as IGetExplorerInfo).GetInfoNodes(tag: tag));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
