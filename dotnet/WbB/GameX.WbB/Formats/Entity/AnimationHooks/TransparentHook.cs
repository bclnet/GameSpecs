using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class TransparentHook : AnimationHook, IHaveMetaInfo
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
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is TransparentHook s) nodes.Add(new MetaInfo($"Start: {s.Start}, End: {s.End}, Time: {s.Time}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
