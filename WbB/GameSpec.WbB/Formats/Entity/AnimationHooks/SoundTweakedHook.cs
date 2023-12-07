using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity.AnimationHooks
{
    public class SoundTweakedHook : AnimationHook, IGetMetadataInfo
    {
        public readonly uint SoundID;
        public readonly float Priority;
        public readonly float Probability;
        public readonly float Volume;

        public SoundTweakedHook(AnimationHook hook) : base(hook) { }
        public SoundTweakedHook(BinaryReader r) : base(r)
        {
            SoundID = r.ReadUInt32();
            Priority = r.ReadSingle();
            Probability = r.ReadSingle();
            Volume = r.ReadSingle();
        }

        //: Entity.SoundTweakedHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is SoundTweakedHook s)
            {
                nodes.Add(new MetadataInfo($"SoundID: {s.SoundID:X8}"));
                nodes.Add(new MetadataInfo($"Priority: {s.Priority}"));
                nodes.Add(new MetadataInfo($"Probability: {s.Probability}"));
                nodes.Add(new MetadataInfo($"Volume: {s.Volume}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
