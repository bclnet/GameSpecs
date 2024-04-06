using System;
using System.Numerics;
using ACE.Server.Physics.Common;
using ACE.Server.Physics.Extensions;

namespace OpenStack.Physics.Animation
{
    public class AFrame: IEquatable<AFrame>
    {
        public Vector3 Origin;
        public Quaternion Orientation;

        public AFrame()
        {
            Origin = Vector3.Zero;
            Orientation = Quaternion.Identity;
        }
        public AFrame(Vector3 origin, Quaternion orientation)
        {
            Origin = origin;
            Orientation = orientation;
        }
        public AFrame(AFrame frame)
        {
            Origin = frame.Origin;
            Orientation = new Quaternion(frame.Orientation.X, frame.Orientation.Y, frame.Orientation.Z, frame.Orientation.W);
        }
        public AFrame(DatLoader.Entity.Frame frame)
        {
            Origin = frame.Origin;
            Orientation = new Quaternion(frame.Orientation.X, frame.Orientation.Y, frame.Orientation.Z, frame.Orientation.W);
        }
        public AFrame(ACE.Entity.Frame frame)
        {
            Origin = frame.Origin;
            Orientation = new Quaternion(frame.Orientation.X, frame.Orientation.Y, frame.Orientation.Z, frame.Orientation.W);
        }

        public static AFrame Combine(AFrame a, AFrame b)
            => new AFrame
            {
                Origin = a.Origin + Vector3.Transform(b.Origin, a.Orientation),
                Orientation = Quaternion.Multiply(a.Orientation, b.Orientation)
            };
        public void Combine(AFrame a, AFrame b, Vector3 scale)
        {
            Origin = a.Origin + Vector3.Transform(b.Origin * scale, a.Orientation);
            Orientation = Quaternion.Multiply(a.Orientation, b.Orientation);
        }

        public Vector3 GlobalToLocal(Vector3 point)
        {
            var offset = point - Origin;
            var rotate = GlobalToLocalVec(offset); 
            return rotate;
        }

        public Vector3 GlobalToLocalVec(Vector3 point)
            => Vector3.Transform(point, Matrix4x4.Transpose(Matrix4x4.CreateFromQuaternion(Orientation)));

        public void InterpolateOrigin(AFrame from, AFrame to, float t)
            => Origin = Vector3.Lerp(from.Origin, to.Origin, t);

        public void InterpolateRotation(AFrame from, AFrame to, float t)
            => Orientation = Quaternion.Lerp(from.Orientation, to.Orientation, t);

        public bool IsEqual(AFrame frame) // implement IEquatable
            => frame.Equals(this);  

        public bool IsQuaternionEqual(AFrame frame)
            => Orientation.Equals(frame.Orientation);

        public bool IsValid()
            => Origin.IsValid() && Orientation.IsValid();

        public bool IsValidExceptForHeading()
            => Origin.IsValid();

        public Vector3 LocalToGlobal(Vector3 point)
            => Origin + LocalToGlobalVec(point);

        public Vector3 LocalToGlobalVec(Vector3 point)
            => Vector3.Transform(point, Orientation);

        public void GRotate(Vector3 rotation)
        {
            Orientation *= Quaternion.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z);
            Orientation  = Quaternion.Normalize(Orientation);
        }

        public void Rotate(Vector3 rotation)
        {
            var angles = Vector3.Transform(rotation, Orientation);
            GRotate(angles);
        }
        public void Rotate(Quaternion rotation)
        {
            Orientation = Quaternion.Multiply(rotation, Orientation);
            Orientation = Quaternion.Normalize(Orientation);
        }

        public void Subtract(AFrame frame)
        {
            Origin -= Vector3.Transform(frame.Origin, frame.Orientation);
            //Orientation *= Quaternion.Conjugate(frame.Orientation);
            Orientation *= Quaternion.Inverse(frame.Orientation);
        }

        public bool close_rotation(AFrame a, AFrame b)
        {
            Quaternion ao = a.Orientation, bo = b.Orientation;
            return Math.Abs(ao.X - bo.X) < PhysicsGlobals.EPSILON &&
                   Math.Abs(ao.Y - bo.Y) < PhysicsGlobals.EPSILON &&
                   Math.Abs(ao.Z - bo.Z) < PhysicsGlobals.EPSILON &&
                   Math.Abs(ao.W - bo.W) < PhysicsGlobals.EPSILON;
        }

        public float get_heading()
        {
            var matrix = Matrix4x4.CreateFromQuaternion(Orientation);
            var heading = (float)Math.Atan2(matrix.M22, matrix.M21);
            return (450.0f - heading.ToDegrees()) % 360.0f;
        }

        public Vector3 get_vector_heading()
        {
            var matrix = Matrix4x4.CreateFromQuaternion(Orientation);
            return new Vector3
            {
                X = matrix.M21,
                Y = matrix.M22,
                Z = matrix.M23
            };
        }

        public static Quaternion get_rotate_offset(Vector3 offset)
        {
            var rotate = Quaternion.CreateFromYawPitchRoll(offset.X, offset.Y, offset.Z);
            rotate = Quaternion.Normalize(rotate);
            return rotate;
        }

        public void rotate_around_axis_to_vector(int axis, Vector3 dir)
        {
            // will implement when actually needed...
        }

        public void set_heading(float degrees)
        {
            //Console.WriteLine($"set_heading({degrees})");

            var rads = degrees.ToRadians();

            var matrix = Matrix4x4.CreateFromQuaternion(Orientation);
            var heading = new Vector3((float)Math.Sin(rads), (float)Math.Cos(rads), matrix.M23 + matrix.M13);
            set_vector_heading(heading);

            var newHeading = get_heading();
            //Console.WriteLine("new_heading: " + newHeading);
        }

        public void set_position(AFrame frame)
        {
            var offset = frame.Origin - Origin;
            Origin += Vector3.Transform(offset, Orientation);
        }

        public void set_rotate(Quaternion orientation)
            => Orientation = Quaternion.Normalize(orientation);

        public void set_vector_heading(Vector3 heading)
        {
            var normal = heading;
            if (Vec.NormalizeCheckSmall(ref normal)) return;

            var zDeg = 450.0f - ((float)Math.Atan2(normal.Y, normal.X)).ToDegrees();
            var zRot = -(zDeg % 360.0f).ToRadians();

            var xRot = (float)Math.Asin(normal.Z);

            var rotate = Quaternion.CreateFromYawPitchRoll(xRot, 0, zRot);
            set_rotate(rotate);
        }

        public override string ToString()
        {
            return $"[{Origin.X} {Origin.Y} {Origin.Z}] {Orientation.W} {Orientation.X} {Orientation.Y} {Orientation.Z}";
        }

        public bool Equals(AFrame frame)
        {
            var originEpsilonEqual = Math.Abs(frame.Origin.X - Origin.X) <= PhysicsGlobals.EPSILON &&
                Math.Abs(frame.Origin.Y - Origin.Y) <= PhysicsGlobals.EPSILON &&
                Math.Abs(frame.Origin.Z - Origin.Z) <= PhysicsGlobals.EPSILON;

            if (!originEpsilonEqual) return false;

            var orientationEpsilonEqual = Math.Abs(frame.Orientation.X - frame.Orientation.X) <= PhysicsGlobals.EPSILON &&
                Math.Abs(frame.Orientation.Y - frame.Orientation.Y) <= PhysicsGlobals.EPSILON &&
                Math.Abs(frame.Orientation.Z - frame.Orientation.Z) <= PhysicsGlobals.EPSILON &&
                Math.Abs(frame.Orientation.W - frame.Orientation.W) <= PhysicsGlobals.EPSILON;

            return orientationEpsilonEqual;
        }
    }
}
