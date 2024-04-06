using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity.AnimationHooks
{
    public class AttackHook : AnimationHook, IHaveMetaInfo
    {
        public readonly AttackCone AttackCone;

        public AttackHook(AnimationHook hook) : base(hook) { }
        public AttackHook(BinaryReader r) : base(r)
            => AttackCone = new AttackCone(r);

        //: Entity.AttackHook
        public override List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo>();
            if (Base is AttackHook attackHook) nodes.AddRange((attackHook.AttackCone as IHaveMetaInfo).GetInfoNodes(tag: tag));
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
