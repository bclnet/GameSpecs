using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.NumericsX
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Vector3i
    {
        public static Vector3i origin = new(0, 0, 0);

        public int x;
        public int y;
        public int z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3i(int xyz)
            => x = y = z = xyz;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3i(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(in Vector3i a)
            => this = a;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()
            => x = y = z = 0;

        public int this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { fixed (int* p = &x) return p[index]; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { fixed (int* p = &x) p[index] = value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i operator -(in Vector3i _)
            => new(-_.x, -_.y, -_.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float operator *(in Vector3i _, in Vector3i a)
            => _.x * a.x + _.y * a.y + _.z * a.z;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i operator *(in Vector3i _, int a)
            => new(_.x * a, _.y * a, _.z * a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i operator /(in Vector3i _, int a)
        {
            var inva = 1 / a;
            return new Vector3i(_.x * inva, _.y * inva, _.z * inva);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i operator +(in Vector3i _, in Vector3i a)
            => new(_.x + a.x, _.y + a.y, _.z + a.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i operator -(in Vector3i _, in Vector3i a)
            => new(_.x - a.x, _.y - a.y, _.z - a.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i operator *(int a, in Vector3i b)
            => new(b.x * a, b.y * a, b.z * a);

        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="a">a.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Vector3i a)
            => x == a.x && y == a.y && z == a.z;
        /// <summary>
        /// exact compare, no epsilon
        /// </summary>
        /// <param name="_">The .</param>
        /// <param name="a">a.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Vector3i _, in Vector3i a)
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
        public static bool operator !=(in Vector3i _, in Vector3i a)
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Vector3i q && Compare(q);
        public override int GetHashCode()
            => x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3i Cross(in Vector3i a)
            => new(y * a.z - z * a.y, z * a.x - x * a.z, x * a.y - y * a.x);
        public Vector3i Cross(in Vector3i a, in Vector3i b)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clamp(in Vector3i min, in Vector3i max)
        {
            if (x < min.x) x = min.x;
            else if (x > max.x) x = max.x;
            if (y < min.y) y = min.y;
            else if (y > max.y) y = max.y;
            if (z < min.z) z = min.z;
            else if (z > max.z) z = max.z;
        }

        public const int Dimension = 3;
    }
}