//#define VECX_SIMD
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.NumericsX.Platform;

namespace System.NumericsX
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Vector2
    {
        public const int ALLOC16 = 2;
        public static Vector2 origin = new(0f, 0f);

        public float x;
        public float y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()
            => x = y = 0f;

        public float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { fixed (float* _ = &x) return _[index]; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { fixed (float* _ = &x) _[index] = value; }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator -(in Vector2 _)
            => new(-_.x, -_.y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator *(in Vector2 _, in Vector2 a)
            => _.x * a.x + _.y * a.y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(in Vector2 _, float a)
            => new(_.x * a, _.y * a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator /(in Vector2 _, float a)
        { var inva = 1f / a; return new(_.x * inva, _.y * inva); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator +(in Vector2 _, in Vector2 a)
            => new(_.x + a.x, _.y + a.y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator -(in Vector2 _, in Vector2 a)
            => new(_.x - a.x, _.y - a.y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(float a, in Vector2 b)
            => new(b.x * a, b.y * a);

        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Vector2 a)
            => x == a.x && y == a.y;
        /// <summary>
        /// compare with epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Vector2 a, float epsilon)
            => MathX.Fabs(x - a.x) <= epsilon &&
               MathX.Fabs(y - a.y) <= epsilon;
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Vector2 _, in Vector2 a)
            => _.Compare(a);
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Vector2 _, in Vector2 a)
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Vector2 q && Compare(q);
        public override int GetHashCode()
            => x.GetHashCode() ^ y.GetHashCode();

        public float Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (float)MathX.Sqrt(x * x + y * y);
        }
        public float LengthFast
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var sqrLength = x * x + y * y;
                return sqrLength * MathX.RSqrt(sqrLength);
            }
        }
        public float LengthSqr
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => x * x + y * y;
        }

        /// <summary>
        /// returns length
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Normalize()
        {
            var sqrLength = x * x + y * y;
            var invLength = MathX.InvSqrt(sqrLength);
            x *= invLength;
            y *= invLength;
            return invLength * sqrLength;
        }
        /// <summary>
        /// returns length
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NormalizeFast()
        {
            var lengthSqr = x * x + y * y;
            var invLength = MathX.RSqrt(lengthSqr);
            x *= invLength;
            y *= invLength;
            return invLength * lengthSqr;
        }

        /// <summary>
        /// cap length
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 Truncate(float length)
        {
            if (length == 0) Zero();
            else
            {
                var length2 = LengthSqr;
                if (length2 > length * length)
                {
                    var ilength = length * MathX.InvSqrt(length2);
                    x *= ilength;
                    y *= ilength;
                }
            }
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clamp(in Vector2 min, in Vector2 max)
        {
            if (x < min.x) x = min.x;
            else if (x > max.x) x = max.x;
            if (y < min.y) y = min.y;
            else if (y > max.y) y = max.y;
        }

        /// <summary>
        /// snap to closest integer value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Snap()
        {
            x = (float)Math.Floor(x + 0.5f);
            y = (float)Math.Floor(y + 0.5f);
        }

        /// <summary>
        /// snap towards integer (floor)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SnapInt()
        {
            x = (int)x;
            y = (int)y;
        }

        public const int Dimension = 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Fixed<T>(FloatPtr<T> callback)
        {
            fixed (float* _ = &x) return callback(_);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fixed(FloatPtr callback)
        {
            fixed (float* _ = &x) callback(_);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(int precision = 2)
        {
            fixed (float* _ = &x) return FloatArrayToString(_, Dimension, precision);
        }

        /// <summary>
        /// Linearly inperpolates one vector to another.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="l">The l.</param>
        /// <returns></returns>
        public void Lerp(in Vector2 v1, in Vector2 v2, float l)
        {
            if (l <= 0f) this = v1;
            else if (l >= 1f) this = v2;
            else this = v1 + l * (v2 - v1);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Vector3
    {
        public const int ALLOC16 = 2;
        public static Vector3 origin = new(0f, 0f, 0f);

        public float x;
        public float y;
        public float z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3(in Vector3 a)
            => this = a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3(float xyz)
            => x = y = z = xyz;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()
            => x = y = z = 0f;

        public float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { fixed (float* _ = &x) return _[index]; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { fixed (float* _ = &x) _[index] = value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(in Vector3 _)
            => new(-_.x, -_.y, -_.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator *(in Vector3 _, in Vector3 a)
            => _.x * a.x + _.y * a.y + _.z * a.z;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(in Vector3 _, float a)
            => new(_.x * a, _.y * a, _.z * a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(in Vector3 _, float a)
        {
            var inva = 1f / a;
            return new Vector3(_.x * inva, _.y * inva, _.z * inva);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator +(in Vector3 _, in Vector3 a)
            => new(_.x + a.x, _.y + a.y, _.z + a.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(in Vector3 _, in Vector3 a)
            => new(_.x - a.x, _.y - a.y, _.z - a.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(float a, in Vector3 b)
            => new(b.x * a, b.y * a, b.z * a);

        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Vector3 a)
            => x == a.x && y == a.y && z == a.z;
        /// <summary>
        /// compare with epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Vector3 a, float epsilon)
            => MathX.Fabs(x - a.x) <= epsilon &&
               MathX.Fabs(y - a.y) <= epsilon &&
               MathX.Fabs(z - a.z) <= epsilon;
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Vector3 _, in Vector3 a)
            => _.Compare(a);
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Vector3 _, in Vector3 a)
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Vector3 q && Compare(q);
        public override int GetHashCode()
            => x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();

        /// <summary>
        /// fix degenerate axial cases
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FixDegenerateNormal()
        {
            if (x == 0f)
            {
                if (y == 0f)
                {
                    if (z > 0f) { if (z != 1f) { z = 1f; return true; } }
                    else { if (z != -1f) { z = -1f; return true; } }
                    return false;
                }
                else if (z == 0f)
                {
                    if (y > 0f) { if (y != 1f) { y = 1f; return true; } }
                    else { if (y != -1f) { y = -1f; return true; } }
                    return false;
                }
            }
            else if (y == 0f)
            {
                if (z == 0f)
                {
                    if (x > 0f) { if (x != 1f) { x = 1f; return true; } }
                    else { if (x != -1f) { x = -1f; return true; } }
                    return false;
                }
            }
            if (MathX.Fabs(x) == 1f)
            {
                if (y != 0f || z != 0f) { y = z = 0f; return true; }
                return false;
            }
            else if (MathX.Fabs(y) == 1f)
            {
                if (x != 0f || z != 0f) { x = z = 0f; return true; }
                return false;
            }
            else if (MathX.Fabs(z) == 1f)
            {
                if (x != 0f || y != 0f) { x = y = 0f; return true; }
                return false;
            }
            return false;
        }

        /// <summary>
        /// change tiny numbers to zero
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FixDenormals()
        {
            var denormal = false;
            if (Math.Abs(x) < 1e-30f) { x = 0f; denormal = true; }
            if (Math.Abs(y) < 1e-30f) { y = 0f; denormal = true; }
            if (Math.Abs(z) < 1e-30f) { z = 0f; denormal = true; }
            return denormal;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 Cross(in Vector3 a)
            => new(y * a.z - z * a.y, z * a.x - x * a.z, x * a.y - y * a.x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 Cross(in Vector3 a, in Vector3 b)
        {
            x = a.y * b.z - a.z * b.y;
            y = a.z * b.x - a.x * b.z;
            z = a.x * b.y - a.y * b.x;
            return this;
        }

        public float Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (float)MathX.Sqrt(x * x + y * y + z * z);
        }
        public float LengthSqr
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => x * x + y * y + z * z;
        }
        public float LengthFast
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var sqrLength = x * x + y * y + z * z;
                return sqrLength * MathX.RSqrt(sqrLength);
            }
        }

        /// <summary>
        /// Normalizes this instance.
        /// </summary>
        /// <returns>length</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Normalize()
        {
            var sqrLength = x * x + y * y + z * z;
            var invLength = MathX.InvSqrt(sqrLength);
            x *= invLength;
            y *= invLength;
            z *= invLength;
            return invLength * sqrLength;
        }
        /// <summary>
        /// Normalizes the fast.
        /// </summary>
        /// <returns>length</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NormalizeFast()
        {
            var sqrLength = x * x + y * y + z * z;
            var invLength = MathX.RSqrt(sqrLength);
            x *= invLength;
            y *= invLength;
            z *= invLength;
            return invLength * sqrLength;
        }

        /// <summary>
        /// cap length
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 Truncate(float length)
        {
            if (length == 0) Zero();
            else
            {
                var length2 = LengthSqr;
                if (length2 > length * length)
                {
                    var ilength = length * MathX.InvSqrt(length2);
                    x *= ilength;
                    y *= ilength;
                    z *= ilength;
                }
            }
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clamp(in Vector3 min, in Vector3 max)
        {
            if (x < min.x) x = min.x;
            else if (x > max.x) x = max.x;
            if (y < min.y) y = min.y;
            else if (y > max.y) y = max.y;
            if (z < min.z) z = min.z;
            else if (z > max.z) z = max.z;
        }

        /// <summary>
        /// snap to closest integer value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Snap()
        {
            x = (float)Math.Floor(x + 0.5f);
            y = (float)Math.Floor(y + 0.5f);
            z = (float)Math.Floor(z + 0.5f);
        }
        /// <summary>
        /// snap towards integer (floor)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SnapInt()
        {
            x = (int)x;
            y = (int)y;
            z = (int)z;
        }

        public const int Dimension = 3;

        public float ToYaw()
        {
            float yaw;
            if (y == 0f && x == 0f) yaw = 0f;
            else
            {
                yaw = (float)MathX.RAD2DEG(Math.Atan2(y, x));
                if (yaw < 0f) yaw += 360f;
            }
            return yaw;
        }

        public float ToPitch()
        {
            float forward, pitch;
            if (x == 0f && y == 0f) pitch = z > 0f ? 90f : 270f;
            else
            {
                forward = (float)MathX.Sqrt(x * x + y * y);
                pitch = (float)MathX.RAD2DEG(Math.Atan2(z, forward));
                if (pitch < 0f) pitch += 360f;
            }
            return pitch;
        }

        public Angles ToAngles()
        {
            float forward, yaw, pitch;
            if (x == 0f && y == 0f)
            {
                yaw = 0f;
                pitch = z > 0f ? 90f : 270f;
            }
            else
            {
                yaw = (float)MathX.RAD2DEG(Math.Atan2(y, x));
                if (yaw < 0f) yaw += 360f;

                forward = (float)MathX.Sqrt(x * x + y * y);
                pitch = (float)MathX.RAD2DEG(Math.Atan2(z, forward));
                if (pitch < 0f) pitch += 360f;
            }
            return new Angles(-pitch, yaw, 0f);
        }

        public Polar3 ToPolar()
        {
            float forward, yaw, pitch;
            if (x == 0f && y == 0f)
            {
                yaw = 0f;
                pitch = z > 0f ? 90f : 270f;
            }
            else
            {
                yaw = (float)MathX.RAD2DEG(Math.Atan2(y, x));
                if (yaw < 0f) yaw += 360f;

                forward = (float)MathX.Sqrt(x * x + y * y);
                pitch = (float)MathX.RAD2DEG(Math.Atan2(z, forward));
                if (pitch < 0f) pitch += 360f;
            }
            return new Polar3(MathX.Sqrt(x * x + y * y + z * z), yaw, -pitch);
        }

        /// <summary>
        /// vector should be normalized
        /// </summary>
        /// <returns></returns>
        public Matrix3x3 ToMat3()
        {
            Matrix3x3 mat = default;
            mat[0] = this;
            var d = x * x + y * y;
            if (d == 0)
            {
                mat[1].x = 1f;
                mat[1].y = 0f;
                mat[1].z = 0f;
            }
            else
            {
                d = MathX.InvSqrt(d);
                mat[1].x = -y * d;
                mat[1].y = x * d;
                mat[1].z = 0f;
            }
            mat[2] = Cross(mat[1]);
            return mat;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Vector2 ToVec2()
            => ref reinterpret.cast_vec2(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Fixed<T>(FloatPtr<T> callback)
        {
            fixed (float* _ = &x) return callback(_);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fixed(FloatPtr callback)
        {
            fixed (float* _ = &x) callback(_);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(int precision = 2)
        {
            fixed (float* _ = &x) return FloatArrayToString(_, Dimension, precision);
        }

        /// <summary>
        /// vector should be normalized
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="down">Down.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NormalVectors(out Vector3 left, out Vector3 down)
        {
            var d = x * x + y * y;
            if (d == 0)
            {
                left.x = 1;
                left.y = 0;
                left.z = 0;
            }
            else
            {
                d = MathX.InvSqrt(d);
                left.x = -y * d;
                left.y = x * d;
                left.z = 0;
            }
            down = left.Cross(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OrthogonalBasis(out Vector3 left, out Vector3 up)
        {
            float l, s;
            if (MathX.Fabs(z) > 0.7f)
            {
                l = y * y + z * z;
                s = MathX.InvSqrt(l);
                up.x = 0;
                up.y = z * s;
                up.z = -y * s;
                left.x = l * s;
                left.y = -x * up.z;
                left.z = x * up.y;
            }
            else
            {
                l = x * x + y * y;
                s = MathX.InvSqrt(l);
                left.x = -y * s;
                left.y = x * s;
                left.z = 0;
                up.x = -z * left.y;
                up.y = z * left.x;
                up.z = l * s;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProjectOntoPlane(in Vector3 normal, float overBounce = 1f)
        {
            var backoff = this * normal;
            if (overBounce != 1.0)
            {
                if (backoff < 0) backoff *= overBounce;
                else backoff /= overBounce;
            }
            this -= backoff * normal;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProjectAlongPlane(in Vector3 normal, float epsilon, float overBounce = 1f)
        {
            var cross = Cross(normal).Cross(this);
            // normalize so a fixed epsilon can be used
            cross.Normalize();
            var len = normal * cross;
            if (MathX.Fabs(len) < epsilon) return false;
            cross *= overBounce * (normal * this) / len;
            this -= cross;
            return true;
        }

        /// <summary>
        /// Projects the z component onto a sphere.
        /// </summary>
        /// <param name="radius">The radius.</param>
        /// <returns></returns>
        public void ProjectSelfOntoSphere(float radius)
        {
            var rsqr = radius * radius;
            var len = Length;
            z = len < rsqr * 0.5f
                ? (float)Math.Sqrt(rsqr - len)
                : rsqr / (2f * (float)Math.Sqrt(len));
        }

        /// <summary>
        /// Linearly inperpolates one vector to another.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="l">The l.</param>
        /// <returns></returns>
        public void Lerp(in Vector3 v1, in Vector3 v2, float l)
        {
            if (l <= 0f) this = v1;
            else if (l >= 1f) this = v2;
            else this = v1 + l * (v2 - v1);
        }

        const float LERP_DELTA = 1e-6f;
        /// <summary>
        /// Spherical linear interpolation from v1 to v2.
        /// Vectors are expected to be normalized.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="l">The l.</param>
        /// <returns></returns>
        public void SLerp(in Vector3 v1, in Vector3 v2, float l)
        {
            float omega, cosom, sinom, scale0, scale1;
            if (l <= 0f) { this = v1; return; }
            else if (l >= 1f) { this = v2; return; }

            cosom = v1 * v2;
            if ((1f - cosom) > LERP_DELTA)
            {
                omega = (float)Math.Acos(cosom);
                sinom = (float)Math.Sin(omega);
                scale0 = (float)Math.Sin((1f - l) * omega) / sinom;
                scale1 = (float)Math.Sin(l * omega) / sinom;
            }
            else
            {
                scale0 = 1f - l;
                scale1 = l;
            }

            this = v1 * scale0 + v2 * scale1;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Vector4
    {
        public const int ALLOC16 = 1;
        public static Vector4 origin = new(0f, 0f, 0f, 0f);

        public float x;
        public float y;
        public float z;
        public float w;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()
            => x = y = z = w = 0f;

        public float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { fixed (float* _ = &x) return _[index]; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { fixed (float* _ = &x) _[index] = value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator -(in Vector4 _)
            => new(-_.x, -_.y, -_.z, -_.w);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator *(in Vector4 _, in Vector4 a)
            => _.x * a.x + _.y * a.y + _.z * a.z + _.w * a.w;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(in Vector4 _, float a)
            => new(_.x * a, _.y * a, _.z * a, _.w * a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator /(in Vector4 _, float a)
        {
            var inva = 1f / a;
            return new(_.x * inva, _.y * inva, _.z * inva, _.w * inva);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator +(in Vector4 _, in Vector4 a)
            => new(_.x + a.x, _.y + a.y, _.z + a.z, _.w + a.w);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator -(in Vector4 _, in Vector4 a)
            => new(_.x - a.x, _.y - a.y, _.z - a.z, _.w - a.w);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(float a, in Vector4 b)
            => new(b.x * a, b.y * a, b.z * a, b.w * a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Vector4 a)                          // exact compare, no epsilon
            => x == a.x && y == a.y && z == a.z && w == a.w;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Vector4 a, float epsilon)     // compare with epsilon
            => MathX.Fabs(x - a.x) <= epsilon &&
               MathX.Fabs(y - a.y) <= epsilon &&
               MathX.Fabs(z - a.z) <= epsilon &&
               MathX.Fabs(w - a.w) <= epsilon;
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Vector4 _, in Vector4 a)
            => _.Compare(a);
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Vector4 _, in Vector4 a)
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Vector4 q && Compare(q);
        public override int GetHashCode()
            => x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();

        public float Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (float)MathX.Sqrt(x * x + y * y + z * z + w * w);
        }
        public float LengthSqr
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => x * x + y * y + z * z + w * w;
        }

        /// <summary>
        /// Normalizes this instance.
        /// </summary>
        /// <returns>length</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Normalize()
        {
            var sqrLength = x * x + y * y + z * z + w * w;
            var invLength = MathX.InvSqrt(sqrLength);
            x *= invLength;
            y *= invLength;
            z *= invLength;
            w *= invLength;
            return invLength * sqrLength;
        }
        /// <summary>
        /// Normalizes the fast.
        /// </summary>
        /// <returns>length</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NormalizeFast()
        {
            var sqrLength = x * x + y * y + z * z + w * w;
            var invLength = MathX.RSqrt(sqrLength);
            x *= invLength;
            y *= invLength;
            z *= invLength;
            w *= invLength;
            return invLength * sqrLength;
        }

        public const int Dimension = 4;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Vector2 ToVec2()
            => ref reinterpret.cast_vec2(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Vector3 ToVec3()
            => ref reinterpret.cast_vec3(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Fixed<T>(FloatPtr<T> callback)
        {
            fixed (float* _ = &x) return callback(_);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fixed(FloatPtr callback)
        {
            fixed (float* _ = &x) callback(_);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(int precision = 2)
        {
            fixed (float* _ = &x) return FloatArrayToString(_, Dimension, precision);
        }

        /// <summary>
        /// Linearly inperpolates one vector to another.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="l">The l.</param>
        /// <returns></returns>
        public void Lerp(in Vector4 v1, in Vector4 v2, float l)
        {
            if (l <= 0f) this = v1;
            else if (l >= 1f) this = v2;
            else this = v1 + l * (v2 - v1);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Vector5
    {
        public const int ALLOC16 = 1;
        public static Vector5 origin = new(0f, 0f, 0f, 0f, 0f);

        public float x;
        public float y;
        public float z;
        public float s;
        public float t;

        public Vector5(in Vector3 xyz)
        {
            x = xyz.x;
            y = xyz.y;
            z = xyz.z;
            s = t = 0f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector5(in Vector3 xyz, in Vector2 st)
        {
            x = xyz.x;
            y = xyz.y;
            z = xyz.z;
            s = st.x;
            t = st.y;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector5(float x, float y, float z, float s, float t)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.s = s;
            this.t = t;
        }

        public float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { fixed (float* _ = &x) return _[index]; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { fixed (float* _ = &x) _[index] = value; }
        }

        public const int Dimension = 5;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Vector3 ToVec3()
            => ref reinterpret.cast_vec3(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Fixed<T>(FloatPtr<T> callback)
        {
            fixed (float* _ = &x) return callback(_);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fixed(FloatPtr callback)
        {
            fixed (float* _ = &x) callback(_);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(int precision = 2)
        {
            fixed (float* _ = &x) return FloatArrayToString(_, Dimension, precision);
        }

        public void Lerp(in Vector5 v1, in Vector5 v2, float l)
        {
            if (l <= 0f) this = v1;
            else if (l >= 1f) this = v2;
            else
            {
                x = v1.x + l * (v2.x - v1.x);
                y = v1.y + l * (v2.y - v1.y);
                z = v1.z + l * (v2.z - v1.z);
                s = v1.s + l * (v2.s - v1.s);
                t = v1.t + l * (v2.t - v1.t);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Vector6
    {
        public const int ALLOC16 = 1;
        public static Vector6 origin = new(0f, 0f, 0f, 0f, 0f, 0f);
        public static Vector6 infinity = new(MathX.INFINITY, MathX.INFINITY, MathX.INFINITY, MathX.INFINITY, MathX.INFINITY, MathX.INFINITY);

        internal fixed float p[6];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector6(float[] a)
        {
            fixed (float* _ = p, a_ = a) Unsafe.CopyBlock(_, a_, 6U * sizeof(float));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector6(float a1, float a2, float a3, float a4, float a5, float a6)
        {
            p[0] = a1;
            p[1] = a2;
            p[2] = a3;
            p[3] = a4;
            p[4] = a5;
            p[5] = a6;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float a1, float a2, float a3, float a4, float a5, float a6)
        {
            p[0] = a1;
            p[1] = a2;
            p[2] = a3;
            p[3] = a4;
            p[4] = a5;
            p[5] = a6;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()
            => p[0] = p[1] = p[2] = p[3] = p[4] = p[5] = 0f;

        public float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => p[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => p[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator -(in Vector6 _)
            => new(-_.p[0], -_.p[1], -_.p[2], -_.p[3], -_.p[4], -_.p[5]);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator *(in Vector6 _, float a)
            => new(_.p[0] * a, _.p[1] * a, _.p[2] * a, _.p[3] * a, _.p[4] * a, _.p[5] * a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator /(in Vector6 _, float a)
        {
            Debug.Assert(a != 0f);
            var inva = 1f / a;
            return new Vector6(_.p[0] * inva, _.p[1] * inva, _.p[2] * inva, _.p[3] * inva, _.p[4] * inva, _.p[5] * inva);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator *(in Vector6 _, in Vector6 a)
            => _.p[0] * a[0] + _.p[1] * a[1] + _.p[2] * a[2] + _.p[3] * a[3] + _.p[4] * a[4] + _.p[5] * a[5];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator -(in Vector6 _, in Vector6 a)
            => new(_.p[0] - a[0], _.p[1] - a[1], _.p[2] - a[2], _.p[3] - a[3], _.p[4] - a[4], _.p[5] - a[5]);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator +(in Vector6 _, in Vector6 a)
            => new(_.p[0] + a[0], _.p[1] + a[1], _.p[2] + a[2], _.p[3] + a[3], _.p[4] + a[4], _.p[5] + a[5]);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator *(float a, in Vector6 b)
            => b * a;

        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Vector6 a)
            => p[0] == a[0] && p[1] == a[1] && p[2] == a[2] && p[3] == a[3] && p[4] == a[4] && p[5] == a[5];
        /// <summary>
        /// compare with epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Vector6 a, float epsilon)
            => MathX.Fabs(p[0] - a[0]) <= epsilon &&
               MathX.Fabs(p[1] - a[1]) <= epsilon &&
               MathX.Fabs(p[2] - a[2]) <= epsilon &&
               MathX.Fabs(p[3] - a[3]) <= epsilon &&
               MathX.Fabs(p[4] - a[4]) <= epsilon &&
               MathX.Fabs(p[5] - a[5]) <= epsilon;
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Vector6 _, in Vector6 a)
            => _.Compare(a);
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Vector6 _, in Vector6 a)
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Vector6 q && Compare(q);
        public override int GetHashCode()
            => p[0].GetHashCode();

        public float Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (float)MathX.Sqrt(p[0] * p[0] + p[1] * p[1] + p[2] * p[2] + p[3] * p[3] + p[4] * p[4] + p[5] * p[5]);
        }
        public float LengthSqr
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => p[0] * p[0] + p[1] * p[1] + p[2] * p[2] + p[3] * p[3] + p[4] * p[4] + p[5] * p[5];
        }
        /// <summary>
        /// Normalizes this instance.
        /// </summary>
        /// <returns>length</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Normalize()
        {
            var sqrLength = p[0] * p[0] + p[1] * p[1] + p[2] * p[2] + p[3] * p[3] + p[4] * p[4] + p[5] * p[5];
            var invLength = MathX.InvSqrt(sqrLength);
            p[0] *= invLength;
            p[1] *= invLength;
            p[2] *= invLength;
            p[3] *= invLength;
            p[4] *= invLength;
            p[5] *= invLength;
            return invLength * sqrLength;
        }
        /// <summary>
        /// Normalizes the fast.
        /// </summary>
        /// <returns>length</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NormalizeFast()
        {
            var sqrLength = p[0] * p[0] + p[1] * p[1] + p[2] * p[2] + p[3] * p[3] + p[4] * p[4] + p[5] * p[5];
            var invLength = MathX.RSqrt(sqrLength);
            p[0] *= invLength;
            p[1] *= invLength;
            p[2] *= invLength;
            p[3] *= invLength;
            p[4] *= invLength;
            p[5] *= invLength;
            return invLength * sqrLength;
        }

        public const int Dimension = 6;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Vector3 SubVec3(int index)
        {
            fixed (float* _ = p) return ref reinterpret.cast_vec3(_, index * 3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Fixed<T>(FloatPtr<T> callback)
        {
            fixed (float* _ = p) return callback(_);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fixed(FloatPtr callback)
        {
            fixed (float* _ = p) callback(_);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(int precision = 2)
        {
            fixed (float* _ = p) return FloatArrayToString(_, Dimension, precision);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct VectorX
    {
        public const float EPSILON = 0.001f;
        static float[] temp = new float[VECX_MAX_TEMP + 4];   // used to store intermediate results
        static int tempPtr = 0; // (float *) ( ( (intptr_t)temp + 15 ) & ~15 );              // pointer to 16 byte aligned temporary memory
        static int tempIndex = 0;               // index into memory pool, wraps around
        const int VECX_MAX_TEMP = 1024;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static int VECX_QUAD(int x) => ((x + 3) & ~3) * sizeof(float);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] void VECX_CLEAREND() { var s = size; while (s < ((s + 3) & ~3)) p[pi + s++] = 0f; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] internal static float[] VECX_ALLOCA(int n) => new float[VECX_QUAD(n)]; //:_alloca16

        internal float[] p;                       // memory the vector is stored
        internal int pi;
        int size;                   // size of the vector
        int alloced;                // if -1 p points to data set with SetData

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorX(in VectorX a)
        {
            size = alloced = 0;
            p = null; pi = 0;
            SetSize(a.size);
#if VECX_SIMD
            Simd.Copy16(p, a.p, a.size);
#else
            fixed (float* _ = p, _p = a.p) Unsafe.CopyBlock(_ + pi, _p + a.pi, (uint)a.size * sizeof(float));
#endif
            tempIndex = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorX(int length)
        {
            size = alloced = 0;
            p = null; pi = 0;
            SetSize(length);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorX(int length, float[] data)
        {
            size = alloced = 0;
            p = null; pi = 0;
            SetData(length, data);
        }

        public float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Debug.Assert(index >= 0 && index < size);
                return p[pi + index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Debug.Assert(index >= 0 && index < size);
                p[pi + index] = value;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorX operator -(in VectorX _)
        {
            var m = new VectorX();
            m.SetTempSize(_.size);
            for (var i = 0; i < _.size; i++) m.p[m.pi + i] = -_.p[_.pi + i];
            return m;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorX operator *(in VectorX _, float a)
        {
            var m = new VectorX();
            m.SetTempSize(_.size);
#if VECX_SIMD
            Simd.Mul16(m.p, _.p, a, _.size);
#else
            for (var i = 0; i < _.size; i++) m.p[m.pi + i] = _.p[_.pi + i] * a;
#endif
            return m;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorX operator /(in VectorX _, float a)
        {
            Debug.Assert(a != 0f);
            return _ * (1f / a);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator *(in VectorX _, in VectorX a)
        {
            Debug.Assert(_.size == a.size);
            var sum = 0f;
            for (var i = 0; i < _.size; i++) sum += _.p[_.pi + i] * a.p[a.pi + i];
            return sum;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorX operator -(in VectorX _, in VectorX a)
        {
            Debug.Assert(_.size == a.size);
            var m = new VectorX();
            m.SetTempSize(_.size);
#if VECX_SIMD
            Simd.Sub16(m.p, _.p, a.p, _.size);
#else
            for (var i = 0; i < _.size; i++) m.p[m.pi + i] = _.p[_.pi + i] - a.p[a.pi + i];
#endif
            return m;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorX operator +(in VectorX _, in VectorX a)
        {
            Debug.Assert(_.size == a.size);
            var m = new VectorX();
            m.SetTempSize(_.size);
#if VECX_SIMD
            Simd.Add16(m.p, _.p, a.p, _.size);
#else
            for (var i = 0; i < _.size; i++) m.p[m.pi + i] = _.p[_.pi + i] + a.p[a.pi + i];
#endif
            return m;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorX operator *(float a, in VectorX b)
            => b * a;

        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in VectorX a)
        {
            Debug.Assert(size == a.size);
            for (var i = 0; i < size; i++) if (p[pi + i] != a.p[a.pi + i]) return false;
            return true;
        }
        /// <summary>
        /// compare with epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in VectorX a, float epsilon)
        {
            Debug.Assert(size == a.size);
            for (var i = 0; i < size; i++) if (MathX.Fabs(p[pi + i] - a.p[a.pi + i]) > epsilon) return false;
            return true;
        }
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in VectorX _, in VectorX a)
            => _.Compare(a);
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in VectorX _, in VectorX a)
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is VectorX q && Compare(q);
        public override int GetHashCode()
            => p.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSize(int size)
        {
            var alloc = (size + 3) & ~3;
            if (alloc > alloced && alloced != -1) { p = new float[alloc]; pi = 0; alloced = alloc; }
            this.size = size;
            VECX_CLEAREND();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ChangeSize(int size, bool makeZero = false)
        {
            var alloc = (size + 3) & ~3;
            if (alloc > alloced && alloced != -1)
            {
                var oldVec = p;
                p = new float[alloc];
                alloced = alloc;
                if (oldVec != null) for (var i = 0; i < this.size; i++) p[i] = oldVec[pi + i];
                pi = 0;
                // zero any new elements
                if (makeZero) for (var i = size; i < size; i++) p[i] = 0f;
            }
            this.size = size;
            VECX_CLEAREND();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetTempSize(int size)
        {
            this.size = size;
            alloced = (size + 3) & ~3;
            Debug.Assert(alloced < VECX_MAX_TEMP);
            if (tempIndex + alloced > VECX_MAX_TEMP) tempIndex = 0;
            p = temp;
            //fixed (float* _ = &temp[0]) tempPtr = (int)((ulong)_ + 15) & ~15;
            pi = tempIndex;
            tempIndex += alloced;
            VECX_CLEAREND();
        }

        public int Size
            => size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetData(int length, float[] data, int index = 0)
        {
            //Debug.Assert((((uintptr_t)data) & 15) == 0); // data must be 16 byte aligned
            p = data;
            pi = index;
            size = length;
            alloced = -1;
            VECX_CLEAREND();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()
        {
#if VECX_SIMD
            Simd.Zero16(p, size);
#else
            fixed (float* _ = p) Unsafe.InitBlock(_ + pi, 0, (uint)size * sizeof(float));
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero(int length)
        {
            SetSize(length);
#if VECX_SIMD
            Simd.Zero16(p, length);
#else
            fixed (float* _ = p) Unsafe.InitBlock(_ + pi, 0, (uint)size * sizeof(float));
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Random(int seed, float l = 0f, float u = 1f)
        {
            var rnd = new RandomX(seed);
            var c = u - l;
            for (var i = 0; i < size; i++) p[pi + i] = l + rnd.RandomFloat() * c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Random(int length, int seed, float l = 0f, float u = 1f)
        {
            var rnd = new RandomX(seed);
            SetSize(length);
            var c = u - l;
            for (var i = 0; i < size; i++) p[pi + i] = l + rnd.RandomFloat() * c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Negate()
        {
#if VECX_SIMD
            Simd.Negate16(p, size);
#else
            for (var i = 0; i < size; i++) p[pi + i] = -p[pi + i];
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clamp(float min, float max)
        {
            for (var i = 0; i < size; i++)
            {
                if (p[pi + i] < min) p[pi + i] = min;
                else if (p[pi + i] > max) p[pi + i] = max;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorX SwapElements(int e1, int e2)
        {
            var tmp = p[pi + e1];
            p[pi + e1] = p[pi + e2];
            p[pi + e2] = tmp;
            return this;
        }

        public float Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var sum = 0f;
                for (var i = 0; i < size; i++) sum += p[pi + i] * p[pi + i];
                return MathX.Sqrt(sum);
            }
        }
        public float LengthSqr
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var sum = 0f;
                for (var i = 0; i < size; i++) sum += p[pi + i] * p[pi + i];
                return sum;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorX Normalize()
        {
            int i;
            var m = new VectorX();
            m.SetTempSize(size);
            var sum = 0f;
            for (i = 0; i < size; i++) sum += p[pi + i] * p[pi + i];
            var invSqrt = MathX.InvSqrt(sum);
            for (i = 0; i < size; i++) m.p[pi + i] = p[pi + i] * invSqrt;
            return m;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NormalizeSelf()
        {
            int i;
            var sum = 0f;
            for (i = 0; i < size; i++) sum += p[pi + i] * p[pi + i];
            var invSqrt = MathX.InvSqrt(sum);
            for (i = 0; i < size; i++) p[pi + i] *= invSqrt;
            return invSqrt * sum;
        }

        public int Dimension
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => size;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Vector3 SubVec3(int index)
        {
            Debug.Assert(index >= 0 && index * 3 + 3 <= size);
            fixed (float* _ = p) return ref reinterpret.cast_vec3(_, pi + index * 3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Vector6 SubVec6(int index)
        {
            Debug.Assert(index >= 0 && index * 6 + 6 <= size);
            fixed (float* _ = p) return ref reinterpret.cast_vec6(_, pi + index * 6);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Fixed<T>(FloatPtr<T> callback)
        {
            fixed (float* _ = p) return callback(_ + pi);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fixed(FloatPtr callback)
        {
            fixed (float* _ = p) callback(_ + pi);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(int precision = 2)
        {
            var dimension = Dimension;
            fixed (float* _ = p) return FloatArrayToString(_ + pi, dimension, precision);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Polar3
    {
        public float radius, theta, phi;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Polar3(float radius, float theta, float phi)
        {
            Debug.Assert(radius > 0);
            this.radius = radius;
            this.theta = theta;
            this.phi = phi;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float radius, float theta, float phi)
        {
            Debug.Assert(radius > 0);
            this.radius = radius;
            this.theta = theta;
            this.phi = phi;
        }

        public float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { fixed (float* _ = &radius) return _[index]; }
        }

        public static Polar3 operator -(in Polar3 _)
            => new(_.radius, -_.theta, -_.phi);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ToVec3()
        {
            MathX.SinCos(phi, out var sp, out var cp);
            MathX.SinCos(theta, out var st, out var ct);
            return new Vector3(cp * radius * ct, cp * radius * st, radius * sp);
        }
    }
}