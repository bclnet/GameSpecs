using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class LuminousPartHook : AnimationHook, IGetMetadataInfo
    {
        public readonly uint Part;
        public readonly float Start;
        public readonly float End;
        public readonly float Time;

        public LuminousPartHook(AnimationHook hook) : base(hook) { }
        public LuminousPartHook(BinaryReader r) : base(r)
        {
            Part = r.ReadUInt32();
            Start = r.ReadSingle();
            End = r.ReadSingle();
            Time = r.ReadSingle();
        }

        //: Entity.LuminousPartHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is LuminousPartHook s)
            {
                nodes.Add(new MetadataInfo($"Part: {s.Part}"));
                nodes.Add(new MetadataInfo($"Start: {s.Start}"));
                nodes.Add(new MetadataInfo($"End: {s.End}"));
                nodes.Add(new MetadataInfo($"Time: {s.Time}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
