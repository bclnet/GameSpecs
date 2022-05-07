using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class CallPESHook : AnimationHook, IGetExplorerInfo
    {
        public readonly uint PES;
        public readonly float Pause;

        public CallPESHook(AnimationHook hook) : base(hook) { }
        public CallPESHook(BinaryReader r) : base(r)
        {
            PES = r.ReadUInt32();
            Pause = r.ReadSingle();
        }

        //: Entity.CallPESHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is CallPESHook s)
            {
                nodes.Add(new ExplorerInfoNode($"PES: {s.PES:X8}"));
                nodes.Add(new ExplorerInfoNode($"Pause: {s.Pause}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
