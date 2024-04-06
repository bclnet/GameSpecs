using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class TexMerge : IHaveMetaInfo
    {
        public readonly uint BaseTexSize;
        public readonly TerrainAlphaMap[] CornerTerrainMaps;
        public readonly TerrainAlphaMap[] SideTerrainMaps;
        public readonly RoadAlphaMap[] RoadMaps;
        public readonly TMTerrainDesc[] TerrainDesc;

        public TexMerge(BinaryReader r)
        {
            BaseTexSize = r.ReadUInt32();
            CornerTerrainMaps = r.ReadL32FArray(x => new TerrainAlphaMap(x));
            SideTerrainMaps = r.ReadL32FArray(x => new TerrainAlphaMap(x));
            RoadMaps = r.ReadL32FArray(x => new RoadAlphaMap(x));
            TerrainDesc = r.ReadL32FArray(x => new TMTerrainDesc(x));
        }

        //: Entity.TexMerge
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"BaseTextureSize: {BaseTexSize}"),
                new MetaInfo("CornerTerrainMaps", items: CornerTerrainMaps.Select(x => new MetaInfo($"{x}", clickable: true))),
                new MetaInfo("SideTerrainMap", items: SideTerrainMaps.Select(x => new MetaInfo($"{x}", clickable: true))),
                new MetaInfo("RoadAlphaMap", items: RoadMaps.Select(x => new MetaInfo($"{x}", clickable: true))),
                new MetaInfo("TMTerrainDesc", items: TerrainDesc.Select(x => {
                    var items = (x as IHaveMetaInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("TerrainType: ", "");
                    items.RemoveAt(0);
                    return new MetaInfo(name, items: items);
                })),
            };
            return nodes;
        }
    }
}
