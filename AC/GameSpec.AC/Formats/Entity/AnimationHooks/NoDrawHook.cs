using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class NoDrawHook : AnimationHook, IGetMetadataInfo
    {
        public readonly uint NoDraw;

        public NoDrawHook(AnimationHook hook) : base(hook) { }
        public NoDrawHook(BinaryReader r) : base(r)
            => NoDraw = r.ReadUInt32();

        //: Entity.NoDrawHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is NoDrawHook s)
            {
                nodes.Add(new MetadataInfo($"NoDraw: {s.NoDraw}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
