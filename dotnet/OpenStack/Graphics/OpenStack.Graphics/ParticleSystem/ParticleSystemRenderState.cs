using System.Collections.Generic;
using System.Numerics;

namespace OpenStack.Graphics.ParticleSystem
{
    public class ParticleSystemRenderState
    {
        public float Lifetime { get; set; } = 0f;

        readonly Dictionary<int, Vector3> _controlPoints = new Dictionary<int, Vector3>();

        public Vector3 GetControlPoint(int cp)
            => _controlPoints.TryGetValue(cp, out var value)
            ? value
            : Vector3.Zero;

        public ParticleSystemRenderState SetControlPoint(int cp, Vector3 value)
        {
            _controlPoints[cp] = value;
            return this;
        }
    }
}
