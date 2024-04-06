using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class RegionMisc : IHaveMetaInfo
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
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Version: {Version}"),
                new MetaInfo($"GameMap ID: {GameMapID:X8}", clickable: true),
                new MetaInfo($"AutoTest Map ID: {AutotestMapId:X8}", clickable: true),
                new MetaInfo($"AutoTest Map Size: {AutotestMapSize}"),
                new MetaInfo($"ClearCell ID: {ClearCellId:X8}", clickable: true),
                new MetaInfo($"ClearMonster ID: {ClearMonsterId:X8}", clickable: true),
            };
            return nodes;
        }
    }
}
