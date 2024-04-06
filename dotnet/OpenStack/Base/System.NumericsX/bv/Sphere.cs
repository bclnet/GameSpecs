using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.NumericsX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Sphere
    {
        static readonly Sphere zero = new(Vector3.origin, 0f);

        Vector3 origin;
        float radius;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sphere(in Vector3 point)
        {
            origin = point;
            radius = 0f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sphere(in Vector3 point, float r)
        {
            origin = point;
            radius = r;
        }

        public unsafe float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { fixed (float* p = &origin.x) return p[index]; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Sphere operator +(in Sphere _, in Vector3 t)              // returns tranlated sphere
            => new(_.origin + t, _.radius);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Sphere a)                          // exact compare, no epsilon
            => origin.Compare(a.origin) && radius == a.radius;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Sphere a, float epsilon)    // compare with epsilon
            => origin.Compare(a.origin, epsilon) && MathX.Fabs(radius - a.radius) <= epsilon;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Sphere _, in Sphere a)                      // exact compare, no epsilon
            => _.Compare(a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Sphere _, in Sphere a)                      // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Sphere q && Compare(q);
        public override int GetHashCode()
            => origin.GetHashCode() ^ radius.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()                                   // inside out sphere
        {
            origin.Zero();
            radius = -1f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()                                    // single point at origin
        {
            origin.Zero();
            radius = 0f;
        }

        // origin of sphere
        public Vector3 Origin
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => origin;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => origin = value;
        }

        // sphere radius
        public float Radius
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => radius;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => radius = value;
        }

        public bool IsCleared                       // returns true if sphere is inside out
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => radius < 0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddPoint(in Vector3 p)                    // add the point, returns true if the sphere expanded
        {
            if (radius < 0f) { origin = p; radius = 0f; return true; }
            else
            {
                var r = (p - origin).LengthSqr;
                if (r > radius * radius)
                {
                    r = MathX.Sqrt(r);
                    origin += (p - origin) * 0.5f * (1f - radius / r);
                    radius += 0.5f * (r - radius);
                    return true;
                }
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AddSphere(in Sphere s)                    // add the sphere, returns true if the sphere expanded
        {
            if (radius < 0f) { origin = s.origin; radius = s.radius; return true; }
            else
            {
                var r = (s.origin - origin).LengthSqr;
                if (r > (radius + s.radius) * (radius + s.radius))
                {
                    r = MathX.Sqrt(r);
                    origin += (s.origin - origin) * 0.5f * (1f - radius / (r + s.radius));
                    radius += 0.5f * ((r + s.radius) - radius);
                    return true;
                }
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sphere Expand(float d)                    // return bounds expanded in all directions with the given value
            => new(origin, radius + d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sphere ExpandSelf(float d)                 // expand bounds in all directions with the given value
        {
            radius += d;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sphere Translate(in Vector3 translation)
            => new(origin + translation, radius);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sphere TranslateSelf(in Vector3 translation)
        {
            origin += translation;
            return this;
        }

        public float PlaneDistance(in Plane plane)
        {
            var d = plane.Distance(origin);
            if (d > radius) return d - radius;
            if (d < -radius) return d + radius;
            return 0f;
        }

        public PLANESIDE PlaneSide(in Plane plane, float epsilon = Plane.ON_EPSILON)
        {
            var d = plane.Distance(origin);
            if (d > radius + epsilon) return PLANESIDE.FRONT;
            if (d < -radius - epsilon) return PLANESIDE.BACK;
            return PLANESIDE.CROSS;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsPoint(in Vector3 p)           // includes touching
            => (p - origin).LengthSqr <= radius * radius;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IntersectsSphere(in Sphere s) // includes touching
        {
            var r = s.radius + radius;
            return (s.origin - origin).LengthSqr <= r * r;
        }

        // Returns true if the line intersects the sphere between the start and end point.
        public bool LineIntersection(in Vector3 start, in Vector3 end)
        {
            var s = start - origin;
            var e = end - origin;
            var r = e - s;
            var a = -s * r;
            if (a <= 0) return s * s < radius * radius;
            else if (a >= r * r) return e * e < radius * radius;
            else { r = s + (a / (r * r)) * r; return r * r < radius * radius; }
        }

        // Returns true if the ray intersects the sphere.
        // The ray can intersect the sphere in both directions from the start point.
        // If start is inside the sphere then scale1< 0 and scale2> 0.
        // intersection points are (start + dir * scale1) and (start + dir * scale2)
        public bool RayIntersection(in Vector3 start, in Vector3 dir, out float scale1, out float scale2)
        {
            var p = start - origin;
            var a = dir * dir; //: double
            var b = dir * p; //: double
            var c = p * p - radius * radius; //: double
            var d = b * b - c * a; //: double

            if (d < 0f) { scale1 = scale2 = default; return false; }

            var sqrtd = MathX.Sqrt(d); //: double
            a = 1f / a;

            scale1 = (-b + sqrtd) * a;
            scale2 = (-b - sqrtd) * a;

            return true;
        }

        // Tight sphere for a point set.
        public unsafe void FromPoints(Vector3[] points, int numPoints)
        {
            Vector3 mins, maxs;
            fixed (Vector3* pointsF = points) Simd.MinMax3(out mins, out maxs, pointsF, numPoints);

            var origin = (mins + maxs) * 0.5f;

            var radiusSqr = 0f;
            for (var i = 0; i < numPoints; i++)
            {
                var dist = (points[i] - origin).LengthSqr;
                if (dist > radiusSqr) radiusSqr = dist;
            }
            radius = MathX.Sqrt(radiusSqr);
        }

        // Most tight sphere for a translation.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromPointTranslation(in Vector3 point, in Vector3 translation)
        {
            origin = point + 0.5f * translation;
            radius = MathX.Sqrt(0.5f * translation.LengthSqr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromSphereTranslation(in Sphere sphere, in Vector3 start, in Vector3 translation)
        {
            origin = start + sphere.origin + 0.5f * translation;
            radius = MathX.Sqrt(0.5f * translation.LengthSqr) + sphere.radius;
        }

        // Most tight sphere for a rotation.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromPointRotation(in Vector3 point, in Rotation rotation)
        {
            var end = rotation * point;
            origin = (point + end) * 0.5f;
            radius = MathX.Sqrt(0.5f * (end - point).LengthSqr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FromSphereRotation(in Sphere sphere, in Vector3 start, in Rotation rotation)
        {
            var end = rotation * sphere.origin;
            origin = start + (sphere.origin + end) * 0.5f;
            radius = MathX.Sqrt(0.5f * (end - sphere.origin).LengthSqr) + sphere.radius;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AxisProjection(in Vector3 dir, out float min, out float max)
        {
            var d = dir * origin;
            min = d - radius;
            max = d + radius;
        }
    }
}