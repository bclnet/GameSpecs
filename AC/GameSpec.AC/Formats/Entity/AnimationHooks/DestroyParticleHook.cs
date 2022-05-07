using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class DestroyParticleHook : AnimationHook, IGetExplorerInfo
    {
        public readonly uint EmitterId;

        public DestroyParticleHook(AnimationHook hook) : base(hook) { }
        public DestroyParticleHook(BinaryReader r) : base(r)
            => EmitterId = r.ReadUInt32();

        //: Entity.DestroyParticleHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is DestroyParticleHook s)
            {
                nodes.Add(new ExplorerInfoNode($"EmitterId: {s.EmitterId}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
