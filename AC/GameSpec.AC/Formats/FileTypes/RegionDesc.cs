using GameSpec.AC.Formats.Entity;
using GameSpec.Metadata;
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
    public class RegionDesc : FileType, IGetMetadataInfo
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
            RegionName = r.ReadL16Encoding(Encoding.Default); r.Align(); // "Dereth"

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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"{nameof(RegionDesc)}: {Id:X8}", items: new List<MetadataInfo> {
                    new MetadataInfo($"RegionNum: {RegionNumber}"),
                    new MetadataInfo($"Version: {Version}"),
                    new MetadataInfo($"Name: {RegionName}"),
                    new MetadataInfo("LandDefs", items: (LandDefs as IGetMetadataInfo).GetInfoNodes()),
                    new MetadataInfo("GameTime", items: (GameTime as IGetMetadataInfo).GetInfoNodes()),
                    new MetadataInfo($"PartsMask: {PartsMask:X8}"),
                    (PartsMask & 0x10) != 0 ? new MetadataInfo("SkyInfo", items: (SkyInfo as IGetMetadataInfo).GetInfoNodes()) : null,
                    (PartsMask & 0x01) != 0 ? new MetadataInfo("SoundInfo", items: (SoundInfo as IGetMetadataInfo).GetInfoNodes()) : null,
                    (PartsMask & 0x02) != 0 ? new MetadataInfo("SceneInfo", items: (SceneInfo as IGetMetadataInfo).GetInfoNodes()) : null,
                    new MetadataInfo("TerrainInfo", items: (TerrainInfo as IGetMetadataInfo).GetInfoNodes()),
                    (PartsMask & 0x200) != 0 ? new MetadataInfo("RegionMisc", items: (RegionMisc as IGetMetadataInfo).GetInfoNodes()) : null,
                })
            };
            return nodes;
        }
    }
}
