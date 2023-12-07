using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity.AnimationHooks
{
    public class LuminousHook : AnimationHook, IGetMetadataInfo
    {
        public readonly float Start;
        public readonly float End;
        public readonly float Time;

        public LuminousHook(AnimationHook hook) : base(hook) { }
        public LuminousHook(BinaryReader r) : base(r)
        {
            Start = r.ReadSingle();
            End = r.ReadSingle();
            Time = r.ReadSingle();
        }

        //: Entity.LuminousHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is LuminousHook s) nodes.Add(new MetadataInfo($"Start: {s.Start}, End: {s.End}, Time: {s.Time}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
