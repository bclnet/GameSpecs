using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.ParticleSystem.Emitters
{
    public class InstantaneousEmitter : IParticleEmitter
    {
        public bool IsFinished { get; private set; }

        readonly IDictionary<string, object> _baseProperties;
        Action _particleEmitCallback;
        INumberProvider _emitCount;
        float _startTime;
        float _time;

        public InstantaneousEmitter(IDictionary<string, object> baseProperties, IDictionary<string, object> keyValues)
        {
            _baseProperties = baseProperties;

            _emitCount = keyValues.GetNumberProvider("m_nParticlesToEmit");
            _startTime = keyValues.GetFloat("m_flStartTime");
        }

        public void Start(Action particleEmitCallback)
        {
            _particleEmitCallback = particleEmitCallback;
            IsFinished = false;
            _time = 0;
        }

        public void Stop() { }

        public void Update(float frameTime)
        {
            _time += frameTime;
            if (!IsFinished && _time >= _startTime)
            {
                var numToEmit = _emitCount.NextInt(); // Get value from number provider
                for (var i = 0; i < numToEmit; i++) _particleEmitCallback();
                IsFinished = true;
            }
        }
    }
}
