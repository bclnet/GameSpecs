using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.ParticleSystem.Emitters
{
    public class ContinuousEmitter : IParticleEmitter
    {
        public bool IsFinished { get; private set; }

        readonly IDictionary<string, object> _baseProperties;
        readonly INumberProvider _emissionDuration;
        readonly INumberProvider _startTime;
        readonly INumberProvider _emitRate;
        readonly float _emitInterval = 0.01f;
        Action _particleEmitCallback;
        float _time;
        float _lastEmissionTime;

        public ContinuousEmitter(IDictionary<string, object> baseProperties, IDictionary<string, object> keyValues)
        {
            _baseProperties = baseProperties;
            _emissionDuration = keyValues.GetNumberProvider("m_flEmissionDuration") ?? new LiteralNumberProvider(0);
            _startTime = keyValues.GetNumberProvider("m_flStartTime") ?? new LiteralNumberProvider(0);

            if (keyValues.ContainsKey("m_flEmitRate"))
            {
                _emitRate = keyValues.GetNumberProvider("m_flEmitRate");
                _emitInterval = 1.0f / (float)_emitRate.NextNumber();
            }
            else _emitRate = new LiteralNumberProvider(100);
        }

        public void Start(Action particleEmitCallback)
        {
            _particleEmitCallback = particleEmitCallback;
            _time = 0f;
            _lastEmissionTime = 0;
            IsFinished = false;
        }

        public void Stop()
            => IsFinished = true;

        public void Update(float frameTime)
        {
            if (IsFinished) return;
            _time += frameTime;
            var nextStartTime = _startTime.NextNumber();
            var nextEmissionDuration = _emissionDuration.NextNumber();
            if (_time >= nextStartTime && (nextEmissionDuration == 0f || _time <= nextStartTime + nextEmissionDuration))
            {
                var numToEmit = (int)Math.Floor((_time - _lastEmissionTime) / _emitInterval);
                var emitCount = Math.Min(5 * _emitRate.NextNumber(), numToEmit); // Limit the amount of particles to emit at once in case of refocus
                for (var i = 0; i < emitCount; i++) _particleEmitCallback();
                _lastEmissionTime += numToEmit * _emitInterval;
            }
        }
    }
}
