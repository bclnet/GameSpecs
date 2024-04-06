using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.ParticleSystem
{
    public interface IParticleRenderer
    {
        void Render(ParticleBag particles, Matrix4x4 viewProjectionMatrix, Matrix4x4 modelViewMatrix);
        void SetRenderMode(string renderMode);
        IEnumerable<string> GetSupportedRenderModes();
    }
}
