using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class TMTerrainDesc : IGetMetadataInfo
    {
        public readonly uint TerrainType;
        public readonly TerrainTex TerrainTex;

        public TMTerrainDesc(BinaryReader r)
        {
            TerrainType = r.ReadUInt32();
            TerrainTex = new TerrainTex(r);
        }

        //: Entity.TMTerrainDesc
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"TerrainType: {TerrainType}"),
                new MetadataInfo("TerrainTexture", items: (TerrainTex as IGetMetadataInfo).GetInfoNodes()),
            };
            return nodes;
        }
    }
}
