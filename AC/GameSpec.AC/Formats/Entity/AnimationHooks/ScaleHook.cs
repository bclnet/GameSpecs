using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class ScaleHook : AnimationHook, IGetMetadataInfo
    {
        public readonly float End;
        public readonly float Time;

        public ScaleHook(AnimationHook hook) : base(hook) { }
        public ScaleHook(BinaryReader r) : base(r)
        {
            End = r.ReadSingle();
            Time = r.ReadSingle();
        }

        //: Entity.ScaleHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is ScaleHook s) nodes.Add(new MetadataInfo($"End: {s.End}, Time: {s.Time}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
