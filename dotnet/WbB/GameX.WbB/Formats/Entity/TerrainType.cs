using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameX.WbB.Formats.Entity
{
    public class TerrainType : IHaveMetaInfo
    {
        public readonly string TerrainName;
        public readonly uint TerrainColor;
        public readonly uint[] SceneTypes;

        public TerrainType(BinaryReader r)
        {
            TerrainName = r.ReadL16Encoding(Encoding.Default); r.Align();
            TerrainColor = r.ReadUInt32();
            SceneTypes = r.ReadL32TArray<uint>(sizeof(uint));
        }

        //: Entity.TerrainType
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"TerrainName: {TerrainName}"),
                new MetaInfo($"TerrainColor: {TerrainColor:X8}"),
                new MetaInfo("SceneTypes", items: SceneTypes.Select((x, i) => new MetaInfo($"{i}: {x}"))),
            };
            return nodes;
        }
    }
}
