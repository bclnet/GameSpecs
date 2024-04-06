using System.IO;
using System.Numerics;

namespace GameX.WbB.Formats.Entity
{
    /// <summary>
    /// Frame consists of a Vector3 Origin and a Quaternion Orientation
    /// </summary>
    public class Frame
    {
        public Vector3 Origin { get; private set; }
        public Quaternion Orientation { get; private set; }

        public Frame()
        {
            Origin = Vector3.Zero;
            Orientation = Quaternion.Identity;
        }
        //public Frame(EPosition position) : this(position.Pos, position.Rotation) { }
        public Frame(Vector3 origin, Quaternion orientation)
        {
            Origin = origin;
            Orientation = new Quaternion(orientation.X, orientation.Y, orientation.Z, orientation.W);
        }
        public Frame(BinaryReader r)
        {
            Origin = r.ReadVector3();
            Orientation = r.ReadQuaternionWFirst();
        }

        //: Entity.Frame
        public override string ToString() => $"{Origin} - {Orientation}";
    }
}
