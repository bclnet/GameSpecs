using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class SoundHook : AnimationHook, IGetExplorerInfo
    {
        public readonly uint Id;

        public SoundHook(AnimationHook hook) : base(hook) { }
        public SoundHook(BinaryReader r) : base(r)
            => Id = r.ReadUInt32();

        //: Entity.SoundHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is SoundHook s)
            {
                nodes.Add(new ExplorerInfoNode($"Id: {s.Id:X8}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
