using System.IO;
using System.Numerics;

namespace GameX.WbB.Formats.Entity
{
    public class Plane
    {
        public Vector3 N;
        public float D;

        //: Entity+Plane
        public Plane() { }
        public Plane(BinaryReader r)
        {
            N = r.ReadVector3();
            D = r.ReadSingle();
        }

        public System.Numerics.Plane ToNumerics() => new System.Numerics.Plane(N, D);

        //: Entity.Plane
        public override string ToString() => $"Normal: {N} - Distance: {D}";
    }
}
