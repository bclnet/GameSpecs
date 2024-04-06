using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.ParticleSystem.Initializers
{
    public class CreateWithinSphere : IParticleInitializer
    {
        readonly Random _random = new Random();
        readonly float _radiusMin = 0f;
        readonly float _radiusMax = 0f;
        readonly float _speedMin = 0f;
        readonly float _speedMax = 0f;
        readonly Vector3 _localCoordinateSystemSpeedMin;
        readonly Vector3 _localCoordinateSystemSpeedMax;

        public CreateWithinSphere(IDictionary<string, object> keyValues)
        {
            _radiusMin = keyValues.GetFloat("m_fRadiusMin");
            _radiusMax = keyValues.GetFloat("m_fRadiusMax");
            _speedMin = keyValues.GetFloat("m_fSpeedMin");
            _speedMax = keyValues.GetFloat("m_fSpeedMax");
            _localCoordinateSystemSpeedMin = keyValues.TryGet<double[]>("m_LocalCoordinateSystemSpeedMin", out var vectorValues)
                ? new Vector3((float)vectorValues[0], (float)vectorValues[1], (float)vectorValues[2])
                : Vector3.Zero;
            _localCoordinateSystemSpeedMax = keyValues.TryGet<double[]>("m_LocalCoordinateSystemSpeedMax", out vectorValues)
                ? new Vector3((float)vectorValues[0], (float)vectorValues[1], (float)vectorValues[2])
                : Vector3.Zero;
        }

        public Particle Initialize(ref Particle particle, ParticleSystemRenderState particleSystemRenderState)
        {
            var randomVector = new Vector3(
                ((float)_random.NextDouble() * 2) - 1,
                ((float)_random.NextDouble() * 2) - 1,
                ((float)_random.NextDouble() * 2) - 1);

            // Normalize
            var direction = randomVector / randomVector.Length();
            var distance = _radiusMin + ((float)_random.NextDouble() * (_radiusMax - _radiusMin));
            var speed = _speedMin + ((float)_random.NextDouble() * (_speedMax - _speedMin));
            var localCoordinateSystemSpeed = _localCoordinateSystemSpeedMin + ((float)_random.NextDouble() * (_localCoordinateSystemSpeedMax - _localCoordinateSystemSpeedMin));
            particle.Position += direction * distance;
            particle.Velocity = (direction * speed) + localCoordinateSystemSpeed;
            return particle;
        }
    }
}
