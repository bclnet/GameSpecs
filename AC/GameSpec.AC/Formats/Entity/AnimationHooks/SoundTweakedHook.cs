using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity.AnimationHooks
{
    public class SoundTweakedHook : AnimationHook, IGetExplorerInfo
    {
        public readonly uint SoundID;
        public readonly float Priority;
        public readonly float Probability;
        public readonly float Volume;

        public SoundTweakedHook(AnimationHook hook) : base(hook) { }
        public SoundTweakedHook(BinaryReader r) : base(r)
        {
            SoundID = r.ReadUInt32();
            Priority = r.ReadSingle();
            Probability = r.ReadSingle();
            Volume = r.ReadSingle();
        }

        //: Entity.SoundTweakedHook
        public override List<ExplorerInfoNode> GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode>();
            if (Base is SoundTweakedHook s)
            {
                nodes.Add(new ExplorerInfoNode($"SoundID: {s.SoundID:X8}"));
                nodes.Add(new ExplorerInfoNode($"Priority: {s.Priority}"));
                nodes.Add(new ExplorerInfoNode($"Probability: {s.Probability}"));
                nodes.Add(new ExplorerInfoNode($"Volume: {s.Volume}"));
            }
            nodes.AddRange(base.GetInfoNodes(resource, file, tag));
            return nodes;
        }
    }
}
