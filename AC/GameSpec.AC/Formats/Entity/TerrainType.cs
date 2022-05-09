using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameSpec.AC.Formats.Entity
{
    public class TerrainType : IGetMetadataInfo
    {
        public readonly string TerrainName;
        public readonly uint TerrainColor;
        public readonly uint[] SceneTypes;

        public TerrainType(BinaryReader r)
        {
            TerrainName = r.ReadL16String(Encoding.Default); r.AlignBoundary();
            TerrainColor = r.ReadUInt32();
            SceneTypes = r.ReadL32Array<uint>(sizeof(uint));
        }

        //: Entity.TerrainType
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"TerrainName: {TerrainName}"),
                new MetadataInfo($"TerrainColor: {TerrainColor:X8}"),
                new MetadataInfo("SceneTypes", items: SceneTypes.Select((x, i) => new MetadataInfo($"{i}: {x}"))),
            };
            return nodes;
        }
    }
}
