using System;

namespace OpenStack.Graphics.ParticleSystem
{
    public class ParticleBag
    {
        readonly bool _isGrowable;
        Particle[] _particles;

        public int Count { get; private set; }

        public Span<Particle> LiveParticles
            => new Span<Particle>(_particles, 0, Count);

        public ParticleBag(int initialCapacity, bool growable)
        {
            _isGrowable = growable;
            _particles = new Particle[initialCapacity];
        }

        public int Add()
        {
            if (Count < _particles.Length) return Count++;
            else if (_isGrowable)
            {
                var newSize = _particles.Length < 1024 ? _particles.Length * 2 : _particles.Length + 1024;
                var newArray = new Particle[newSize];
                Array.Copy(_particles, 0, newArray, 0, Count);
                _particles = newArray;
                return Count++;
            }
            return -1;
        }

        public void PruneExpired()
        {
            // TODO: This alters the order of the particles so they are no longer in creation order after something expires. Fix that.
            for (var i = 0; i < Count;)
                if (_particles[i].Lifetime <= 0) { _particles[i] = _particles[Count - 1]; Count--; }
                else ++i;
        }

        public void Clear()
            => Count = 0;
    }
}
