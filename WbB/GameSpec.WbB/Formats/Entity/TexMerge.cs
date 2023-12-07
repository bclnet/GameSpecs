using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.WbB.Formats.Entity
{
    public class TexMerge : IGetMetadataInfo
    {
        public readonly uint BaseTexSize;
        public readonly TerrainAlphaMap[] CornerTerrainMaps;
        public readonly TerrainAlphaMap[] SideTerrainMaps;
        public readonly RoadAlphaMap[] RoadMaps;
        public readonly TMTerrainDesc[] TerrainDesc;

        public TexMerge(BinaryReader r)
        {
            BaseTexSize = r.ReadUInt32();
            CornerTerrainMaps = r.ReadL32Array(x => new TerrainAlphaMap(x));
            SideTerrainMaps = r.ReadL32Array(x => new TerrainAlphaMap(x));
            RoadMaps = r.ReadL32Array(x => new RoadAlphaMap(x));
            TerrainDesc = r.ReadL32Array(x => new TMTerrainDesc(x));
        }

        //: Entity.TexMerge
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"BaseTextureSize: {BaseTexSize}"),
                new MetadataInfo("CornerTerrainMaps", items: CornerTerrainMaps.Select(x => new MetadataInfo($"{x}", clickable: true))),
                new MetadataInfo("SideTerrainMap", items: SideTerrainMaps.Select(x => new MetadataInfo($"{x}", clickable: true))),
                new MetadataInfo("RoadAlphaMap", items: RoadMaps.Select(x => new MetadataInfo($"{x}", clickable: true))),
                new MetadataInfo("TMTerrainDesc", items: TerrainDesc.Select(x => {
                    var items = (x as IGetMetadataInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("TerrainType: ", "");
                    items.RemoveAt(0);
                    return new MetadataInfo(name, items: items);
                })),
            };
            return nodes;
        }
    }
}
