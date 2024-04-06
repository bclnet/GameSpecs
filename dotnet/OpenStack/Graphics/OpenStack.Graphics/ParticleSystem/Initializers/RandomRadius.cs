using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.ParticleSystem.Initializers
{
    public class RandomRadius : IParticleInitializer
    {
        readonly Random _random = new Random();
        readonly float _radiusMin;
        readonly float _radiusMax;

        public RandomRadius(IDictionary<string, object> keyValues)
        {
            _radiusMin = keyValues.GetFloat("m_flRadiusMin");
            _radiusMax = keyValues.GetFloat("m_flRadiusMax");
        }

        public Particle Initialize(ref Particle particle, ParticleSystemRenderState particleSystemRenderState)
        {
            particle.ConstantRadius = _radiusMin + ((float)_random.NextDouble() * (_radiusMax - _radiusMin));
            particle.Radius = particle.ConstantRadius;
            return particle;
        }
    }
}
