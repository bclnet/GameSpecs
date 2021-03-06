using GameSpec.Metadata;
using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    /// <summary>
    /// A list of indexed vertices, and their associated type
    /// </summary>
    public class CVertexArray : IGetMetadataInfo
    {
        public readonly int VertexType;
        public readonly Dictionary<ushort, SWVertex> Vertices;

        public CVertexArray(BinaryReader r)
        {
            VertexType = r.ReadInt32();
            var numVertices = r.ReadUInt32();
            if (VertexType == 1) Vertices = r.ReadTMany<ushort, SWVertex>(sizeof(ushort), x => new SWVertex(x), (int)numVertices);
            else throw new FormatException("VertexType should be 1");
        }

        //: Entity.VertexArray
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"VertexType: {VertexType}"),
                new MetadataInfo($"Vertices", items: Vertices.Select(x => new MetadataInfo($"{x.Key}", items: (x.Value as IGetMetadataInfo).GetInfoNodes(resource, file)))),
            };
            return nodes;
        }
    }
}
