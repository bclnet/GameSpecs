using System.IO;
using System.Numerics;

namespace GameX.WbB.Formats.Entity
{
    public class Sphere
    {
        public static readonly Sphere Empty = new Sphere();
        public Vector3 Origin;
        public float Radius;

        //: Base + Entity+Sphere
        public Sphere() { Origin = Vector3.Zero; }
        public Sphere(BinaryReader r)
        {
            Origin = r.ReadVector3();
            Radius = r.ReadSingle();
        }

        //: Entity.Sphere
        public override string ToString() => $"Origin: {Origin}, Radius: {Radius}";
    }
}
