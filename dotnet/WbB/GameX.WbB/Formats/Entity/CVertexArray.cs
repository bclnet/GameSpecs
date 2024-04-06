using GameX.Meta;
using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    /// <summary>
    /// A list of indexed vertices, and their associated type
    /// </summary>
    public class CVertexArray : IHaveMetaInfo
    {
        public readonly int VertexType;
        public readonly IDictionary<ushort, SWVertex> Vertices;

        public CVertexArray(BinaryReader r)
        {
            VertexType = r.ReadInt32();
            var numVertices = r.ReadUInt32();
            if (VertexType == 1) Vertices = r.ReadTMany<ushort, SWVertex>(sizeof(ushort), x => new SWVertex(x), (int)numVertices);
            else throw new FormatException("VertexType should be 1");
        }

        //: Entity.VertexArray
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"VertexType: {VertexType}"),
                new MetaInfo($"Vertices", items: Vertices.Select(x => new MetaInfo($"{x.Key}", items: (x.Value as IHaveMetaInfo).GetInfoNodes(resource, file)))),
            };
            return nodes;
        }
    }
}
