using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.ParticleSystem.Initializers
{
    public class RemapParticleCountToScalar : IParticleInitializer
    {
        readonly long _fieldOutput = 3;
        readonly long _inputMin = 0;
        readonly long _inputMax = 10;
        readonly float _outputMin = 0f;
        readonly float _outputMax = 1f;
        readonly bool _scaleInitialRange = false;

        public RemapParticleCountToScalar(IDictionary<string, object> keyValues)
        {
            _fieldOutput = keyValues.GetInt64("m_nFieldOutput", 3);
            _inputMin = keyValues.GetInt64("m_nInputMin");
            _inputMax = keyValues.GetInt64("m_nInputMax", 10);
            _outputMin = keyValues.GetFloat("m_flOutputMin");
            _outputMax = keyValues.GetFloat("m_flOutputMax", 1f);
            _scaleInitialRange = keyValues.Get<bool>("m_bScaleInitialRange");
        }

        public Particle Initialize(ref Particle particle, ParticleSystemRenderState particleSystemRenderState)
        {
            var particleCount = Math.Min(_inputMax, Math.Max(_inputMin, particle.ParticleCount));
            var t = (particleCount - _inputMin) / (float)(_inputMax - _inputMin);
            var output = _outputMin + (t * (_outputMax - _outputMin));
            switch (_fieldOutput)
            {
                case 3:
                    particle.Radius = _scaleInitialRange
                        ? particle.Radius * output
                        : output;
                    break;
            }
            return particle;
        }
    }
}
