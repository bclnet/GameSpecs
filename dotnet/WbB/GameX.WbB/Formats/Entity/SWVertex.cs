using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace GameX.WbB.Formats.Entity
{
    /// <summary>
    /// A vertex position, normal, and texture coords
    /// </summary>
    public class SWVertex : IHaveMetaInfo
    {
        public readonly Vector3 Origin;
        public readonly Vector3 Normal;
        public readonly Vec2Duv[] UVs;

        //: Entity+SWVertex
        public SWVertex(Vector3 origin, Vector3 normal)
        {
            Origin = origin;    // ref?
            Normal = normal;
        }
        public SWVertex(BinaryReader r)
        {
            var numUVs = r.ReadUInt16();
            Origin = r.ReadVector3();
            Normal = r.ReadVector3();
            UVs = r.ReadFArray(x => new Vec2Duv(x), numUVs);
        }

        //: Entity.Vertex
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Origin: {Origin}"),
                new MetaInfo($"Normal: {Normal}"),
                new MetaInfo($"UVs", items: UVs.SelectMany(x => (x as IHaveMetaInfo).GetInfoNodes(resource, file))),
            };
            return nodes;
        }
    }
}
