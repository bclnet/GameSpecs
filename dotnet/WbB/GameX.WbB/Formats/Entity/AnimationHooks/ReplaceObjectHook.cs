using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class ReplaceObjectHook : AnimationHook, IHaveMetaInfo
    {
        public readonly AnimationPartChange APChange;

        public ReplaceObjectHook(AnimationHook hook) : base(hook) { }
        public ReplaceObjectHook(BinaryReader r) : base(r)
            => APChange = new AnimationPartChange(r, r.ReadUInt16());

        //: Entity.ReplaceObjectHook
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is ReplaceObjectHook s) nodes.AddRange((s.APChange as IHaveMetaInfo).GetInfoNodes(tag: tag));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
