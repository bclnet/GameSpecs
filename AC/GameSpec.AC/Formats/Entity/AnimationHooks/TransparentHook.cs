using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class TransparentHook : AnimationHook, IGetMetadataInfo
    {
        public readonly float Start;
        public readonly float End;
        public readonly float Time;

        public TransparentHook(AnimationHook hook) : base(hook) { }
        public TransparentHook(BinaryReader r) : base(r)
        {
            Start = r.ReadSingle();
            End = r.ReadSingle();
            Time = r.ReadSingle();
        }

        //: Entity.TransparentHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is TransparentHook s)
            {
                nodes.Add(new MetadataInfo($"Start: {s.Start}"));
                nodes.Add(new MetadataInfo($"End: {s.End}"));
                nodes.Add(new MetadataInfo($"Time: {s.Time}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
