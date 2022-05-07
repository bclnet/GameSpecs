using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class SetOmegaHook : AnimationHook, IGetExplorerInfo
    {
        public readonly Vector3 Axis;

        public SetOmegaHook(AnimationHook hook) : base(hook) { }
        public SetOmegaHook(BinaryReader r) : base(r)
            => Axis = r.ReadVector3();

        //: Entity.SetOmegaHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is SetOmegaHook s)
            {
                nodes.Add(new ExplorerInfoNode($"Axis: {s.Axis}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
