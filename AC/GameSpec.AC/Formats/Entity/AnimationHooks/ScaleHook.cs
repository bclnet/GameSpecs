using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class ScaleHook : AnimationHook, IGetExplorerInfo
    {
        public readonly float End;
        public readonly float Time;

        public ScaleHook(AnimationHook hook) : base(hook) { }
        public ScaleHook(BinaryReader r) : base(r)
        {
            End = r.ReadSingle();
            Time = r.ReadSingle();
        }

        //: Entity.ScaleHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is ScaleHook s)
            {
                nodes.Add(new ExplorerInfoNode($"End: {s.End}"));
                nodes.Add(new ExplorerInfoNode($"Time: {s.Time}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
