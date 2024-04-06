using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class SetLightHook : AnimationHook, IHaveMetaInfo
    {
        public readonly int LightsOn;

        public SetLightHook(AnimationHook hook) : base(hook) { }
        public SetLightHook(BinaryReader r) : base(r)
            => LightsOn = r.ReadInt32();

        //: Entity.SetLightHook
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is SetLightHook s) nodes.Add(new MetaInfo($"LightsOn: {s.LightsOn}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
