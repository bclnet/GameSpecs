using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class StopParticleHook : AnimationHook, IGetExplorerInfo
    {
        public readonly uint EmitterId;

        public StopParticleHook(AnimationHook hook) : base(hook) { }
        public StopParticleHook(BinaryReader r) : base(r)
            => EmitterId = r.ReadUInt32();

        //: Entity.StopParticleHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is StopParticleHook s)
            {
                nodes.Add(new ExplorerInfoNode($"EmitterId: {s.EmitterId}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
