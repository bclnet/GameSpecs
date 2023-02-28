using OpenStack;
using OpenStack.Graphics.OpenGL.Renderer1;
using OpenStack.Graphics.Renderer1;
using System.Collections.Generic;
using System.Numerics;

namespace GameSpec.Graphics.Scenes
{
    public class MeshSceneNode : SceneNode, IMeshCollection
    {
        GLRenderableMesh _mesh;

        public MeshSceneNode(Scene scene, IMesh mesh, int meshIndex, IDictionary<string, string> skinMaterials = null)
            : base(scene)
        {
            _mesh = new GLRenderableMesh(Scene.Graphic as IOpenGLGraphic, mesh, meshIndex, skinMaterials);
            LocalBoundingBox = _mesh.BoundingBox;
        }

        public Vector4 Tint
        {
            get => _mesh.Tint;
            set => _mesh.Tint = value;
        }

        public IEnumerable<RenderableMesh> RenderableMeshes
        {
            get { yield return _mesh; }
        }

        public override IEnumerable<string> GetSupportedRenderModes() => _mesh.GetSupportedRenderModes();
        public override void SetRenderMode(string renderMode) => _mesh.SetRenderMode(renderMode);
        public override void Update(Scene.UpdateContext context) => _mesh.Update(context.Timestep);
        public override void Render(Scene.RenderContext context) { } // This node does not render itself; it uses the batching system via IRenderableMeshCollection
    }
}
