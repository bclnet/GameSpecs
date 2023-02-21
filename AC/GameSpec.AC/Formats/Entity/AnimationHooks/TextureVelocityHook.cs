using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class TextureVelocityHook : AnimationHook, IGetMetadataInfo
    {
        public readonly float USpeed;
        public readonly float VSpeed;

        public TextureVelocityHook(AnimationHook hook) : base(hook) { }
        public TextureVelocityHook(BinaryReader r) : base(r)
        {
            USpeed = r.ReadSingle();
            VSpeed = r.ReadSingle();
        }

        //: Entity.TextureVelocityHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is TextureVelocityHook s) nodes.Add(new MetadataInfo($"USpeed: {s.USpeed}, VSpeed: {s.VSpeed}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
