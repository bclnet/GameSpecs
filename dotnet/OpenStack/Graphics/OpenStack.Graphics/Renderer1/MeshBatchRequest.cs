using System.Numerics;

namespace OpenStack.Graphics.Renderer1
{
    //was:Render/MeshBatchRequest
    public struct MeshBatchRequest
    {
        public Matrix4x4 Transform;
        public RenderableMesh Mesh;
        public DrawCall Call;
        public float DistanceFromCamera;
        public uint NodeId;
        public uint MeshId;
    }
}
