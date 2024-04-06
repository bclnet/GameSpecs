using System.Numerics;

namespace OpenStack.Graphics.Renderer1.Animations
{
    public struct FrameBone
    {
        public Vector3 Position { get; set; }
        public Quaternion Angle { get; set; }
        public float Scale { get; set; }
    }
}
