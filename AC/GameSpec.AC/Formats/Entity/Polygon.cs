using GameSpec.AC.Formats.Props;
using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class Polygon : IGetExplorerInfo
    {
        public byte NumPts;
        public StipplingType Stippling; // Whether it has that textured/bumpiness to it
        public CullMode SidesType;
        public short PosSurface;
        public short NegSurface;
        public short[]  VertexIds;
        public byte[] PosUVIndices;
        public byte[] NegUVIndices;
        public SWVertex[] Vertices;

        //: Entity+Polygon
        public Polygon() { }
        public Polygon(BinaryReader r)
        {
            NumPts = r.ReadByte();
            Stippling = (StipplingType)r.ReadByte();
            SidesType = (CullMode)r.ReadInt32();
            PosSurface = r.ReadInt16();
            NegSurface = r.ReadInt16();
            VertexIds = r.ReadTArray<short>(sizeof(short), NumPts);
            PosUVIndices = !Stippling.HasFlag(StipplingType.NoPos) ? r.ReadBytes(NumPts) : new byte[0];
            NegUVIndices = SidesType == CullMode.Clockwise && !Stippling.HasFlag(StipplingType.NoNeg) ? r.ReadBytes(NumPts) : new byte[0];
            if (SidesType == CullMode.None) { NegSurface = PosSurface; NegUVIndices = PosUVIndices; }
        }

        //: Entity.Polygon
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                //new ExplorerInfoNode($"NumPoints: {NumPts}"),
                new ExplorerInfoNode($"Stippling: {Stippling}"),
                new ExplorerInfoNode($"CullMode: {SidesType}"),
                new ExplorerInfoNode($"PosSurface: {PosSurface}"),
                new ExplorerInfoNode($"NegSurface: {NegSurface}"),
                new ExplorerInfoNode($"Vertex IDs: {string.Join(", ", VertexIds)}"),
                new ExplorerInfoNode($"PosUVIndices: {string.Join(", ", PosUVIndices)}"),
                new ExplorerInfoNode($"NegUVIndices: {string.Join(", ", NegUVIndices)}"),
            };
            return nodes;
        }

        public void LoadVertices(CVertexArray vertexArray) => Vertices = VertexIds.Select(id => vertexArray.Vertices[(ushort)id]).ToArray();

    }
}
