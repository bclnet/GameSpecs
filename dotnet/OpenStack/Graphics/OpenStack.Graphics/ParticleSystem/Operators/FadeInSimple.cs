using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.ParticleSystem.Operators
{
    public class FadeInSimple : IParticleOperator
    {
        readonly float _fadeInTime;

        public FadeInSimple(IDictionary<string, object> keyValues)
        {
            _fadeInTime = keyValues.GetFloat("m_flFadeInTime", .25f);
        }

        public void Update(Span<Particle> particles, float frameTime, ParticleSystemRenderState particleSystemState)
        {
            for (var i = 0; i < particles.Length; ++i)
            {
                var time = 1 - (particles[i].Lifetime / particles[i].ConstantLifetime);
                if (time <= _fadeInTime) particles[i].Alpha = (time / _fadeInTime) * particles[i].ConstantAlpha;
            }
        }
    }
}
