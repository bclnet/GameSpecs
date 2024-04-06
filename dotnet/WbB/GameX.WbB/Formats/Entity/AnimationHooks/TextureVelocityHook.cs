using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class TextureVelocityHook : AnimationHook, IHaveMetaInfo
    {
        public readonly float USpeed;
        public readonly float VSpeed;

        public TextureVelocityHook(AnimationHook hook) : base(hook) { }
        public TextureVelocityHook(BinaryReader r) : base(r)
        {
            USpeed = r.ReadSingle();
            VSpeed = r.ReadSingle();
        }

        //: Entity.TextureVelocityHook
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is TextureVelocityHook s) nodes.Add(new MetaInfo($"USpeed: {s.USpeed}, VSpeed: {s.VSpeed}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
