using GameSpec.AC.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class SoundTableHook : AnimationHook, IGetMetadataInfo
    {
        public readonly uint SoundType;

        public SoundTableHook(AnimationHook hook) : base(hook) { }
        public SoundTableHook(BinaryReader r) : base(r)
            => SoundType = r.ReadUInt32();

        //: Entity.SoundTableHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is SoundTableHook s) nodes.Add(new MetadataInfo($"SoundType: {(Sound)s.SoundType}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
