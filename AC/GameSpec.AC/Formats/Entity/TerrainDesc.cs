using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class TerrainDesc : IGetMetadataInfo
    {
        public readonly TerrainType[] TerrainTypes;
        public readonly LandSurf LandSurfaces;

        public TerrainDesc(BinaryReader r)
        {
            TerrainTypes = r.ReadL32Array(x => new TerrainType(x));
            LandSurfaces = new LandSurf(r);
        }

        //: Entity.TerrainDesc
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo("TerrainTypes", items: TerrainTypes.Select(x => {
                    var items = (x as IGetMetadataInfo).GetInfoNodes();
                    var name = items[0].Name.Replace("TerrainName: ", "");
                    items.RemoveAt(0);
                    return new MetadataInfo(name, items: items);
                })),
                new MetadataInfo($"LandSurf", items: (LandSurfaces as IGetMetadataInfo).GetInfoNodes()),
            };
            return nodes;
        }
    }
}
