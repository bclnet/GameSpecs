using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class CreateParticleHook : AnimationHook, IGetExplorerInfo
    {
        public readonly uint EmitterInfoId;
        public readonly uint PartIndex;
        public readonly Frame Offset;
        public readonly uint EmitterId;

        public CreateParticleHook(AnimationHook hook) : base(hook) { }
        public CreateParticleHook(BinaryReader r) : base(r)
        {
            EmitterInfoId = r.ReadUInt32();
            PartIndex = r.ReadUInt32();
            Offset = new Frame(r);
            EmitterId = r.ReadUInt32();
        }

        //: Entity.CreateParticleHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is CreateParticleHook s)
            {
                nodes.Add(new ExplorerInfoNode($"EmitterInfoId: {s.EmitterInfoId:X8}"));
                nodes.Add(new ExplorerInfoNode($"PartIndex: {(int)s.PartIndex}"));
                nodes.Add(new ExplorerInfoNode($"Offset: {s.Offset}"));
                nodes.Add(new ExplorerInfoNode($"EmitterId: {s.EmitterId}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
