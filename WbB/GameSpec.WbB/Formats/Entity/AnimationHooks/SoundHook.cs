using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity.AnimationHooks
{
    public class SoundHook : AnimationHook, IGetMetadataInfo
    {
        public readonly uint Id;

        public SoundHook(AnimationHook hook) : base(hook) { }
        public SoundHook(BinaryReader r) : base(r)
            => Id = r.ReadUInt32();

        //: Entity.SoundHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is SoundHook s) nodes.Add(new MetadataInfo($"Id: {s.Id:X8}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
