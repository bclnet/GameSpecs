using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class TextureVelocityHook : AnimationHook, IGetExplorerInfo
    {
        public readonly float USpeed;
        public readonly float VSpeed;

        public TextureVelocityHook(AnimationHook hook) : base(hook) { }
        public TextureVelocityHook(BinaryReader r) : base(r)
        {
            USpeed = r.ReadSingle();
            VSpeed = r.ReadSingle();
        }

        //: Entity.TextureVelocityHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is TextureVelocityHook s)
            {
                nodes.Add(new ExplorerInfoNode($"USpeed: {s.USpeed}"));
                nodes.Add(new ExplorerInfoNode($"VSpeed: {s.VSpeed}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
