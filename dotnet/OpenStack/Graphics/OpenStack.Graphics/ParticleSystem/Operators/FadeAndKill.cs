using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.ParticleSystem.Operators
{
    public class FadeAndKill : IParticleOperator
    {
        readonly float _startFadeInTime;
        readonly float _endFadeInTime;
        readonly float _startFadeOutTime;
        readonly float _endFadeOutTime;
        readonly float _startAlpha;
        readonly float _endAlpha;

        public FadeAndKill(IDictionary<string, object> keyValues)
        {
            _startFadeInTime = keyValues.GetFloat("m_flStartFadeInTime");
            _endFadeInTime = keyValues.GetFloat("m_flEndFadeInTime", .5f);
            _startFadeOutTime = keyValues.GetFloat("m_flStartFadeOutTime", .5f);
            _endFadeOutTime = keyValues.GetFloat("m_flEndFadeOutTime", 1f);
            _startAlpha = keyValues.GetFloat("m_flStartAlpha", 1f);
            _endAlpha = keyValues.GetFloat("m_flEndAlpha");
        }

        public void Update(Span<Particle> particles, float frameTime, ParticleSystemRenderState particleSystemState)
        {
            for (var i = 0; i < particles.Length; ++i)
            {
                var time = 1 - (particles[i].Lifetime / particles[i].ConstantLifetime);
                // If fading in
                if (time >= _startFadeInTime && time <= _endFadeInTime)
                {
                    var t = (time - _startFadeInTime) / (_endFadeInTime - _startFadeInTime);
                    // Interpolate from startAlpha to constantAlpha
                    particles[i].Alpha = ((1 - t) * _startAlpha) + (t * particles[i].ConstantAlpha);
                }
                // If fading out
                if (time >= _startFadeOutTime && time <= _endFadeOutTime)
                {
                    var t = (time - _startFadeOutTime) / (_endFadeOutTime - _startFadeOutTime);
                    // Interpolate from constantAlpha to end alpha
                    particles[i].Alpha = ((1 - t) * particles[i].ConstantAlpha) + (t * _endAlpha);
                }
                particles[i].Lifetime -= frameTime;
            }
        }
    }
}
