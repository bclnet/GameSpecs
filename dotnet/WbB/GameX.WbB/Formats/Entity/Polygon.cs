using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameX.WbB.Formats.Entity
{
    public class Polygon : IHaveMetaInfo
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
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                //new MetaInfo($"NumPoints: {NumPts}"),
                new MetaInfo($"Stippling: {Stippling}"),
                new MetaInfo($"CullMode: {SidesType}"),
                new MetaInfo($"PosSurface: {PosSurface}"),
                new MetaInfo($"NegSurface: {NegSurface}"),
                new MetaInfo($"Vertex IDs: {string.Join(", ", VertexIds)}"),
                new MetaInfo($"PosUVIndices: {string.Join(", ", PosUVIndices)}"),
                new MetaInfo($"NegUVIndices: {string.Join(", ", NegUVIndices)}"),
            };
            return nodes;
        }

        public void LoadVertices(CVertexArray vertexArray) => Vertices = VertexIds.Select(id => vertexArray.Vertices[(ushort)id]).ToArray();

    }
}
