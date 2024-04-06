using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class AmbientSoundDesc : IHaveMetaInfo
    {
        public readonly Sound SType;
        public readonly float Volume;
        public readonly float BaseChance;
        public readonly float MinRate;
        public readonly float MaxRate;

        public AmbientSoundDesc(BinaryReader r)
        {
            SType = (Sound)r.ReadUInt32();
            Volume = r.ReadSingle();
            BaseChance = r.ReadSingle();
            MinRate = r.ReadSingle();
            MaxRate = r.ReadSingle();
        }

        public bool IsContinuous => BaseChance == 0;

        //: Entity.AmbientSoundDesc
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"SoundType: {SType}"),
                new MetaInfo($"Volume: {Volume}"),
                new MetaInfo($"BaseChance: {BaseChance}"),
                new MetaInfo($"MinRate: {MinRate}"),
                new MetaInfo($"MaxRate: {MaxRate}"),
            };
            return nodes;
        }
    }
}
