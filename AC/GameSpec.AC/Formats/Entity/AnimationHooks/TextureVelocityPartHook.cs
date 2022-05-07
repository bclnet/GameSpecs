using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class TextureVelocityPartHook : AnimationHook, IGetExplorerInfo
    {
        public readonly uint PartIndex;
        public readonly float USpeed;
        public readonly float VSpeed;

        public TextureVelocityPartHook(AnimationHook hook) : base(hook) { }
        public TextureVelocityPartHook(BinaryReader r) : base(r)
        {
            PartIndex = r.ReadUInt32();
            USpeed = r.ReadSingle();
            VSpeed = r.ReadSingle();
        }

        //: Entity.TextureVelocityPartHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is TextureVelocityPartHook s)
            {
                nodes.Add(new ExplorerInfoNode($"PartIndex: {s.PartIndex}"));
                nodes.Add(new ExplorerInfoNode($"USpeed: {s.USpeed}"));
                nodes.Add(new ExplorerInfoNode($"VSpeed: {s.VSpeed}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
