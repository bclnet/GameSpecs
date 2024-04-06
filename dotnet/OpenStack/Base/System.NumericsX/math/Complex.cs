using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.NumericsX.Platform;

namespace System.NumericsX
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Complex
    {
        public static int ALLOC16 = 2;
        public static Complex origin = new(0f, 0f);
        public float r;     // real part
        public float i;      // imaginary part

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Complex(float r, float i)
        {
            this.r = r;
            this.i = i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float r, float i)
        {
            this.r = r;
            this.i = i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()
            => r = i = 0f;

        public unsafe float this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                fixed (float* p = &r)
                    return p[index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                fixed (float* p = &r)
                    p[index] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Complex operator -(in Complex _)
            => new(-_.r, -_.i);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Complex operator *(in Complex _, in Complex a)
            => new(_.r * a.r - _.i * a.i, _.i * a.r + _.r * a.i);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Complex operator /(in Complex _, in Complex a)
        {
            float s, t;
            if (MathX.Fabs(a.r) >= MathX.Fabs(a.i))
            {
                s = a.i / a.r;
                t = 1f / (a.r + s * a.i);
                return new((_.r + s * _.i) * t, (_.i - s * _.r) * t);
            }
            else
            {
                s = a.r / a.i;
                t = 1f / (s * a.r + a.i);
                return new((_.r * s + _.i) * t, (_.i * s - _.r) * t);
            }
        }
        public static Complex operator +(in Complex _, in Complex a)
            => new(_.r + a.r, _.i + a.i);
        public static Complex operator -(in Complex _, in Complex a)
            => new(_.r - a.r, _.i - a.i);

        public static Complex operator *(in Complex _, float a)
            => new(_.r * a, _.i * a);
        public static Complex operator /(in Complex _, float a)
        {
            var s = 1f / a;
            return new(_.r * s, _.i * s);
        }
        public static Complex operator +(in Complex _, float a)
            => new(_.r + a, _.i);
        public static Complex operator -(in Complex _, float a)
            => new(_.r - a, _.i);

        public static Complex operator *(float a, in Complex b)
            => new(a * b.r, a * b.i);
        public static Complex operator /(float a, in Complex b)
        {
            float s, t;
            if (MathX.Fabs(b.r) >= MathX.Fabs(b.i))
            {
                s = b.i / b.r;
                t = a / (b.r + s * b.i);
                return new(t, -s * t);
            }
            else
            {
                s = b.r / b.i;
                t = a / (s * b.r + b.i);
                return new(s * t, -t);
            }
        }
        public static Complex operator +(float a, in Complex b)
            => new(a + b.r, b.i);
        public static Complex operator -(float a, in Complex b)
            => new(a - b.r, -b.i);

        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <returns></returns>
        public bool Compare(in Complex a)
            => r == a.r && i == a.i;
        /// <summary>
        /// compare with epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns></returns>
        public bool Compare(in Complex a, float epsilon)
            => MathX.Fabs(r - a.r) <= epsilon &&
               MathX.Fabs(i - a.i) <= epsilon;
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(in Complex _, in Complex a)
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
        public static bool operator !=(in Complex _, in Complex a)
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Complex q && Compare(q);
        public override int GetHashCode()
            => r.GetHashCode() ^ i.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Complex Reciprocal()
        {
            float s, t;
            if (MathX.Fabs(r) >= MathX.Fabs(i))
            {
                s = i / r;
                t = 1f / (r + s * i);
                return new(t, -s * t);
            }
            else
            {
                s = r / i;
                t = 1f / (s * r + i);
                return new(s * t, -t);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Complex Sqrt()
        {
            if (r == 0f && i == 0f)
                return new(0f, 0f);
            float w;
            var x = MathX.Fabs(r);
            var y = MathX.Fabs(i);
            if (x >= y)
            {
                w = y / x;
                w = MathX.Sqrt(x) * MathX.Sqrt(0.5f * (1f + MathX.Sqrt(1f + w * w)));
            }
            else
            {
                w = x / y;
                w = MathX.Sqrt(y) * MathX.Sqrt(0.5f * (w + MathX.Sqrt(1f + w * w)));
            }
            if (w == 0f) return new(0f, 0f);
            if (r >= 0f) return new(w, 0.5f * i / w);
            else return new(0.5f * y / w, (i >= 0f) ? w : -w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Abs()
        {
            float t;
            var x = MathX.Fabs(r);
            var y = MathX.Fabs(i);
            if (x == 0f) return y;
            else if (y == 0f) return x;
            else if (x > y) { t = y / x; return x * MathX.Sqrt(1f + t * t); }
            else { t = x / y; return y * MathX.Sqrt(1f + t * t); }
        }

        public const int Dimension = 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T ToFloatPtr<T>(FloatPtr<T> callback)
        {
            fixed (float* _ = &r) return callback(_);
        }

        public unsafe string ToString(int precision = 2)
            => ToFloatPtr(_ => FloatArrayToString(_, Dimension, precision));
    }
}