using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    public class RegionMisc : IGetMetadataInfo
    {
        public readonly uint Version;
        public readonly uint GameMapID;
        public readonly uint AutotestMapId;
        public readonly uint AutotestMapSize;
        public readonly uint ClearCellId;
        public readonly uint ClearMonsterId;

        public RegionMisc(BinaryReader r)
        {
            Version = r.ReadUInt32();
            GameMapID = r.ReadUInt32();
            AutotestMapId = r.ReadUInt32();
            AutotestMapSize = r.ReadUInt32();
            ClearCellId = r.ReadUInt32();
            ClearMonsterId = r.ReadUInt32();
        }

        //: Entity.RegionMisc
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Version: {Version}"),
                new MetadataInfo($"GameMap ID: {GameMapID:X8}", clickable: true),
                new MetadataInfo($"AutoTest Map ID: {AutotestMapId:X8}", clickable: true),
                new MetadataInfo($"AutoTest Map Size: {AutotestMapSize}"),
                new MetadataInfo($"ClearCell ID: {ClearCellId:X8}", clickable: true),
                new MetadataInfo($"ClearMonster ID: {ClearMonsterId:X8}", clickable: true),
            };
            return nodes;
        }
    }
}
