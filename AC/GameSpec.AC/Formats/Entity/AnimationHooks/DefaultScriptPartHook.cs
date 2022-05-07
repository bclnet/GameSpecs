using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class DefaultScriptPartHook : AnimationHook, IGetExplorerInfo
    {
        public readonly uint PartIndex;

        public DefaultScriptPartHook(AnimationHook hook) : base(hook) { }
        public DefaultScriptPartHook(BinaryReader r) : base(r)
            => PartIndex = r.ReadUInt32();

        //: Entity.DefaultScriptPartHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is DefaultScriptPartHook s)
            {
                nodes.Add(new ExplorerInfoNode($"PartIndex: {s.PartIndex}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
