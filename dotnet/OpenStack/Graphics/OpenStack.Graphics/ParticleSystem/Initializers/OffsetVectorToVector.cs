using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.ParticleSystem.Initializers
{
    public class OffsetVectorToVector : IParticleInitializer
    {
        readonly Random _random = new Random();
        readonly ParticleField _inputField;
        readonly ParticleField _outputField;
        readonly Vector3 _offsetMin;
        readonly Vector3 _offsetMax;

        public OffsetVectorToVector(IDictionary<string, object> keyValues)
        {
            _inputField = (ParticleField)keyValues.GetInt64("m_nFieldInput");
            _outputField = (ParticleField)keyValues.GetInt64("m_nFieldOutput");
            _offsetMin = keyValues.TryGet<double[]>("m_vecOutputMin", out var vectorValues)
                ? new Vector3((float)vectorValues[0], (float)vectorValues[1], (float)vectorValues[2])
                : Vector3.Zero;
            _offsetMax = keyValues.TryGet<double[]>("m_vecOutputMax", out vectorValues)
                ? new Vector3((float)vectorValues[0], (float)vectorValues[1], (float)vectorValues[2])
                : Vector3.One;
        }

        public Particle Initialize(ref Particle particle, ParticleSystemRenderState particleSystemState)
        {
            var input = particle.GetVector(_inputField);

            var offset = new Vector3(
                Lerp(_offsetMin.X, _offsetMax.X, (float)_random.NextDouble()),
                Lerp(_offsetMin.Y, _offsetMax.Y, (float)_random.NextDouble()),
                Lerp(_offsetMin.Z, _offsetMax.Z, (float)_random.NextDouble()));
            if (_outputField == ParticleField.Position) particle.Position += input + offset;
            else if (_outputField == ParticleField.PositionPrevious) particle.PositionPrevious = input + offset;
            return particle;
        }

        static float Lerp(float min, float max, float t)
           => min + (t * (max - min));
    }
}
