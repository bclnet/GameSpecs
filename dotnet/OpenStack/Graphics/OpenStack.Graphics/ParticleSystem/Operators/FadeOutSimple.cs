using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.ParticleSystem.Operators
{
    public class FadeOutSimple : IParticleOperator
    {
        readonly float _fadeOutTime;

        public FadeOutSimple(IDictionary<string, object> keyValues)
        {
            _fadeOutTime = keyValues.GetFloat("m_flFadeOutTime", .25f);
        }

        public void Update(Span<Particle> particles, float frameTime, ParticleSystemRenderState particleSystemState)
        {
            for (var i = 0; i < particles.Length; ++i)
            {
                var timeLeft = particles[i].Lifetime / particles[i].ConstantLifetime;
                if (timeLeft <= _fadeOutTime) { var t = timeLeft / _fadeOutTime; particles[i].Alpha = t * particles[i].ConstantAlpha; }
            }
        }
    }
}
