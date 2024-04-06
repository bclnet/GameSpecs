using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.ParticleSystem.Initializers
{
    public class RandomAlpha : IParticleInitializer
    {
        readonly Random _random = new Random();
        readonly int _alphaMin = 255;
        readonly int _alphaMax = 255;

        public RandomAlpha(IDictionary<string, object> keyValue)
        {
            _alphaMin = (int)keyValue.GetInt64("m_nAlphaMin", 255);
            _alphaMax = (int)keyValue.GetInt64("m_nAlphaMax", 255);
            if (_alphaMin > _alphaMax)
            {
                var temp = _alphaMin;
                _alphaMin = _alphaMax;
                _alphaMax = temp;
            }
        }

        public Particle Initialize(ref Particle particle, ParticleSystemRenderState particleSystemRenderState)
        {
            var alpha = _random.Next(_alphaMin, _alphaMax) / 255f;
            particle.ConstantAlpha = alpha;
            particle.Alpha = alpha;
            return particle;
        }
    }
}
