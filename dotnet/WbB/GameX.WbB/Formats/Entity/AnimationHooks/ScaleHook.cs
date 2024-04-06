using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class ScaleHook : AnimationHook, IHaveMetaInfo
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
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is ScaleHook s) nodes.Add(new MetaInfo($"End: {s.End}, Time: {s.Time}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
