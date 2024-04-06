using OpenStack.Graphics.OpenGL.Renderer1;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.Renderer1
{
    //was:Render/DrawCall
    public class DrawCall
    {
        [Flags]
        public enum RenderMeshDrawPrimitiveFlags //was:Resource/Enum/RenderMeshDrawPrimitiveFlags
        {
            None = 0x0,
            UseShadowFastPath = 0x1,
            UseCompressedNormalTangent = 0x2,
            IsOccluder = 0x4,
            InputLayoutIsNotMatchedToMaterial = 0x8,
            HasBakedLightingFromVertexStream = 0x10,
            HasBakedLightingFromLightmap = 0x20,
            CanBatchWithDynamicShaderConstants = 0x40,
            DrawLast = 0x80,
            HasPerInstanceBakedLightingData = 0x100,
        }

        public int PrimitiveType;
        public Shader Shader;
        public uint BaseVertex;
        //public uint VertexCount;
        public uint StartIndex;
        public int IndexCount;
        //public uint InstanceIndex;
        //public uint InstanceCount;
        //public float UvDensity;
        //public string Flags;
        public Vector3 TintColor { get; set; } = Vector3.One;
        public RenderMaterial Material { get; set; }
        public uint VertexArrayObject { get; set; }
        public (uint Id, uint Offset) VertexBuffer { get; set; }
        public int IndexType { get; set; }
        public (uint Id, uint Offset) IndexBuffer { get; set; }

        //was:Resource/ResourceTypes/Mesh.IsCompressedNormalTangent
        public static bool IsCompressedNormalTangent(IDictionary<string, object> drawCall)
        {
            if (drawCall.ContainsKey("m_bUseCompressedNormalTangent")) return drawCall.Get<bool>("m_bUseCompressedNormalTangent");
            if (!drawCall.ContainsKey("m_nFlags")) return false;
            var flags = drawCall.Get<object>("m_nFlags");
            return flags switch
            {
                string flagsString => flagsString.Contains("MESH_DRAW_FLAGS_USE_COMPRESSED_NORMAL_TANGENT", StringComparison.InvariantCulture),
                long flagsLong => ((RenderMeshDrawPrimitiveFlags)flagsLong & RenderMeshDrawPrimitiveFlags.UseCompressedNormalTangent) != 0,
                byte flagsByte => ((RenderMeshDrawPrimitiveFlags)flagsByte & RenderMeshDrawPrimitiveFlags.UseCompressedNormalTangent) != 0,
                _ => false
            };
        }
    }

    //public class DrawCall<T> : DrawCall
    //{
    //    public T Material { get; set; } //: Material
    //}
}
