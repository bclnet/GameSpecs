using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.ParticleSystem.Initializers
{
    public class RandomRotationSpeed : IParticleInitializer
    {
        const float PiOver180 = (float)Math.PI / 180f;
        readonly Random _random = new Random();
        readonly ParticleField _fieldOutput;
        readonly bool _randomlyFlipDirection;
        readonly float _degrees;
        readonly float _degreesMin;
        readonly float _degreesMax;

        public RandomRotationSpeed(IDictionary<string, object> keyValues)
        {
            _fieldOutput = (ParticleField)keyValues.GetInt64("m_nFieldOutput", (int)ParticleField.Roll);
            _randomlyFlipDirection = keyValues.Get<bool>("m_bRandomlyFlipDirection", true);
            _degrees = keyValues.GetFloat("m_flDegrees");
            _degreesMin = keyValues.GetFloat("m_flDegreesMin");
            _degreesMax = keyValues.GetFloat("m_flDegreesMax", 360f);
        }

        public Particle Initialize(ref Particle particle, ParticleSystemRenderState particleSystemState)
        {
            var value = PiOver180 * (_degrees + _degreesMin + ((float)_random.NextDouble() * (_degreesMax - _degreesMin)));
            if (_randomlyFlipDirection && _random.NextDouble() > 0.5) value *= -1;
            if (_fieldOutput == ParticleField.Yaw) particle.RotationSpeed = new Vector3(value, 0, 0);
            else if (_fieldOutput == ParticleField.Roll) particle.RotationSpeed = new Vector3(0, 0, value);
            return particle;
        }
    }
}
