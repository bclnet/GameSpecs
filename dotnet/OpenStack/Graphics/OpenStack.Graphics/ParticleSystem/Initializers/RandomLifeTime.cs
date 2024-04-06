using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.ParticleSystem.Initializers
{
    public class RandomLifeTime : IParticleInitializer
    {
        readonly Random _random = new Random();
        readonly float _lifetimeMin;
        readonly float _lifetimeMax;

        public RandomLifeTime(IDictionary<string, object> keyValues)
        {
            _lifetimeMin = keyValues.GetFloat("m_fLifetimeMin");
            _lifetimeMax = keyValues.GetFloat("m_fLifetimeMax");
        }

        public Particle Initialize(ref Particle particle, ParticleSystemRenderState particleSystemRenderState)
        {
            var lifetime = _lifetimeMin + ((_lifetimeMax - _lifetimeMin) * (float)_random.NextDouble());
            particle.ConstantLifetime = lifetime;
            particle.Lifetime = lifetime;
            return particle;
        }
    }
}
