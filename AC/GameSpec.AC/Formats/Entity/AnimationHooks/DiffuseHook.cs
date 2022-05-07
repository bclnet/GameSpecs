using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class DiffuseHook : AnimationHook, IGetExplorerInfo
    {
        public readonly float Start;
        public readonly float End;
        public readonly float Time;

        public DiffuseHook(AnimationHook hook) : base(hook) { }
        public DiffuseHook(BinaryReader r) : base(r)
        {
            Start = r.ReadSingle();
            End = r.ReadSingle();
            Time = r.ReadSingle();
        }

        //: Entity.DiffuseHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is DiffuseHook s)
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
