using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity.AnimationHooks
{
    public class TransparentPartHook : AnimationHook, IGetMetadataInfo
    {
        public readonly uint Part;
        public readonly float Start;
        public readonly float End;
        public readonly float Time;

        public TransparentPartHook(AnimationHook hook) : base(hook) { }
        public TransparentPartHook(BinaryReader r) : base(r)
        {
            Part = r.ReadUInt32();
            Start = r.ReadSingle();
            End = r.ReadSingle();
            Time = r.ReadSingle();
        }

        //: Entity.TransparentPartHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is TransparentPartHook s)
            {
                nodes.Add(new MetadataInfo($"Part: {s.Part}"));
                nodes.Add(new MetadataInfo($"Start: {s.Start}, End: {s.End}, Time: {s.Time}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
