using GameSpec.Valve.Formats.Blocks;
using OpenStack;
using OpenStack.Graphics;
using OpenStack.Graphics.OpenGL.Renderers;
using OpenStack.Graphics.Renderer;
using System.Collections.Generic;

namespace GameSpec.Valve.Graphics.OpenGL.Scenes
{
    public class DebugParticleSceneNode : SceneNode
    {
        class ParticleSystemWrapper : IParticleSystemInfo
        {
            readonly DATAParticleSystem _source;
            public ParticleSystemWrapper(DATAParticleSystem source) => _source = source;
            IDictionary<string, object> IParticleSystemInfo.Data => _source.Data;
            IEnumerable<IDictionary<string, object>> IParticleSystemInfo.Renderers => _source.Renderers;
            IEnumerable<IDictionary<string, object>> IParticleSystemInfo.Operators => _source.Operators;
            IEnumerable<IDictionary<string, object>> IParticleSystemInfo.Initializers => _source.Initializers;
            IEnumerable<IDictionary<string, object>> IParticleSystemInfo.Emitters => _source.Emitters;
            IEnumerable<string> IParticleSystemInfo.GetChildParticleNames(bool enabledOnly) => _source.GetChildParticleNames(enabledOnly);
        }

        ParticleRenderer _particleRenderer;

        public DebugParticleSceneNode(Scene scene, DATAParticleSystem particleSystem) : base(scene)
        {
            _particleRenderer = new ParticleRenderer(Scene.Graphic as IOpenGLGraphic, new ParticleSystemWrapper(particleSystem));
            LocalBoundingBox = _particleRenderer.BoundingBox;
        }

        public override void Update(Scene.UpdateContext context)
        {
            _particleRenderer.Position = Transform.Translation;
            _particleRenderer.Update(context.Timestep);

            LocalBoundingBox = _particleRenderer.BoundingBox.Translate(-_particleRenderer.Position);
        }

        public override void Render(Scene.RenderContext context) => _particleRenderer.Render(context.Camera, context.RenderPass);
    }
}
