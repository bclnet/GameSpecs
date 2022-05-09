using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class SetOmegaHook : AnimationHook, IGetMetadataInfo
    {
        public readonly Vector3 Axis;

        public SetOmegaHook(AnimationHook hook) : base(hook) { }
        public SetOmegaHook(BinaryReader r) : base(r)
            => Axis = r.ReadVector3();

        //: Entity.SetOmegaHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is SetOmegaHook s)
            {
                nodes.Add(new MetadataInfo($"Axis: {s.Axis}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
