using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class TMTerrainDesc : IHaveMetaInfo
    {
        public readonly uint TerrainType;
        public readonly TerrainTex TerrainTex;

        public TMTerrainDesc(BinaryReader r)
        {
            TerrainType = r.ReadUInt32();
            TerrainTex = new TerrainTex(r);
        }

        //: Entity.TMTerrainDesc
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"TerrainType: {TerrainType}"),
                new MetaInfo("TerrainTexture", items: (TerrainTex as IHaveMetaInfo).GetInfoNodes()),
            };
            return nodes;
        }
    }
}
