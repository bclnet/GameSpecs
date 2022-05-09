using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class DefaultScriptPartHook : AnimationHook, IGetMetadataInfo
    {
        public readonly uint PartIndex;

        public DefaultScriptPartHook(AnimationHook hook) : base(hook) { }
        public DefaultScriptPartHook(BinaryReader r) : base(r)
            => PartIndex = r.ReadUInt32();

        //: Entity.DefaultScriptPartHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is DefaultScriptPartHook s)
            {
                nodes.Add(new MetadataInfo($"PartIndex: {s.PartIndex}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
