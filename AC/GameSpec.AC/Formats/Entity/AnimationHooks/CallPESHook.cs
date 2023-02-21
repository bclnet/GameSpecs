using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class CallPESHook : AnimationHook, IGetMetadataInfo
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
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is CallPESHook s) nodes.Add(new MetadataInfo($"PES: {s.PES:X8}, Pause: {s.Pause}"));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
