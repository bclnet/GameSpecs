using GameSpec.WbB.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class AmbientSoundDesc : IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"SoundType: {SType}"),
                new MetadataInfo($"Volume: {Volume}"),
                new MetadataInfo($"BaseChance: {BaseChance}"),
                new MetadataInfo($"MinRate: {MinRate}"),
                new MetadataInfo($"MaxRate: {MaxRate}"),
            };
            return nodes;
        }
    }
}
