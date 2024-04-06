using OpenStack.Graphics.OpenGL.Renderer1.Renderers;
using OpenStack.Graphics.ParticleSystem.Emitters;
using OpenStack.Graphics.ParticleSystem.Initializers;
using OpenStack.Graphics.ParticleSystem.Operators;
using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.ParticleSystem
{
    public static class ParticleControllerFactory
    {
        // Register particle emitters
        static readonly IDictionary<string, Func<IDictionary<string, object>, IDictionary<string, object>, IParticleEmitter>> EmitterDictionary
           = new Dictionary<string, Func<IDictionary<string, object>, IDictionary<string, object>, IParticleEmitter>>
           {
               ["C_OP_InstantaneousEmitter"] = (baseProperties, emitterInfo) => new InstantaneousEmitter(baseProperties, emitterInfo),
               ["C_OP_ContinuousEmitter"] = (baseProperties, emitterInfo) => new ContinuousEmitter(baseProperties, emitterInfo),
           };

        // Register particle initializers
        static readonly IDictionary<string, Func<IDictionary<string, object>, IParticleInitializer>> InitializerDictionary
           = new Dictionary<string, Func<IDictionary<string, object>, IParticleInitializer>>
           {
               ["C_INIT_CreateWithinSphere"] = initializerInfo => new CreateWithinSphere(initializerInfo),
               ["C_INIT_InitialVelocityNoise"] = initializerInfo => new InitialVelocityNoise(initializerInfo),
               ["C_INIT_OffsetVectorToVector"] = initializerInfo => new OffsetVectorToVector(initializerInfo),
               ["C_INIT_PositionOffset"] = initializerInfo => new PositionOffset(initializerInfo),
               ["C_INIT_RandomAlpha"] = initializerInfo => new RandomAlpha(initializerInfo),
               ["C_INIT_RandomColor"] = initializerInfo => new RandomColor(initializerInfo),
               ["C_INIT_RandomLifeTime"] = initializerInfo => new RandomLifeTime(initializerInfo),
               ["C_INIT_RandomRadius"] = initializerInfo => new RandomRadius(initializerInfo),
               ["C_INIT_RandomRotation"] = initializerInfo => new RandomRotation(initializerInfo),
               ["C_INIT_RandomRotationSpeed"] = initializerInfo => new RandomRotationSpeed(initializerInfo),
               ["C_INIT_RandomSequence"] = initializerInfo => new RandomSequence(initializerInfo),
               ["C_INIT_RandomTrailLength"] = initializerInfo => new RandomTrailLength(initializerInfo),
               ["C_INIT_RemapParticleCountToScalar"] = initializerInfo => new RemapParticleCountToScalar(initializerInfo),
               ["C_INIT_RingWave"] = initializerInfo => new RingWave(initializerInfo),
           };

        // Register particle operators
        static readonly IDictionary<string, Func<IDictionary<string, object>, IParticleOperator>> OperatorDictionary
           = new Dictionary<string, Func<IDictionary<string, object>, IParticleOperator>>
           {
               ["C_OP_Decay"] = operatorInfo => new Decay(operatorInfo),
               ["C_OP_BasicMovement"] = operatorInfo => new BasicMovement(operatorInfo),
               ["C_OP_ColorInterpolate"] = operatorInfo => new ColorInterpolate(operatorInfo),
               ["C_OP_InterpolateRadius"] = operatorInfo => new InterpolateRadius(operatorInfo),
               ["C_OP_FadeAndKill"] = operatorInfo => new FadeAndKill(operatorInfo),
               ["C_OP_FadeInSimple"] = operatorInfo => new FadeInSimple(operatorInfo),
               ["C_OP_FadeOutSimple"] = operatorInfo => new FadeOutSimple(operatorInfo),
               ["C_OP_OscillateScalar"] = operatorInfo => new OscillateScalar(operatorInfo),
               ["C_OP_SpinUpdate"] = operatorInfo => new SpinUpdate(operatorInfo),
           };

        // Register particle renderers
        static readonly IDictionary<string, Func<IDictionary<string, object>, IOpenGLGraphic, IParticleRenderer>> RendererDictionary
           = new Dictionary<string, Func<IDictionary<string, object>, IOpenGLGraphic, IParticleRenderer>>
           {
               ["C_OP_RenderSprites"] = (rendererInfo, graphic) => new SpritesParticleRenderer(rendererInfo, graphic as IOpenGLGraphic),
               ["C_OP_RenderTrails"] = (rendererInfo, graphic) => new TrailsParticleRenderer(rendererInfo, graphic as IOpenGLGraphic),
           };

        public static bool TryCreateEmitter(string name, IDictionary<string, object> baseProperties, IDictionary<string, object> emitterInfo, out IParticleEmitter emitter)
        {
            if (EmitterDictionary.TryGetValue(name, out var factory))
            {
                emitter = factory(baseProperties, emitterInfo);
                return true;
            }
            emitter = default;
            return false;
        }

        public static bool TryCreateInitializer(string name, IDictionary<string, object> initializerInfo, out IParticleInitializer initializer)
        {
            if (InitializerDictionary.TryGetValue(name, out var factory)) { initializer = factory(initializerInfo); return true; }
            initializer = default;
            return false;
        }

        public static bool TryCreateOperator(string name, IDictionary<string, object> operatorInfo, out IParticleOperator @operator)
        {
            if (OperatorDictionary.TryGetValue(name, out var factory)) { @operator = factory(operatorInfo); return true; }
            @operator = default;
            return false;
        }

        public static bool TryCreateRender(string name, IDictionary<string, object> rendererInfo, IOpenGLGraphic graphic, out IParticleRenderer renderer)
        {
            if (RendererDictionary.TryGetValue(name, out var factory)) { renderer = factory(rendererInfo, graphic); return true; }
            renderer = default;
            return false;
        }
    }
}
