using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.ParticleSystem.Operators
{
    public class ColorInterpolate : IParticleOperator
    {
        readonly Vector3 _colorFade;
        readonly float _fadeStartTime;
        readonly float _fadeEndTime;

        public ColorInterpolate(IDictionary<string, object> keyValues)
        {
            if (keyValues.ContainsKey("m_ColorFade"))
            {
                var vectorValues = keyValues.GetInt64Array("m_ColorFade");
                _colorFade = new Vector3(vectorValues[0], vectorValues[1], vectorValues[2]) / 255f;
            }
            else _colorFade = Vector3.One;
            _fadeStartTime = keyValues.GetFloat("m_flFadeStartTime");
            _fadeEndTime = keyValues.GetFloat("m_flFadeEndTime", 1f);
        }

        public void Update(Span<Particle> particles, float frameTime, ParticleSystemRenderState particleSystemState)
        {
            for (var i = 0; i < particles.Length; ++i)
            {
                var time = 1 - (particles[i].Lifetime / particles[i].ConstantLifetime);
                if (time >= _fadeStartTime && time <= _fadeEndTime)
                {
                    var t = (time - _fadeStartTime) / (_fadeEndTime - _fadeStartTime);
                    // Interpolate from constant color to fade color
                    particles[i].Color = ((1 - t) * particles[i].ConstantColor) + (t * _colorFade);
                }
            }
        }
    }
}
