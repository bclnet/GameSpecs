using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class DiffusePartHook : AnimationHook, IGetMetadataInfo
    {
        public readonly uint Part;
        public readonly float Start;
        public readonly float End;
        public readonly float Time;

        public DiffusePartHook(AnimationHook hook) : base(hook) { }
        public DiffusePartHook(BinaryReader r) : base(r)
        {
            Part = r.ReadUInt32();
            Start = r.ReadSingle();
            End = r.ReadSingle();
            Time = r.ReadSingle();
        }

        //: Entity.DiffusePartHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is DiffusePartHook s)
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
