using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class SoundHook : AnimationHook, IHaveMetaInfo
    {
        public readonly uint Id;

        public SoundHook(AnimationHook hook) : base(hook) { }
        public SoundHook(BinaryReader r) : base(r)
            => Id = r.ReadUInt32();

        //: Entity.SoundHook
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is SoundHook s) nodes.Add(new MetaInfo($"Id: {s.Id:X8}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
