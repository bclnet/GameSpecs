using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class AttackHook : AnimationHook, IGetMetadataInfo
    {
        public readonly AttackCone AttackCone;

        public AttackHook(AnimationHook hook) : base(hook) { }
        public AttackHook(BinaryReader r) : base(r)
            => AttackCone = new AttackCone(r);

        //: Entity.AttackHook
        public override List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo>();
            if (Base is AttackHook attackHook) nodes.AddRange((attackHook.AttackCone as IGetMetadataInfo).GetInfoNodes(tag: tag));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
