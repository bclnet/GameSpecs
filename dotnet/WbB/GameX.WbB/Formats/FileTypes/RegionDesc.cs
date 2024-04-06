using GameX.WbB.Formats.Entity;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameX.WbB.Formats.FileTypes
{
    /// <summary>
    /// This is the client_portal.dat file starting with 0x13 -- There is only one of these, which is why REGION_ID is a constant.
    /// </summary>
    [PakFileType(PakFileType.Region)]
    public class RegionDesc : FileType, IHaveMetaInfo
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
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(RegionDesc)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo($"RegionNum: {RegionNumber}"),
                    new MetaInfo($"Version: {Version}"),
                    new MetaInfo($"Name: {RegionName}"),
                    new MetaInfo("LandDefs", items: (LandDefs as IHaveMetaInfo).GetInfoNodes()),
                    new MetaInfo("GameTime", items: (GameTime as IHaveMetaInfo).GetInfoNodes()),
                    new MetaInfo($"PartsMask: {PartsMask:X8}"),
                    (PartsMask & 0x10) != 0 ? new MetaInfo("SkyInfo", items: (SkyInfo as IHaveMetaInfo).GetInfoNodes()) : null,
                    (PartsMask & 0x01) != 0 ? new MetaInfo("SoundInfo", items: (SoundInfo as IHaveMetaInfo).GetInfoNodes()) : null,
                    (PartsMask & 0x02) != 0 ? new MetaInfo("SceneInfo", items: (SceneInfo as IHaveMetaInfo).GetInfoNodes()) : null,
                    new MetaInfo("TerrainInfo", items: (TerrainInfo as IHaveMetaInfo).GetInfoNodes()),
                    (PartsMask & 0x200) != 0 ? new MetaInfo("RegionMisc", items: (RegionMisc as IHaveMetaInfo).GetInfoNodes()) : null,
                })
            };
            return nodes;
        }
    }
}
