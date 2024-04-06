using System;
using System.Numerics;

namespace GameX.Formats.Unknown
{
    public class UnknownMesh
    {
        [Flags]
        public enum Effect
        {
            ScaleOffset = 0x1
        }

        public struct Subset
        {
            public Range Vertexs;
            public Range Indexs;
            public int MatId;
            //public float Radius;
            //public Vector3 Center;
        }

        public ref struct SubsetMesh
        {
            public Span<Vector3> Vertexs;
            public Span<int> Indexs;
            public Span<Vector3> Normals;
            public Span<Vector2> UVs;
        }

        public string Name;
        public Vector3 MinBound;
        public Vector3 MaxBound;
        public Subset[] Subsets;
        public Vector3[] Vertexs;
        public Vector2[] UVs;
        public Vector3[] Normals;
        public int[] Indexs;
        public Effect Effects;
        public (Vector3 scale, Vector3 offset) ScaleOffset3;
        public (Vector4 scale, Vector4 offset) ScaleOffset4;

        public SubsetMesh this[Subset i] => new SubsetMesh
        {
            Vertexs = Vertexs.AsSpan(i.Vertexs),
            UVs = UVs.AsSpan(i.Vertexs),
            Normals = Normals != null ? Normals.AsSpan(i.Vertexs) : null,
            Indexs = Indexs.AsSpan(i.Indexs),
        };

        public Vector3 GetTransform(Vector3 vertex) => vertex;
    }
}
