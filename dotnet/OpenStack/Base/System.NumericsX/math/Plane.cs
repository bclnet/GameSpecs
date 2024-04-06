using System.Runtime.CompilerServices;
using static System.NumericsX.Platform;

namespace System.NumericsX
{
    // plane sides
    public enum PLANESIDE : int
    {
        FRONT = 0,
        BACK = 1,
        ON = 2,
        CROSS = 3,
    }

    // plane types
    public enum PLANETYPE : int
    {
        X = 0,
        Y = 1,
        Z = 2,
        NEGX = 3,
        NEGY = 4,
        NEGZ = 5,
        TRUEAXIAL = 6, // all types < 6 are true axial planes
        ZEROX = 6,
        ZEROY = 7,
        ZEROZ = 8,
        NONAXIAL = 9,
    }

    public unsafe struct Plane
    {
        public const int ALLOC16 = 1;
        public static Plane origin = new(0f, 0f, 0f, 0f);

        public const float ON_EPSILON = 0.1f;
        public const float DEGENERATE_DIST_EPSILON = 1e-4f;

        public float a;
        public float b;
        public float c;
        public float d;

        public const int SIDE_FRONT = 0;
        public const int SIDE_BACK = 1;
        public const int SIDE_ON = 2;
        public const int SIDE_CROSS = 3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane(float a, float b, float c, float d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane(in Vector3 normal, float dist)
        {
            this.a = normal.x;
            this.b = normal.y;
            this.c = normal.z;
            this.d = -dist;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Plane(in Vector3 v) // sets normal and sets Plane::d to zero
            => new()
            {
                a = v.x,
                b = v.y,
                c = v.z,
                d = 0,
            };

        public float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { fixed (float* p = &a) return p[index]; }
            set { fixed (float* p = &a) p[index] = value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane operator -(in Plane _)                     // flips plane
            => new(-_.a, -_.b, -_.c, -_.d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane operator +(in Plane _, in Plane p)   // add plane equations
            => new(_.a + p.a, _.b + p.b, _.c + p.c, _.d + p.d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane operator -(in Plane _, in Plane p)   // subtract plane equations
            => new(_.a - p.a, _.b - p.b, _.c - p.c, _.d - p.d);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane operator *(Plane _, Matrix3x3 m)       // Normal *= m
        {
            _.Normal *= m;
            return _;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Plane p)                      // exact compare, no epsilon
            => a == p.a && b == p.b && c == p.c && d == p.d;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Plane p, float epsilon) // compare with epsilon
            => MathX.Fabs(a - p.a) <= epsilon &&
               MathX.Fabs(b - p.b) <= epsilon &&
               MathX.Fabs(c - p.c) <= epsilon &&
               MathX.Fabs(d - p.d) <= epsilon;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Plane p, float normalEps, float distEps)  // compare with epsilon
            => MathX.Fabs(d - p.d) <= distEps &&
               Normal.Compare(p.Normal, normalEps);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Plane _, in Plane p)                   // exact compare, no epsilon
            => _.Compare(p);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Plane _, in Plane p)                   // exact compare, no epsilon
            => !_.Compare(p);
        public override bool Equals(object obj)
            => obj is Plane q && Compare(q);
        public override int GetHashCode()
            => a.GetHashCode() ^ b.GetHashCode() ^ c.GetHashCode() ^ d.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()                         // zero plane
            => a = b = c = d = 0f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetNormal(in Vector3 normal)      // sets the normal
        {
            a = normal.x;
            b = normal.y;
            c = normal.z;
        }

        public ref Vector3 Normal
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref reinterpret.cast_vec3(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Normalize(bool fixDegenerate = true)  // only normalizes the plane normal, does not adjust d
        {
            var length = reinterpret.cast_vec3(this).Normalize();
            if (fixDegenerate) FixDegenerateNormal();
            return length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FixDegenerateNormal()          // fix degenerate normal
            => Normal.FixDegenerateNormal();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FixDegeneracies(float distEpsilon) // fix degenerate normal and dist
        {
            var fixedNormal = FixDegenerateNormal();
            // only fix dist if the normal was degenerate
            if (fixedNormal && MathX.Fabs(d - MathX.Rint(d)) < distEpsilon) d = MathX.Rint(d);
            return fixedNormal;
        }

        public float Dist
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => -d; // returns: -d
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => d = -value; // sets: d = -dist
        }

        public PLANETYPE Type                      // returns plane type
        {
            get
            {
                if (Normal[0] == 0f)
                {
                    if (Normal[1] == 0f) return Normal[2] > 0f ? PLANETYPE.Z : PLANETYPE.NEGZ;
                    else if (Normal[2] == 0f) return Normal[1] > 0f ? PLANETYPE.Y : PLANETYPE.NEGY;
                    else return PLANETYPE.ZEROX;
                }
                else if (Normal[1] == 0f)
                {
                    if (Normal[2] == 0f) return Normal[0] > 0f ? PLANETYPE.X : PLANETYPE.NEGX;
                    else return PLANETYPE.ZEROY;
                }
                else if (Normal[2] == 0f) return PLANETYPE.ZEROZ;
                else return PLANETYPE.NONAXIAL;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FromPoints(in Vector3 p1, in Vector3 p2, in Vector3 p3, bool fixDegenerate = true)
        {
            Normal = (p1 - p2).Cross(p3 - p2);
            if (Normalize(fixDegenerate) == 0f) return false;
            d = -(Normal * p2);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FromVecs(in Vector3 dir1, in Vector3 dir2, in Vector3 p, bool fixDegenerate = true)
        {
            Normal = dir1.Cross(dir2);
            if (Normalize(fixDegenerate) == 0f) return false;
            d = -(Normal * p);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FitThroughPoint(in Vector3 p) // assumes normal is valid
             => d = -(Normal * p);

        public bool HeightFit(Vector3[] points, int numPoints)
        {
            int i;
            float sumXX = 0f, sumXY = 0f, sumXZ = 0f;
            float sumYY = 0f, sumYZ = 0f;
            Vector3 sum = new(), average, dir;

            if (numPoints == 1)
            {
                a = 0f;
                b = 0f;
                c = 1f;
                d = -points[0].z;
                return true;
            }
            if (numPoints == 2)
            {
                dir = points[1] - points[0];
                Normal = dir.Cross(new Vector3(0, 0, 1)).Cross(dir);
                Normalize();
                d = -(Normal * points[0]);
                return true;
            }

            sum.Zero();
            for (i = 0; i < numPoints; i++) sum += points[i];
            average = sum / numPoints;

            for (i = 0; i < numPoints; i++)
            {
                dir = points[i] - average;
                sumXX += dir.x * dir.x;
                sumXY += dir.x * dir.y;
                sumXZ += dir.x * dir.z;
                sumYY += dir.y * dir.y;
                sumYZ += dir.y * dir.z;
            }

            Matrix2x2 m = new(sumXX, sumXY, sumXY, sumYY);
            if (!m.InverseSelf()) return false;

            a = -sumXZ * m.mat0.x - sumYZ * m.mat0.y;
            b = -sumXZ * m.mat1.x - sumYZ * m.mat1.y;
            c = 1f;
            Normalize();
            d = -(a * average.x + b * average.y + c * average.z);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane Translate(in Vector3 translation)
            => new(a, b, c, d - translation * Normal);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane TranslateSelf(in Vector3 translation)
        {
            d -= translation * Normal;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane Rotate(in Vector3 origin, in Matrix3x3 axis)
        {
            Plane p = new();
            p.Normal = Normal * axis;
            p.d = d + origin * Normal - origin * p.Normal;
            return p;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Plane RotateSelf(in Vector3 origin, in Matrix3x3 axis)
        {
            d += origin * Normal;
            Normal *= axis;
            d -= origin * Normal;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Distance(in Vector3 v)
            => a * v.x + b * v.y + c * v.z + d;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PLANESIDE Side(in Vector3 v, float epsilon = 0f)
        {
            var dist = Distance(v);
            if (dist > epsilon) return PLANESIDE.FRONT;
            else if (dist < -epsilon) return PLANESIDE.BACK;
            else return PLANESIDE.ON;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LineIntersection(in Vector3 start, in Vector3 end)
        {
            float d1, d2, fraction;

            d1 = Normal * start + d;
            d2 = Normal * end + d;
            if (d1 == d2) return false;
            if (d1 > 0f && d2 > 0f) return false;
            if (d1 < 0f && d2 < 0f) return false;
            fraction = d1 / (d1 - d2);
            return fraction >= 0f && fraction <= 1f;
        }

        // intersection point is start + dir * scale
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RayIntersection(in Vector3 start, in Vector3 dir, out float scale)
        {
            float d1, d2;

            d1 = Normal * start + d;
            d2 = Normal * dir;
            if (d2 == 0f) { scale = 0; return false; }
            scale = -(d1 / d2);
            return true;
        }

        public bool PlaneIntersection(in Plane plane, out Vector3 start, out Vector3 dir)
        {
            double n00, n01, n11, det, invDet, f0, f1;

            n00 = Normal.LengthSqr;
            n01 = Normal * plane.Normal;
            n11 = plane.Normal.LengthSqr;
            det = n00 * n11 - n01 * n01;

            if (MathX.Fabs(det) < 1e-6f) { start = dir = default; return false; }

            invDet = 1f / det;
            f0 = (n01 * plane.d - n11 * d) * invDet;
            f1 = (n01 * d - n00 * plane.d) * invDet;

            dir = Normal.Cross(plane.Normal);
            start = (float)f0 * Normal + (float)f1 * plane.Normal;
            return true;
        }

        public const int Dimension = 4;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Vector4 ToVec4()
            => ref reinterpret.cast_vec4(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Fixed<T>(FloatPtr<T> callback)
        {
            fixed (float* _ = &a) return callback(_);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fixed(FloatPtr callback)
        {
            fixed (float* _ = &a) callback(_);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(int precision = 2)
        {
            var dimension = Dimension;
            fixed (float* _ = &a) return FloatArrayToString(_, dimension, precision);
        }
    }
}