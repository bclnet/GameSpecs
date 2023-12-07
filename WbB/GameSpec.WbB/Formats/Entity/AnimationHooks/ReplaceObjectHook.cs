using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity.AnimationHooks
{
    public class ReplaceObjectHook : AnimationHook, IGetMetadataInfo
    {
        public readonly AnimationPartChange APChange;

        public ReplaceObjectHook(AnimationHook hook) : base(hook) { }
        public ReplaceObjectHook(BinaryReader r) : base(r)
            => APChange = new AnimationPartChange(r, r.ReadUInt16());

        //: Entity.ReplaceObjectHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is ReplaceObjectHook s) nodes.AddRange((s.APChange as IGetMetadataInfo).GetInfoNodes(tag: tag));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
