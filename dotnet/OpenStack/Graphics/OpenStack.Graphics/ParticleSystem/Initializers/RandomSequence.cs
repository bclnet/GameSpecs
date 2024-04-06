using System;
using System.Collections.Generic;

namespace OpenStack.Graphics.ParticleSystem.Initializers
{
    public class RandomSequence : IParticleInitializer
    {
        readonly Random _random = new Random();
        readonly int _sequenceMin;
        readonly int _sequenceMax;
        readonly bool _shuffle;
        int _counter = 0;

        public RandomSequence(IDictionary<string, object> keyValues)
        {
            _sequenceMin = (int)keyValues.GetInt64("m_nSequenceMin");
            _sequenceMax = (int)keyValues.GetInt64("m_nSequenceMax");
            _shuffle = keyValues.Get<bool>("m_bShuffle");
        }

        public Particle Initialize(ref Particle particle, ParticleSystemRenderState particleSystemState)
        {
            if (_shuffle) particle.Sequence = _random.Next(_sequenceMin, _sequenceMax + 1);
            else particle.Sequence = _sequenceMin + (_sequenceMax > _sequenceMin ? (_counter++ % (_sequenceMax - _sequenceMin)) : 0);
            return particle;
        }
    }
}
