using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class CreateParticleHook : AnimationHook, IGetMetadataInfo
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
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is CreateParticleHook s)
            {
                nodes.Add(new MetadataInfo($"EmitterInfoId: {s.EmitterInfoId:X8}"));
                nodes.Add(new MetadataInfo($"PartIndex: {(int)s.PartIndex}"));
                nodes.Add(new MetadataInfo($"Offset: {s.Offset}"));
                nodes.Add(new MetadataInfo($"EmitterId: {s.EmitterId}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
