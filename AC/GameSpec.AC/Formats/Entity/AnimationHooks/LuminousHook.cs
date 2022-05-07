using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class LuminousHook : AnimationHook, IGetExplorerInfo
    {
        public readonly float Start;
        public readonly float End;
        public readonly float Time;

        public LuminousHook(AnimationHook hook) : base(hook) { }
        public LuminousHook(BinaryReader r) : base(r)
        {
            Start = r.ReadSingle();
            End = r.ReadSingle();
            Time = r.ReadSingle();
        }

        //: Entity.LuminousHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is LuminousHook s)
            {
                nodes.Add(new ExplorerInfoNode($"Start: {s.Start}"));
                nodes.Add(new ExplorerInfoNode($"End: {s.End}"));
                nodes.Add(new ExplorerInfoNode($"Time: {s.Time}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
