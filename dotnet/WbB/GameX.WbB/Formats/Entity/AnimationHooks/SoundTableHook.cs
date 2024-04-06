using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class SoundTableHook : AnimationHook, IHaveMetaInfo
    {
        public readonly uint SoundType;

        public SoundTableHook(AnimationHook hook) : base(hook) { }
        public SoundTableHook(BinaryReader r) : base(r)
            => SoundType = r.ReadUInt32();

        //: Entity.SoundTableHook
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is SoundTableHook s) nodes.Add(new MetaInfo($"SoundType: {(Sound)s.SoundType}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
