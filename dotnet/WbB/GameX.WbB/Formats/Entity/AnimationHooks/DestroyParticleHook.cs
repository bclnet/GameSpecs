using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class DestroyParticleHook : AnimationHook, IHaveMetaInfo
    {
        public readonly uint EmitterId;

        public DestroyParticleHook(AnimationHook hook) : base(hook) { }
        public DestroyParticleHook(BinaryReader r) : base(r)
            => EmitterId = r.ReadUInt32();

        //: Entity.DestroyParticleHook
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is DestroyParticleHook s) nodes.Add(new MetaInfo($"EmitterId: {s.EmitterId}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
