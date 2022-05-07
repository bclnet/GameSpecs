using GameSpec.AC.Formats.Entity;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameSpec.AC.Formats.FileTypes
{
    /// <summary>
    /// This is the client_portal.dat file starting with 0x13 -- There is only one of these, which is why REGION_ID is a constant.
    /// </summary>
    [PakFileType(PakFileType.Region)]
    public class RegionDesc : FileType, IGetExplorerInfo
    {
        public const uint FILE_ID = 0x13000000;

        public readonly uint RegionNumber;
        public readonly uint Version;
        public readonly string RegionName;
        public readonly LandDefs LandDefs;
        public readonly GameTime GameTime;
        public readonly uint PartsMask;
        public readonly SkyDesc SkyInfo;
        public readonly SoundDesc SoundInfo;
        public readonly SceneDesc SceneInfo;
        public readonly TerrainDesc TerrainInfo;
        public readonly RegionMisc RegionMisc;

        public RegionDesc(BinaryReader r)
        {
            Id = r.ReadUInt32();
            RegionNumber = r.ReadUInt32();
            Version = r.ReadUInt32();
            RegionName = r.ReadL16String(Encoding.Default); r.AlignBoundary(); // "Dereth"

            LandDefs = new LandDefs(r);
            GameTime = new GameTime(r);
            PartsMask = r.ReadUInt32();
            if ((PartsMask & 0x10) != 0) SkyInfo = new SkyDesc(r);
            if ((PartsMask & 0x01) != 0) SoundInfo = new SoundDesc(r);
            if ((PartsMask & 0x02) != 0) SceneInfo = new SceneDesc(r);
            TerrainInfo = new TerrainDesc(r);
            if ((PartsMask & 0x0200) != 0) RegionMisc = new RegionMisc(r);
        }

        //: FileTypes.Region
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"{nameof(RegionDesc)}: {Id:X8}", items: new List<ExplorerInfoNode> {
                    new ExplorerInfoNode($"RegionNum: {RegionNumber}"),
                    new ExplorerInfoNode($"Version: {Version}"),
                    new ExplorerInfoNode($"Name: {RegionName}"),
                    new ExplorerInfoNode("LandDefs", items: (LandDefs as IGetExplorerInfo).GetInfoNodes()),
                    new ExplorerInfoNode("GameTime", items: (GameTime as IGetExplorerInfo).GetInfoNodes()),
                    new ExplorerInfoNode($"PartsMask: {PartsMask:X8}"),
                    (PartsMask & 0x10) != 0 ? new ExplorerInfoNode("SkyInfo", items: (SkyInfo as IGetExplorerInfo).GetInfoNodes()) : null,
                    (PartsMask & 0x01) != 0 ? new ExplorerInfoNode("SoundInfo", items: (SoundInfo as IGetExplorerInfo).GetInfoNodes()) : null,
                    (PartsMask & 0x02) != 0 ? new ExplorerInfoNode("SceneInfo", items: (SceneInfo as IGetExplorerInfo).GetInfoNodes()) : null,
                    new ExplorerInfoNode("TerrainInfo", items: (TerrainInfo as IGetExplorerInfo).GetInfoNodes()),
                    (PartsMask & 0x200) != 0 ? new ExplorerInfoNode("RegionMisc", items: (RegionMisc as IGetExplorerInfo).GetInfoNodes()) : null,
                })
            };
            return nodes;
        }
    }
}
