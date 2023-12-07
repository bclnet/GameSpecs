using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.WbB.Formats.Entity
{
    /// <summary>
    /// Info on texture UV mapping
    /// </summary>
    public class Vec2Duv : IGetMetadataInfo
    {
        public readonly float U;
        public readonly float V;

        public Vec2Duv(BinaryReader r)
        {
            U = r.ReadSingle();
            V = r.ReadSingle();
        }

        //: Entity.UV
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"U: {U} V: {V}"),
            };
            return nodes;
        }
    }
}
