using GameSpec.AC.Formats.Props;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class SoundTableHook : AnimationHook, IGetExplorerInfo
    {
        public readonly uint SoundType;

        public SoundTableHook(AnimationHook hook) : base(hook) { }
        public SoundTableHook(BinaryReader r) : base(r)
            => SoundType = r.ReadUInt32();

        //: Entity.SoundTableHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is SoundTableHook s)
            {
                nodes.Add(new ExplorerInfoNode($"SoundType: {(Sound)s.SoundType}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
