using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity.AnimationHooks
{
    public class EtherealHook : AnimationHook, IGetMetadataInfo
    {
        public readonly int Ethereal;

        public EtherealHook(AnimationHook hook) : base(hook) { }
        public EtherealHook(BinaryReader r) : base(r)
            => Ethereal = r.ReadInt32();

        //: Entity.EtherealHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is EtherealHook s) nodes.Add(new MetadataInfo($"Ethereal: {s.Ethereal}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
