using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class CreateParticleHook : AnimationHook, IHaveMetaInfo
    {
        public readonly uint EmitterInfoId;
        public readonly uint PartIndex;
        public readonly Frame Offset;
        public readonly uint EmitterId;

        public CreateParticleHook(AnimationHook hook) : base(hook) { }
        public CreateParticleHook(BinaryReader r) : base(r)
        {
            EmitterInfoId = r.ReadUInt32();
            PartIndex = r.ReadUInt32();
            Offset = new Frame(r);
            EmitterId = r.ReadUInt32();
        }

        //: Entity.CreateParticleHook
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is CreateParticleHook s)
            {
                nodes.Add(new MetaInfo($"EmitterInfoId: {s.EmitterInfoId:X8}"));
                nodes.Add(new MetaInfo($"PartIndex: {(int)s.PartIndex}"));
                nodes.Add(new MetaInfo($"Offset: {s.Offset}"));
                nodes.Add(new MetaInfo($"EmitterId: {s.EmitterId}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
