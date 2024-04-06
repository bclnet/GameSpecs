using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.ParticleSystem.Initializers
{
    public class RandomRotation : IParticleInitializer
    {
        const float PiOver180 = (float)Math.PI / 180f;
        readonly Random _random = new Random();
        readonly float _degreesMin;
        readonly float _degreesMax;
        readonly float _degreesOffset;
        readonly long _fieldOutput;
        readonly bool _randomlyFlipDirection;

        public RandomRotation(IDictionary<string, object> keyValues)
        {
            _degreesMin = keyValues.GetFloat("m_flDegreesMin");
            _degreesMax = keyValues.GetFloat("m_flDegreesMax", 360f);
            _degreesOffset = keyValues.GetFloat("m_flDegrees");
            _fieldOutput = keyValues.GetInt64("m_nFieldOutput", 4);
            _randomlyFlipDirection = keyValues.Get<bool>("m_bRandomlyFlipDirection");
        }

        public Particle Initialize(ref Particle particle, ParticleSystemRenderState particleSystemRenderState)
        {
            var degrees = _degreesOffset + _degreesMin + ((float)_random.NextDouble() * (_degreesMax - _degreesMin));
            if (_randomlyFlipDirection && _random.NextDouble() > 0.5) degrees *= -1;
            if (_fieldOutput == 4) particle.Rotation = new Vector3(particle.Rotation.X, particle.Rotation.Y, degrees * PiOver180); // Roll
            else if (_fieldOutput == 12) particle.Rotation = new Vector3(particle.Rotation.X, degrees * PiOver180, particle.Rotation.Z); // Yaw
            return particle;
        }
    }
}
