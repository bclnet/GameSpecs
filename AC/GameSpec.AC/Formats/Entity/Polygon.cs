using GameSpec.AC.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameSpec.AC.Formats.Entity
{
    public class Polygon : IGetMetadataInfo
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
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                //new MetadataInfo($"NumPoints: {NumPts}"),
                new MetadataInfo($"Stippling: {Stippling}"),
                new MetadataInfo($"CullMode: {SidesType}"),
                new MetadataInfo($"PosSurface: {PosSurface}"),
                new MetadataInfo($"NegSurface: {NegSurface}"),
                new MetadataInfo($"Vertex IDs: {string.Join(", ", VertexIds)}"),
                new MetadataInfo($"PosUVIndices: {string.Join(", ", PosUVIndices)}"),
                new MetadataInfo($"NegUVIndices: {string.Join(", ", NegUVIndices)}"),
            };
            return nodes;
        }

        public void LoadVertices(CVertexArray vertexArray) => Vertices = VertexIds.Select(id => vertexArray.Vertices[(ushort)id]).ToArray();

    }
}
