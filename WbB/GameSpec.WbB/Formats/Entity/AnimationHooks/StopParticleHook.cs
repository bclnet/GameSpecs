using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity.AnimationHooks
{
    public class StopParticleHook : AnimationHook, IGetMetadataInfo
    {
        public readonly uint EmitterId;

        public StopParticleHook(AnimationHook hook) : base(hook) { }
        public StopParticleHook(BinaryReader r) : base(r)
            => EmitterId = r.ReadUInt32();

        //: Entity.StopParticleHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is StopParticleHook s) nodes.Add(new MetadataInfo($"EmitterId: {s.EmitterId}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
