using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class DiffusePartHook : AnimationHook, IGetExplorerInfo
    {
        public readonly uint Part;
        public readonly float Start;
        public readonly float End;
        public readonly float Time;

        public DiffusePartHook(AnimationHook hook) : base(hook) { }
        public DiffusePartHook(BinaryReader r) : base(r)
        {
            Part = r.ReadUInt32();
            Start = r.ReadSingle();
            End = r.ReadSingle();
            Time = r.ReadSingle();
        }

        //: Entity.DiffusePartHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is DiffusePartHook s)
            {
                nodes.Add(new ExplorerInfoNode($"Part: {s.Part}"));
                nodes.Add(new ExplorerInfoNode($"Start: {s.Start}"));
                nodes.Add(new ExplorerInfoNode($"End: {s.End}"));
                nodes.Add(new ExplorerInfoNode($"Time: {s.Time}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
