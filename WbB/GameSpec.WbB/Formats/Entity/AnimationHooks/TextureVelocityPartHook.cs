using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity.AnimationHooks
{
    public class TextureVelocityPartHook : AnimationHook, IGetMetadataInfo
    {
        public readonly uint PartIndex;
        public readonly float USpeed;
        public readonly float VSpeed;

        public TextureVelocityPartHook(AnimationHook hook) : base(hook) { }
        public TextureVelocityPartHook(BinaryReader r) : base(r)
        {
            PartIndex = r.ReadUInt32();
            USpeed = r.ReadSingle();
            VSpeed = r.ReadSingle();
        }

        //: Entity.TextureVelocityPartHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is TextureVelocityPartHook s)
            {
                nodes.Add(new MetadataInfo($"PartIndex: {s.PartIndex}"));
                nodes.Add(new MetadataInfo($"USpeed: {s.USpeed}, VSpeed: {s.VSpeed}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
