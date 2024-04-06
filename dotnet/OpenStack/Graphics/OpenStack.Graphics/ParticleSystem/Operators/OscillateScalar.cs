using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.ParticleSystem.Operators
{
    public class OscillateScalar : IParticleOperator
    {
        Random _random = new Random();

        ParticleField _outputField;
        float _rateMin;
        float _rateMax;
        float _frequencyMin;
        float _frequencyMax;
        float _oscillationMultiplier;
        float _oscillationOffset;
        bool _proportional;

        public OscillateScalar(IDictionary<string, object> keyValues)
        {
            _outputField = (ParticleField)keyValues.GetInt64("m_nField", (int)ParticleField.Alpha);
            _rateMin = keyValues.GetFloat("m_RateMin");
            _rateMax = keyValues.GetFloat("m_RateMax");
            _frequencyMin = keyValues.GetFloat("m_FrequencyMin", 1f);
            _frequencyMax = keyValues.GetFloat("m_FrequencyMax", 1f);
            _oscillationMultiplier = keyValues.GetFloat("m_flOscMult", 2f);
            _oscillationOffset = keyValues.GetFloat("m_flOscAdd", .5f);
            _proportional = keyValues.Get<bool>("m_bProportionalOp", true);
        }

        public void Update(Span<Particle> particles, float frameTime, ParticleSystemRenderState particleSystemState)
        {
            // Remove expired particles
            /*var particlesToRemove = particleRates.Keys.Except(particles[i]).ToList();
            foreach (var p in particlesToRemove)
            {
                particleRates.Remove(p);
                particleFrequencies.Remove(p);
            }*/

            // Update remaining particles
            for (var i = 0; i < particles.Length; ++i)
            {
                var rate = GetParticleRate(particles[i].ParticleCount);
                var frequency = GetParticleFrequency(particles[i].ParticleCount);
                var t = _proportional
                    ? 1 - (particles[i].Lifetime / particles[i].ConstantLifetime)
                    : particles[i].Lifetime;
                var delta = (float)Math.Sin(((t * frequency * _oscillationMultiplier) + _oscillationOffset) * Math.PI);
                if (_outputField == ParticleField.Radius) particles[i].Radius += delta * rate * frameTime;
                else if (_outputField == ParticleField.Alpha) particles[i].Alpha += delta * rate * frameTime;
                else if (_outputField == ParticleField.AlphaAlternate) particles[i].AlphaAlternate += delta * rate * frameTime;
            }
        }

        Dictionary<int, float> _particleRates = new Dictionary<int, float>();
        Dictionary<int, float> _particleFrequencies = new Dictionary<int, float>();

        float GetParticleRate(int particleId)
        {
            if (_particleRates.TryGetValue(particleId, out var rate)) return rate;
            else { var newRate = _rateMin + ((float)_random.NextDouble() * (_rateMax - _rateMin)); _particleRates[particleId] = newRate; return newRate; }
        }

        float GetParticleFrequency(int particleId)
        {
            if (_particleFrequencies.TryGetValue(particleId, out var frequency)) return frequency;
            else { var newFrequency = _frequencyMin + ((float)_random.NextDouble() * (_frequencyMax - _frequencyMin)); _particleFrequencies[particleId] = newFrequency; return newFrequency; }
        }
    }
}
