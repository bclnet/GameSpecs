using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class SoundTweakedHook : AnimationHook, IHaveMetaInfo
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
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is SoundTweakedHook s)
            {
                nodes.Add(new MetaInfo($"SoundID: {s.SoundID:X8}"));
                nodes.Add(new MetaInfo($"Priority: {s.Priority}"));
                nodes.Add(new MetaInfo($"Probability: {s.Probability}"));
                nodes.Add(new MetaInfo($"Volume: {s.Volume}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
