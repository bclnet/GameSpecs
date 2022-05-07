using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace GameSpec.AC.Formats.Entity
{
    /// <summary>
    /// A vertex position, normal, and texture coords
    /// </summary>
    public class SWVertex : IGetExplorerInfo
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
            UVs = r.ReadTArray(x => new Vec2Duv(x), numUVs);
        }

        //: Entity.Vertex
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"Origin: {Origin}"),
                new ExplorerInfoNode($"Normal: {Normal}"),
                new ExplorerInfoNode($"UVs", items: UVs.SelectMany(x => (x as IGetExplorerInfo).GetInfoNodes(resource, file))),
            };
            return nodes;
        }
    }
}
