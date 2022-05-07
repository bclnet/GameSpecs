using System.Numerics;

namespace GameSpec.Valve.Formats.Blocks.Animation
{
    public class ModelFrameBone
    {
        public Vector3 Position { get; set; }
        public Quaternion Angle { get; set; }

        public ModelFrameBone(Vector3 pos, Quaternion a)
        {
            Position = pos;
            Angle = a;
        }
    }
}
