using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class DiffusePartHook : AnimationHook, IHaveMetaInfo
    {
        public readonly uint Part;
        public readonly float Start;
        public readonly float End;
        public readonly float Time;

        public DiffusePartHook(AnimationHook hook) : base(hook) { }
        public DiffusePartHook(BinaryReader r) : base(r)
        {
            Part = r.ReadUInt32();
            Start = r.ReadSingle();
            End = r.ReadSingle();
            Time = r.ReadSingle();
        }

        //: Entity.DiffusePartHook
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is DiffusePartHook s)
            {
                nodes.Add(new MetaInfo($"Part: {s.Part}"));
                nodes.Add(new MetaInfo($"Start: {s.Start}, End: {s.End}, Time: {s.Time}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
