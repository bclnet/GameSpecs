using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class SoundTableData : IGetMetadataInfo
    {
        public readonly uint SoundId; // Corresponds to the DatFileType.Wave
        public readonly float Priority;
        public readonly float Probability;
        public readonly float Volume;

        public SoundTableData(BinaryReader r)
        {
            SoundId = r.ReadUInt32();
            Priority = r.ReadSingle();
            Probability = r.ReadSingle();
            Volume = r.ReadSingle();
        }

        //: Entity.SoundTableData
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Sound ID: {SoundId:X8}", clickable: true),
                new MetadataInfo($"Priority: {Priority}"),
                new MetadataInfo($"Probability: {Probability}"),
                new MetadataInfo($"Volume: {Volume}"),
            };
            return nodes;
        }
    }
}
