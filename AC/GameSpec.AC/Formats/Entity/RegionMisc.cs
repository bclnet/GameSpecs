using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class RegionMisc : IGetExplorerInfo
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
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Version: {Version}"),
                new ExplorerInfoNode($"GameMap ID: {GameMapID:X8}", clickable: true),
                new ExplorerInfoNode($"AutoTest Map ID: {AutotestMapId:X8}", clickable: true),
                new ExplorerInfoNode($"AutoTest Map Size: {AutotestMapSize}"),
                new ExplorerInfoNode($"ClearCell ID: {ClearCellId:X8}", clickable: true),
                new ExplorerInfoNode($"ClearMonster ID: {ClearMonsterId:X8}", clickable: true),
            };
            return nodes;
        }
    }
}
