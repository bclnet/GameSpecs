using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class NoDrawHook : AnimationHook, IGetExplorerInfo
    {
        public readonly uint NoDraw;

        public NoDrawHook(AnimationHook hook) : base(hook) { }
        public NoDrawHook(BinaryReader r) : base(r)
            => NoDraw = r.ReadUInt32();

        //: Entity.NoDrawHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is NoDrawHook s)
            {
                nodes.Add(new ExplorerInfoNode($"NoDraw: {s.NoDraw}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
