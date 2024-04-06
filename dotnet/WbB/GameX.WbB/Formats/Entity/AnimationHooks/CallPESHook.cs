using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class CallPESHook : AnimationHook, IHaveMetaInfo
    {
        public readonly uint PES;
        public readonly float Pause;

        public CallPESHook(AnimationHook hook) : base(hook) { }
        public CallPESHook(BinaryReader r) : base(r)
        {
            PES = r.ReadUInt32();
            Pause = r.ReadSingle();
        }

        //: Entity.CallPESHook
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is CallPESHook s) nodes.Add(new MetaInfo($"PES: {s.PES:X8}, Pause: {s.Pause}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
