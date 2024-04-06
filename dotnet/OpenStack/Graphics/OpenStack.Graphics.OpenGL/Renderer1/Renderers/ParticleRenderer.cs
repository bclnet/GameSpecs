using OpenStack.Graphics.ParticleSystem;
using OpenStack.Graphics.ParticleSystem.Emitters;
using OpenStack.Graphics.ParticleSystem.Initializers;
using OpenStack.Graphics.ParticleSystem.Operators;
using OpenStack.Graphics.Renderer1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenStack.Graphics.OpenGL.Renderer1.Renderers
{
    public class ParticleRenderer : IRenderer
    {
        public IEnumerable<IParticleEmitter> Emitters { get; private set; } = new List<IParticleEmitter>();

        public IEnumerable<IParticleInitializer> Initializers { get; private set; } = new List<IParticleInitializer>();

        public IEnumerable<IParticleOperator> Operators { get; private set; } = new List<IParticleOperator>();

        public IEnumerable<IParticleRenderer> Renderers { get; private set; } = new List<IParticleRenderer>();

        public AABB BoundingBox { get; private set; }

        public Vector3 Position
        {
            get => _systemRenderState.GetControlPoint(0);
            set
            {
                _systemRenderState.SetControlPoint(0, value);
                foreach (var child in _childParticleRenderers) child.Position = value;
            }
        }

        readonly IOpenGLGraphic _graphic;
        readonly List<ParticleRenderer> _childParticleRenderers;
        bool _hasStarted = false;

        ParticleBag _particleBag;
        int _particlesEmitted = 0;
        ParticleSystemRenderState _systemRenderState;

        // TODO: Passing in position here was for testing, do it properly
        public ParticleRenderer(IOpenGLGraphic graphic, IParticleSystem particleSystem, Vector3 pos = default)
        {
            _graphic = graphic;
            _childParticleRenderers = new List<ParticleRenderer>();

            _particleBag = new ParticleBag(100, true);
            _systemRenderState = new ParticleSystemRenderState();

            _systemRenderState.SetControlPoint(0, pos);

            BoundingBox = new AABB(pos + new Vector3(-32, -32, -32), pos + new Vector3(32, 32, 32));

            SetupEmitters(particleSystem.Data, particleSystem.Emitters);
            SetupInitializers(particleSystem.Initializers);
            SetupOperators(particleSystem.Operators);
            SetupRenderers(particleSystem.Renderers);

            SetupChildParticles(particleSystem.GetChildParticleNames(true));
        }

        public void Start()
        {
            foreach (var emitter in Emitters) emitter.Start(EmitParticle);
            foreach (var childParticleRenderer in _childParticleRenderers) childParticleRenderer.Start();
        }

        void EmitParticle()
        {
            var index = _particleBag.Add();
            if (index < 0) { Console.WriteLine("Out of space in particle bag"); return; }
            _particleBag.LiveParticles[index].ParticleCount = _particlesEmitted++;
            InitializeParticle(ref _particleBag.LiveParticles[index]);
        }

        void InitializeParticle(ref Particle p)
        {
            p.Position = _systemRenderState.GetControlPoint(0);
            foreach (var initializer in Initializers) initializer.Initialize(ref p, _systemRenderState);
        }

        public void Stop()
        {
            foreach (var emitter in Emitters) emitter.Stop();
            foreach (var childParticleRenderer in _childParticleRenderers) childParticleRenderer.Stop();
        }

        public void Restart()
        {
            Stop();
            _systemRenderState.Lifetime = 0;
            _particleBag.Clear();
            Start();

            foreach (var childParticleRenderer in _childParticleRenderers) childParticleRenderer.Restart();
        }

        public void Update(float frameTime)
        {
            if (!_hasStarted) { Start(); _hasStarted = true; }

            _systemRenderState.Lifetime += frameTime;

            foreach (var emitter in Emitters) emitter.Update(frameTime);
            foreach (var particleOperator in Operators) particleOperator.Update(_particleBag.LiveParticles, frameTime, _systemRenderState);

            // Remove all dead particles
            _particleBag.PruneExpired();

            var center = _systemRenderState.GetControlPoint(0);
            if (_particleBag.Count == 0) BoundingBox = new AABB(center, center);
            else
            {
                var minParticlePos = center;
                var maxParticlePos = center;

                var liveParticles = _particleBag.LiveParticles;
                for (var i = 0; i < liveParticles.Length; ++i)
                {
                    var pos = liveParticles[i].Position;
                    var radius = liveParticles[i].Radius;
                    minParticlePos = Vector3.Min(minParticlePos, pos - new Vector3(radius));
                    maxParticlePos = Vector3.Max(maxParticlePos, pos + new Vector3(radius));
                }

                BoundingBox = new AABB(minParticlePos, maxParticlePos);
            }

            foreach (var childParticleRenderer in _childParticleRenderers)
            {
                childParticleRenderer.Update(frameTime);
                BoundingBox = BoundingBox.Union(childParticleRenderer.BoundingBox);
            }

            // Restart if all emitters are done and all particles expired
            if (IsFinished()) Restart();
        }

        public bool IsFinished()
            => Emitters.All(e => e.IsFinished)
            && _particleBag.Count == 0
            && _childParticleRenderers.All(r => r.IsFinished());

        public void Render(Camera camera, RenderPass renderPass)
        {
            if (_particleBag.Count == 0) return;
            if (renderPass == RenderPass.Translucent || renderPass == RenderPass.Both)
                foreach (var renderer in Renderers) renderer.Render(_particleBag, camera.ViewProjectionMatrix, camera.CameraViewMatrix);
            foreach (var childParticleRenderer in _childParticleRenderers) childParticleRenderer.Render(camera, RenderPass.Both);
        }

        public IEnumerable<string> GetSupportedRenderModes() => Renderers.SelectMany(renderer => renderer.GetSupportedRenderModes()).Distinct();

        void SetupEmitters(IDictionary<string, object> baseProperties, IEnumerable<IDictionary<string, object>> emitterData)
        {
            var emitters = new List<IParticleEmitter>();
            foreach (var emitterInfo in emitterData)
            {
                var emitterClass = emitterInfo.Get<string>("_class");
                if (ParticleControllerFactory.TryCreateEmitter(emitterClass, baseProperties, emitterInfo, out var emitter)) emitters.Add(emitter);
                else Console.WriteLine($"Unsupported emitter class '{emitterClass}'.");
            }
            Emitters = emitters;
        }

        void SetupInitializers(IEnumerable<IDictionary<string, object>> initializerData)
        {
            var initializers = new List<IParticleInitializer>();
            foreach (var initializerInfo in initializerData)
            {
                var initializerClass = initializerInfo.Get<string>("_class");
                if (ParticleControllerFactory.TryCreateInitializer(initializerClass, initializerInfo, out var initializer)) initializers.Add(initializer);
                else Console.WriteLine($"Unsupported initializer class '{initializerClass}'.");
            }
            Initializers = initializers;
        }

        void SetupOperators(IEnumerable<IDictionary<string, object>> operatorData)
        {
            var operators = new List<IParticleOperator>();
            foreach (var operatorInfo in operatorData)
            {
                var operatorClass = operatorInfo.Get<string>("_class");
                if (ParticleControllerFactory.TryCreateOperator(operatorClass, operatorInfo, out var @operator)) operators.Add(@operator);
                else Console.WriteLine($"Unsupported operator class '{operatorClass}'.");
            }
            Operators = operators;
        }

        void SetupRenderers(IEnumerable<IDictionary<string, object>> rendererData)
        {
            var renderers = new List<IParticleRenderer>();
            foreach (var rendererInfo in rendererData)
            {
                var rendererClass = rendererInfo.Get<string>("_class");
                if (ParticleControllerFactory.TryCreateRender(rendererClass, rendererInfo, _graphic, out var renderer)) renderers.Add(renderer);
                else Console.WriteLine($"Unsupported renderer class '{rendererClass}'.");
            }
            Renderers = renderers;
        }

        void SetupChildParticles(IEnumerable<string> childNames)
        {
            foreach (var childName in childNames)
            {
                var childSystem = _graphic.LoadFileObject<IParticleSystem>(childName).Result;
                _childParticleRenderers.Add(new ParticleRenderer(_graphic as IOpenGLGraphic, childSystem, _systemRenderState.GetControlPoint(0)));
            }
        }
    }
}
