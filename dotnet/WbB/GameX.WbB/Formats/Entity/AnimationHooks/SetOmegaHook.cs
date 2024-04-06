using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class SetOmegaHook : AnimationHook, IHaveMetaInfo
    {
        public readonly Vector3 Axis;

        public SetOmegaHook(AnimationHook hook) : base(hook) { }
        public SetOmegaHook(BinaryReader r) : base(r)
            => Axis = r.ReadVector3();

        //: Entity.SetOmegaHook
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is SetOmegaHook s) nodes.Add(new MetaInfo($"Axis: {s.Axis}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
