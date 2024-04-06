using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.ParticleSystem.Initializers
{
    public class PositionOffset : IParticleInitializer
    {
        readonly Random _random = new Random();
        readonly Vector3 _offsetMin;
        readonly Vector3 _offsetMax;

        public PositionOffset(IDictionary<string, object> keyValues)
        {
            _offsetMin = keyValues.TryGet<double[]>("m_OffsetMin", out var vectorValues)
                ? new Vector3((float)vectorValues[0], (float)vectorValues[1], (float)vectorValues[2])
                : Vector3.Zero;
            _offsetMax = keyValues.TryGet<double[]>("m_OffsetMax", out vectorValues)
                ? new Vector3((float)vectorValues[0], (float)vectorValues[1], (float)vectorValues[2])
                : Vector3.Zero;
        }

        public Particle Initialize(ref Particle particle, ParticleSystemRenderState particleSystemRenderState)
        {
            var distance = _offsetMax - _offsetMin;
            var offset = _offsetMin + (distance * new Vector3((float)_random.NextDouble(), (float)_random.NextDouble(), (float)_random.NextDouble()));
            particle.Position += offset;
            return particle;
        }
    }
}
