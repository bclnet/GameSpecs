using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace OpenStack.Graphics.Renderer1
{
    //was:Render/RenderableMesh
    public abstract class RenderableMesh
    {
        public readonly AABB BoundingBox;
        public Vector4 Tint = Vector4.One;

        public readonly List<DrawCall> DrawCallsAll = new List<DrawCall>();
        public readonly List<DrawCall> DrawCallsOpaque = new List<DrawCall>();
        public readonly List<DrawCall> DrawCallsBlended = new List<DrawCall>();
        public int? AnimationTexture;
        public int AnimationTextureSize;

        public float Time = 0f;

        public int MeshIndex;

        protected readonly IMesh Mesh;
        protected readonly IVBIB VBIB;

        public RenderableMesh(Action<RenderableMesh> action, IMesh mesh, int meshIndex, IDictionary<string, string> skinMaterials = null, IModel model = null)
        {
            action(this);
            Mesh = mesh;
            VBIB = model != null ? model.RemapBoneIndices(mesh.VBIB, meshIndex) : mesh.VBIB;
            Mesh.GetBounds();
            BoundingBox = new AABB(Mesh.MinBounds, Mesh.MaxBounds);
            MeshIndex = meshIndex;
            ConfigureDrawCalls(skinMaterials, true);
        }

        public IEnumerable<string> GetSupportedRenderModes() => DrawCallsAll.SelectMany(drawCall => drawCall.Shader.RenderModes).Distinct();

        public abstract void SetRenderMode(string renderMode);

        public void SetAnimationTexture(int? texture, int animationTextureSize)
        {
            AnimationTexture = texture;
            AnimationTextureSize = animationTextureSize;
        }

        public void Update(float timeStep) => Time += timeStep;

        public void SetSkin(IDictionary<string, string> skinMaterials) => ConfigureDrawCalls(skinMaterials, false);

        protected abstract void ConfigureDrawCalls(IDictionary<string, string> skinMaterials, bool firstSetup);
    }
}
