using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class NoDrawHook : AnimationHook, IHaveMetaInfo
    {
        public readonly uint NoDraw;

        public NoDrawHook(AnimationHook hook) : base(hook) { }
        public NoDrawHook(BinaryReader r) : base(r)
            => NoDraw = r.ReadUInt32();

        //: Entity.NoDrawHook
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is NoDrawHook s) nodes.Add(new MetaInfo($"NoDraw: {s.NoDraw}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
