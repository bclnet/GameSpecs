using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class EtherealHook : AnimationHook, IGetExplorerInfo
    {
        public readonly int Ethereal;

        public EtherealHook(AnimationHook hook) : base(hook) { }
        public EtherealHook(BinaryReader r) : base(r)
            => Ethereal = r.ReadInt32();

        //: Entity.EtherealHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is EtherealHook s)
            {
                nodes.Add(new ExplorerInfoNode($"Ethereal: {s.Ethereal}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
