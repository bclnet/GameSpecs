using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class DefaultScriptPartHook : AnimationHook, IHaveMetaInfo
    {
        public readonly uint PartIndex;

        public DefaultScriptPartHook(AnimationHook hook) : base(hook) { }
        public DefaultScriptPartHook(BinaryReader r) : base(r)
            => PartIndex = r.ReadUInt32();

        //: Entity.DefaultScriptPartHook
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is DefaultScriptPartHook s) nodes.Add(new MetaInfo($"PartIndex: {s.PartIndex}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
