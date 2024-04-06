using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class TextureVelocityPartHook : AnimationHook, IHaveMetaInfo
    {
        public readonly uint PartIndex;
        public readonly float USpeed;
        public readonly float VSpeed;

        public TextureVelocityPartHook(AnimationHook hook) : base(hook) { }
        public TextureVelocityPartHook(BinaryReader r) : base(r)
        {
            PartIndex = r.ReadUInt32();
            USpeed = r.ReadSingle();
            VSpeed = r.ReadSingle();
        }

        //: Entity.TextureVelocityPartHook
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is TextureVelocityPartHook s)
            {
                nodes.Add(new MetaInfo($"PartIndex: {s.PartIndex}"));
                nodes.Add(new MetaInfo($"USpeed: {s.USpeed}, VSpeed: {s.VSpeed}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
