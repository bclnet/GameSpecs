using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class SoundTableData : IHaveMetaInfo
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
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Sound ID: {SoundId:X8}", clickable: true),
                new MetaInfo($"Priority: {Priority}"),
                new MetaInfo($"Probability: {Probability}"),
                new MetaInfo($"Volume: {Volume}"),
            };
            return nodes;
        }
    }
}
