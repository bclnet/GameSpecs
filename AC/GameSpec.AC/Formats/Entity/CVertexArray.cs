using GameSpec.Explorer;
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
    public class CVertexArray : IGetExplorerInfo
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
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"VertexType: {VertexType}"),
                new ExplorerInfoNode($"Vertices", items: Vertices.Select(x => new ExplorerInfoNode($"{x.Key}", items: (x.Value as IGetExplorerInfo).GetInfoNodes(resource, file)))),
            };
            return nodes;
        }
    }
}
