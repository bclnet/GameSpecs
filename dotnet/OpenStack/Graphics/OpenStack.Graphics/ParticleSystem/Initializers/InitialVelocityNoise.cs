using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.ParticleSystem.Initializers
{
    public class InitialVelocityNoise : IParticleInitializer
    {
        readonly IVectorProvider _outputMin;
        readonly IVectorProvider _outputMax;
        readonly INumberProvider _noiseScale;

        public InitialVelocityNoise(IDictionary<string, object> keyValues)
        {
            _outputMin = keyValues.GetVectorProvider("m_vecOutputMin") ?? new LiteralVectorProvider(Vector3.Zero);
            _outputMax = keyValues.GetVectorProvider("m_vecOutputMax") ?? new LiteralVectorProvider(Vector3.One);
            _noiseScale = keyValues.GetNumberProvider("m_flNoiseScale") ?? new LiteralNumberProvider(1f);
        }

        public Particle Initialize(ref Particle particle, ParticleSystemRenderState particleSystemState)
        {
            var noiseScale = (float)_noiseScale.NextNumber();
            var r = new Vector3(
                Simplex1D(particleSystemState.Lifetime * noiseScale),
                Simplex1D((particleSystemState.Lifetime * noiseScale) + 101723),
                Simplex1D((particleSystemState.Lifetime * noiseScale) + 555557));
            var min = _outputMin.NextVector();
            var max = _outputMax.NextVector();
            particle.Velocity = min + (r * (max - min));
            return particle;
        }

        // Simple perlin noise implementation

        static float Simplex1D(float t)
        {
            var previous = PseudoRandom((float)Math.Floor(t));
            var next = PseudoRandom((float)Math.Ceiling(t));
            return CosineInterpolate(previous, next, t % 1f);
        }

        static float PseudoRandom(float t)
           => ((1013904223517 * t) % 1664525) / 1664525f;

        static float CosineInterpolate(float start, float end, float mu)
        {
            var mu2 = (1 - (float)Math.Cos(mu * Math.PI)) / 2f;
            return (start * (1 - mu2)) + (end * mu2);
        }
    }
}
