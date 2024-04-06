using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.ParticleSystem.Initializers
{
    public class RingWave : IParticleInitializer
    {
        readonly Random random = new Random();
        readonly bool _evenDistribution;
        readonly float _initialRadius;
        readonly float _thickness;
        readonly float _particlesPerOrbit;
        float _orbitCount;

        public RingWave(IDictionary<string, object> keyValues)
        {
            _evenDistribution = keyValues.Get<bool>("m_bEvenDistribution");
            _particlesPerOrbit = keyValues.GetFloat("m_flParticlesPerOrbit", -1f);
            _initialRadius = keyValues.GetFloat("m_flInitialRadius");
            _thickness = keyValues.GetFloat("m_flThickness");
        }

        public Particle Initialize(ref Particle particle, ParticleSystemRenderState particleSystemState)
        {
            var radius = _initialRadius + ((float)random.NextDouble() * _thickness);
            var angle = GetNextAngle();
            particle.Position += radius * new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0);
            return particle;
        }

        double GetNextAngle()
        {
            if (_evenDistribution)
            {
                var offset = _orbitCount / _particlesPerOrbit;
                _orbitCount = (_orbitCount + 1) % _particlesPerOrbit;
                return offset * 2 * Math.PI;
            }
            else return 2 * Math.PI * random.NextDouble(); // Return a random angle between 0 and 2pi
        }
    }
}
