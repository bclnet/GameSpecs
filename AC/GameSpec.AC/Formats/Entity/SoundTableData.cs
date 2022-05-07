using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class SoundTableData : IGetExplorerInfo
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
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Sound ID: {SoundId:X8}", clickable: true),
                new ExplorerInfoNode($"Priority: {Priority}"),
                new ExplorerInfoNode($"Probability: {Probability}"),
                new ExplorerInfoNode($"Volume: {Volume}"),
            };
            return nodes;
        }
    }
}
