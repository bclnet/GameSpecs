using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class EtherealHook : AnimationHook, IHaveMetaInfo
    {
        public readonly int Ethereal;

        public EtherealHook(AnimationHook hook) : base(hook) { }
        public EtherealHook(BinaryReader r) : base(r)
            => Ethereal = r.ReadInt32();

        //: Entity.EtherealHook
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is EtherealHook s) nodes.Add(new MetaInfo($"Ethereal: {s.Ethereal}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
