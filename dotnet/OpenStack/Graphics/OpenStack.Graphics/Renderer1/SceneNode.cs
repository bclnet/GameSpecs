using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenStack.Graphics.Renderer1
{
    //was:Render/SceneNode
    public abstract class SceneNode
    {
        public Matrix4x4 Transform
        {
            get => _transform;
            set { _transform = value; BoundingBox = _localBoundingBox.Transform(_transform); }
        }

        public string LayerName { get; set; }
        public bool LayerEnabled { get; set; } = true;
        public AABB BoundingBox { get; private set; }
        public AABB LocalBoundingBox
        {
            get => _localBoundingBox;
            protected set { _localBoundingBox = value; BoundingBox = _localBoundingBox.Transform(_transform); }
        }

        public string Name { get; set; }
        public uint Id { get; set; }

        public Scene Scene { get; }

        AABB _localBoundingBox;
        Matrix4x4 _transform = Matrix4x4.Identity;

        protected SceneNode(Scene scene) => Scene = scene;

        public abstract void Update(Scene.UpdateContext context);
        public abstract void Render(Scene.RenderContext context);

        public virtual IEnumerable<string> GetSupportedRenderModes() => Enumerable.Empty<string>();
        public virtual void SetRenderMode(string mode) { }
    }
}
