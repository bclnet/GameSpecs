using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.NumericsX.Platform;

namespace System.NumericsX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Angles
    {
        public static Angles zero = new(0f, 0f, 0f);
        // angle indexes
        public const int PITCH = 0;     // up / down
        public const int YAW = 1;       // left / right
        public const int ROLL = 2;      // fall over

        public float pitch;
        public float yaw;
        public float roll;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Angles(float pitch, float yaw, float roll)
        {
            this.pitch = pitch;
            this.yaw = yaw;
            this.roll = roll;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Angles(in Vector3 v)
        {
            this.pitch = v.x;
            this.yaw = v.y;
            this.roll = v.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float pitch, float yaw, float roll)
        {
            this.pitch = pitch;
            this.yaw = yaw;
            this.roll = roll;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Angles Zero()
        {
            pitch = yaw = roll = 0f;
            return this;
        }

        public unsafe float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { fixed (float* p = &pitch) return p[index]; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { fixed (float* p = &pitch) p[index] = value; }
        }

        /// <summary>
        /// negate angles, in general not the inverse rotation
        /// </summary>
        /// <param name="_">The .</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static Angles operator -(in Angles _)
            => new(-_.pitch, -_.yaw, -_.roll);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Angles operator +(in Angles _, in Angles a)
            => new(_.pitch + a.pitch, _.yaw + a.yaw, _.roll + a.roll);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Angles operator -(in Angles _, in Angles a)
            => new(_.pitch - a.pitch, _.yaw - a.yaw, _.roll - a.roll);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Angles operator *(in Angles _, float a)
            => new(_.pitch * a, _.yaw * a, _.roll * a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Angles operator /(in Angles _, float a)
        {
            var inva = 1f / a;
            return new(_.pitch * inva, _.yaw * inva, _.roll * inva);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Angles operator *(float a, in Angles b)
            => new(a * b.pitch, a * b.yaw, a * b.roll);

        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Angles a)
            => a.pitch == pitch && a.yaw == yaw && a.roll == roll;
        /// <summary>
        /// compare with epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Angles a, float epsilon)
            => MathX.Fabs(pitch - a.pitch) <= epsilon &&
               MathX.Fabs(yaw - a.yaw) <= epsilon &&
               MathX.Fabs(roll - a.roll) <= epsilon;
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Angles _, in Angles a)
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
        public static bool operator !=(in Angles _, in Angles a)
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Angles q && Compare(q);
        public override int GetHashCode()
            => pitch.GetHashCode() ^ yaw.GetHashCode() & roll.GetHashCode();
        /// <summary>
        /// returns angles normalized to the range [0 <= angle < 360]
        /// normalizes 'this'
        /// </summary>
        /// <returns></returns>
        public Angles Normalize360()
        {
            for (var i = 0; i < 3; i++)
                if ((this[i] >= 360f) || (this[i] < 0f))
                {
                    this[i] -= (float)Math.Floor(this[i] / 360f) * 360f;
                    if (this[i] >= 360f) this[i] -= 360f;
                    if (this[i] < 0f) this[i] += 360f;
                }
            return this;
        }
        /// <summary>
        /// returns angles normalized to the range [-180 < angle <= 180]
        /// normalizes 'this'
        /// </summary>
        /// <returns></returns>
        public Angles Normalize180()
        {
            Normalize360();
            if (pitch > 180f) pitch -= 360f;
            if (yaw > 180f) yaw -= 360f;
            if (roll > 180f) roll -= 360f;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clamp(in Angles min, in Angles max)
        {
            if (pitch < min.pitch) pitch = min.pitch;
            else if (pitch > max.pitch) pitch = max.pitch;
            if (yaw < min.yaw) yaw = min.yaw;
            else if (yaw > max.yaw) yaw = max.yaw;
            if (roll < min.roll) roll = min.roll;
            else if (roll > max.roll) roll = max.roll;
        }

        public const int Dimension = 3;

        public void ToVectors(out Vector3 forward, out Vector3 right, out Vector3 up)
        {
            MathX.SinCos(MathX.DEG2RAD(yaw), out var sy, out var cy);
            MathX.SinCos(MathX.DEG2RAD(pitch), out var sp, out var cp);
            MathX.SinCos(MathX.DEG2RAD(roll), out var sr, out var cr);

            forward = new Vector3(cp * cy, cp * sy, -sp);
            right = new Vector3(-sr * sp * cy + cr * sy, -sr * sp * sy + -cr * cy, -sr * cp);
            up = new Vector3(cr * sp * cy + -sr * -sy, cr * sp * sy + -sr * cy, cr * cp);
        }

        public Vector3 ToForward()
        {
            MathX.SinCos(MathX.DEG2RAD(yaw), out var sy, out var cy);
            MathX.SinCos(MathX.DEG2RAD(pitch), out var sp, out var cp);
            return new(cp * cy, cp * sy, -sp);
        }

        public Quat ToQuat()
        {
            MathX.SinCos(MathX.DEG2RAD(yaw) * 0.5f, out var sz, out var cz);
            MathX.SinCos(MathX.DEG2RAD(pitch) * 0.5f, out var sy, out var cy);
            MathX.SinCos(MathX.DEG2RAD(roll) * 0.5f, out var sx, out var cx);

            var sxcy = sx * cy;
            var cxcy = cx * cy;
            var sxsy = sx * sy;
            var cxsy = cx * sy;

            return new(cxsy * sz - sxcy * cz, -cxsy * cz - sxcy * sz, sxsy * cz - cxcy * sz, cxcy * cz + sxsy * sz);
        }

        public Rotation ToRotation()
        {
            if (pitch == 0f)
            {
                if (yaw == 0f) return new(Vector3.origin, new Vector3(-1f, 0f, 0f), roll);
                if (roll == 0f) return new(Vector3.origin, new Vector3(0f, 0f, -1f), yaw);
            }
            else if (yaw == 0f && roll == 0f) return new(Vector3.origin, new Vector3(0f, -1f, 0f), pitch);

            MathX.SinCos(MathX.DEG2RAD(yaw) * 0.5f, out var sz, out var cz);
            MathX.SinCos(MathX.DEG2RAD(pitch) * 0.5f, out var sy, out var cy);
            MathX.SinCos(MathX.DEG2RAD(roll) * 0.5f, out var sx, out var cx);

            var sxcy = sx * cy;
            var cxcy = cx * cy;
            var sxsy = sx * sy;
            var cxsy = cx * sy;

            var vec = new Vector3
            {
                x = cxsy * sz - sxcy * cz,
                y = -cxsy * cz - sxcy * sz,
                z = sxsy * cz - cxcy * sz
            };
            var w = cxcy * cz + sxsy * sz;
            var angle = MathX.ACos(w);
            if (angle == 0f) vec.Set(0f, 0f, 1f);
            else
            {
                vec.Normalize();
                vec.FixDegenerateNormal();
                angle *= 2f * MathX.M_RAD2DEG;
            }
            return new(Vector3.origin, vec, angle);
        }

        public Matrix3x3 ToMat3()
        {
            MathX.SinCos(MathX.DEG2RAD(yaw), out var sy, out var cy);
            MathX.SinCos(MathX.DEG2RAD(pitch), out var sp, out var cp);
            MathX.SinCos(MathX.DEG2RAD(roll), out var sr, out var cr);

            var mat = new Matrix3x3();
            mat[0].Set(cp * cy, cp * sy, -sp);
            mat[1].Set(sr * sp * cy + cr * -sy, sr * sp * sy + cr * cy, sr * cp);
            mat[2].Set(cr * sp * cy + -sr * -sy, cr * sp * sy + -sr * cy, cr * cp);

            return mat;
        }

        public Matrix4x4 ToMat4()
            => ToMat3().ToMat4();

        public Vector3 ToAngularVelocity()
        {
            var rotation = ToRotation();
            return rotation.Vec * MathX.DEG2RAD(rotation.Angle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
        {
            fixed (float* _ = &pitch) return callback(_);
        }

        public unsafe string ToString(int precision = 2)
            => ToFloatPtr(_ => FloatArrayToString(_, Dimension, precision));
    }
}