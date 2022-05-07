using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class SetLightHook : AnimationHook, IGetExplorerInfo
    {
        public readonly int LightsOn;

        public SetLightHook(AnimationHook hook) : base(hook) { }
        public SetLightHook(BinaryReader r) : base(r)
            => LightsOn = r.ReadInt32();

        //: Entity.SetLightHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is SetLightHook s)
            {
                nodes.Add(new ExplorerInfoNode($"LightsOn: {s.LightsOn}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
