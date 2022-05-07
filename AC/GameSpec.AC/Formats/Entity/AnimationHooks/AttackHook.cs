using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class AttackHook : AnimationHook, IGetExplorerInfo
    {
        public readonly AttackCone AttackCone;

        public AttackHook(AnimationHook hook) : base(hook) { }
        public AttackHook(BinaryReader r) : base(r)
            => AttackCone = new AttackCone(r);

        //: Entity.AttackHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is AttackHook attackHook)
            {
                nodes.AddRange((attackHook.AttackCone as IGetExplorerInfo).GetInfoNodes(tag: tag));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
