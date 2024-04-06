using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.ParticleSystem.Operators
{
    public class InterpolateRadius : IParticleOperator
    {
        readonly float _startTime = 0;
        readonly float _endTime = 1;
        readonly float _startScale = 1;
        readonly float _endScale = 1;

        public InterpolateRadius(IDictionary<string, object> keyValues)
        {
            _startTime = keyValues.GetFloat("m_flStartTime");
            _endTime = keyValues.GetFloat("m_flEndTime", 1f);
            _startScale = keyValues.GetFloat("m_flStartScale", 1f);
            _endScale = keyValues.GetFloat("m_flEndScale", 1f);
        }

        public void Update(Span<Particle> particles, float frameTime, ParticleSystemRenderState particleSystemState)
        {
            for (var i = 0; i < particles.Length; ++i)
            {
                var time = 1 - (particles[i].Lifetime / particles[i].ConstantLifetime);
                if (time >= _startTime && time <= _endTime)
                {
                    var t = (time - _startTime) / (_endTime - _startTime);
                    var radiusScale = (_startScale * (1 - t)) + (_endScale * t);
                    particles[i].Radius = particles[i].ConstantRadius * radiusScale;
                }
            }
        }
    }
}
