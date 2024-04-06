using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.ParticleSystem.Initializers
{
    public class RandomColor : IParticleInitializer
    {
        readonly Random _random = new Random();
        readonly Vector3 _colorMin = Vector3.One;
        readonly Vector3 _colorMax = Vector3.One;

        public RandomColor(IDictionary<string, object> keyValues)
        {
            if (keyValues.ContainsKey("m_ColorMin"))
            {
                var vectorValues = keyValues.GetInt64Array("m_ColorMin");
                _colorMin = new Vector3(vectorValues[0], vectorValues[1], vectorValues[2]) / 255f;
            }

            if (keyValues.ContainsKey("m_ColorMax"))
            {
                var vectorValues = keyValues.GetInt64Array("m_ColorMax");
                _colorMax = new Vector3(vectorValues[0], vectorValues[1], vectorValues[2]) / 255f;
            }
        }

        public Particle Initialize(ref Particle particle, ParticleSystemRenderState particleSystemRenderState)
        {
            var t = (float)_random.NextDouble();
            particle.ConstantColor = _colorMin + (t * (_colorMax - _colorMin));
            particle.Color = particle.ConstantColor;
            return particle;
        }
    }
}
