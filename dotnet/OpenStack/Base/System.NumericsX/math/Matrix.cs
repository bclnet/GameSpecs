//#define MATX_SIMD
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.NumericsX.Platform;

namespace System.NumericsX
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Matrix2x2
    {
        public static Matrix2x2 zero = new(new Vector2(0f, 0f), new Vector2(0f, 0f));
        public static Matrix2x2 identity = new(new Vector2(1f, 0f), new Vector2(0f, 1f));

        internal Vector2 mat0;
        internal Vector2 mat1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix2x2(in Matrix2x2 a)
            => this = a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix2x2(in Vector2 x, in Vector2 y)
        {
            mat0.x = x.x; mat0.y = x.y;
            mat1.x = y.x; mat1.y = y.y;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix2x2(float xx, float xy, float yx, float yy)
        {
            mat0.x = xx; mat0.y = xy;
            mat1.x = yx; mat1.y = yy;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix2x2(float[] src)
        {
            this = default;
            fixed (void* mat_ = &mat0, src_ = src) Unsafe.CopyBlock(mat_, src_, 2 * 2 * sizeof(float));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix2x2(float[,] src)
        {
            this = default;
            fixed (void* mat_ = &mat0, src_ = &src[0, 0]) Unsafe.CopyBlock(mat_, src_, 2 * 2 * sizeof(float));
        }

        public ref Vector2 this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { fixed (Vector2* mat_ = &mat0) return ref mat_[index]; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix2x2 operator -(in Matrix2x2 _)
            => new(
            -_.mat0.x, -_.mat0.y,
            -_.mat1.x, -_.mat1.y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix2x2 operator *(in Matrix2x2 _, float a)
            => new(
            _.mat0.x * a, _.mat0.y * a,
            _.mat1.x * a, _.mat1.y * a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(in Matrix2x2 _, in Vector2 vec)
            => new(
            _.mat0.x * vec.x + _.mat0.y * vec.y,
            _.mat1.x * vec.x + _.mat1.y * vec.y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix2x2 operator *(in Matrix2x2 _, in Matrix2x2 a)
            => new(
            _.mat0.x * a.mat0.x + _.mat0.y * a.mat1.x,
            _.mat0.x * a.mat0.y + _.mat0.y * a.mat1.y,
            _.mat1.x * a.mat0.x + _.mat1.y * a.mat1.x,
            _.mat1.x * a.mat0.y + _.mat1.y * a.mat1.y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix2x2 operator +(in Matrix2x2 _, in Matrix2x2 a)
            => new(
            _.mat0.x + a.mat0.x, _.mat0.y + a.mat0.y,
            _.mat1.x + a.mat1.x, _.mat1.y + a.mat1.y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix2x2 operator -(in Matrix2x2 _, in Matrix2x2 a)
            => new(
            _.mat0.x - a.mat0.x, _.mat0.y - a.mat0.y,
            _.mat1.x - a.mat1.x, _.mat1.y - a.mat1.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix2x2 operator *(float a, in Matrix2x2 mat)
            => mat * a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(in Vector2 vec, in Matrix2x2 mat)
            => mat * vec;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Matrix2x2 a)                        // exact compare, no epsilon
            => mat0.Compare(a.mat0) &&
               mat1.Compare(a.mat1);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Matrix2x2 a, float epsilon)   // compare with epsilon
            => mat0.Compare(a.mat0, epsilon) &&
               mat1.Compare(a.mat1, epsilon);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Matrix2x2 _, in Matrix2x2 a)                 // exact compare, no epsilon
            => _.Compare(a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Matrix2x2 _, in Matrix2x2 a)                 // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Matrix2x2 q && Compare(q);
        public override int GetHashCode()
            => mat0.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()
        {
            mat0.Zero();
            mat1.Zero();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Identity()
            => this = new(identity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsIdentity(float epsilon = MatrixX.EPSILON)
            => Compare(identity, epsilon);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSymmetric(float epsilon = MatrixX.EPSILON)
            => MathX.Fabs(mat0.y - mat1.x) < epsilon;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDiagonal(float epsilon = MatrixX.EPSILON)
            => MathX.Fabs(mat0.y) <= epsilon &&
               MathX.Fabs(mat1.x) <= epsilon;

        public float Trace
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => mat0.x + mat1.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Determinant()
            => mat0.x * mat1.y - mat0.y * mat1.x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix2x2 Transpose()   // returns transpose
            => new(
            mat0.x, mat1.x,
            mat0.y, mat1.y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix2x2 TransposeSelf()
        {
            var tmp = mat0.y;
            mat0.y = mat1.x;
            mat1.x = tmp;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix2x2 Inverse()      // returns the inverse ( m * m.Inverse() = identity )
        {
            Matrix2x2 invMat = new(this);
            var r = invMat.InverseSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseSelf()     // returns false if determinant is zero
        {
            // 2+4 = 6 multiplications
            //		 1 division
            double det, invDet, a;

            det = mat0.x * mat1.y - mat0.y * mat1.x;
            if (MathX.Fabs(det) < MatrixX.INVERSE_EPSILON) return false;

            invDet = 1f / det;
            a = mat0.x;
            mat0.x = (float)(mat1.y * invDet);
            mat0.y = (float)(-mat0.y * invDet);
            mat1.x = (float)(-mat1.x * invDet);
            mat1.y = (float)(a * invDet);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix2x2 InverseFast()  // returns the inverse ( m * m.Inverse() = identity )
        {
            Matrix2x2 invMat = new(this);
            var r = invMat.InverseFastSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseFastSelf()    // returns false if determinant is zero
        {
            // 2+4 = 6 multiplications
            //		 1 division
            double det, invDet, a;

            det = mat0.x * mat1.y - mat0.y * mat1.x;
            if (MathX.Fabs(det) < MatrixX.INVERSE_EPSILON) return false;

            invDet = 1f / det;
            a = mat0.x;
            mat0.x = (float)(mat1.y * invDet);
            mat0.y = (float)(-mat0.y * invDet);
            mat1.x = (float)(-mat1.x * invDet);
            mat1.y = (float)(a * invDet);
            return true;
        }

        public const int Dimension = 4;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Fixed<T>(FloatPtr<T> callback)
            => mat0.Fixed(callback);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fixed(FloatPtr callback)
            => mat0.Fixed(callback);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(int precision = 2)
        {
            fixed (float* _ = &mat0.x) return FloatArrayToString(_, Dimension, precision);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Matrix3x3
    {
        public const int SizeOf = 3 * 3 * sizeof(float);
        public static Matrix3x3 zero = new(new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        public static Matrix3x3 identity = new(new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1));

        public Vector3 mat0;
        internal Vector3 mat1;
        internal Vector3 mat2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3x3(in Matrix3x3 a)
            => this = a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3x3(in Vector3 x, in Vector3 y, in Vector3 z)
        {
            mat0.x = x.x; mat0.y = x.y; mat0.z = x.z;
            mat1.x = y.x; mat1.y = y.y; mat1.z = y.z;
            mat2.x = z.x; mat2.y = z.y; mat2.z = z.z;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3x3(float xx, float xy, float xz, float yx, float yy, float yz, float zx, float zy, float zz)
        {
            mat0.x = xx; mat0.y = xy; mat0.z = xz;
            mat1.x = yx; mat1.y = yy; mat1.z = yz;
            mat2.x = zx; mat2.y = zy; mat2.z = zz;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3x3(float[] src)
        {
            this = default;
            fixed (void* mat_ = &mat0, src_ = src) Unsafe.CopyBlock(mat_, src_, 2U * 2U * sizeof(float));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3x3(float[,] src)
        {
            this = default;
            fixed (void* mat_ = &mat0, src_ = &src[0, 0]) Unsafe.CopyBlock(mat_, src_, 2U * 2U * sizeof(float));
        }

        public ref Vector3 this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { fixed (Vector3* mat_ = &mat0) return ref mat_[index]; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 operator -(in Matrix3x3 _)
            => new(
            -_.mat0.x, -_.mat0.y, -_.mat0.z,
            -_.mat1.x, -_.mat1.y, -_.mat1.z,
            -_.mat2.x, -_.mat2.y, -_.mat2.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 operator *(in Matrix3x3 _, float a)
            => new(
            _.mat0.x * a, _.mat0.y * a, _.mat0.z * a,
            _.mat1.x * a, _.mat1.y * a, _.mat1.z * a,
            _.mat2.x * a, _.mat2.y * a, _.mat2.z * a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(in Matrix3x3 _, in Vector3 vec)
            => new(
            _.mat0.x * vec.x + _.mat1.x * vec.y + _.mat2.x * vec.z,
            _.mat0.y * vec.x + _.mat1.y * vec.y + _.mat2.y * vec.z,
            _.mat0.z * vec.x + _.mat1.z * vec.y + _.mat2.z * vec.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 operator *(in Matrix3x3 _, in Matrix3x3 a)
        {
            Matrix3x3 dst = default;
            void* dst_ = &dst.mat0;
            fixed (void* __ = &_.mat0, a_ = &a.mat0)
            {
                var m1Ptr = (float*)__;
                var m2Ptr = (float*)a_;
                var dstPtr = (float*)dst_;
                for (var i = 0; i < 3; i++)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        *dstPtr = m1Ptr[0] * m2Ptr[0 * 3 + j]
                                + m1Ptr[1] * m2Ptr[1 * 3 + j]
                                + m1Ptr[2] * m2Ptr[2 * 3 + j];
                        dstPtr++;
                    }
                    m1Ptr += 3;
                }
                return dst;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 operator +(in Matrix3x3 _, in Matrix3x3 a)
            => new(
            _.mat0.x + a.mat0.x, _.mat0.y + a.mat0.y, _.mat0.z + a.mat0.z,
            _.mat1.x + a.mat1.x, _.mat1.y + a.mat1.y, _.mat1.z + a.mat1.z,
            _.mat2.x + a.mat2.x, _.mat2.y + a.mat2.y, _.mat2.z + a.mat2.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 operator -(in Matrix3x3 _, in Matrix3x3 a)
            => new(
            _.mat0.x - a.mat0.x, _.mat0.y - a.mat0.y, _.mat0.z - a.mat0.z,
            _.mat1.x - a.mat1.x, _.mat1.y - a.mat1.y, _.mat1.z - a.mat1.z,
            _.mat2.x - a.mat2.x, _.mat2.y - a.mat2.y, _.mat2.z - a.mat2.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3x3 operator *(float a, in Matrix3x3 mat)
            => mat * a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(in Vector3 vec, in Matrix3x3 mat)
            => mat * vec;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Matrix3x3 a)                       // exact compare, no epsilon
            => mat0.Compare(a.mat0) &&
               mat1.Compare(a.mat1) &&
               mat2.Compare(a.mat2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Matrix3x3 a, float epsilon)  // compare with epsilon
            => mat0.Compare(a.mat0, epsilon) &&
               mat1.Compare(a.mat1, epsilon) &&
               mat2.Compare(a.mat2, epsilon);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Matrix3x3 _, in Matrix3x3 a)                   // exact compare, no epsilon
            => _.Compare(a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Matrix3x3 _, in Matrix3x3 a)                   // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Matrix3x3 q && Compare(q);
        public override int GetHashCode()
            => mat0.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()
        {
            fixed (void* mat_ = &mat0) Unsafe.InitBlock(mat_, 0, 3U * (uint)sizeof(Vector3));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Identity()
            => this = new(identity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsIdentity(float epsilon = MatrixX.EPSILON)
            => Compare(identity, epsilon);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSymmetric(float epsilon = MatrixX.EPSILON)
            => MathX.Fabs(mat0.y - mat1.x) <= epsilon &&
               MathX.Fabs(mat0.z - mat2.x) <= epsilon &&
               MathX.Fabs(mat1.z - mat2.y) <= epsilon;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDiagonal(float epsilon = MatrixX.EPSILON)
            => MathX.Fabs(mat0.y) <= epsilon &&
               MathX.Fabs(mat0.z) <= epsilon &&
               MathX.Fabs(mat1.x) <= epsilon &&
               MathX.Fabs(mat1.z) <= epsilon &&
               MathX.Fabs(mat2.x) <= epsilon &&
               MathX.Fabs(mat2.y) <= epsilon;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsRotated()
            => !Compare(identity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProjectVector(in Vector3 src, out Vector3 dst)
        {
            dst.x = src * mat0;
            dst.y = src * mat1;
            dst.z = src * mat2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnprojectVector(in Vector3 src, out Vector3 dst)
            => dst = mat0 * src.x + mat1 * src.y + mat2 * src.z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FixDegeneracies()    // fix degenerate axial cases
        {
            var r = mat0.FixDegenerateNormal();
            r |= mat1.FixDegenerateNormal();
            r |= mat2.FixDegenerateNormal();
            return r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FixDenormals()       // change tiny numbers to zero
        {
            var r = mat0.FixDenormals();
            r |= mat1.FixDenormals();
            r |= mat2.FixDenormals();
            return r;
        }

        public float Trace
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => mat0.x + mat1.y + mat2.z;
        }

        public float Determinant()
        {
            var det2_12_01 = mat1.x * mat2.y - mat1.y * mat2.x;
            var det2_12_02 = mat1.x * mat2.z - mat1.z * mat2.x;
            var det2_12_12 = mat1.y * mat2.z - mat1.z * mat2.y;
            return mat0.x * det2_12_12 - mat0.y * det2_12_02 + mat0.z * det2_12_01;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3x3 OrthoNormalize()
        {
            Matrix3x3 ortho = new(this);
            ortho.mat0.Normalize();
            ortho.mat2.Cross(mat0, mat1); ortho.mat2.Normalize();
            ortho.mat1.Cross(mat2, mat0); ortho.mat1.Normalize();
            return ortho;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3x3 OrthoNormalizeSelf()
        {
            mat0.Normalize();
            mat2.Cross(mat0, mat1); mat2.Normalize();
            mat1.Cross(mat2, mat0); mat1.Normalize();
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3x3 Transpose()   // returns transpose
            => new(
            mat0.x, mat1.x, mat2.x,
            mat0.y, mat1.y, mat2.y,
            mat0.z, mat1.z, mat2.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3x3 TransposeSelf()
        {
            var tmp0 = mat0.y; mat0.y = mat1.x; mat1.x = tmp0;
            var tmp1 = mat0.z; mat0.z = mat2.x; mat2.x = tmp1;
            var tmp2 = mat1.z; mat1.z = mat2.y; mat2.y = tmp2;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3x3 Inverse()     // returns the inverse ( m * m.Inverse() = identity )
        {
            Matrix3x3 invMat = new(this);
            var r = invMat.InverseSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseSelf()        // returns false if determinant is zero
        {
            // 18+3+9 = 30 multiplications
            //			 1 division
            Matrix3x3 inverse = new(); double det, invDet;

            inverse.mat0.x = mat1.y * mat2.z - mat1.z * mat2.y;
            inverse.mat1.x = mat1.z * mat2.x - mat1.x * mat2.z;
            inverse.mat2.x = mat1.x * mat2.y - mat1.y * mat2.x;

            det = mat0.x * inverse.mat0.x + mat0.y * inverse.mat1.x + mat0.z * inverse.mat2.x;
            if (MathX.Fabs(det) < MatrixX.INVERSE_EPSILON)
                return false;

            inverse.mat0.y = mat0.z * mat2.y - mat0.y * mat2.z;
            inverse.mat0.z = mat0.y * mat1.z - mat0.z * mat1.y;
            inverse.mat1.y = mat0.x * mat2.z - mat0.z * mat2.x;
            inverse.mat1.z = mat0.z * mat1.x - mat0.x * mat1.z;
            inverse.mat2.y = mat0.y * mat2.x - mat0.x * mat2.y;
            inverse.mat2.z = mat0.x * mat1.y - mat0.y * mat1.x;

            invDet = 1f / det;
            mat0.x = (float)(inverse.mat0.x * invDet); mat0.y = (float)(inverse.mat0.y * invDet); mat0.z = (float)(inverse.mat0.z * invDet);
            mat1.x = (float)(inverse.mat1.x * invDet); mat1.y = (float)(inverse.mat1.y * invDet); mat1.z = (float)(inverse.mat1.z * invDet);
            mat2.x = (float)(inverse.mat2.x * invDet); mat2.y = (float)(inverse.mat2.y * invDet); mat2.z = (float)(inverse.mat2.z * invDet);

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3x3 InverseFast() // returns the inverse ( m * m.Inverse() = identity )
        {
            Matrix3x3 invMat = new(this);
            var r = invMat.InverseFastSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseFastSelf()    // returns false if determinant is zero
        {
            // 18+3+9 = 30 multiplications
            //			 1 division
            Matrix3x3 inverse = new(); double det, invDet;

            inverse.mat0.x = mat1.y * mat2.z - mat1.z * mat2.y;
            inverse.mat1.x = mat1.z * mat2.x - mat1.x * mat2.z;
            inverse.mat2.x = mat1.x * mat2.y - mat1.y * mat2.x;

            det = mat0.x * inverse.mat0.x + mat0.y * inverse.mat1.x + mat0.z * inverse.mat2.x;
            if (MathX.Fabs(det) < MatrixX.INVERSE_EPSILON)
                return false;

            inverse.mat0.y = mat0.z * mat2.y - mat0.y * mat2.z;
            inverse.mat0.z = mat0.y * mat1.z - mat0.z * mat1.y;
            inverse.mat1.y = mat0.x * mat2.z - mat0.z * mat2.x;
            inverse.mat1.z = mat0.z * mat1.x - mat0.x * mat1.z;
            inverse.mat2.y = mat0.y * mat2.x - mat0.x * mat2.y;
            inverse.mat2.z = mat0.x * mat1.y - mat0.y * mat1.x;

            invDet = 1f / det;
            mat0.x = (float)(inverse.mat0.x * invDet); mat0.y = (float)(inverse.mat0.y * invDet); mat0.z = (float)(inverse.mat0.z * invDet);
            mat1.x = (float)(inverse.mat1.x * invDet); mat1.y = (float)(inverse.mat1.y * invDet); mat1.z = (float)(inverse.mat1.z * invDet);
            mat2.x = (float)(inverse.mat2.x * invDet); mat2.y = (float)(inverse.mat2.y * invDet); mat2.z = (float)(inverse.mat2.z * invDet);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3x3 TransposeMultiply(Matrix3x3 b)
            => new(
            mat0.x * b.mat0.x + mat1.x * b.mat1.x + mat2.x * b.mat2.x,
            mat0.x * b.mat0.y + mat1.x * b.mat1.y + mat2.x * b.mat2.y,
            mat0.x * b.mat0.z + mat1.x * b.mat1.z + mat2.x * b.mat2.z,
            mat0.y * b.mat0.x + mat1.y * b.mat1.x + mat2.y * b.mat2.x,
            mat0.y * b.mat0.y + mat1.y * b.mat1.y + mat2.y * b.mat2.y,
            mat0.y * b.mat0.z + mat1.y * b.mat1.z + mat2.y * b.mat2.z,
            mat0.z * b.mat0.x + mat1.z * b.mat1.x + mat2.z * b.mat2.x,
            mat0.z * b.mat0.y + mat1.z * b.mat1.y + mat2.z * b.mat2.y,
            mat0.z * b.mat0.z + mat1.z * b.mat1.z + mat2.z * b.mat2.z);

        public Matrix3x3 InertiaTranslate(float mass, in Vector3 centerOfMass, in Vector3 translation)
        {
            Matrix3x3 m = new(); Vector3 newCenter;

            newCenter = centerOfMass + translation;

            m.mat0.x = mass * ((centerOfMass.y * centerOfMass.y + centerOfMass.z * centerOfMass.z) - (newCenter.y * newCenter.y + newCenter.z * newCenter.z));
            m.mat1.y = mass * ((centerOfMass.x * centerOfMass.x + centerOfMass.z * centerOfMass.z) - (newCenter.x * newCenter.x + newCenter.z * newCenter.z));
            m.mat2.z = mass * ((centerOfMass.x * centerOfMass.x + centerOfMass.y * centerOfMass.y) - (newCenter.x * newCenter.x + newCenter.y * newCenter.y));

            m.mat0.y = m.mat1.x = mass * (newCenter.x * newCenter.y - centerOfMass.x * centerOfMass.y);
            m.mat1.z = m.mat2.y = mass * (newCenter.y * newCenter.z - centerOfMass.y * centerOfMass.z);
            m.mat0.z = m.mat2.x = mass * (newCenter.x * newCenter.z - centerOfMass.x * centerOfMass.z);

            return this + m;
        }
        public Matrix3x3 InertiaTranslateSelf(float mass, in Vector3 centerOfMass, in Vector3 translation)
        {
            Matrix3x3 m = new(); Vector3 newCenter;

            newCenter = centerOfMass + translation;

            m.mat0.x = mass * ((centerOfMass.y * centerOfMass.y + centerOfMass.z * centerOfMass.z) - (newCenter.y * newCenter.y + newCenter.z * newCenter.z));
            m.mat1.y = mass * ((centerOfMass.x * centerOfMass.x + centerOfMass.z * centerOfMass.z) - (newCenter.x * newCenter.x + newCenter.z * newCenter.z));
            m.mat2.z = mass * ((centerOfMass.x * centerOfMass.x + centerOfMass.y * centerOfMass.y) - (newCenter.x * newCenter.x + newCenter.y * newCenter.y));

            m.mat0.y = m.mat1.x = mass * (newCenter.x * newCenter.y - centerOfMass.x * centerOfMass.y);
            m.mat1.z = m.mat2.y = mass * (newCenter.y * newCenter.z - centerOfMass.y * centerOfMass.z);
            m.mat0.z = m.mat2.x = mass * (newCenter.x * newCenter.z - centerOfMass.x * centerOfMass.z);

            this += m;

            return this;
        }

        public Matrix3x3 InertiaRotate(in Matrix3x3 rotation)
            // NOTE: the rotation matrix is stored column-major
            => rotation.Transpose() * this * rotation;
        public Matrix3x3 InertiaRotateSelf(in Matrix3x3 rotation)
        {
            // NOTE: the rotation matrix is stored column-major
            this = new(rotation.Transpose() * this * rotation);
            return this;
        }

        public const int Dimension = 9;

        public Angles ToAngles()
        {
            Angles angles; double theta, cp; float sp;

            sp = mat0.z;
            // cap off our sin value so that we don't get any NANs
            if (sp > 1f) sp = 1f;
            else if (sp < -1f) sp = -1f;

            theta = -Math.Asin(sp);
            cp = Math.Cos(theta);

            if (cp > 8192f * MathX.FLT_EPSILON)
            {
                angles.pitch = (float)MathX.RAD2DEG(theta);
                angles.yaw = (float)MathX.RAD2DEG(Math.Atan2(mat0.y, mat0.x));
                angles.roll = (float)MathX.RAD2DEG(Math.Atan2(mat1.z, mat2.z));
            }
            else
            {
                angles.pitch = (float)MathX.RAD2DEG(theta);
                angles.yaw = (float)MathX.RAD2DEG(-Math.Atan2(mat1.x, mat1.y));
                angles.roll = 0f;
            }
            return angles;
        }

        static int[] _ToQuat_next = { 1, 2, 0 };
        public Quat ToQuat()
        {
            Quat q = new(); float trace, s, t; int i, j, k;

            trace = mat0.x + mat1.y + mat2.z;
            if (trace > 0f)
            {
                t = trace + 1f;
                s = MathX.InvSqrt(t) * 0.5f;

                q.w = s * t;
                q.x = (mat2.y - mat1.z) * s;
                q.y = (mat0.z - mat2.x) * s;
                q.z = (mat1.x - mat0.y) * s;
            }
            else
            {
                i = 0;
                if (mat1.y > mat0.x) i = 1;
                if (mat2.z > this[i][i]) i = 2;
                j = _ToQuat_next[i];
                k = _ToQuat_next[j];

                t = this[i][i] - (this[j][j] + this[k][k]) + 1f;
                s = MathX.InvSqrt(t) * 0.5f;

                q[i] = s * t;
                q.w = (this[k][j] - this[j][k]) * s;
                q[j] = (this[j][i] + this[i][j]) * s;
                q[k] = (this[k][i] + this[i][k]) * s;
            }
            return q;
        }

        public CQuat ToCQuat()
        {
            var q = ToQuat();
            return q.w < 0f
                ? new CQuat(-q.x, -q.y, -q.z)
                : new CQuat(q.x, q.y, q.z);
        }

        static int[] _ToRotation_next = { 1, 2, 0 };
        public Rotation ToRotation()
        {
            Rotation r = new(); float trace, s, t; int i, j, k;

            trace = mat0.x + mat1.y + mat2.z;
            if (trace > 0f)
            {
                t = trace + 1f;
                s = MathX.InvSqrt(t) * 0.5f;

                r.angle = s * t;
                r.vec.x = (mat2.y - mat1.z) * s;
                r.vec.y = (mat0.z - mat2.x) * s;
                r.vec.z = (mat1.x - mat0.y) * s;
            }
            else
            {
                i = 0;
                if (mat1.y > mat0.x) i = 1;
                if (mat2.z > this[i][i]) i = 2;
                j = _ToRotation_next[i];
                k = _ToRotation_next[j];

                t = (this[i][i] - (this[j][j] + this[k][k])) + 1f;
                s = MathX.InvSqrt(t) * 0.5f;

                r.vec[i] = s * t;
                r.angle = (this[k][j] - this[j][k]) * s;
                r.vec[j] = (this[j][i] + this[i][j]) * s;
                r.vec[k] = (this[k][i] + this[i][k]) * s;
            }
            r.angle = MathX.ACos(r.angle);
            if (MathX.Fabs(r.angle) < 1e-10f)
            {
                r.vec.Set(0f, 0f, 1f);
                r.angle = 0f;
            }
            else
            {
                r.vec.Normalize();
                r.vec.FixDegenerateNormal();
                r.angle *= 2f * MathX.M_RAD2DEG;
            }

            r.origin.Zero();
            r.axis = this;
            r.axisValid = true;
            return r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4 ToMat4()
            // NOTE: Matrix3x3 is transposed because it is column-major
            => new(
            mat0.x, mat1.x, mat2.x, 0f,
            mat0.y, mat1.y, mat2.y, 0f,
            mat0.z, mat1.z, mat2.z, 0f,
            0f, 0f, 0f, 1f);

        public Vector3 ToAngularVelocity()
        {
            var rotation = ToRotation();
            return rotation.Vec * MathX.DEG2RAD(rotation.Angle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Fixed<T>(FloatPtr<T> callback)
            => mat0.Fixed(callback);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fixed(FloatPtr callback)
            => mat0.Fixed(callback);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(int precision = 2)
        {
            fixed (float* _ = &mat0.x) return FloatArrayToString(_, Dimension, precision);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TransposeMultiply(in Matrix3x3 inv, in Matrix3x3 b, out Matrix3x3 dst)
        {
            dst = new();
            dst.mat0.x = inv.mat0.x * b.mat0.x + inv.mat1.x * b.mat1.x + inv.mat2.x * b.mat2.x;
            dst.mat0.y = inv.mat0.x * b.mat0.y + inv.mat1.x * b.mat1.y + inv.mat2.x * b.mat2.y;
            dst.mat0.z = inv.mat0.x * b.mat0.z + inv.mat1.x * b.mat1.z + inv.mat2.x * b.mat2.z;
            dst.mat1.x = inv.mat0.y * b.mat0.x + inv.mat1.y * b.mat1.x + inv.mat2.y * b.mat2.x;
            dst.mat1.y = inv.mat0.y * b.mat0.y + inv.mat1.y * b.mat1.y + inv.mat2.y * b.mat2.y;
            dst.mat1.z = inv.mat0.y * b.mat0.z + inv.mat1.y * b.mat1.z + inv.mat2.y * b.mat2.z;
            dst.mat2.x = inv.mat0.z * b.mat0.x + inv.mat1.z * b.mat1.x + inv.mat2.z * b.mat2.x;
            dst.mat2.y = inv.mat0.z * b.mat0.y + inv.mat1.z * b.mat1.y + inv.mat2.z * b.mat2.y;
            dst.mat2.z = inv.mat0.z * b.mat0.z + inv.mat1.z * b.mat1.z + inv.mat2.z * b.mat2.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3x3 SkewSymmetric(in Vector3 src)
            => new(0f, -src.z, src.y, src.z, 0f, -src.x, -src.y, src.x, 0f);
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Matrix4x4
    {
        public static Matrix4x4 zero = new(new Vector4(0, 0, 0, 0), new Vector4(0, 0, 0, 0), new Vector4(0, 0, 0, 0), new Vector4(0, 0, 0, 0));
        public static Matrix4x4 identity = new(new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));

        internal Vector4 mat0;
        Vector4 mat1;
        Vector4 mat2;
        Vector4 mat3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4(in Matrix4x4 a)
            => this = a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4(in Vector4 x, in Vector4 y, in Vector4 z, in Vector4 w)
        {
            mat0 = x;
            mat1 = y;
            mat2 = z;
            mat3 = w;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4(float xx, float xy, float xz, float xw,
            float yx, float yy, float yz, float yw,
            float zx, float zy, float zz, float zw,
            float wx, float wy, float wz, float ww)
        {
            mat0.x = xx; mat0.y = xy; mat0.z = xz; mat0.w = xw;
            mat1.x = yx; mat1.y = yy; mat1.z = yz; mat1.w = yw;
            mat2.x = zx; mat2.y = zy; mat2.z = zz; mat2.w = zw;
            mat3.x = wx; mat3.y = wy; mat3.z = wz; mat3.w = ww;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4(in Matrix3x3 rotation, in Vector3 translation)
        {
            // NOTE: Matrix3x3 is transposed because it is column-major
            mat0.x = rotation.mat0.x;
            mat0.y = rotation.mat1.x;
            mat0.z = rotation.mat2.x;
            mat0.w = translation.x;
            mat1.x = rotation.mat0.y;
            mat1.y = rotation.mat1.y;
            mat1.z = rotation.mat2.y;
            mat1.w = translation.y;
            mat2.x = rotation.mat0.z;
            mat2.y = rotation.mat1.z;
            mat2.z = rotation.mat2.z;
            mat2.w = translation.z;
            mat3.x = 0f;
            mat3.y = 0f;
            mat3.z = 0f;
            mat3.w = 1f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4(float[] src)
        {
            this = default;
            fixed (void* mat_ = &mat0, src_ = src) Unsafe.CopyBlock(mat_, src_, 4U * 4U * sizeof(float));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4(float[,] src)
        {
            this = default;
            fixed (void* mat_ = &mat0, src_ = &src[0, 0]) Unsafe.CopyBlock(mat_, src_, 4U * 4U * sizeof(float));
        }

        public ref Vector4 this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { fixed (Vector4* mat_ = &mat0) return ref mat_[index]; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 operator *(in Matrix4x4 _, float a)
            => new(
            _.mat0.x * a, _.mat0.y * a, _.mat0.z * a, _.mat0.w * a,
            _.mat1.x * a, _.mat1.y * a, _.mat1.z * a, _.mat1.w * a,
            _.mat2.x * a, _.mat2.y * a, _.mat2.z * a, _.mat2.w * a,
            _.mat3.x * a, _.mat3.y * a, _.mat3.z * a, _.mat3.w * a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(in Matrix4x4 _, in Vector4 vec)
            => new(
            _.mat0.x * vec.x + _.mat0.y * vec.y + _.mat0.z * vec.z + _.mat0.w * vec.w,
            _.mat1.x * vec.x + _.mat1.y * vec.y + _.mat1.z * vec.z + _.mat1.w * vec.w,
            _.mat2.x * vec.x + _.mat2.y * vec.y + _.mat2.z * vec.z + _.mat2.w * vec.w,
            _.mat3.x * vec.x + _.mat3.y * vec.y + _.mat3.z * vec.z + _.mat3.w * vec.w);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(in Matrix4x4 _, in Vector3 vec)
        {
            var s = _.mat3.x * vec.x + _.mat3.y * vec.y + _.mat3.z * vec.z + _.mat3.w;
            if (s == 0f) return new(0f, 0f, 0f);
            if (s == 1f)
                return new(
                _.mat0.x * vec.x + _.mat0.y * vec.y + _.mat0.z * vec.z + _.mat0.w,
                _.mat1.x * vec.x + _.mat1.y * vec.y + _.mat1.z * vec.z + _.mat1.w,
                _.mat2.x * vec.x + _.mat2.y * vec.y + _.mat2.z * vec.z + _.mat2.w);
            else
            {
                var invS = 1f / s;
                return new(
                (_.mat0.x * vec.x + _.mat0.y * vec.y + _.mat0.z * vec.z + _.mat0.w) * invS,
                (_.mat1.x * vec.x + _.mat1.y * vec.y + _.mat1.z * vec.z + _.mat1.w) * invS,
                (_.mat2.x * vec.x + _.mat2.y * vec.y + _.mat2.z * vec.z + _.mat2.w) * invS);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 operator *(in Matrix4x4 _, in Matrix4x4 a)
        {
            Matrix4x4 dst = default;
            void* dst_ = &dst.mat0;
            fixed (void* __ = &_.mat0, a_ = &a.mat0)
            {
                var m1Ptr = (float*)__;
                var m2Ptr = (float*)a_;
                var dstPtr = (float*)dst_;
                for (var i = 0; i < 4; i++)
                {
                    for (var j = 0; j < 4; j++)
                    {
                        *dstPtr = m1Ptr[0] * m2Ptr[0 * 4 + j]
                                + m1Ptr[1] * m2Ptr[1 * 4 + j]
                                + m1Ptr[2] * m2Ptr[2 * 4 + j]
                                + m1Ptr[3] * m2Ptr[3 * 4 + j];
                        dstPtr++;
                    }
                    m1Ptr += 4;
                }
                return dst;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 operator +(in Matrix4x4 _, in Matrix4x4 a)
            => new(
            _.mat0.x + a.mat0.x, _.mat0.y + a.mat0.y, _.mat0.z + a.mat0.z, _.mat0.w + a.mat0.w,
            _.mat1.x + a.mat1.x, _.mat1.y + a.mat1.y, _.mat1.z + a.mat1.z, _.mat1.w + a.mat1.w,
            _.mat2.x + a.mat2.x, _.mat2.y + a.mat2.y, _.mat2.z + a.mat2.z, _.mat2.w + a.mat2.w,
            _.mat3.x + a.mat3.x, _.mat3.y + a.mat3.y, _.mat3.z + a.mat3.z, _.mat3.w + a.mat3.w);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 operator -(in Matrix4x4 _, in Matrix4x4 a)
            => new(
            _.mat0.x - a.mat0.x, _.mat0.y - a.mat0.y, _.mat0.z - a.mat0.z, _.mat0.w - a.mat0.w,
            _.mat1.x - a.mat1.x, _.mat1.y - a.mat1.y, _.mat1.z - a.mat1.z, _.mat1.w - a.mat1.w,
            _.mat2.x - a.mat2.x, _.mat2.y - a.mat2.y, _.mat2.z - a.mat2.z, _.mat2.w - a.mat2.w,
            _.mat3.x - a.mat3.x, _.mat3.y - a.mat3.y, _.mat3.z - a.mat3.z, _.mat3.w - a.mat3.w);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 operator *(float a, in Matrix4x4 mat)
            => mat * a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(in Vector4 vec, in Matrix4x4 mat)
            => mat * vec;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(in Vector3 vec, in Matrix4x4 mat)
            => mat * vec;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Matrix4x4 a)                       // exact compare, no epsilon
        {
            fixed (void* mat_ = &mat0, a_ = &a.mat0)
            {
                var ptr1 = (float*)mat_;
                var ptr2 = (float*)a_;
                for (var i = 0; i < 4 * 4; i++) if (ptr1[i] != ptr2[i]) return false;
                return true;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Matrix4x4 a, float epsilon)  // compare with epsilon
        {
            fixed (void* mat_ = &mat0, a_ = &a.mat0)
            {
                var ptr1 = (float*)mat_;
                var ptr2 = (float*)a_;
                for (var i = 0; i < 4 * 4; i++) if (MathX.Fabs(ptr1[i] - ptr2[i]) > epsilon) return false;
                return true;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Matrix4x4 _, in Matrix4x4 a)                   // exact compare, no epsilon
            => _.Compare(a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Matrix4x4 _, in Matrix4x4 a)                   // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Matrix4x4 q && Compare(q);
        public override int GetHashCode()
            => mat0.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()
        {
            fixed (void* mat_ = &mat0) Unsafe.InitBlock(mat_, 0, 4U * (uint)sizeof(Vector4));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Identity()
            => this = new(identity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsIdentity(float epsilon = MatrixX.EPSILON)
            => Compare(identity, epsilon);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSymmetric(float epsilon = MatrixX.EPSILON)
        {
            for (var i = 1; i < 4; i++) for (var j = 0; j < i; j++) if (MathX.Fabs(this[i][j] - this[j][i]) > epsilon) return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDiagonal(float epsilon = MatrixX.EPSILON)
        {
            for (var i = 0; i < 4; i++) for (var j = 0; j < 4; j++) if (i != j && MathX.Fabs(this[i][j]) > epsilon) return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsRotated()
            => mat0.y != 0 || mat0.z != 0 ||
               mat1.x != 0 || mat1.z != 0 ||
               mat2.x != 0 || mat2.y != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProjectVector(in Vector4 src, out Vector4 dst)
        {
            dst.x = src * mat0;
            dst.y = src * mat1;
            dst.z = src * mat2;
            dst.w = src * mat3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnprojectVector(in Vector4 src, out Vector4 dst)
            => dst = mat0 * src.x + mat1 * src.y + mat2 * src.z + mat3 * src.w;

        public float Trace
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => mat0.x + mat1.y + mat2.z + mat3.w;
        }

        public float Determinant()
        {
            // 2x2 sub-determinants
            var det2_01_01 = mat0.x * mat1.y - mat0.y * mat1.x;
            var det2_01_02 = mat0.x * mat1.z - mat0.z * mat1.x;
            var det2_01_03 = mat0.x * mat1.w - mat0.w * mat1.x;
            var det2_01_12 = mat0.y * mat1.z - mat0.z * mat1.y;
            var det2_01_13 = mat0.y * mat1.w - mat0.w * mat1.y;
            var det2_01_23 = mat0.z * mat1.w - mat0.w * mat1.z;

            // 3x3 sub-determinants
            var det3_201_012 = mat2.x * det2_01_12 - mat2.y * det2_01_02 + mat2.z * det2_01_01;
            var det3_201_013 = mat2.x * det2_01_13 - mat2.y * det2_01_03 + mat2.w * det2_01_01;
            var det3_201_023 = mat2.x * det2_01_23 - mat2.z * det2_01_03 + mat2.w * det2_01_02;
            var det3_201_123 = mat2.y * det2_01_23 - mat2.z * det2_01_13 + mat2.w * det2_01_12;

            return -det3_201_123 * mat3.x + det3_201_023 * mat3.y - det3_201_013 * mat3.z + det3_201_012 * mat3.w;
        }

        public Matrix4x4 Transpose()   // returns transpose
        {
            var transpose = new Matrix4x4();
            for (var i = 0; i < 4; i++) for (var j = 0; j < 4; j++) transpose[i][j] = this[j][i];
            return transpose;
        }
        public Matrix4x4 TransposeSelf()
        {
            for (var i = 0; i < 4; i++)
                for (var j = i + 1; j < 4; j++)
                {
                    var temp = this[i][j];
                    this[i][j] = this[j][i];
                    this[j][i] = temp;
                }
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4 Inverse()     // returns the inverse ( m * m.Inverse() = identity )
        {
            Matrix4x4 invMat = new(this);
            var r = invMat.InverseSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseSelf()        // returns false if determinant is zero
        {
            // 84+4+16 = 104 multiplications
            //			   1 division
            double det, invDet;

            // 2x2 sub-determinants required to calculate 4x4 determinant
            var det2_01_01 = mat0.x * mat1.y - mat0.y * mat1.x;
            var det2_01_02 = mat0.x * mat1.z - mat0.z * mat1.x;
            var det2_01_03 = mat0.x * mat1.w - mat0.w * mat1.x;
            var det2_01_12 = mat0.y * mat1.z - mat0.z * mat1.y;
            var det2_01_13 = mat0.y * mat1.w - mat0.w * mat1.y;
            var det2_01_23 = mat0.z * mat1.w - mat0.w * mat1.z;

            // 3x3 sub-determinants required to calculate 4x4 determinant
            var det3_201_012 = mat2.x * det2_01_12 - mat2.y * det2_01_02 + mat2.z * det2_01_01;
            var det3_201_013 = mat2.x * det2_01_13 - mat2.y * det2_01_03 + mat2.w * det2_01_01;
            var det3_201_023 = mat2.x * det2_01_23 - mat2.z * det2_01_03 + mat2.w * det2_01_02;
            var det3_201_123 = mat2.y * det2_01_23 - mat2.z * det2_01_13 + mat2.w * det2_01_12;

            det = -det3_201_123 * mat3.x + det3_201_023 * mat3.y - det3_201_013 * mat3.z + det3_201_012 * mat3.w;

            if (MathX.Fabs(det) < MatrixX.INVERSE_EPSILON) return false;

            invDet = 1f / det;

            // remaining 2x2 sub-determinants
            var det2_03_01 = mat0.x * mat3.y - mat0.y * mat3.x;
            var det2_03_02 = mat0.x * mat3.z - mat0.z * mat3.x;
            var det2_03_03 = mat0.x * mat3.w - mat0.w * mat3.x;
            var det2_03_12 = mat0.y * mat3.z - mat0.y * mat3.y;
            var det2_03_13 = mat0.y * mat3.z - mat0.z * mat3.y;
            var det2_03_23 = mat0.z * mat3.z - mat0.z * mat3.z;

            var det2_13_01 = mat1.x * mat3.y - mat1.y * mat3.x;
            var det2_13_02 = mat1.x * mat3.z - mat1.z * mat3.x;
            var det2_13_03 = mat1.x * mat3.w - mat1.w * mat3.x;
            var det2_13_12 = mat1.y * mat3.z - mat1.z * mat3.y;
            var det2_13_13 = mat1.y * mat3.w - mat1.w * mat3.y;
            var det2_13_23 = mat1.z * mat3.w - mat1.w * mat3.z;

            // remaining 3x3 sub-determinants
            var det3_203_012 = mat2.x * det2_03_12 - mat2.y * det2_03_02 + mat2.z * det2_03_01;
            var det3_203_013 = mat2.x * det2_03_13 - mat2.y * det2_03_03 + mat2.w * det2_03_01;
            var det3_203_023 = mat2.x * det2_03_23 - mat2.z * det2_03_03 + mat2.w * det2_03_02;
            var det3_203_123 = mat2.y * det2_03_23 - mat2.z * det2_03_13 + mat2.w * det2_03_12;

            var det3_213_012 = mat2.x * det2_13_12 - mat2.y * det2_13_02 + mat2.z * det2_13_01;
            var det3_213_013 = mat2.x * det2_13_13 - mat2.y * det2_13_03 + mat2.w * det2_13_01;
            var det3_213_023 = mat2.x * det2_13_23 - mat2.z * det2_13_03 + mat2.w * det2_13_02;
            var det3_213_123 = mat2.y * det2_13_23 - mat2.z * det2_13_13 + mat2.w * det2_13_12;

            var det3_301_012 = mat3.x * det2_01_12 - mat3.y * det2_01_02 + mat3.z * det2_01_01;
            var det3_301_013 = mat3.x * det2_01_13 - mat3.y * det2_01_03 + mat3.w * det2_01_01;
            var det3_301_023 = mat3.x * det2_01_23 - mat3.z * det2_01_03 + mat3.w * det2_01_02;
            var det3_301_123 = mat3.y * det2_01_23 - mat3.z * det2_01_13 + mat3.w * det2_01_12;

            mat0.x = (float)(-det3_213_123 * invDet); mat1.x = (float)(+det3_213_023 * invDet); mat2.x = (float)(-det3_213_013 * invDet); mat3.x = (float)(+det3_213_012 * invDet);
            mat0.y = (float)(+det3_203_123 * invDet); mat1.y = (float)(-det3_203_023 * invDet); mat2.y = (float)(+det3_203_013 * invDet); mat3.y = (float)(-det3_203_012 * invDet);
            mat0.z = (float)(+det3_301_123 * invDet); mat1.z = (float)(-det3_301_023 * invDet); mat2.z = (float)(+det3_301_013 * invDet); mat3.z = (float)(-det3_301_012 * invDet);
            mat0.w = (float)(-det3_201_123 * invDet); mat1.w = (float)(+det3_201_023 * invDet); mat2.w = (float)(-det3_201_013 * invDet); mat3.w = (float)(+det3_201_012 * invDet);

            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4 InverseFast() // returns the inverse ( m * m.Inverse() = identity )
        {
            Matrix4x4 invMat = new(this);
            var r = invMat.InverseFastSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseFastSelf()    // returns false if determinant is zero
        {
#if false
            // 84+4+16 = 104 multiplications
            //			   1 division
            double det, invDet;

            // 2x2 sub-determinants required to calculate 4x4 determinant
            var det2_01_01 = mat0.x * mat1.y - mat0.y * mat1.x;
            var det2_01_02 = mat0.x * mat1.z - mat0.z * mat1.x;
            var det2_01_03 = mat0.x * mat1.w - mat0.w * mat1.x;
            var det2_01_12 = mat0.y * mat1.z - mat0.z * mat1.y;
            var det2_01_13 = mat0.y * mat1.w - mat0.w * mat1.y;
            var det2_01_23 = mat0.z * mat1.w - mat0.w * mat1.z;

            // 3x3 sub-determinants required to calculate 4x4 determinant
            var det3_201_012 = mat2.x * det2_01_12 - mat2.y * det2_01_02 + mat2.z * det2_01_01;
            var det3_201_013 = mat2.x * det2_01_13 - mat2.y * det2_01_03 + mat2.w * det2_01_01;
            var det3_201_023 = mat2.x * det2_01_23 - mat2.z * det2_01_03 + mat2.w * det2_01_02;
            var det3_201_123 = mat2.y * det2_01_23 - mat2.z * det2_01_13 + mat2.w * det2_01_12;

             det = -det3_201_123 * mat3.x + det3_201_023 * mat3.y - det3_201_013 * mat3.z + det3_201_012 * mat3.w;
            if (MathX.Fabs(det) < MatrixX.INVERSE_EPSILON) return false;

            invDet = 1f / det;

            // remaining 2x2 sub-determinants
            var det2_03_01 = mat0.x * mat3.y - mat0.y * mat3.x;
            var det2_03_02 = mat0.x * mat3.z - mat0.z * mat3.x;
            var det2_03_03 = mat0.x * mat3.w - mat0.w * mat3.x;
            var det2_03_12 = mat0.y * mat3.z - mat0.z * mat3.y;
            var det2_03_13 = mat0.y * mat3.w - mat0.w * mat3.y;
            var det2_03_23 = mat0.z * mat3.w - mat0.w * mat3.z;

            var det2_13_01 = mat1.x * mat3.y - mat1.y * mat3.x;
            var det2_13_02 = mat1.x * mat3.z - mat1.z * mat3.x;
            var det2_13_03 = mat1.x * mat3.w - mat1.w * mat3.x;
            var det2_13_12 = mat1.y * mat3.z - mat1.z * mat3.y;
            var det2_13_13 = mat1.y * mat3.w - mat1.w * mat3.y;
            var det2_13_23 = mat1.z * mat3.w - mat1.w * mat3.z;

            // remaining 3x3 sub-determinants
            var det3_203_012 = mat2.x * det2_03_12 - mat2.y * det2_03_02 + mat2.y * det2_03_01;
            var det3_203_013 = mat2.x * det2_03_13 - mat2.y * det2_03_03 + mat2.z * det2_03_01;
            var det3_203_023 = mat2.x * det2_03_23 - mat2.z * det2_03_03 + mat2.z * det2_03_02;
            var det3_203_123 = mat2.y * det2_03_23 - mat2.z * det2_03_13 + mat2.z * det2_03_12;

            var det3_213_012 = mat2.x * det2_13_12 - mat2.y * det2_13_02 + mat2.y * det2_13_01;
            var det3_213_013 = mat2.x * det2_13_13 - mat2.y * det2_13_03 + mat2.z * det2_13_01;
            var det3_213_023 = mat2.x * det2_13_23 - mat2.z * det2_13_03 + mat2.z * det2_13_02;
            var det3_213_123 = mat2.y * det2_13_23 - mat2.z * det2_13_13 + mat2.z * det2_13_12;

            var det3_301_012 = mat3.x * det2_01_12 - mat3.y * det2_01_02 + mat3.y * det2_01_01;
            var det3_301_013 = mat3.x * det2_01_13 - mat3.y * det2_01_03 + mat3.z * det2_01_01;
            var det3_301_023 = mat3.x * det2_01_23 - mat3.z * det2_01_03 + mat3.z * det2_01_02;
            var det3_301_123 = mat3.y * det2_01_23 - mat3.z * det2_01_13 + mat3.z * det2_01_12;

            mat0.x = -det3_213_123 * invDet; mat1.x = +det3_213_023 * invDet; mat2.x = -det3_213_013 * invDet; mat3.x = +det3_213_012 * invDet;
            mat0.y = +det3_203_123 * invDet; mat1.y = -det3_203_023 * invDet; mat2.y = +det3_203_013 * invDet; mat3.y = -det3_203_012 * invDet;
            mat0.z = +det3_301_123 * invDet; mat1.z = -det3_301_023 * invDet; mat2.z = +det3_301_013 * invDet; mat3.z = -det3_301_012 * invDet;
            mat0.w = -det3_201_123 * invDet; mat1.w = +det3_201_023 * invDet; mat2.w = -det3_201_013 * invDet; mat3.w = +det3_201_012 * invDet;
#else
            //	6*8+2*6 = 60 multiplications
            //		2*1 =  2 divisions
            Matrix2x2 r0 = new(), r1 = new(), r2 = new(), r3 = new(); float a, det, invDet;

            fixed (float* mat_ = &mat0.x)
            {
                // r0 = m0.Inverse();
                det = mat_[0 * 4 + 0] * mat_[1 * 4 + 1] - mat_[0 * 4 + 1] * mat_[1 * 4 + 0];

                if (MathX.Fabs(det) < MatrixX.INVERSE_EPSILON) return false;

                invDet = 1f / det;

                r0.mat0.x = mat_[1 * 4 + 1] * invDet;
                r0.mat0.y = -mat_[0 * 4 + 1] * invDet;
                r0.mat1.x = -mat_[1 * 4 + 0] * invDet;
                r0.mat1.y = mat_[0 * 4 + 0] * invDet;

                // r1 = r0 * m1;
                r1.mat0.x = r0.mat0.x * mat_[0 * 4 + 2] + r0.mat0.y * mat_[1 * 4 + 2];
                r1.mat0.y = r0.mat0.x * mat_[0 * 4 + 3] + r0.mat0.y * mat_[1 * 4 + 3];
                r1.mat1.x = r0.mat1.x * mat_[0 * 4 + 2] + r0.mat1.y * mat_[1 * 4 + 2];
                r1.mat1.y = r0.mat1.x * mat_[0 * 4 + 3] + r0.mat1.y * mat_[1 * 4 + 3];

                // r2 = m2 * r1;
                r2.mat0.x = mat_[2 * 4 + 0] * r1.mat0.x + mat_[2 * 4 + 1] * r1.mat1.x;
                r2.mat0.y = mat_[2 * 4 + 0] * r1.mat0.y + mat_[2 * 4 + 1] * r1.mat1.y;
                r2.mat1.x = mat_[3 * 4 + 0] * r1.mat0.x + mat_[3 * 4 + 1] * r1.mat1.x;
                r2.mat1.y = mat_[3 * 4 + 0] * r1.mat0.y + mat_[3 * 4 + 1] * r1.mat1.y;

                // r3 = r2 - m3;
                r3.mat0.x = r2.mat0.x - mat_[2 * 4 + 2];
                r3.mat0.y = r2.mat0.y - mat_[2 * 4 + 3];
                r3.mat1.x = r2.mat1.x - mat_[3 * 4 + 2];
                r3.mat1.y = r2.mat1.y - mat_[3 * 4 + 3];

                // r3.InverseSelf();
                det = r3.mat0.x * r3.mat1.y - r3.mat0.y * r3.mat1.x;

                if (MathX.Fabs(det) < MatrixX.INVERSE_EPSILON) return false;

                invDet = 1f / det;

                a = r3.mat0.x;
                r3.mat0.x = r3.mat1.y * invDet;
                r3.mat0.y = -r3.mat0.y * invDet;
                r3.mat1.x = -r3.mat1.x * invDet;
                r3.mat1.y = a * invDet;

                // r2 = m2 * r0;
                r2.mat0.x = mat_[2 * 4 + 0] * r0.mat0.x + mat_[2 * 4 + 1] * r0.mat1.x;
                r2.mat0.y = mat_[2 * 4 + 0] * r0.mat0.y + mat_[2 * 4 + 1] * r0.mat1.y;
                r2.mat1.x = mat_[3 * 4 + 0] * r0.mat0.x + mat_[3 * 4 + 1] * r0.mat1.x;
                r2.mat1.y = mat_[3 * 4 + 0] * r0.mat0.y + mat_[3 * 4 + 1] * r0.mat1.y;

                // m2 = r3 * r2;
                mat_[2 * 4 + 0] = r3.mat0.x * r2.mat0.x + r3.mat0.y * r2.mat1.x;
                mat_[2 * 4 + 1] = r3.mat0.x * r2.mat0.y + r3.mat0.y * r2.mat1.y;
                mat_[3 * 4 + 0] = r3.mat1.x * r2.mat0.x + r3.mat1.y * r2.mat1.x;
                mat_[3 * 4 + 1] = r3.mat1.x * r2.mat0.y + r3.mat1.y * r2.mat1.y;

                // m0 = r0 - r1 * m2;
                mat_[0 * 4 + 0] = r0.mat0.x - r1.mat0.x * mat_[2 * 4 + 0] - r1.mat0.y * mat_[3 * 4 + 0];
                mat_[0 * 4 + 1] = r0.mat0.y - r1.mat0.x * mat_[2 * 4 + 1] - r1.mat0.y * mat_[3 * 4 + 1];
                mat_[1 * 4 + 0] = r0.mat1.x - r1.mat1.x * mat_[2 * 4 + 0] - r1.mat1.y * mat_[3 * 4 + 0];
                mat_[1 * 4 + 1] = r0.mat1.y - r1.mat1.x * mat_[2 * 4 + 1] - r1.mat1.y * mat_[3 * 4 + 1];

                // m1 = r1 * r3;
                mat_[0 * 4 + 2] = r1.mat0.x * r3.mat0.x + r1.mat0.y * r3.mat1.x;
                mat_[0 * 4 + 3] = r1.mat0.x * r3.mat0.y + r1.mat0.y * r3.mat1.y;
                mat_[1 * 4 + 2] = r1.mat1.x * r3.mat0.x + r1.mat1.y * r3.mat1.x;
                mat_[1 * 4 + 3] = r1.mat1.x * r3.mat0.y + r1.mat1.y * r3.mat1.y;

                // m3 = -r3;
                mat_[2 * 4 + 2] = -r3.mat0.x;
                mat_[2 * 4 + 3] = -r3.mat0.y;
                mat_[3 * 4 + 2] = -r3.mat1.x;
                mat_[3 * 4 + 3] = -r3.mat1.y;

                return true;
            }
#endif
        }

        public Matrix4x4 TransposeMultiply(in Matrix4x4 b)
            => throw new NotSupportedException();

        public const int Dimension = 16;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Fixed<T>(FloatPtr<T> callback)
            => mat0.Fixed(callback);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fixed(FloatPtr callback)
            => mat0.Fixed(callback);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(int precision = 2)
        {
            fixed (float* _ = &mat0.x) return FloatArrayToString(_, Dimension, precision);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Matrix5x5
    {
        public static Matrix5x5 zero = new(new Vector5(0, 0, 0, 0, 0), new Vector5(0, 0, 0, 0, 0), new Vector5(0, 0, 0, 0, 0), new Vector5(0, 0, 0, 0, 0), new Vector5(0, 0, 0, 0, 0));
        public static Matrix5x5 identity = new(new Vector5(1, 0, 0, 0, 0), new Vector5(0, 1, 0, 0, 0), new Vector5(0, 0, 1, 0, 0), new Vector5(0, 0, 0, 1, 0), new Vector5(0, 0, 0, 0, 1));

        internal Vector5 mat0;
        internal Vector5 mat1;
        internal Vector5 mat2;
        internal Vector5 mat3;
        internal Vector5 mat4;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix5x5(in Matrix5x5 a)
            => this = a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix5x5(in Vector5 v0, in Vector5 v1, in Vector5 v2, in Vector5 v3, in Vector5 v4)
        {
            mat0 = v0;
            mat1 = v1;
            mat2 = v2;
            mat3 = v3;
            mat4 = v4;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix5x5(float[] src)
        {
            this = default;
            fixed (void* mat_ = &mat0, src_ = src) Unsafe.CopyBlock(mat_, src_, 5U * 5U * sizeof(float));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix5x5(float[,] src)
        {
            this = default;
            fixed (void* mat_ = &mat0, src_ = &src[0, 0]) Unsafe.CopyBlock(mat_, src_, 5U * 5U * sizeof(float));
        }

        public ref Vector5 this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { fixed (Vector5* mat_ = &mat0) return ref mat_[index]; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix5x5 operator *(in Matrix5x5 _, float a)
            => new(
            new Vector5(_.mat0.x * a, _.mat0.y * a, _.mat0.z * a, _.mat0.s * a, _.mat0.t * a),
            new Vector5(_.mat1.x * a, _.mat1.y * a, _.mat1.z * a, _.mat1.s * a, _.mat1.t * a),
            new Vector5(_.mat2.x * a, _.mat2.y * a, _.mat2.z * a, _.mat2.s * a, _.mat2.t * a),
            new Vector5(_.mat3.x * a, _.mat3.y * a, _.mat3.z * a, _.mat3.s * a, _.mat3.t * a),
            new Vector5(_.mat4.x * a, _.mat4.y * a, _.mat4.z * a, _.mat4.s * a, _.mat4.t * a));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector5 operator *(in Matrix5x5 _, in Vector5 vec)
            => new(
            _.mat0.x * vec.x + _.mat0.y * vec.y + _.mat0.z * vec.z + _.mat0.s * vec.s + _.mat0.t * vec.t,
            _.mat1.x * vec.x + _.mat1.y * vec.y + _.mat1.z * vec.z + _.mat1.s * vec.s + _.mat1.t * vec.t,
            _.mat2.x * vec.x + _.mat2.y * vec.y + _.mat2.z * vec.z + _.mat2.s * vec.s + _.mat2.t * vec.t,
            _.mat3.x * vec.x + _.mat3.y * vec.y + _.mat3.z * vec.z + _.mat3.s * vec.s + _.mat3.t * vec.t,
            _.mat4.x * vec.x + _.mat4.y * vec.y + _.mat4.z * vec.z + _.mat4.s * vec.s + _.mat4.t * vec.t);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix5x5 operator *(in Matrix5x5 _, in Matrix5x5 a)
        {
            Matrix5x5 dst = default;
            void* dst_ = &dst.mat0;
            fixed (void* __ = &_.mat0, a_ = &a.mat0)
            {
                var m1Ptr = (float*)__;
                var m2Ptr = (float*)a_;
                var dstPtr = (float*)dst_;
                for (var i = 0; i < 5; i++)
                {
                    for (var j = 0; j < 5; j++)
                    {
                        *dstPtr = m1Ptr[0] * m2Ptr[0 * 5 + j]
                            + m1Ptr[1] * m2Ptr[1 * 5 + j]
                            + m1Ptr[2] * m2Ptr[2 * 5 + j]
                            + m1Ptr[3] * m2Ptr[3 * 5 + j]
                            + m1Ptr[4] * m2Ptr[4 * 5 + j];
                        dstPtr++;
                    }
                    m1Ptr += 5;
                }
                return dst;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix5x5 operator +(in Matrix5x5 _, in Matrix5x5 a)
            => new(
            new Vector5(_.mat0.x + a.mat0.x, _.mat0.y + a.mat0.y, _.mat0.z + a.mat0.z, _.mat0.s + a.mat0.s, _.mat0.t + a.mat0.t),
            new Vector5(_.mat1.x + a.mat1.x, _.mat1.y + a.mat1.y, _.mat1.z + a.mat1.z, _.mat1.s + a.mat1.s, _.mat1.t + a.mat1.t),
            new Vector5(_.mat2.x + a.mat2.x, _.mat2.y + a.mat2.y, _.mat2.z + a.mat2.z, _.mat2.s + a.mat2.s, _.mat2.t + a.mat2.t),
            new Vector5(_.mat3.x + a.mat3.x, _.mat3.y + a.mat3.y, _.mat3.z + a.mat3.z, _.mat3.s + a.mat3.s, _.mat3.t + a.mat3.t),
            new Vector5(_.mat4.x + a.mat4.x, _.mat4.y + a.mat4.y, _.mat4.z + a.mat4.z, _.mat4.s + a.mat4.s, _.mat4.t + a.mat4.t));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix5x5 operator -(in Matrix5x5 _, in Matrix5x5 a)
            => new(
            new Vector5(_.mat0.x - a.mat0.x, _.mat0.y - a.mat0.y, _.mat0.z - a.mat0.z, _.mat0.s - a.mat0.s, _.mat0.t - a.mat0.t),
            new Vector5(_.mat1.x - a.mat1.x, _.mat1.y - a.mat1.y, _.mat1.z - a.mat1.z, _.mat1.s - a.mat1.s, _.mat1.t - a.mat1.t),
            new Vector5(_.mat2.x - a.mat2.x, _.mat2.y - a.mat2.y, _.mat2.z - a.mat2.z, _.mat2.s - a.mat2.s, _.mat2.t - a.mat2.t),
            new Vector5(_.mat3.x - a.mat3.x, _.mat3.y - a.mat3.y, _.mat3.z - a.mat3.z, _.mat3.s - a.mat3.s, _.mat3.t - a.mat3.t),
            new Vector5(_.mat4.x - a.mat4.x, _.mat4.y - a.mat4.y, _.mat4.z - a.mat4.z, _.mat4.s - a.mat4.s, _.mat4.t - a.mat4.t));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix5x5 operator *(float a, in Matrix5x5 mat)
            => mat * a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector5 operator *(in Vector5 vec, in Matrix5x5 mat)
            => mat * vec;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Matrix5x5 a)                       // exact compare, no epsilon
        {
            fixed (void* mat_ = &mat0, a_ = &a.mat0)
            {
                var ptr1 = (float*)mat_;
                var ptr2 = (float*)a_;
                for (var i = 0; i < 5 * 5; i++) if (ptr1[i] != ptr2[i]) return false;
                return true;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Matrix5x5 a, float epsilon)  // compare with epsilon
        {
            fixed (void* mat_ = &mat0, a_ = &a.mat0)
            {
                var ptr1 = (float*)mat_;
                var ptr2 = (float*)a_;
                for (var i = 0; i < 5 * 5; i++) if (MathX.Fabs(ptr1[i] - ptr2[i]) > epsilon) return false;
                return true;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Matrix5x5 _, in Matrix5x5 a)                   // exact compare, no epsilon
            => _.Compare(a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Matrix5x5 _, in Matrix5x5 a)                   // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Matrix5x5 q && Compare(q);
        public override int GetHashCode()
            => mat0.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()
        {
            fixed (void* mat_ = &mat0) Unsafe.InitBlock(mat_, 0, 5U * (uint)sizeof(Vector5));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Identity()
            => this = new(identity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsIdentity(float epsilon = MatrixX.EPSILON)
            => Compare(identity, epsilon);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSymmetric(float epsilon = MatrixX.EPSILON)
        {
            for (var i = 1; i < 5; i++) for (var j = 0; j < i; j++) if (MathX.Fabs(this[i][j] - this[j][i]) > epsilon) return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDiagonal(float epsilon = MatrixX.EPSILON)
        {
            for (var i = 0; i < 5; i++) for (var j = 0; j < 5; j++) if (i != j && MathX.Fabs(this[i][j]) > epsilon) return false;
            return true;
        }

        public float Trace
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => mat0.x + mat1.y + mat2.z + mat3.s + mat4.t;
        }

        public float Determinant()
        {
            // 2x2 sub-determinants required to calculate 5x5 determinant
            var det2_34_01 = mat3.x * mat4.y - mat3.y * mat4.x;
            var det2_34_02 = mat3.x * mat4.z - mat3.z * mat4.x;
            var det2_34_03 = mat3.x * mat4.s - mat3.s * mat4.x;
            var det2_34_04 = mat3.x * mat4.t - mat3.t * mat4.x;
            var det2_34_12 = mat3.y * mat4.z - mat3.z * mat4.y;
            var det2_34_13 = mat3.y * mat4.s - mat3.s * mat4.y;
            var det2_34_14 = mat3.y * mat4.t - mat3.t * mat4.y;
            var det2_34_23 = mat3.z * mat4.s - mat3.s * mat4.z;
            var det2_34_24 = mat3.z * mat4.t - mat3.t * mat4.z;
            var det2_34_34 = mat3.s * mat4.t - mat3.t * mat4.s;

            // 3x3 sub-determinants required to calculate 5x5 determinant
            var det3_234_012 = mat2.x * det2_34_12 - mat2.y * det2_34_02 + mat2.z * det2_34_01;
            var det3_234_013 = mat2.x * det2_34_13 - mat2.y * det2_34_03 + mat2.s * det2_34_01;
            var det3_234_014 = mat2.x * det2_34_14 - mat2.y * det2_34_04 + mat2.t * det2_34_01;
            var det3_234_023 = mat2.x * det2_34_23 - mat2.z * det2_34_03 + mat2.s * det2_34_02;
            var det3_234_024 = mat2.x * det2_34_24 - mat2.z * det2_34_04 + mat2.t * det2_34_02;
            var det3_234_034 = mat2.x * det2_34_34 - mat2.s * det2_34_04 + mat2.t * det2_34_03;
            var det3_234_123 = mat2.y * det2_34_23 - mat2.z * det2_34_13 + mat2.s * det2_34_12;
            var det3_234_124 = mat2.y * det2_34_24 - mat2.z * det2_34_14 + mat2.t * det2_34_12;
            var det3_234_134 = mat2.y * det2_34_34 - mat2.s * det2_34_14 + mat2.t * det2_34_13;
            var det3_234_234 = mat2.z * det2_34_34 - mat2.s * det2_34_24 + mat2.t * det2_34_23;

            // 4x4 sub-determinants required to calculate 5x5 determinant
            var det4_1234_0123 = mat1.x * det3_234_123 - mat1.y * det3_234_023 + mat1.z * det3_234_013 - mat1.s * det3_234_012;
            var det4_1234_0124 = mat1.x * det3_234_124 - mat1.y * det3_234_024 + mat1.z * det3_234_014 - mat1.t * det3_234_012;
            var det4_1234_0134 = mat1.x * det3_234_134 - mat1.y * det3_234_034 + mat1.s * det3_234_014 - mat1.t * det3_234_013;
            var det4_1234_0234 = mat1.x * det3_234_234 - mat1.z * det3_234_034 + mat1.s * det3_234_024 - mat1.t * det3_234_023;
            var det4_1234_1234 = mat1.y * det3_234_234 - mat1.z * det3_234_134 + mat1.s * det3_234_124 - mat1.t * det3_234_123;

            // determinant of 5x5 matrix
            return mat0.x * det4_1234_1234 - mat0.y * det4_1234_0234 + mat0.z * det4_1234_0134 - mat0.s * det4_1234_0124 + mat0.t * det4_1234_0123;
        }

        public Matrix5x5 Transpose()   // returns transpose
        {
            Matrix5x5 transpose = new();
            for (var i = 0; i < 5; i++) for (var j = 0; j < 5; j++) transpose[i][j] = this[j][i];
            return transpose;
        }
        public Matrix5x5 TransposeSelf()
        {
            for (var i = 0; i < 5; i++)
                for (var j = i + 1; j < 5; j++)
                {
                    var temp = this[i][j];
                    this[i][j] = this[j][i];
                    this[j][i] = temp;
                }
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix5x5 Inverse()     // returns the inverse ( m * m.Inverse() = identity )
        {
            Matrix5x5 invMat = new(this);
            var r = invMat.InverseSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseSelf()        // returns false if determinant is zero
        {
            // 280+5+25 = 310 multiplications
            //				1 division
            double det, invDet;

            // 2x2 sub-determinants required to calculate 5x5 determinant
            var det2_34_01 = mat3.x * mat4.y - mat3.y * mat4.x;
            var det2_34_02 = mat3.x * mat4.z - mat3.z * mat4.x;
            var det2_34_03 = mat3.x * mat4.s - mat3.s * mat4.x;
            var det2_34_04 = mat3.x * mat4.t - mat3.t * mat4.x;
            var det2_34_12 = mat3.y * mat4.y - mat3.z * mat4.y;
            var det2_34_13 = mat3.y * mat4.s - mat3.s * mat4.y;
            var det2_34_14 = mat3.y * mat4.t - mat3.t * mat4.y;
            var det2_34_23 = mat3.z * mat4.s - mat3.s * mat4.z;
            var det2_34_24 = mat3.z * mat4.t - mat3.t * mat4.z;
            var det2_34_34 = mat3.s * mat4.t - mat3.t * mat4.s;

            // 3x3 sub-determinants required to calculate 5x5 determinant
            var det3_234_012 = mat2.x * det2_34_12 - mat2.y * det2_34_02 + mat2.z * det2_34_01;
            var det3_234_013 = mat2.x * det2_34_13 - mat2.y * det2_34_03 + mat2.s * det2_34_01;
            var det3_234_014 = mat2.x * det2_34_14 - mat2.y * det2_34_04 + mat2.t * det2_34_01;
            var det3_234_023 = mat2.x * det2_34_23 - mat2.z * det2_34_03 + mat2.s * det2_34_02;
            var det3_234_024 = mat2.x * det2_34_24 - mat2.z * det2_34_04 + mat2.t * det2_34_02;
            var det3_234_034 = mat2.x * det2_34_34 - mat2.s * det2_34_04 + mat2.t * det2_34_03;
            var det3_234_123 = mat2.y * det2_34_23 - mat2.z * det2_34_13 + mat2.s * det2_34_12;
            var det3_234_124 = mat2.y * det2_34_24 - mat2.z * det2_34_14 + mat2.t * det2_34_12;
            var det3_234_134 = mat2.y * det2_34_34 - mat2.s * det2_34_14 + mat2.t * det2_34_13;
            var det3_234_234 = mat2.z * det2_34_34 - mat2.s * det2_34_24 + mat2.t * det2_34_23;

            // 4x4 sub-determinants required to calculate 5x5 determinant
            var det4_1234_0123 = mat1.x * det3_234_123 - mat1.y * det3_234_023 + mat1.z * det3_234_013 - mat1.s * det3_234_012;
            var det4_1234_0124 = mat1.x * det3_234_124 - mat1.y * det3_234_024 + mat1.z * det3_234_014 - mat1.t * det3_234_012;
            var det4_1234_0134 = mat1.x * det3_234_134 - mat1.y * det3_234_034 + mat1.s * det3_234_014 - mat1.t * det3_234_013;
            var det4_1234_0234 = mat1.x * det3_234_234 - mat1.z * det3_234_034 + mat1.s * det3_234_024 - mat1.t * det3_234_023;
            var det4_1234_1234 = mat1.y * det3_234_234 - mat1.z * det3_234_134 + mat1.s * det3_234_124 - mat1.t * det3_234_123;

            // determinant of 5x5 matrix
            det = mat0.x * det4_1234_1234 - mat0.y * det4_1234_0234 + mat0.z * det4_1234_0134 - mat0.s * det4_1234_0124 + mat0.t * det4_1234_0123;
            if (MathX.Fabs(det) < MatrixX.INVERSE_EPSILON)
                return false;

            invDet = 1f / det;

            // remaining 2x2 sub-determinants
            var det2_23_01 = mat2.x * mat3.y - mat2.y * mat3.x;
            var det2_23_02 = mat2.x * mat3.z - mat2.z * mat3.x;
            var det2_23_03 = mat2.x * mat3.s - mat2.s * mat3.x;
            var det2_23_04 = mat2.x * mat3.t - mat2.t * mat3.x;
            var det2_23_12 = mat2.y * mat3.z - mat2.z * mat3.y;
            var det2_23_13 = mat2.y * mat3.s - mat2.s * mat3.y;
            var det2_23_14 = mat2.y * mat3.t - mat2.t * mat3.y;
            var det2_23_23 = mat2.z * mat3.s - mat2.s * mat3.z;
            var det2_23_24 = mat2.z * mat3.t - mat2.t * mat3.z;
            var det2_23_34 = mat2.s * mat3.t - mat2.t * mat3.s;
            var det2_24_01 = mat2.x * mat4.y - mat2.y * mat4.x;
            var det2_24_02 = mat2.x * mat4.z - mat2.z * mat4.x;
            var det2_24_03 = mat2.x * mat4.s - mat2.s * mat4.x;
            var det2_24_04 = mat2.x * mat4.t - mat2.t * mat4.x;
            var det2_24_12 = mat2.y * mat4.z - mat2.z * mat4.y;
            var det2_24_13 = mat2.y * mat4.s - mat2.s * mat4.y;
            var det2_24_14 = mat2.y * mat4.t - mat2.t * mat4.y;
            var det2_24_23 = mat2.z * mat4.s - mat2.s * mat4.z;
            var det2_24_24 = mat2.z * mat4.t - mat2.t * mat4.z;
            var det2_24_34 = mat2.s * mat4.t - mat2.t * mat4.s;

            // remaining 3x3 sub-determinants
            var det3_123_012 = mat1.x * det2_23_12 - mat1.y * det2_23_02 + mat1.z * det2_23_01;
            var det3_123_013 = mat1.x * det2_23_13 - mat1.y * det2_23_03 + mat1.s * det2_23_01;
            var det3_123_014 = mat1.x * det2_23_14 - mat1.y * det2_23_04 + mat1.t * det2_23_01;
            var det3_123_023 = mat1.x * det2_23_23 - mat1.z * det2_23_03 + mat1.s * det2_23_02;
            var det3_123_024 = mat1.x * det2_23_24 - mat1.z * det2_23_04 + mat1.t * det2_23_02;
            var det3_123_034 = mat1.x * det2_23_34 - mat1.s * det2_23_04 + mat1.t * det2_23_03;
            var det3_123_123 = mat1.y * det2_23_23 - mat1.z * det2_23_13 + mat1.s * det2_23_12;
            var det3_123_124 = mat1.y * det2_23_24 - mat1.z * det2_23_14 + mat1.t * det2_23_12;
            var det3_123_134 = mat1.y * det2_23_34 - mat1.s * det2_23_14 + mat1.t * det2_23_13;
            var det3_123_234 = mat1.z * det2_23_34 - mat1.s * det2_23_24 + mat1.t * det2_23_23;
            var det3_124_012 = mat1.x * det2_24_12 - mat1.y * det2_24_02 + mat1.z * det2_24_01;
            var det3_124_013 = mat1.x * det2_24_13 - mat1.y * det2_24_03 + mat1.s * det2_24_01;
            var det3_124_014 = mat1.x * det2_24_14 - mat1.y * det2_24_04 + mat1.t * det2_24_01;
            var det3_124_023 = mat1.x * det2_24_23 - mat1.z * det2_24_03 + mat1.s * det2_24_02;
            var det3_124_024 = mat1.x * det2_24_24 - mat1.z * det2_24_04 + mat1.t * det2_24_02;
            var det3_124_034 = mat1.x * det2_24_34 - mat1.s * det2_24_04 + mat1.t * det2_24_03;
            var det3_124_123 = mat1.y * det2_24_23 - mat1.z * det2_24_13 + mat1.s * det2_24_12;
            var det3_124_124 = mat1.y * det2_24_24 - mat1.z * det2_24_14 + mat1.t * det2_24_12;
            var det3_124_134 = mat1.y * det2_24_34 - mat1.s * det2_24_14 + mat1.t * det2_24_13;
            var det3_124_234 = mat1.z * det2_24_34 - mat1.s * det2_24_24 + mat1.t * det2_24_23;
            var det3_134_012 = mat1.x * det2_34_12 - mat1.y * det2_34_02 + mat1.z * det2_34_01;
            var det3_134_013 = mat1.x * det2_34_13 - mat1.y * det2_34_03 + mat1.s * det2_34_01;
            var det3_134_014 = mat1.x * det2_34_14 - mat1.y * det2_34_04 + mat1.t * det2_34_01;
            var det3_134_023 = mat1.x * det2_34_23 - mat1.z * det2_34_03 + mat1.s * det2_34_02;
            var det3_134_024 = mat1.x * det2_34_24 - mat1.z * det2_34_04 + mat1.t * det2_34_02;
            var det3_134_034 = mat1.x * det2_34_34 - mat1.s * det2_34_04 + mat1.t * det2_34_03;
            var det3_134_123 = mat1.y * det2_34_23 - mat1.z * det2_34_13 + mat1.s * det2_34_12;
            var det3_134_124 = mat1.y * det2_34_24 - mat1.z * det2_34_14 + mat1.t * det2_34_12;
            var det3_134_134 = mat1.y * det2_34_34 - mat1.s * det2_34_14 + mat1.t * det2_34_13;
            var det3_134_234 = mat1.z * det2_34_34 - mat1.s * det2_34_24 + mat1.t * det2_34_23;

            // remaining 4x4 sub-determinants
            var det4_0123_0123 = mat0.x * det3_123_123 - mat0.y * det3_123_023 + mat0.z * det3_123_013 - mat0.s * det3_123_012;
            var det4_0123_0124 = mat0.x * det3_123_124 - mat0.y * det3_123_024 + mat0.z * det3_123_014 - mat0.t * det3_123_012;
            var det4_0123_0134 = mat0.x * det3_123_134 - mat0.y * det3_123_034 + mat0.s * det3_123_014 - mat0.t * det3_123_013;
            var det4_0123_0234 = mat0.x * det3_123_234 - mat0.z * det3_123_034 + mat0.s * det3_123_024 - mat0.t * det3_123_023;
            var det4_0123_1234 = mat0.y * det3_123_234 - mat0.z * det3_123_134 + mat0.s * det3_123_124 - mat0.t * det3_123_123;
            var det4_0124_0123 = mat0.x * det3_124_123 - mat0.y * det3_124_023 + mat0.z * det3_124_013 - mat0.s * det3_124_012;
            var det4_0124_0124 = mat0.x * det3_124_124 - mat0.y * det3_124_024 + mat0.z * det3_124_014 - mat0.t * det3_124_012;
            var det4_0124_0134 = mat0.x * det3_124_134 - mat0.y * det3_124_034 + mat0.s * det3_124_014 - mat0.t * det3_124_013;
            var det4_0124_0234 = mat0.x * det3_124_234 - mat0.z * det3_124_034 + mat0.s * det3_124_024 - mat0.t * det3_124_023;
            var det4_0124_1234 = mat0.y * det3_124_234 - mat0.z * det3_124_134 + mat0.s * det3_124_124 - mat0.t * det3_124_123;
            var det4_0134_0123 = mat0.x * det3_134_123 - mat0.y * det3_134_023 + mat0.z * det3_134_013 - mat0.s * det3_134_012;
            var det4_0134_0124 = mat0.x * det3_134_124 - mat0.y * det3_134_024 + mat0.z * det3_134_014 - mat0.t * det3_134_012;
            var det4_0134_0134 = mat0.x * det3_134_134 - mat0.y * det3_134_034 + mat0.s * det3_134_014 - mat0.t * det3_134_013;
            var det4_0134_0234 = mat0.x * det3_134_234 - mat0.z * det3_134_034 + mat0.s * det3_134_024 - mat0.t * det3_134_023;
            var det4_0134_1234 = mat0.y * det3_134_234 - mat0.z * det3_134_134 + mat0.s * det3_134_124 - mat0.t * det3_134_123;
            var det4_0234_0123 = mat0.x * det3_234_123 - mat0.y * det3_234_023 + mat0.z * det3_234_013 - mat0.s * det3_234_012;
            var det4_0234_0124 = mat0.x * det3_234_124 - mat0.y * det3_234_024 + mat0.z * det3_234_014 - mat0.t * det3_234_012;
            var det4_0234_0134 = mat0.x * det3_234_134 - mat0.y * det3_234_034 + mat0.s * det3_234_014 - mat0.t * det3_234_013;
            var det4_0234_0234 = mat0.x * det3_234_234 - mat0.z * det3_234_034 + mat0.s * det3_234_024 - mat0.t * det3_234_023;
            var det4_0234_1234 = mat0.y * det3_234_234 - mat0.z * det3_234_134 + mat0.s * det3_234_124 - mat0.t * det3_234_123;

            mat0.x = (float)(det4_1234_1234 * invDet); mat0.y = (float)(-det4_0234_1234 * invDet); mat0.z = (float)(det4_0134_1234 * invDet); mat0.s = (float)(-det4_0124_1234 * invDet); mat0.t = (float)(det4_0123_1234 * invDet);
            mat1.x = (float)(-det4_1234_0234 * invDet); mat1.y = (float)(det4_0234_0234 * invDet); mat1.z = (float)(-det4_0134_0234 * invDet); mat1.s = (float)(det4_0124_0234 * invDet); mat1.t = (float)(-det4_0123_0234 * invDet);
            mat2.x = (float)(det4_1234_0134 * invDet); mat2.y = (float)(-det4_0234_0134 * invDet); mat2.z = (float)(det4_0134_0134 * invDet); mat2.s = (float)(-det4_0124_0134 * invDet); mat2.t = (float)(det4_0123_0134 * invDet);
            mat3.x = (float)(-det4_1234_0124 * invDet); mat3.y = (float)(det4_0234_0124 * invDet); mat3.z = (float)(-det4_0134_0124 * invDet); mat3.s = (float)(det4_0124_0124 * invDet); mat3.t = (float)(-det4_0123_0124 * invDet);
            mat4.x = (float)(det4_1234_0123 * invDet); mat4.y = (float)(-det4_0234_0123 * invDet); mat4.z = (float)(det4_0134_0123 * invDet); mat4.s = (float)(-det4_0124_0123 * invDet); mat4.t = (float)(det4_0123_0123 * invDet);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix5x5 InverseFast() // returns the inverse ( m * m.Inverse() = identity )
        {
            Matrix5x5 invMat = new(this);
            var r = invMat.InverseFastSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseFastSelf()    // returns false if determinant is zero
        {
            // 86+30+6 = 122 multiplications
            //	  2*1  =   2 divisions
            Matrix3x3 r0 = new(), r1 = new(), r2 = new(), r3 = new(); float c0, c1, c2, det, invDet;

            fixed (float* mat_ = &mat0.x)
            {
                //r0 = m0.Inverse();	// 3x3
                c0 = mat_[1 * 5 + 1] * mat_[2 * 5 + 2] - mat_[1 * 5 + 2] * mat_[2 * 5 + 1];
                c1 = mat_[1 * 5 + 2] * mat_[2 * 5 + 0] - mat_[1 * 5 + 0] * mat_[2 * 5 + 2];
                c2 = mat_[1 * 5 + 0] * mat_[2 * 5 + 1] - mat_[1 * 5 + 1] * mat_[2 * 5 + 0];

                det = mat_[0 * 5 + 0] * c0 + mat_[0 * 5 + 1] * c1 + mat_[0 * 5 + 2] * c2;
                if (MathX.Fabs(det) < MatrixX.INVERSE_EPSILON) return false;

                invDet = 1f / det;

                r0.mat0.x = c0 * invDet;
                r0.mat0.y = (mat_[0 * 5 + 2] * mat_[2 * 5 + 1] - mat_[0 * 5 + 1] * mat_[2 * 5 + 2]) * invDet;
                r0.mat0.z = (mat_[0 * 5 + 1] * mat_[1 * 5 + 2] - mat_[0 * 5 + 2] * mat_[1 * 5 + 1]) * invDet;
                r0.mat1.x = c1 * invDet;
                r0.mat1.y = (mat_[0 * 5 + 0] * mat_[2 * 5 + 2] - mat_[0 * 5 + 2] * mat_[2 * 5 + 0]) * invDet;
                r0.mat1.z = (mat_[0 * 5 + 2] * mat_[1 * 5 + 0] - mat_[0 * 5 + 0] * mat_[1 * 5 + 2]) * invDet;
                r0.mat2.x = c2 * invDet;
                r0.mat2.y = (mat_[0 * 5 + 1] * mat_[2 * 5 + 0] - mat_[0 * 5 + 0] * mat_[2 * 5 + 1]) * invDet;
                r0.mat2.z = (mat_[0 * 5 + 0] * mat_[1 * 5 + 1] - mat_[0 * 5 + 1] * mat_[1 * 5 + 0]) * invDet;

                // r1 = r0 * m1;		// 3x2 = 3x3 * 3x2
                r1.mat0.x = r0.mat0.x * mat_[0 * 5 + 3] + r0.mat0.y * mat_[1 * 5 + 3] + r0.mat0.z * mat_[2 * 5 + 3];
                r1.mat0.y = r0.mat0.x * mat_[0 * 5 + 4] + r0.mat0.y * mat_[1 * 5 + 4] + r0.mat0.z * mat_[2 * 5 + 4];
                r1.mat1.x = r0.mat1.x * mat_[0 * 5 + 3] + r0.mat1.y * mat_[1 * 5 + 3] + r0.mat1.z * mat_[2 * 5 + 3];
                r1.mat1.y = r0.mat1.x * mat_[0 * 5 + 4] + r0.mat1.y * mat_[1 * 5 + 4] + r0.mat1.z * mat_[2 * 5 + 4];
                r1.mat2.x = r0.mat2.x * mat_[0 * 5 + 3] + r0.mat2.y * mat_[1 * 5 + 3] + r0.mat2.z * mat_[2 * 5 + 3];
                r1.mat2.y = r0.mat2.x * mat_[0 * 5 + 4] + r0.mat2.y * mat_[1 * 5 + 4] + r0.mat2.z * mat_[2 * 5 + 4];

                // r2 = m2 * r1;		// 2x2 = 2x3 * 3x2
                r2.mat0.x = mat_[3 * 5 + 0] * r1.mat0.x + mat_[3 * 5 + 1] * r1.mat1.x + mat_[3 * 5 + 2] * r1.mat2.x;
                r2.mat0.y = mat_[3 * 5 + 0] * r1.mat0.y + mat_[3 * 5 + 1] * r1.mat1.y + mat_[3 * 5 + 2] * r1.mat2.y;
                r2.mat1.x = mat_[4 * 5 + 0] * r1.mat0.x + mat_[4 * 5 + 1] * r1.mat1.x + mat_[4 * 5 + 2] * r1.mat2.x;
                r2.mat1.y = mat_[4 * 5 + 0] * r1.mat0.y + mat_[4 * 5 + 1] * r1.mat1.y + mat_[4 * 5 + 2] * r1.mat2.y;

                // r3 = r2 - m3;		// 2x2 = 2x2 - 2x2
                r3.mat0.x = r2.mat0.x - mat_[3 * 5 + 3];
                r3.mat0.y = r2.mat0.y - mat_[3 * 5 + 4];
                r3.mat1.x = r2.mat1.x - mat_[4 * 5 + 3];
                r3.mat1.y = r2.mat1.y - mat_[4 * 5 + 4];

                // r3.InverseSelf();	// 2x2
                det = r3.mat0.x * r3.mat1.y - r3.mat0.y * r3.mat1.x;
                if (MathX.Fabs(det) < MatrixX.INVERSE_EPSILON) return false;

                invDet = 1f / det;

                c0 = r3.mat0.x;
                r3.mat0[0] = r3.mat1.y * invDet;
                r3.mat0[1] = -r3.mat0.y * invDet;
                r3.mat1[0] = -r3.mat1.x * invDet;
                r3.mat1[1] = c0 * invDet;

                // r2 = m2 * r0;		// 2x3 = 2x3 * 3x3
                r2.mat0[0] = mat_[3 * 5 + 0] * r0.mat0.x + mat_[3 * 5 + 1] * r0.mat1.x + mat_[3 * 5 + 2] * r0.mat2.x;
                r2.mat0[1] = mat_[3 * 5 + 0] * r0.mat0.y + mat_[3 * 5 + 1] * r0.mat1.y + mat_[3 * 5 + 2] * r0.mat2.y;
                r2.mat0[2] = mat_[3 * 5 + 0] * r0.mat0.z + mat_[3 * 5 + 1] * r0.mat1.z + mat_[3 * 5 + 2] * r0.mat2.z;
                r2.mat1[0] = mat_[4 * 5 + 0] * r0.mat0.x + mat_[4 * 5 + 1] * r0.mat1.x + mat_[4 * 5 + 2] * r0.mat2.x;
                r2.mat1[1] = mat_[4 * 5 + 0] * r0.mat0.y + mat_[4 * 5 + 1] * r0.mat1.y + mat_[4 * 5 + 2] * r0.mat2.y;
                r2.mat1[2] = mat_[4 * 5 + 0] * r0.mat0.z + mat_[4 * 5 + 1] * r0.mat1.z + mat_[4 * 5 + 2] * r0.mat2.z;

                // m2 = r3 * r2;		// 2x3 = 2x2 * 2x3
                mat_[3 * 5 + 0] = r3.mat0.x * r2.mat0.x + r3.mat0.y * r2.mat1.x;
                mat_[3 * 5 + 1] = r3.mat0.x * r2.mat0.y + r3.mat0.y * r2.mat1.y;
                mat_[3 * 5 + 2] = r3.mat0.x * r2.mat0.z + r3.mat0.y * r2.mat1.z;
                mat_[4 * 5 + 0] = r3.mat1.x * r2.mat0.x + r3.mat1.y * r2.mat1.x;
                mat_[4 * 5 + 1] = r3.mat1.x * r2.mat0.y + r3.mat1.y * r2.mat1.y;
                mat_[4 * 5 + 2] = r3.mat1.x * r2.mat0.z + r3.mat1.y * r2.mat1.z;

                // m0 = r0 - r1 * m2;	// 3x3 = 3x3 - 3x2 * 2x3
                mat_[0 * 5 + 0] = r0.mat0.x - r1.mat0[0] * mat_[3 * 5 + 0] - r1.mat0.y * mat_[4 * 5 + 0];
                mat_[0 * 5 + 1] = r0.mat0.y - r1.mat0[0] * mat_[3 * 5 + 1] - r1.mat0.y * mat_[4 * 5 + 1];
                mat_[0 * 5 + 2] = r0.mat0.z - r1.mat0[0] * mat_[3 * 5 + 2] - r1.mat0.y * mat_[4 * 5 + 2];
                mat_[1 * 5 + 0] = r0.mat1.x - r1.mat1[0] * mat_[3 * 5 + 0] - r1.mat1.y * mat_[4 * 5 + 0];
                mat_[1 * 5 + 1] = r0.mat1.y - r1.mat1[0] * mat_[3 * 5 + 1] - r1.mat1.y * mat_[4 * 5 + 1];
                mat_[1 * 5 + 2] = r0.mat1.z - r1.mat1[0] * mat_[3 * 5 + 2] - r1.mat1.y * mat_[4 * 5 + 2];
                mat_[2 * 5 + 0] = r0.mat2.x - r1.mat2[0] * mat_[3 * 5 + 0] - r1.mat2.y * mat_[4 * 5 + 0];
                mat_[2 * 5 + 1] = r0.mat2.y - r1.mat2[0] * mat_[3 * 5 + 1] - r1.mat2.y * mat_[4 * 5 + 1];
                mat_[2 * 5 + 2] = r0.mat2.z - r1.mat2[0] * mat_[3 * 5 + 2] - r1.mat2.y * mat_[4 * 5 + 2];

                // m1 = r1 * r3;		// 3x2 = 3x2 * 2x2
                mat_[0 * 5 + 3] = r1.mat0.x * r3.mat0.x + r1.mat0.y * r3.mat1.x;
                mat_[0 * 5 + 4] = r1.mat0.x * r3.mat0.y + r1.mat0.y * r3.mat1.y;
                mat_[1 * 5 + 3] = r1.mat1.x * r3.mat0.x + r1.mat1.y * r3.mat1.x;
                mat_[1 * 5 + 4] = r1.mat1.x * r3.mat0.y + r1.mat1.y * r3.mat1.y;
                mat_[2 * 5 + 3] = r1.mat2.x * r3.mat0.x + r1.mat2.y * r3.mat1.x;
                mat_[2 * 5 + 4] = r1.mat2.x * r3.mat0.y + r1.mat2.y * r3.mat1.y;

                // m3 = -r3;			// 2x2 = - 2x2
                mat_[3 * 5 + 3] = -r3.mat0.x;
                mat_[3 * 5 + 4] = -r3.mat0.y;
                mat_[4 * 5 + 3] = -r3.mat1.x;
                mat_[4 * 5 + 4] = -r3.mat1.y;
            }

            return true;
        }

        public const int Dimension = 25;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Fixed<T>(FloatPtr<T> callback)
            => mat0.Fixed(callback);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fixed(FloatPtr callback)
            => mat0.Fixed(callback);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(int precision = 2)
        {
            fixed (float* _ = &mat0.x) return FloatArrayToString(_, Dimension, precision);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Matrix6x6
    {
        public static Matrix6x6 zero = new(new Vector6(0, 0, 0, 0, 0, 0), new Vector6(0, 0, 0, 0, 0, 0), new Vector6(0, 0, 0, 0, 0, 0), new Vector6(0, 0, 0, 0, 0, 0), new Vector6(0, 0, 0, 0, 0, 0), new Vector6(0, 0, 0, 0, 0, 0));
        public static Matrix6x6 identity = new(new Vector6(1, 0, 0, 0, 0, 0), new Vector6(0, 1, 0, 0, 0, 0), new Vector6(0, 0, 1, 0, 0, 0), new Vector6(0, 0, 0, 1, 0, 0), new Vector6(0, 0, 0, 0, 1, 0), new Vector6(0, 0, 0, 0, 0, 1));

        internal Vector6 mat0;
        internal Vector6 mat1;
        internal Vector6 mat2;
        internal Vector6 mat3;
        internal Vector6 mat4;
        internal Vector6 mat5;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix6x6(in Matrix6x6 a)
            => this = a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix6x6(in Vector6 v0, in Vector6 v1, in Vector6 v2, in Vector6 v3, in Vector6 v4, in Vector6 v5)
        {
            mat0 = v0;
            mat1 = v1;
            mat2 = v2;
            mat3 = v3;
            mat4 = v4;
            mat5 = v5;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix6x6(in Matrix3x3 m0, in Matrix3x3 m1, in Matrix3x3 m2, in Matrix3x3 m3)
        {
            mat0 = new Vector6(m0.mat0.x, m0.mat0.y, m0.mat0.z, m1.mat0.x, m1.mat0.y, m1.mat0.z);
            mat1 = new Vector6(m0.mat1.x, m0.mat1.y, m0.mat1.z, m1.mat1.x, m1.mat1.y, m1.mat1.z);
            mat2 = new Vector6(m0.mat2.x, m0.mat2.y, m0.mat2.z, m1.mat2.x, m1.mat2.y, m1.mat2.z);
            mat3 = new Vector6(m2.mat0.x, m2.mat0.y, m2.mat0.z, m3.mat0.x, m3.mat0.y, m3.mat0.z);
            mat4 = new Vector6(m2.mat1.x, m2.mat1.y, m2.mat1.z, m3.mat1.x, m3.mat1.y, m3.mat1.z);
            mat5 = new Vector6(m2.mat2.x, m2.mat2.y, m2.mat2.z, m3.mat2.x, m3.mat2.y, m3.mat2.z);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix6x6(float[] src)
        {
            this = default;
            fixed (void* mat_ = &mat0, src_ = src) Unsafe.CopyBlock(mat_, src_, 6U * 6U * sizeof(float));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix6x6(float[,] src)
        {
            this = default;
            fixed (void* mat_ = &mat0, src_ = &src[0, 0]) Unsafe.CopyBlock(mat_, src_, 6U * 6U * sizeof(float));
        }

        public ref Vector6 this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { fixed (Vector6* mat_ = &mat0) return ref mat_[index]; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix6x6 operator *(in Matrix6x6 _, float a)
            => new(
            new Vector6(_.mat0[0] * a, _.mat0[1] * a, _.mat0[2] * a, _.mat0[3] * a, _.mat0[4] * a, _.mat0[5] * a),
            new Vector6(_.mat1[0] * a, _.mat1[1] * a, _.mat1[2] * a, _.mat1[3] * a, _.mat1[4] * a, _.mat1[5] * a),
            new Vector6(_.mat2[0] * a, _.mat2[1] * a, _.mat2[2] * a, _.mat2[3] * a, _.mat2[4] * a, _.mat2[5] * a),
            new Vector6(_.mat3[0] * a, _.mat3[1] * a, _.mat3[2] * a, _.mat3[3] * a, _.mat3[4] * a, _.mat3[5] * a),
            new Vector6(_.mat4[0] * a, _.mat4[1] * a, _.mat4[2] * a, _.mat4[3] * a, _.mat4[4] * a, _.mat4[5] * a),
            new Vector6(_.mat5[0] * a, _.mat5[1] * a, _.mat5[2] * a, _.mat5[3] * a, _.mat5[4] * a, _.mat5[5] * a));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator *(in Matrix6x6 _, in Vector6 vec)
            => new(
            _.mat0[0] * vec[0] + _.mat0[1] * vec[1] + _.mat0[2] * vec[2] + _.mat0[3] * vec[3] + _.mat0[4] * vec[4] + _.mat0[5] * vec[5],
            _.mat1[0] * vec[0] + _.mat1[1] * vec[1] + _.mat1[2] * vec[2] + _.mat1[3] * vec[3] + _.mat1[4] * vec[4] + _.mat1[5] * vec[5],
            _.mat2[0] * vec[0] + _.mat2[1] * vec[1] + _.mat2[2] * vec[2] + _.mat2[3] * vec[3] + _.mat2[4] * vec[4] + _.mat2[5] * vec[5],
            _.mat3[0] * vec[0] + _.mat3[1] * vec[1] + _.mat3[2] * vec[2] + _.mat3[3] * vec[3] + _.mat3[4] * vec[4] + _.mat3[5] * vec[5],
            _.mat4[0] * vec[0] + _.mat4[1] * vec[1] + _.mat4[2] * vec[2] + _.mat4[3] * vec[3] + _.mat4[4] * vec[4] + _.mat4[5] * vec[5],
            _.mat5[0] * vec[0] + _.mat5[1] * vec[1] + _.mat5[2] * vec[2] + _.mat5[3] * vec[3] + _.mat5[4] * vec[4] + _.mat5[5] * vec[5]);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix6x6 operator *(in Matrix6x6 _, in Matrix6x6 a)
        {
            Matrix6x6 dst = default;
            void* dst_ = &dst.mat0;
            fixed (void* __ = &_.mat0, a_ = &a.mat0)
            {
                var m1Ptr = (float*)__;
                var m2Ptr = (float*)a_;
                var dstPtr = (float*)dst_;
                for (var i = 0; i < 6; i++)
                {
                    for (var j = 0; j < 6; j++)
                    {
                        *dstPtr = m1Ptr[0] * m2Ptr[0 * 6 + j]
                                + m1Ptr[1] * m2Ptr[1 * 6 + j]
                                + m1Ptr[2] * m2Ptr[2 * 6 + j]
                                + m1Ptr[3] * m2Ptr[3 * 6 + j]
                                + m1Ptr[4] * m2Ptr[4 * 6 + j]
                                + m1Ptr[5] * m2Ptr[5 * 6 + j];
                        dstPtr++;
                    }
                    m1Ptr += 6;
                }
                return dst;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix6x6 operator +(in Matrix6x6 _, in Matrix6x6 a)
            => new(
            new Vector6(_.mat0[0] + a.mat0[0], _.mat0[1] + a.mat0[1], _.mat0[2] + a.mat0[2], _.mat0[3] + a.mat0[3], _.mat0[4] + a.mat0[4], _.mat0[5] + a.mat0[5]),
            new Vector6(_.mat1[0] + a.mat1[0], _.mat1[1] + a.mat1[1], _.mat1[2] + a.mat1[2], _.mat1[3] + a.mat1[3], _.mat1[4] + a.mat1[4], _.mat1[5] + a.mat1[5]),
            new Vector6(_.mat2[0] + a.mat2[0], _.mat2[1] + a.mat2[1], _.mat2[2] + a.mat2[2], _.mat2[3] + a.mat2[3], _.mat2[4] + a.mat2[4], _.mat2[5] + a.mat2[5]),
            new Vector6(_.mat3[0] + a.mat3[0], _.mat3[1] + a.mat3[1], _.mat3[2] + a.mat3[2], _.mat3[3] + a.mat3[3], _.mat3[4] + a.mat3[4], _.mat3[5] + a.mat3[5]),
            new Vector6(_.mat4[0] + a.mat4[0], _.mat4[1] + a.mat4[1], _.mat4[2] + a.mat4[2], _.mat4[3] + a.mat4[3], _.mat4[4] + a.mat4[4], _.mat4[5] + a.mat4[5]),
            new Vector6(_.mat5[0] + a.mat5[0], _.mat5[1] + a.mat5[1], _.mat5[2] + a.mat5[2], _.mat5[3] + a.mat5[3], _.mat5[4] + a.mat5[4], _.mat5[5] + a.mat5[5]));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix6x6 operator -(in Matrix6x6 _, in Matrix6x6 a)
            => new(
            new Vector6(_.mat0[0] - a.mat0[0], _.mat0[1] - a.mat0[1], _.mat0[2] - a.mat0[2], _.mat0[3] - a.mat0[3], _.mat0[4] - a.mat0[4], _.mat0[5] - a.mat0[5]),
            new Vector6(_.mat1[0] - a.mat1[0], _.mat1[1] - a.mat1[1], _.mat1[2] - a.mat1[2], _.mat1[3] - a.mat1[3], _.mat1[4] - a.mat1[4], _.mat1[5] - a.mat1[5]),
            new Vector6(_.mat2[0] - a.mat2[0], _.mat2[1] - a.mat2[1], _.mat2[2] - a.mat2[2], _.mat2[3] - a.mat2[3], _.mat2[4] - a.mat2[4], _.mat2[5] - a.mat2[5]),
            new Vector6(_.mat3[0] - a.mat3[0], _.mat3[1] - a.mat3[1], _.mat3[2] - a.mat3[2], _.mat3[3] - a.mat3[3], _.mat3[4] - a.mat3[4], _.mat3[5] - a.mat3[5]),
            new Vector6(_.mat4[0] - a.mat4[0], _.mat4[1] - a.mat4[1], _.mat4[2] - a.mat4[2], _.mat4[3] - a.mat4[3], _.mat4[4] - a.mat4[4], _.mat4[5] - a.mat4[5]),
            new Vector6(_.mat5[0] - a.mat5[0], _.mat5[1] - a.mat5[1], _.mat5[2] - a.mat5[2], _.mat5[3] - a.mat5[3], _.mat5[4] - a.mat5[4], _.mat5[5] - a.mat5[5]));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix6x6 operator *(float a, in Matrix6x6 mat)
            => mat * a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector6 operator *(in Vector6 vec, in Matrix6x6 mat)
            => mat * vec;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Matrix6x6 a)                       // exact compare, no epsilon
        {
            fixed (void* mat_ = &mat0, a_ = &a.mat0)
            {
                var ptr1 = (float*)mat_;
                var ptr2 = (float*)a_;
                for (var i = 0; i < 6 * 6; i++) if (ptr1[i] != ptr2[i]) return false;
                return true;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in Matrix6x6 a, float epsilon)  // compare with epsilon
        {
            fixed (void* mat_ = &mat0, a_ = &a.mat0)
            {
                var ptr1 = (float*)mat_;
                var ptr2 = (float*)a_;
                for (var i = 0; i < 6 * 6; i++) if (MathX.Fabs(ptr1[i] - ptr2[i]) > epsilon) return false;
                return true;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Matrix6x6 _, in Matrix6x6 a)                   // exact compare, no epsilon
            => _.Compare(a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Matrix6x6 _, in Matrix6x6 a)                   // exact compare, no epsilon
            => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is Matrix6x6 q && Compare(q);
        public override int GetHashCode()
            => mat0.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()
        {
            fixed (void* mat_ = &mat0) Unsafe.InitBlock(mat_, 0, 6U * (uint)sizeof(Vector6));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Identity()
            => this = new(identity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsIdentity(float epsilon = MatrixX.EPSILON)
            => Compare(identity, epsilon);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSymmetric(float epsilon = MatrixX.EPSILON)
        {
            for (var i = 1; i < 6; i++) for (var j = 0; j < i; j++) if (MathX.Fabs(this[i][j] - this[j][i]) > epsilon) return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDiagonal(float epsilon = MatrixX.EPSILON)
        {
            for (var i = 0; i < 6; i++) for (var j = 0; j < 6; j++) if (i != j && MathX.Fabs(this[i][j]) > epsilon) return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix3x3 SubMat3(int n)
        {
            Debug.Assert(n >= 0 && n < 4);
            var b0 = ((n & 2) >> 1) * 3;
            var b1 = (n & 1) * 3;
            return new(
                this[b0 + 0][b1 + 0], this[b0 + 0][b1 + 1], this[b0 + 0][b1 + 2],
                this[b0 + 1][b1 + 0], this[b0 + 1][b1 + 1], this[b0 + 1][b1 + 2],
                this[b0 + 2][b1 + 0], this[b0 + 2][b1 + 1], this[b0 + 2][b1 + 2]);
        }

        public float Trace
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => mat0[0] + mat1[1] + mat2[2] + mat3[3] + mat4[4] + mat5[5];
        }

        public float Determinant()
        {
            // 2x2 sub-determinants required to calculate 6x6 determinant
            var det2_45_01 = mat4[0] * mat5[1] - mat4[1] * mat5[0];
            var det2_45_02 = mat4[0] * mat5[2] - mat4[2] * mat5[0];
            var det2_45_03 = mat4[0] * mat5[3] - mat4[3] * mat5[0];
            var det2_45_04 = mat4[0] * mat5[4] - mat4[4] * mat5[0];
            var det2_45_05 = mat4[0] * mat5[5] - mat4[5] * mat5[0];
            var det2_45_12 = mat4[1] * mat5[2] - mat4[2] * mat5[1];
            var det2_45_13 = mat4[1] * mat5[3] - mat4[3] * mat5[1];
            var det2_45_14 = mat4[1] * mat5[4] - mat4[4] * mat5[1];
            var det2_45_15 = mat4[1] * mat5[5] - mat4[5] * mat5[1];
            var det2_45_23 = mat4[2] * mat5[3] - mat4[3] * mat5[2];
            var det2_45_24 = mat4[2] * mat5[4] - mat4[4] * mat5[2];
            var det2_45_25 = mat4[2] * mat5[5] - mat4[5] * mat5[2];
            var det2_45_34 = mat4[3] * mat5[4] - mat4[4] * mat5[3];
            var det2_45_35 = mat4[3] * mat5[5] - mat4[5] * mat5[3];
            var det2_45_45 = mat4[4] * mat5[5] - mat4[5] * mat5[4];

            // 3x3 sub-determinants required to calculate 6x6 determinant
            var det3_345_012 = mat3[0] * det2_45_12 - mat3[1] * det2_45_02 + mat3[2] * det2_45_01;
            var det3_345_013 = mat3[0] * det2_45_13 - mat3[1] * det2_45_03 + mat3[3] * det2_45_01;
            var det3_345_014 = mat3[0] * det2_45_14 - mat3[1] * det2_45_04 + mat3[4] * det2_45_01;
            var det3_345_015 = mat3[0] * det2_45_15 - mat3[1] * det2_45_05 + mat3[5] * det2_45_01;
            var det3_345_023 = mat3[0] * det2_45_23 - mat3[2] * det2_45_03 + mat3[3] * det2_45_02;
            var det3_345_024 = mat3[0] * det2_45_24 - mat3[2] * det2_45_04 + mat3[4] * det2_45_02;
            var det3_345_025 = mat3[0] * det2_45_25 - mat3[2] * det2_45_05 + mat3[5] * det2_45_02;
            var det3_345_034 = mat3[0] * det2_45_34 - mat3[3] * det2_45_04 + mat3[4] * det2_45_03;
            var det3_345_035 = mat3[0] * det2_45_35 - mat3[3] * det2_45_05 + mat3[5] * det2_45_03;
            var det3_345_045 = mat3[0] * det2_45_45 - mat3[4] * det2_45_05 + mat3[5] * det2_45_04;
            var det3_345_123 = mat3[1] * det2_45_23 - mat3[2] * det2_45_13 + mat3[3] * det2_45_12;
            var det3_345_124 = mat3[1] * det2_45_24 - mat3[2] * det2_45_14 + mat3[4] * det2_45_12;
            var det3_345_125 = mat3[1] * det2_45_25 - mat3[2] * det2_45_15 + mat3[5] * det2_45_12;
            var det3_345_134 = mat3[1] * det2_45_34 - mat3[3] * det2_45_14 + mat3[4] * det2_45_13;
            var det3_345_135 = mat3[1] * det2_45_35 - mat3[3] * det2_45_15 + mat3[5] * det2_45_13;
            var det3_345_145 = mat3[1] * det2_45_45 - mat3[4] * det2_45_15 + mat3[5] * det2_45_14;
            var det3_345_234 = mat3[2] * det2_45_34 - mat3[3] * det2_45_24 + mat3[4] * det2_45_23;
            var det3_345_235 = mat3[2] * det2_45_35 - mat3[3] * det2_45_25 + mat3[5] * det2_45_23;
            var det3_345_245 = mat3[2] * det2_45_45 - mat3[4] * det2_45_25 + mat3[5] * det2_45_24;
            var det3_345_345 = mat3[3] * det2_45_45 - mat3[4] * det2_45_35 + mat3[5] * det2_45_34;

            // 4x4 sub-determinants required to calculate 6x6 determinant
            var det4_2345_0123 = mat2[0] * det3_345_123 - mat2[1] * det3_345_023 + mat2[2] * det3_345_013 - mat2[3] * det3_345_012;
            var det4_2345_0124 = mat2[0] * det3_345_124 - mat2[1] * det3_345_024 + mat2[2] * det3_345_014 - mat2[4] * det3_345_012;
            var det4_2345_0125 = mat2[0] * det3_345_125 - mat2[1] * det3_345_025 + mat2[2] * det3_345_015 - mat2[5] * det3_345_012;
            var det4_2345_0134 = mat2[0] * det3_345_134 - mat2[1] * det3_345_034 + mat2[3] * det3_345_014 - mat2[4] * det3_345_013;
            var det4_2345_0135 = mat2[0] * det3_345_135 - mat2[1] * det3_345_035 + mat2[3] * det3_345_015 - mat2[5] * det3_345_013;
            var det4_2345_0145 = mat2[0] * det3_345_145 - mat2[1] * det3_345_045 + mat2[4] * det3_345_015 - mat2[5] * det3_345_014;
            var det4_2345_0234 = mat2[0] * det3_345_234 - mat2[2] * det3_345_034 + mat2[3] * det3_345_024 - mat2[4] * det3_345_023;
            var det4_2345_0235 = mat2[0] * det3_345_235 - mat2[2] * det3_345_035 + mat2[3] * det3_345_025 - mat2[5] * det3_345_023;
            var det4_2345_0245 = mat2[0] * det3_345_245 - mat2[2] * det3_345_045 + mat2[4] * det3_345_025 - mat2[5] * det3_345_024;
            var det4_2345_0345 = mat2[0] * det3_345_345 - mat2[3] * det3_345_045 + mat2[4] * det3_345_035 - mat2[5] * det3_345_034;
            var det4_2345_1234 = mat2[1] * det3_345_234 - mat2[2] * det3_345_134 + mat2[3] * det3_345_124 - mat2[4] * det3_345_123;
            var det4_2345_1235 = mat2[1] * det3_345_235 - mat2[2] * det3_345_135 + mat2[3] * det3_345_125 - mat2[5] * det3_345_123;
            var det4_2345_1245 = mat2[1] * det3_345_245 - mat2[2] * det3_345_145 + mat2[4] * det3_345_125 - mat2[5] * det3_345_124;
            var det4_2345_1345 = mat2[1] * det3_345_345 - mat2[3] * det3_345_145 + mat2[4] * det3_345_135 - mat2[5] * det3_345_134;
            var det4_2345_2345 = mat2[2] * det3_345_345 - mat2[3] * det3_345_245 + mat2[4] * det3_345_235 - mat2[5] * det3_345_234;

            // 5x5 sub-determinants required to calculate 6x6 determinant
            var det5_12345_01234 = mat1[0] * det4_2345_1234 - mat1[1] * det4_2345_0234 + mat1[2] * det4_2345_0134 - mat1[3] * det4_2345_0124 + mat1[4] * det4_2345_0123;
            var det5_12345_01235 = mat1[0] * det4_2345_1235 - mat1[1] * det4_2345_0235 + mat1[2] * det4_2345_0135 - mat1[3] * det4_2345_0125 + mat1[5] * det4_2345_0123;
            var det5_12345_01245 = mat1[0] * det4_2345_1245 - mat1[1] * det4_2345_0245 + mat1[2] * det4_2345_0145 - mat1[4] * det4_2345_0125 + mat1[5] * det4_2345_0124;
            var det5_12345_01345 = mat1[0] * det4_2345_1345 - mat1[1] * det4_2345_0345 + mat1[3] * det4_2345_0145 - mat1[4] * det4_2345_0135 + mat1[5] * det4_2345_0134;
            var det5_12345_02345 = mat1[0] * det4_2345_2345 - mat1[2] * det4_2345_0345 + mat1[3] * det4_2345_0245 - mat1[4] * det4_2345_0235 + mat1[5] * det4_2345_0234;
            var det5_12345_12345 = mat1[1] * det4_2345_2345 - mat1[2] * det4_2345_1345 + mat1[3] * det4_2345_1245 - mat1[4] * det4_2345_1235 + mat1[5] * det4_2345_1234;

            // determinant of 6x6 matrix
            return mat0[0] * det5_12345_12345
                 - mat0[1] * det5_12345_02345
                 + mat0[2] * det5_12345_01345
                 - mat0[3] * det5_12345_01245
                 + mat0[4] * det5_12345_01235
                 - mat0[5] * det5_12345_01234;
        }

        public Matrix6x6 Transpose()   // returns transpose
        {
            Matrix6x6 transpose = new();
            for (var i = 0; i < 6; i++) for (var j = 0; j < 6; j++) transpose[i][j] = this[j][i];
            return transpose;
        }
        public Matrix6x6 TransposeSelf()
        {
            for (var i = 0; i < 6; i++)
                for (var j = i + 1; j < 6; j++)
                {
                    var temp = this[i][j];
                    this[i][j] = this[j][i];
                    this[j][i] = temp;
                }
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix6x6 Inverse()     // returns the inverse ( m * m.Inverse() = identity )
        {
            Matrix6x6 invMat = new(this);
            var r = invMat.InverseSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseSelf()        // returns false if determinant is zero
        {
            // 810+6+36 = 852 multiplications
            //				1 division
            double det, invDet;

            // 2x2 sub-determinants required to calculate 6x6 determinant
            var det2_45_01 = mat4[0] * mat5[1] - mat4[1] * mat5[0];
            var det2_45_02 = mat4[0] * mat5[2] - mat4[2] * mat5[0];
            var det2_45_03 = mat4[0] * mat5[3] - mat4[3] * mat5[0];
            var det2_45_04 = mat4[0] * mat5[4] - mat4[4] * mat5[0];
            var det2_45_05 = mat4[0] * mat5[5] - mat4[5] * mat5[0];
            var det2_45_12 = mat4[1] * mat5[2] - mat4[2] * mat5[1];
            var det2_45_13 = mat4[1] * mat5[3] - mat4[3] * mat5[1];
            var det2_45_14 = mat4[1] * mat5[4] - mat4[4] * mat5[1];
            var det2_45_15 = mat4[1] * mat5[5] - mat4[5] * mat5[1];
            var det2_45_23 = mat4[2] * mat5[3] - mat4[3] * mat5[2];
            var det2_45_24 = mat4[2] * mat5[4] - mat4[4] * mat5[2];
            var det2_45_25 = mat4[2] * mat5[5] - mat4[5] * mat5[2];
            var det2_45_34 = mat4[3] * mat5[4] - mat4[4] * mat5[3];
            var det2_45_35 = mat4[3] * mat5[5] - mat4[5] * mat5[3];
            var det2_45_45 = mat4[4] * mat5[5] - mat4[5] * mat5[4];

            // 3x3 sub-determinants required to calculate 6x6 determinant
            var det3_345_012 = mat3[0] * det2_45_12 - mat3[1] * det2_45_02 + mat3[2] * det2_45_01;
            var det3_345_013 = mat3[0] * det2_45_13 - mat3[1] * det2_45_03 + mat3[3] * det2_45_01;
            var det3_345_014 = mat3[0] * det2_45_14 - mat3[1] * det2_45_04 + mat3[4] * det2_45_01;
            var det3_345_015 = mat3[0] * det2_45_15 - mat3[1] * det2_45_05 + mat3[5] * det2_45_01;
            var det3_345_023 = mat3[0] * det2_45_23 - mat3[2] * det2_45_03 + mat3[3] * det2_45_02;
            var det3_345_024 = mat3[0] * det2_45_24 - mat3[2] * det2_45_04 + mat3[4] * det2_45_02;
            var det3_345_025 = mat3[0] * det2_45_25 - mat3[2] * det2_45_05 + mat3[5] * det2_45_02;
            var det3_345_034 = mat3[0] * det2_45_34 - mat3[3] * det2_45_04 + mat3[4] * det2_45_03;
            var det3_345_035 = mat3[0] * det2_45_35 - mat3[3] * det2_45_05 + mat3[5] * det2_45_03;
            var det3_345_045 = mat3[0] * det2_45_45 - mat3[4] * det2_45_05 + mat3[5] * det2_45_04;
            var det3_345_123 = mat3[1] * det2_45_23 - mat3[2] * det2_45_13 + mat3[3] * det2_45_12;
            var det3_345_124 = mat3[1] * det2_45_24 - mat3[2] * det2_45_14 + mat3[4] * det2_45_12;
            var det3_345_125 = mat3[1] * det2_45_25 - mat3[2] * det2_45_15 + mat3[5] * det2_45_12;
            var det3_345_134 = mat3[1] * det2_45_34 - mat3[3] * det2_45_14 + mat3[4] * det2_45_13;
            var det3_345_135 = mat3[1] * det2_45_35 - mat3[3] * det2_45_15 + mat3[5] * det2_45_13;
            var det3_345_145 = mat3[1] * det2_45_45 - mat3[4] * det2_45_15 + mat3[5] * det2_45_14;
            var det3_345_234 = mat3[2] * det2_45_34 - mat3[3] * det2_45_24 + mat3[4] * det2_45_23;
            var det3_345_235 = mat3[2] * det2_45_35 - mat3[3] * det2_45_25 + mat3[5] * det2_45_23;
            var det3_345_245 = mat3[2] * det2_45_45 - mat3[4] * det2_45_25 + mat3[5] * det2_45_24;
            var det3_345_345 = mat3[3] * det2_45_45 - mat3[4] * det2_45_35 + mat3[5] * det2_45_34;

            // 4x4 sub-determinants required to calculate 6x6 determinant
            var det4_2345_0123 = mat2[0] * det3_345_123 - mat2[1] * det3_345_023 + mat2[2] * det3_345_013 - mat2[3] * det3_345_012;
            var det4_2345_0124 = mat2[0] * det3_345_124 - mat2[1] * det3_345_024 + mat2[2] * det3_345_014 - mat2[4] * det3_345_012;
            var det4_2345_0125 = mat2[0] * det3_345_125 - mat2[1] * det3_345_025 + mat2[2] * det3_345_015 - mat2[5] * det3_345_012;
            var det4_2345_0134 = mat2[0] * det3_345_134 - mat2[1] * det3_345_034 + mat2[3] * det3_345_014 - mat2[4] * det3_345_013;
            var det4_2345_0135 = mat2[0] * det3_345_135 - mat2[1] * det3_345_035 + mat2[3] * det3_345_015 - mat2[5] * det3_345_013;
            var det4_2345_0145 = mat2[0] * det3_345_145 - mat2[1] * det3_345_045 + mat2[4] * det3_345_015 - mat2[5] * det3_345_014;
            var det4_2345_0234 = mat2[0] * det3_345_234 - mat2[2] * det3_345_034 + mat2[3] * det3_345_024 - mat2[4] * det3_345_023;
            var det4_2345_0235 = mat2[0] * det3_345_235 - mat2[2] * det3_345_035 + mat2[3] * det3_345_025 - mat2[5] * det3_345_023;
            var det4_2345_0245 = mat2[0] * det3_345_245 - mat2[2] * det3_345_045 + mat2[4] * det3_345_025 - mat2[5] * det3_345_024;
            var det4_2345_0345 = mat2[0] * det3_345_345 - mat2[3] * det3_345_045 + mat2[4] * det3_345_035 - mat2[5] * det3_345_034;
            var det4_2345_1234 = mat2[1] * det3_345_234 - mat2[2] * det3_345_134 + mat2[3] * det3_345_124 - mat2[4] * det3_345_123;
            var det4_2345_1235 = mat2[1] * det3_345_235 - mat2[2] * det3_345_135 + mat2[3] * det3_345_125 - mat2[5] * det3_345_123;
            var det4_2345_1245 = mat2[1] * det3_345_245 - mat2[2] * det3_345_145 + mat2[4] * det3_345_125 - mat2[5] * det3_345_124;
            var det4_2345_1345 = mat2[1] * det3_345_345 - mat2[3] * det3_345_145 + mat2[4] * det3_345_135 - mat2[5] * det3_345_134;
            var det4_2345_2345 = mat2[2] * det3_345_345 - mat2[3] * det3_345_245 + mat2[4] * det3_345_235 - mat2[5] * det3_345_234;

            // 5x5 sub-determinants required to calculate 6x6 determinant
            var det5_12345_01234 = mat1[0] * det4_2345_1234 - mat1[1] * det4_2345_0234 + mat1[2] * det4_2345_0134 - mat1[3] * det4_2345_0124 + mat1[4] * det4_2345_0123;
            var det5_12345_01235 = mat1[0] * det4_2345_1235 - mat1[1] * det4_2345_0235 + mat1[2] * det4_2345_0135 - mat1[3] * det4_2345_0125 + mat1[5] * det4_2345_0123;
            var det5_12345_01245 = mat1[0] * det4_2345_1245 - mat1[1] * det4_2345_0245 + mat1[2] * det4_2345_0145 - mat1[4] * det4_2345_0125 + mat1[5] * det4_2345_0124;
            var det5_12345_01345 = mat1[0] * det4_2345_1345 - mat1[1] * det4_2345_0345 + mat1[3] * det4_2345_0145 - mat1[4] * det4_2345_0135 + mat1[5] * det4_2345_0134;
            var det5_12345_02345 = mat1[0] * det4_2345_2345 - mat1[2] * det4_2345_0345 + mat1[3] * det4_2345_0245 - mat1[4] * det4_2345_0235 + mat1[5] * det4_2345_0234;
            var det5_12345_12345 = mat1[1] * det4_2345_2345 - mat1[2] * det4_2345_1345 + mat1[3] * det4_2345_1245 - mat1[4] * det4_2345_1235 + mat1[5] * det4_2345_1234;

            // determinant of 6x6 matrix
            det = mat0[0] * det5_12345_12345
                - mat0[1] * det5_12345_02345
                + mat0[2] * det5_12345_01345
                - mat0[3] * det5_12345_01245
                + mat0[4] * det5_12345_01235
                - mat0[5] * det5_12345_01234;

            if (MathX.Fabs(det) < MatrixX.INVERSE_EPSILON) return false;

            invDet = 1f / det;

            // remaining 2x2 sub-determinants
            var det2_34_01 = mat3[0] * mat4[1] - mat3[1] * mat4[0];
            var det2_34_02 = mat3[0] * mat4[2] - mat3[2] * mat4[0];
            var det2_34_03 = mat3[0] * mat4[3] - mat3[3] * mat4[0];
            var det2_34_04 = mat3[0] * mat4[4] - mat3[4] * mat4[0];
            var det2_34_05 = mat3[0] * mat4[5] - mat3[5] * mat4[0];
            var det2_34_12 = mat3[1] * mat4[2] - mat3[2] * mat4[1];
            var det2_34_13 = mat3[1] * mat4[3] - mat3[3] * mat4[1];
            var det2_34_14 = mat3[1] * mat4[4] - mat3[4] * mat4[1];
            var det2_34_15 = mat3[1] * mat4[5] - mat3[5] * mat4[1];
            var det2_34_23 = mat3[2] * mat4[3] - mat3[3] * mat4[2];
            var det2_34_24 = mat3[2] * mat4[4] - mat3[4] * mat4[2];
            var det2_34_25 = mat3[2] * mat4[5] - mat3[5] * mat4[2];
            var det2_34_34 = mat3[3] * mat4[4] - mat3[4] * mat4[3];
            var det2_34_35 = mat3[3] * mat4[5] - mat3[5] * mat4[3];
            var det2_34_45 = mat3[4] * mat4[5] - mat3[5] * mat4[4];
            var det2_35_01 = mat3[0] * mat5[1] - mat3[1] * mat5[0];
            var det2_35_02 = mat3[0] * mat5[2] - mat3[2] * mat5[0];
            var det2_35_03 = mat3[0] * mat5[3] - mat3[3] * mat5[0];
            var det2_35_04 = mat3[0] * mat5[4] - mat3[4] * mat5[0];
            var det2_35_05 = mat3[0] * mat5[5] - mat3[5] * mat5[0];
            var det2_35_12 = mat3[1] * mat5[2] - mat3[2] * mat5[1];
            var det2_35_13 = mat3[1] * mat5[3] - mat3[3] * mat5[1];
            var det2_35_14 = mat3[1] * mat5[4] - mat3[4] * mat5[1];
            var det2_35_15 = mat3[1] * mat5[5] - mat3[5] * mat5[1];
            var det2_35_23 = mat3[2] * mat5[3] - mat3[3] * mat5[2];
            var det2_35_24 = mat3[2] * mat5[4] - mat3[4] * mat5[2];
            var det2_35_25 = mat3[2] * mat5[5] - mat3[5] * mat5[2];
            var det2_35_34 = mat3[3] * mat5[4] - mat3[4] * mat5[3];
            var det2_35_35 = mat3[3] * mat5[5] - mat3[5] * mat5[3];
            var det2_35_45 = mat3[4] * mat5[5] - mat3[5] * mat5[4];

            // remaining 3x3 sub-determinants
            var det3_234_012 = mat2[0] * det2_34_12 - mat2[1] * det2_34_02 + mat2[2] * det2_34_01;
            var det3_234_013 = mat2[0] * det2_34_13 - mat2[1] * det2_34_03 + mat2[3] * det2_34_01;
            var det3_234_014 = mat2[0] * det2_34_14 - mat2[1] * det2_34_04 + mat2[4] * det2_34_01;
            var det3_234_015 = mat2[0] * det2_34_15 - mat2[1] * det2_34_05 + mat2[5] * det2_34_01;
            var det3_234_023 = mat2[0] * det2_34_23 - mat2[2] * det2_34_03 + mat2[3] * det2_34_02;
            var det3_234_024 = mat2[0] * det2_34_24 - mat2[2] * det2_34_04 + mat2[4] * det2_34_02;
            var det3_234_025 = mat2[0] * det2_34_25 - mat2[2] * det2_34_05 + mat2[5] * det2_34_02;
            var det3_234_034 = mat2[0] * det2_34_34 - mat2[3] * det2_34_04 + mat2[4] * det2_34_03;
            var det3_234_035 = mat2[0] * det2_34_35 - mat2[3] * det2_34_05 + mat2[5] * det2_34_03;
            var det3_234_045 = mat2[0] * det2_34_45 - mat2[4] * det2_34_05 + mat2[5] * det2_34_04;
            var det3_234_123 = mat2[1] * det2_34_23 - mat2[2] * det2_34_13 + mat2[3] * det2_34_12;
            var det3_234_124 = mat2[1] * det2_34_24 - mat2[2] * det2_34_14 + mat2[4] * det2_34_12;
            var det3_234_125 = mat2[1] * det2_34_25 - mat2[2] * det2_34_15 + mat2[5] * det2_34_12;
            var det3_234_134 = mat2[1] * det2_34_34 - mat2[3] * det2_34_14 + mat2[4] * det2_34_13;
            var det3_234_135 = mat2[1] * det2_34_35 - mat2[3] * det2_34_15 + mat2[5] * det2_34_13;
            var det3_234_145 = mat2[1] * det2_34_45 - mat2[4] * det2_34_15 + mat2[5] * det2_34_14;
            var det3_234_234 = mat2[2] * det2_34_34 - mat2[3] * det2_34_24 + mat2[4] * det2_34_23;
            var det3_234_235 = mat2[2] * det2_34_35 - mat2[3] * det2_34_25 + mat2[5] * det2_34_23;
            var det3_234_245 = mat2[2] * det2_34_45 - mat2[4] * det2_34_25 + mat2[5] * det2_34_24;
            var det3_234_345 = mat2[3] * det2_34_45 - mat2[4] * det2_34_35 + mat2[5] * det2_34_34;
            var det3_235_012 = mat2[0] * det2_35_12 - mat2[1] * det2_35_02 + mat2[2] * det2_35_01;
            var det3_235_013 = mat2[0] * det2_35_13 - mat2[1] * det2_35_03 + mat2[3] * det2_35_01;
            var det3_235_014 = mat2[0] * det2_35_14 - mat2[1] * det2_35_04 + mat2[4] * det2_35_01;
            var det3_235_015 = mat2[0] * det2_35_15 - mat2[1] * det2_35_05 + mat2[5] * det2_35_01;
            var det3_235_023 = mat2[0] * det2_35_23 - mat2[2] * det2_35_03 + mat2[3] * det2_35_02;
            var det3_235_024 = mat2[0] * det2_35_24 - mat2[2] * det2_35_04 + mat2[4] * det2_35_02;
            var det3_235_025 = mat2[0] * det2_35_25 - mat2[2] * det2_35_05 + mat2[5] * det2_35_02;
            var det3_235_034 = mat2[0] * det2_35_34 - mat2[3] * det2_35_04 + mat2[4] * det2_35_03;
            var det3_235_035 = mat2[0] * det2_35_35 - mat2[3] * det2_35_05 + mat2[5] * det2_35_03;
            var det3_235_045 = mat2[0] * det2_35_45 - mat2[4] * det2_35_05 + mat2[5] * det2_35_04;
            var det3_235_123 = mat2[1] * det2_35_23 - mat2[2] * det2_35_13 + mat2[3] * det2_35_12;
            var det3_235_124 = mat2[1] * det2_35_24 - mat2[2] * det2_35_14 + mat2[4] * det2_35_12;
            var det3_235_125 = mat2[1] * det2_35_25 - mat2[2] * det2_35_15 + mat2[5] * det2_35_12;
            var det3_235_134 = mat2[1] * det2_35_34 - mat2[3] * det2_35_14 + mat2[4] * det2_35_13;
            var det3_235_135 = mat2[1] * det2_35_35 - mat2[3] * det2_35_15 + mat2[5] * det2_35_13;
            var det3_235_145 = mat2[1] * det2_35_45 - mat2[4] * det2_35_15 + mat2[5] * det2_35_14;
            var det3_235_234 = mat2[2] * det2_35_34 - mat2[3] * det2_35_24 + mat2[4] * det2_35_23;
            var det3_235_235 = mat2[2] * det2_35_35 - mat2[3] * det2_35_25 + mat2[5] * det2_35_23;
            var det3_235_245 = mat2[2] * det2_35_45 - mat2[4] * det2_35_25 + mat2[5] * det2_35_24;
            var det3_235_345 = mat2[3] * det2_35_45 - mat2[4] * det2_35_35 + mat2[5] * det2_35_34;
            var det3_245_012 = mat2[0] * det2_45_12 - mat2[1] * det2_45_02 + mat2[2] * det2_45_01;
            var det3_245_013 = mat2[0] * det2_45_13 - mat2[1] * det2_45_03 + mat2[3] * det2_45_01;
            var det3_245_014 = mat2[0] * det2_45_14 - mat2[1] * det2_45_04 + mat2[4] * det2_45_01;
            var det3_245_015 = mat2[0] * det2_45_15 - mat2[1] * det2_45_05 + mat2[5] * det2_45_01;
            var det3_245_023 = mat2[0] * det2_45_23 - mat2[2] * det2_45_03 + mat2[3] * det2_45_02;
            var det3_245_024 = mat2[0] * det2_45_24 - mat2[2] * det2_45_04 + mat2[4] * det2_45_02;
            var det3_245_025 = mat2[0] * det2_45_25 - mat2[2] * det2_45_05 + mat2[5] * det2_45_02;
            var det3_245_034 = mat2[0] * det2_45_34 - mat2[3] * det2_45_04 + mat2[4] * det2_45_03;
            var det3_245_035 = mat2[0] * det2_45_35 - mat2[3] * det2_45_05 + mat2[5] * det2_45_03;
            var det3_245_045 = mat2[0] * det2_45_45 - mat2[4] * det2_45_05 + mat2[5] * det2_45_04;
            var det3_245_123 = mat2[1] * det2_45_23 - mat2[2] * det2_45_13 + mat2[3] * det2_45_12;
            var det3_245_124 = mat2[1] * det2_45_24 - mat2[2] * det2_45_14 + mat2[4] * det2_45_12;
            var det3_245_125 = mat2[1] * det2_45_25 - mat2[2] * det2_45_15 + mat2[5] * det2_45_12;
            var det3_245_134 = mat2[1] * det2_45_34 - mat2[3] * det2_45_14 + mat2[4] * det2_45_13;
            var det3_245_135 = mat2[1] * det2_45_35 - mat2[3] * det2_45_15 + mat2[5] * det2_45_13;
            var det3_245_145 = mat2[1] * det2_45_45 - mat2[4] * det2_45_15 + mat2[5] * det2_45_14;
            var det3_245_234 = mat2[2] * det2_45_34 - mat2[3] * det2_45_24 + mat2[4] * det2_45_23;
            var det3_245_235 = mat2[2] * det2_45_35 - mat2[3] * det2_45_25 + mat2[5] * det2_45_23;
            var det3_245_245 = mat2[2] * det2_45_45 - mat2[4] * det2_45_25 + mat2[5] * det2_45_24;
            var det3_245_345 = mat2[3] * det2_45_45 - mat2[4] * det2_45_35 + mat2[5] * det2_45_34;

            // remaining 4x4 sub-determinants
            var det4_1234_0123 = mat1[0] * det3_234_123 - mat1[1] * det3_234_023 + mat1[2] * det3_234_013 - mat1[3] * det3_234_012;
            var det4_1234_0124 = mat1[0] * det3_234_124 - mat1[1] * det3_234_024 + mat1[2] * det3_234_014 - mat1[4] * det3_234_012;
            var det4_1234_0125 = mat1[0] * det3_234_125 - mat1[1] * det3_234_025 + mat1[2] * det3_234_015 - mat1[5] * det3_234_012;
            var det4_1234_0134 = mat1[0] * det3_234_134 - mat1[1] * det3_234_034 + mat1[3] * det3_234_014 - mat1[4] * det3_234_013;
            var det4_1234_0135 = mat1[0] * det3_234_135 - mat1[1] * det3_234_035 + mat1[3] * det3_234_015 - mat1[5] * det3_234_013;
            var det4_1234_0145 = mat1[0] * det3_234_145 - mat1[1] * det3_234_045 + mat1[4] * det3_234_015 - mat1[5] * det3_234_014;
            var det4_1234_0234 = mat1[0] * det3_234_234 - mat1[2] * det3_234_034 + mat1[3] * det3_234_024 - mat1[4] * det3_234_023;
            var det4_1234_0235 = mat1[0] * det3_234_235 - mat1[2] * det3_234_035 + mat1[3] * det3_234_025 - mat1[5] * det3_234_023;
            var det4_1234_0245 = mat1[0] * det3_234_245 - mat1[2] * det3_234_045 + mat1[4] * det3_234_025 - mat1[5] * det3_234_024;
            var det4_1234_0345 = mat1[0] * det3_234_345 - mat1[3] * det3_234_045 + mat1[4] * det3_234_035 - mat1[5] * det3_234_034;
            var det4_1234_1234 = mat1[1] * det3_234_234 - mat1[2] * det3_234_134 + mat1[3] * det3_234_124 - mat1[4] * det3_234_123;
            var det4_1234_1235 = mat1[1] * det3_234_235 - mat1[2] * det3_234_135 + mat1[3] * det3_234_125 - mat1[5] * det3_234_123;
            var det4_1234_1245 = mat1[1] * det3_234_245 - mat1[2] * det3_234_145 + mat1[4] * det3_234_125 - mat1[5] * det3_234_124;
            var det4_1234_1345 = mat1[1] * det3_234_345 - mat1[3] * det3_234_145 + mat1[4] * det3_234_135 - mat1[5] * det3_234_134;
            var det4_1234_2345 = mat1[2] * det3_234_345 - mat1[3] * det3_234_245 + mat1[4] * det3_234_235 - mat1[5] * det3_234_234;
            var det4_1235_0123 = mat1[0] * det3_235_123 - mat1[1] * det3_235_023 + mat1[2] * det3_235_013 - mat1[3] * det3_235_012;
            var det4_1235_0124 = mat1[0] * det3_235_124 - mat1[1] * det3_235_024 + mat1[2] * det3_235_014 - mat1[4] * det3_235_012;
            var det4_1235_0125 = mat1[0] * det3_235_125 - mat1[1] * det3_235_025 + mat1[2] * det3_235_015 - mat1[5] * det3_235_012;
            var det4_1235_0134 = mat1[0] * det3_235_134 - mat1[1] * det3_235_034 + mat1[3] * det3_235_014 - mat1[4] * det3_235_013;
            var det4_1235_0135 = mat1[0] * det3_235_135 - mat1[1] * det3_235_035 + mat1[3] * det3_235_015 - mat1[5] * det3_235_013;
            var det4_1235_0145 = mat1[0] * det3_235_145 - mat1[1] * det3_235_045 + mat1[4] * det3_235_015 - mat1[5] * det3_235_014;
            var det4_1235_0234 = mat1[0] * det3_235_234 - mat1[2] * det3_235_034 + mat1[3] * det3_235_024 - mat1[4] * det3_235_023;
            var det4_1235_0235 = mat1[0] * det3_235_235 - mat1[2] * det3_235_035 + mat1[3] * det3_235_025 - mat1[5] * det3_235_023;
            var det4_1235_0245 = mat1[0] * det3_235_245 - mat1[2] * det3_235_045 + mat1[4] * det3_235_025 - mat1[5] * det3_235_024;
            var det4_1235_0345 = mat1[0] * det3_235_345 - mat1[3] * det3_235_045 + mat1[4] * det3_235_035 - mat1[5] * det3_235_034;
            var det4_1235_1234 = mat1[1] * det3_235_234 - mat1[2] * det3_235_134 + mat1[3] * det3_235_124 - mat1[4] * det3_235_123;
            var det4_1235_1235 = mat1[1] * det3_235_235 - mat1[2] * det3_235_135 + mat1[3] * det3_235_125 - mat1[5] * det3_235_123;
            var det4_1235_1245 = mat1[1] * det3_235_245 - mat1[2] * det3_235_145 + mat1[4] * det3_235_125 - mat1[5] * det3_235_124;
            var det4_1235_1345 = mat1[1] * det3_235_345 - mat1[3] * det3_235_145 + mat1[4] * det3_235_135 - mat1[5] * det3_235_134;
            var det4_1235_2345 = mat1[2] * det3_235_345 - mat1[3] * det3_235_245 + mat1[4] * det3_235_235 - mat1[5] * det3_235_234;
            var det4_1245_0123 = mat1[0] * det3_245_123 - mat1[1] * det3_245_023 + mat1[2] * det3_245_013 - mat1[3] * det3_245_012;
            var det4_1245_0124 = mat1[0] * det3_245_124 - mat1[1] * det3_245_024 + mat1[2] * det3_245_014 - mat1[4] * det3_245_012;
            var det4_1245_0125 = mat1[0] * det3_245_125 - mat1[1] * det3_245_025 + mat1[2] * det3_245_015 - mat1[5] * det3_245_012;
            var det4_1245_0134 = mat1[0] * det3_245_134 - mat1[1] * det3_245_034 + mat1[3] * det3_245_014 - mat1[4] * det3_245_013;
            var det4_1245_0135 = mat1[0] * det3_245_135 - mat1[1] * det3_245_035 + mat1[3] * det3_245_015 - mat1[5] * det3_245_013;
            var det4_1245_0145 = mat1[0] * det3_245_145 - mat1[1] * det3_245_045 + mat1[4] * det3_245_015 - mat1[5] * det3_245_014;
            var det4_1245_0234 = mat1[0] * det3_245_234 - mat1[2] * det3_245_034 + mat1[3] * det3_245_024 - mat1[4] * det3_245_023;
            var det4_1245_0235 = mat1[0] * det3_245_235 - mat1[2] * det3_245_035 + mat1[3] * det3_245_025 - mat1[5] * det3_245_023;
            var det4_1245_0245 = mat1[0] * det3_245_245 - mat1[2] * det3_245_045 + mat1[4] * det3_245_025 - mat1[5] * det3_245_024;
            var det4_1245_0345 = mat1[0] * det3_245_345 - mat1[3] * det3_245_045 + mat1[4] * det3_245_035 - mat1[5] * det3_245_034;
            var det4_1245_1234 = mat1[1] * det3_245_234 - mat1[2] * det3_245_134 + mat1[3] * det3_245_124 - mat1[4] * det3_245_123;
            var det4_1245_1235 = mat1[1] * det3_245_235 - mat1[2] * det3_245_135 + mat1[3] * det3_245_125 - mat1[5] * det3_245_123;
            var det4_1245_1245 = mat1[1] * det3_245_245 - mat1[2] * det3_245_145 + mat1[4] * det3_245_125 - mat1[5] * det3_245_124;
            var det4_1245_1345 = mat1[1] * det3_245_345 - mat1[3] * det3_245_145 + mat1[4] * det3_245_135 - mat1[5] * det3_245_134;
            var det4_1245_2345 = mat1[2] * det3_245_345 - mat1[3] * det3_245_245 + mat1[4] * det3_245_235 - mat1[5] * det3_245_234;
            var det4_1345_0123 = mat1[0] * det3_345_123 - mat1[1] * det3_345_023 + mat1[2] * det3_345_013 - mat1[3] * det3_345_012;
            var det4_1345_0124 = mat1[0] * det3_345_124 - mat1[1] * det3_345_024 + mat1[2] * det3_345_014 - mat1[4] * det3_345_012;
            var det4_1345_0125 = mat1[0] * det3_345_125 - mat1[1] * det3_345_025 + mat1[2] * det3_345_015 - mat1[5] * det3_345_012;
            var det4_1345_0134 = mat1[0] * det3_345_134 - mat1[1] * det3_345_034 + mat1[3] * det3_345_014 - mat1[4] * det3_345_013;
            var det4_1345_0135 = mat1[0] * det3_345_135 - mat1[1] * det3_345_035 + mat1[3] * det3_345_015 - mat1[5] * det3_345_013;
            var det4_1345_0145 = mat1[0] * det3_345_145 - mat1[1] * det3_345_045 + mat1[4] * det3_345_015 - mat1[5] * det3_345_014;
            var det4_1345_0234 = mat1[0] * det3_345_234 - mat1[2] * det3_345_034 + mat1[3] * det3_345_024 - mat1[4] * det3_345_023;
            var det4_1345_0235 = mat1[0] * det3_345_235 - mat1[2] * det3_345_035 + mat1[3] * det3_345_025 - mat1[5] * det3_345_023;
            var det4_1345_0245 = mat1[0] * det3_345_245 - mat1[2] * det3_345_045 + mat1[4] * det3_345_025 - mat1[5] * det3_345_024;
            var det4_1345_0345 = mat1[0] * det3_345_345 - mat1[3] * det3_345_045 + mat1[4] * det3_345_035 - mat1[5] * det3_345_034;
            var det4_1345_1234 = mat1[1] * det3_345_234 - mat1[2] * det3_345_134 + mat1[3] * det3_345_124 - mat1[4] * det3_345_123;
            var det4_1345_1235 = mat1[1] * det3_345_235 - mat1[2] * det3_345_135 + mat1[3] * det3_345_125 - mat1[5] * det3_345_123;
            var det4_1345_1245 = mat1[1] * det3_345_245 - mat1[2] * det3_345_145 + mat1[4] * det3_345_125 - mat1[5] * det3_345_124;
            var det4_1345_1345 = mat1[1] * det3_345_345 - mat1[3] * det3_345_145 + mat1[4] * det3_345_135 - mat1[5] * det3_345_134;
            var det4_1345_2345 = mat1[2] * det3_345_345 - mat1[3] * det3_345_245 + mat1[4] * det3_345_235 - mat1[5] * det3_345_234;

            // remaining 5x5 sub-determinants
            var det5_01234_01234 = mat0[0] * det4_1234_1234 - mat0[1] * det4_1234_0234 + mat0[2] * det4_1234_0134 - mat0[3] * det4_1234_0124 + mat0[4] * det4_1234_0123;
            var det5_01234_01235 = mat0[0] * det4_1234_1235 - mat0[1] * det4_1234_0235 + mat0[2] * det4_1234_0135 - mat0[3] * det4_1234_0125 + mat0[5] * det4_1234_0123;
            var det5_01234_01245 = mat0[0] * det4_1234_1245 - mat0[1] * det4_1234_0245 + mat0[2] * det4_1234_0145 - mat0[4] * det4_1234_0125 + mat0[5] * det4_1234_0124;
            var det5_01234_01345 = mat0[0] * det4_1234_1345 - mat0[1] * det4_1234_0345 + mat0[3] * det4_1234_0145 - mat0[4] * det4_1234_0135 + mat0[5] * det4_1234_0134;
            var det5_01234_02345 = mat0[0] * det4_1234_2345 - mat0[2] * det4_1234_0345 + mat0[3] * det4_1234_0245 - mat0[4] * det4_1234_0235 + mat0[5] * det4_1234_0234;
            var det5_01234_12345 = mat0[1] * det4_1234_2345 - mat0[2] * det4_1234_1345 + mat0[3] * det4_1234_1245 - mat0[4] * det4_1234_1235 + mat0[5] * det4_1234_1234;
            var det5_01235_01234 = mat0[0] * det4_1235_1234 - mat0[1] * det4_1235_0234 + mat0[2] * det4_1235_0134 - mat0[3] * det4_1235_0124 + mat0[4] * det4_1235_0123;
            var det5_01235_01235 = mat0[0] * det4_1235_1235 - mat0[1] * det4_1235_0235 + mat0[2] * det4_1235_0135 - mat0[3] * det4_1235_0125 + mat0[5] * det4_1235_0123;
            var det5_01235_01245 = mat0[0] * det4_1235_1245 - mat0[1] * det4_1235_0245 + mat0[2] * det4_1235_0145 - mat0[4] * det4_1235_0125 + mat0[5] * det4_1235_0124;
            var det5_01235_01345 = mat0[0] * det4_1235_1345 - mat0[1] * det4_1235_0345 + mat0[3] * det4_1235_0145 - mat0[4] * det4_1235_0135 + mat0[5] * det4_1235_0134;
            var det5_01235_02345 = mat0[0] * det4_1235_2345 - mat0[2] * det4_1235_0345 + mat0[3] * det4_1235_0245 - mat0[4] * det4_1235_0235 + mat0[5] * det4_1235_0234;
            var det5_01235_12345 = mat0[1] * det4_1235_2345 - mat0[2] * det4_1235_1345 + mat0[3] * det4_1235_1245 - mat0[4] * det4_1235_1235 + mat0[5] * det4_1235_1234;
            var det5_01245_01234 = mat0[0] * det4_1245_1234 - mat0[1] * det4_1245_0234 + mat0[2] * det4_1245_0134 - mat0[3] * det4_1245_0124 + mat0[4] * det4_1245_0123;
            var det5_01245_01235 = mat0[0] * det4_1245_1235 - mat0[1] * det4_1245_0235 + mat0[2] * det4_1245_0135 - mat0[3] * det4_1245_0125 + mat0[5] * det4_1245_0123;
            var det5_01245_01245 = mat0[0] * det4_1245_1245 - mat0[1] * det4_1245_0245 + mat0[2] * det4_1245_0145 - mat0[4] * det4_1245_0125 + mat0[5] * det4_1245_0124;
            var det5_01245_01345 = mat0[0] * det4_1245_1345 - mat0[1] * det4_1245_0345 + mat0[3] * det4_1245_0145 - mat0[4] * det4_1245_0135 + mat0[5] * det4_1245_0134;
            var det5_01245_02345 = mat0[0] * det4_1245_2345 - mat0[2] * det4_1245_0345 + mat0[3] * det4_1245_0245 - mat0[4] * det4_1245_0235 + mat0[5] * det4_1245_0234;
            var det5_01245_12345 = mat0[1] * det4_1245_2345 - mat0[2] * det4_1245_1345 + mat0[3] * det4_1245_1245 - mat0[4] * det4_1245_1235 + mat0[5] * det4_1245_1234;
            var det5_01345_01234 = mat0[0] * det4_1345_1234 - mat0[1] * det4_1345_0234 + mat0[2] * det4_1345_0134 - mat0[3] * det4_1345_0124 + mat0[4] * det4_1345_0123;
            var det5_01345_01235 = mat0[0] * det4_1345_1235 - mat0[1] * det4_1345_0235 + mat0[2] * det4_1345_0135 - mat0[3] * det4_1345_0125 + mat0[5] * det4_1345_0123;
            var det5_01345_01245 = mat0[0] * det4_1345_1245 - mat0[1] * det4_1345_0245 + mat0[2] * det4_1345_0145 - mat0[4] * det4_1345_0125 + mat0[5] * det4_1345_0124;
            var det5_01345_01345 = mat0[0] * det4_1345_1345 - mat0[1] * det4_1345_0345 + mat0[3] * det4_1345_0145 - mat0[4] * det4_1345_0135 + mat0[5] * det4_1345_0134;
            var det5_01345_02345 = mat0[0] * det4_1345_2345 - mat0[2] * det4_1345_0345 + mat0[3] * det4_1345_0245 - mat0[4] * det4_1345_0235 + mat0[5] * det4_1345_0234;
            var det5_01345_12345 = mat0[1] * det4_1345_2345 - mat0[2] * det4_1345_1345 + mat0[3] * det4_1345_1245 - mat0[4] * det4_1345_1235 + mat0[5] * det4_1345_1234;
            var det5_02345_01234 = mat0[0] * det4_2345_1234 - mat0[1] * det4_2345_0234 + mat0[2] * det4_2345_0134 - mat0[3] * det4_2345_0124 + mat0[4] * det4_2345_0123;
            var det5_02345_01235 = mat0[0] * det4_2345_1235 - mat0[1] * det4_2345_0235 + mat0[2] * det4_2345_0135 - mat0[3] * det4_2345_0125 + mat0[5] * det4_2345_0123;
            var det5_02345_01245 = mat0[0] * det4_2345_1245 - mat0[1] * det4_2345_0245 + mat0[2] * det4_2345_0145 - mat0[4] * det4_2345_0125 + mat0[5] * det4_2345_0124;
            var det5_02345_01345 = mat0[0] * det4_2345_1345 - mat0[1] * det4_2345_0345 + mat0[3] * det4_2345_0145 - mat0[4] * det4_2345_0135 + mat0[5] * det4_2345_0134;
            var det5_02345_02345 = mat0[0] * det4_2345_2345 - mat0[2] * det4_2345_0345 + mat0[3] * det4_2345_0245 - mat0[4] * det4_2345_0235 + mat0[5] * det4_2345_0234;
            var det5_02345_12345 = mat0[1] * det4_2345_2345 - mat0[2] * det4_2345_1345 + mat0[3] * det4_2345_1245 - mat0[4] * det4_2345_1235 + mat0[5] * det4_2345_1234;

            mat0[0] = (float)(det5_12345_12345 * invDet); mat0[1] = (float)(-det5_02345_12345 * invDet); mat0[2] = (float)(det5_01345_12345 * invDet); mat0[3] = (float)(-det5_01245_12345 * invDet); mat0[4] = (float)(det5_01235_12345 * invDet); mat0[5] = (float)(-det5_01234_12345 * invDet);
            mat1[0] = (float)(-det5_12345_02345 * invDet); mat1[1] = (float)(det5_02345_02345 * invDet); mat1[2] = (float)(-det5_01345_02345 * invDet); mat1[3] = (float)(det5_01245_02345 * invDet); mat1[4] = (float)(-det5_01235_02345 * invDet); mat1[5] = (float)(det5_01234_02345 * invDet);
            mat2[0] = (float)(det5_12345_01345 * invDet); mat2[1] = (float)(-det5_02345_01345 * invDet); mat2[2] = (float)(det5_01345_01345 * invDet); mat2[3] = (float)(-det5_01245_01345 * invDet); mat2[4] = (float)(det5_01235_01345 * invDet); mat2[5] = (float)(-det5_01234_01345 * invDet);
            mat3[0] = (float)(-det5_12345_01245 * invDet); mat3[1] = (float)(det5_02345_01245 * invDet); mat3[2] = (float)(-det5_01345_01245 * invDet); mat3[3] = (float)(det5_01245_01245 * invDet); mat3[4] = (float)(-det5_01235_01245 * invDet); mat3[5] = (float)(det5_01234_01245 * invDet);
            mat4[0] = (float)(det5_12345_01235 * invDet); mat4[1] = (float)(-det5_02345_01235 * invDet); mat4[2] = (float)(det5_01345_01235 * invDet); mat4[3] = (float)(-det5_01245_01235 * invDet); mat4[4] = (float)(det5_01235_01235 * invDet); mat4[5] = (float)(-det5_01234_01235 * invDet);
            mat5[0] = (float)(-det5_12345_01234 * invDet); mat5[1] = (float)(det5_02345_01234 * invDet); mat5[2] = (float)(-det5_01345_01234 * invDet); mat5[3] = (float)(det5_01245_01234 * invDet); mat5[4] = (float)(-det5_01235_01234 * invDet); mat5[5] = (float)(det5_01234_01234 * invDet);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix6x6 InverseFast() // returns the inverse ( m * m.Inverse() = identity )
        {
            Matrix6x6 invMat = new(this);
            var r = invMat.InverseFastSelf();
            Debug.Assert(r);
            return invMat;
        }
        public bool InverseFastSelf()    // returns false if determinant is zero
        {
            // 6*27+2*30 = 222 multiplications
            //		2*1  =	 2 divisions
            Matrix3x3 r0 = new(), r1 = new(), r2 = new(), r3 = new(); float c0, c1, c2, det, invDet;

            fixed (float* mat = &this.mat0.p[0])
            {
                // r0 = m0.Inverse();
                c0 = mat[1 * 6 + 1] * mat[2 * 6 + 2] - mat[1 * 6 + 2] * mat[2 * 6 + 1];
                c1 = mat[1 * 6 + 2] * mat[2 * 6 + 0] - mat[1 * 6 + 0] * mat[2 * 6 + 2];
                c2 = mat[1 * 6 + 0] * mat[2 * 6 + 1] - mat[1 * 6 + 1] * mat[2 * 6 + 0];

                det = mat[0 * 6 + 0] * c0 + mat[0 * 6 + 1] * c1 + mat[0 * 6 + 2] * c2;
                if (MathX.Fabs(det) < MatrixX.INVERSE_EPSILON) return false;

                invDet = 1f / det;

                r0.mat0.x = c0 * invDet;
                r0.mat0.y = (mat[0 * 6 + 2] * mat[2 * 6 + 1] - mat[0 * 6 + 1] * mat[2 * 6 + 2]) * invDet;
                r0.mat0.z = (mat[0 * 6 + 1] * mat[1 * 6 + 2] - mat[0 * 6 + 2] * mat[1 * 6 + 1]) * invDet;
                r0.mat1.x = c1 * invDet;
                r0.mat1.y = (mat[0 * 6 + 0] * mat[2 * 6 + 2] - mat[0 * 6 + 2] * mat[2 * 6 + 0]) * invDet;
                r0.mat1.z = (mat[0 * 6 + 2] * mat[1 * 6 + 0] - mat[0 * 6 + 0] * mat[1 * 6 + 2]) * invDet;
                r0.mat2.x = c2 * invDet;
                r0.mat2.y = (mat[0 * 6 + 1] * mat[2 * 6 + 0] - mat[0 * 6 + 0] * mat[2 * 6 + 1]) * invDet;
                r0.mat2.z = (mat[0 * 6 + 0] * mat[1 * 6 + 1] - mat[0 * 6 + 1] * mat[1 * 6 + 0]) * invDet;

                // r1 = r0 * m1;
                r1.mat0.x = r0.mat0.x * mat[0 * 6 + 3] + r0.mat0.y * mat[1 * 6 + 3] + r0.mat0.z * mat[2 * 6 + 3];
                r1.mat0.y = r0.mat0.x * mat[0 * 6 + 4] + r0.mat0.y * mat[1 * 6 + 4] + r0.mat0.z * mat[2 * 6 + 4];
                r1.mat0.z = r0.mat0.x * mat[0 * 6 + 5] + r0.mat0.y * mat[1 * 6 + 5] + r0.mat0.z * mat[2 * 6 + 5];
                r1.mat1.x = r0.mat1.x * mat[0 * 6 + 3] + r0.mat1.y * mat[1 * 6 + 3] + r0.mat1.z * mat[2 * 6 + 3];
                r1.mat1.y = r0.mat1.x * mat[0 * 6 + 4] + r0.mat1.y * mat[1 * 6 + 4] + r0.mat1.z * mat[2 * 6 + 4];
                r1.mat1.z = r0.mat1.x * mat[0 * 6 + 5] + r0.mat1.y * mat[1 * 6 + 5] + r0.mat1.z * mat[2 * 6 + 5];
                r1.mat2.x = r0.mat2.x * mat[0 * 6 + 3] + r0.mat2.y * mat[1 * 6 + 3] + r0.mat2.z * mat[2 * 6 + 3];
                r1.mat2.y = r0.mat2.x * mat[0 * 6 + 4] + r0.mat2.y * mat[1 * 6 + 4] + r0.mat2.z * mat[2 * 6 + 4];
                r1.mat2.z = r0.mat2.x * mat[0 * 6 + 5] + r0.mat2.y * mat[1 * 6 + 5] + r0.mat2.z * mat[2 * 6 + 5];

                // r2 = m2 * r1;
                r2.mat0.x = mat[3 * 6 + 0] * r1.mat0.x + mat[3 * 6 + 1] * r1.mat1.x + mat[3 * 6 + 2] * r1.mat2.x;
                r2.mat0.y = mat[3 * 6 + 0] * r1.mat0.y + mat[3 * 6 + 1] * r1.mat1.y + mat[3 * 6 + 2] * r1.mat2.y;
                r2.mat0.z = mat[3 * 6 + 0] * r1.mat0.z + mat[3 * 6 + 1] * r1.mat1.z + mat[3 * 6 + 2] * r1.mat2.z;
                r2.mat1.x = mat[4 * 6 + 0] * r1.mat0.x + mat[4 * 6 + 1] * r1.mat1.x + mat[4 * 6 + 2] * r1.mat2.x;
                r2.mat1.y = mat[4 * 6 + 0] * r1.mat0.y + mat[4 * 6 + 1] * r1.mat1.y + mat[4 * 6 + 2] * r1.mat2.y;
                r2.mat1.z = mat[4 * 6 + 0] * r1.mat0.z + mat[4 * 6 + 1] * r1.mat1.z + mat[4 * 6 + 2] * r1.mat2.z;
                r2.mat2.x = mat[5 * 6 + 0] * r1.mat0.x + mat[5 * 6 + 1] * r1.mat1.x + mat[5 * 6 + 2] * r1.mat2.x;
                r2.mat2.y = mat[5 * 6 + 0] * r1.mat0.y + mat[5 * 6 + 1] * r1.mat1.y + mat[5 * 6 + 2] * r1.mat2.y;
                r2.mat2.z = mat[5 * 6 + 0] * r1.mat0.z + mat[5 * 6 + 1] * r1.mat1.z + mat[5 * 6 + 2] * r1.mat2.z;

                // r3 = r2 - m3;
                r3.mat0.x = r2.mat0.x - mat[3 * 6 + 3];
                r3.mat0.y = r2.mat0.y - mat[3 * 6 + 4];
                r3.mat0.z = r2.mat0.z - mat[3 * 6 + 5];
                r3.mat1.x = r2.mat1.x - mat[4 * 6 + 3];
                r3.mat1.y = r2.mat1.y - mat[4 * 6 + 4];
                r3.mat1.z = r2.mat1.z - mat[4 * 6 + 5];
                r3.mat2.x = r2.mat2.x - mat[5 * 6 + 3];
                r3.mat2.y = r2.mat2.y - mat[5 * 6 + 4];
                r3.mat2.z = r2.mat2.z - mat[5 * 6 + 5];

                // r3.InverseSelf();
                r2.mat0.x = r3.mat1.y * r3.mat2.z - r3.mat1.z * r3.mat2.y;
                r2.mat1.x = r3.mat1.z * r3.mat2.x - r3.mat1.x * r3.mat2.z;
                r2.mat2.x = r3.mat1.x * r3.mat2.y - r3.mat1.y * r3.mat2.x;

                det = r3.mat0.x * r2.mat0.x + r3.mat0.y * r2.mat1.x + r3.mat0.z * r2.mat2.x;
                if (MathX.Fabs(det) < MatrixX.INVERSE_EPSILON) return false;

                invDet = 1f / det;

                r2.mat0.y = r3.mat0.z * r3.mat2.y - r3.mat0.y * r3.mat2.z;
                r2.mat0.z = r3.mat0.y * r3.mat1.z - r3.mat0.z * r3.mat1.y;
                r2.mat1.y = r3.mat0.x * r3.mat2.z - r3.mat0.z * r3.mat2.x;
                r2.mat1.z = r3.mat0.z * r3.mat1.x - r3.mat0.x * r3.mat1.z;
                r2.mat2.y = r3.mat0.y * r3.mat2.x - r3.mat0.x * r3.mat2.y;
                r2.mat2.z = r3.mat0.x * r3.mat1.y - r3.mat0.y * r3.mat1.x;

                r3.mat0.x = r2.mat0.x * invDet;
                r3.mat0.y = r2.mat0.y * invDet;
                r3.mat0.z = r2.mat0.z * invDet;
                r3.mat1.x = r2.mat1.x * invDet;
                r3.mat1.y = r2.mat1.y * invDet;
                r3.mat1.z = r2.mat1.z * invDet;
                r3.mat2.x = r2.mat2.x * invDet;
                r3.mat2.y = r2.mat2.y * invDet;
                r3.mat2.z = r2.mat2.z * invDet;

                // r2 = m2 * r0;
                r2.mat0.x = mat[3 * 6 + 0] * r0.mat0.x + mat[3 * 6 + 1] * r0.mat1.x + mat[3 * 6 + 2] * r0.mat2.x;
                r2.mat0.y = mat[3 * 6 + 0] * r0.mat0.y + mat[3 * 6 + 1] * r0.mat1.y + mat[3 * 6 + 2] * r0.mat2.y;
                r2.mat0.z = mat[3 * 6 + 0] * r0.mat0.z + mat[3 * 6 + 1] * r0.mat1.z + mat[3 * 6 + 2] * r0.mat2.z;
                r2.mat1.x = mat[4 * 6 + 0] * r0.mat0.x + mat[4 * 6 + 1] * r0.mat1.x + mat[4 * 6 + 2] * r0.mat2.x;
                r2.mat1.y = mat[4 * 6 + 0] * r0.mat0.y + mat[4 * 6 + 1] * r0.mat1.y + mat[4 * 6 + 2] * r0.mat2.y;
                r2.mat1.z = mat[4 * 6 + 0] * r0.mat0.z + mat[4 * 6 + 1] * r0.mat1.z + mat[4 * 6 + 2] * r0.mat2.z;
                r2.mat2.x = mat[5 * 6 + 0] * r0.mat0.x + mat[5 * 6 + 1] * r0.mat1.x + mat[5 * 6 + 2] * r0.mat2.x;
                r2.mat2.y = mat[5 * 6 + 0] * r0.mat0.y + mat[5 * 6 + 1] * r0.mat1.y + mat[5 * 6 + 2] * r0.mat2.y;
                r2.mat2.z = mat[5 * 6 + 0] * r0.mat0.z + mat[5 * 6 + 1] * r0.mat1.z + mat[5 * 6 + 2] * r0.mat2.z;

                // m2 = r3 * r2;
                mat[3 * 6 + 0] = r3.mat0.x * r2.mat0.x + r3.mat0.y * r2.mat1.x + r3.mat0.z * r2.mat2.x;
                mat[3 * 6 + 1] = r3.mat0.x * r2.mat0.y + r3.mat0.y * r2.mat1.y + r3.mat0.z * r2.mat2.y;
                mat[3 * 6 + 2] = r3.mat0.x * r2.mat0.z + r3.mat0.y * r2.mat1.z + r3.mat0.z * r2.mat2.z;
                mat[4 * 6 + 0] = r3.mat1.x * r2.mat0.x + r3.mat1.y * r2.mat1.x + r3.mat1.z * r2.mat2.x;
                mat[4 * 6 + 1] = r3.mat1.x * r2.mat0.y + r3.mat1.y * r2.mat1.y + r3.mat1.z * r2.mat2.y;
                mat[4 * 6 + 2] = r3.mat1.x * r2.mat0.z + r3.mat1.y * r2.mat1.z + r3.mat1.z * r2.mat2.z;
                mat[5 * 6 + 0] = r3.mat2.x * r2.mat0.x + r3.mat2.y * r2.mat1.x + r3.mat2.z * r2.mat2.x;
                mat[5 * 6 + 1] = r3.mat2.x * r2.mat0.y + r3.mat2.y * r2.mat1.y + r3.mat2.z * r2.mat2.y;
                mat[5 * 6 + 2] = r3.mat2.x * r2.mat0.z + r3.mat2.y * r2.mat1.z + r3.mat2.z * r2.mat2.z;

                // m0 = r0 - r1 * m2;
                mat[0 * 6 + 0] = r0.mat0.x - r1.mat0.x * mat[3 * 6 + 0] - r1.mat0.y * mat[4 * 6 + 0] - r1.mat0.z * mat[5 * 6 + 0];
                mat[0 * 6 + 1] = r0.mat0.y - r1.mat0.x * mat[3 * 6 + 1] - r1.mat0.y * mat[4 * 6 + 1] - r1.mat0.z * mat[5 * 6 + 1];
                mat[0 * 6 + 2] = r0.mat0.z - r1.mat0.x * mat[3 * 6 + 2] - r1.mat0.y * mat[4 * 6 + 2] - r1.mat0.z * mat[5 * 6 + 2];
                mat[1 * 6 + 0] = r0.mat1.x - r1.mat1.x * mat[3 * 6 + 0] - r1.mat1.y * mat[4 * 6 + 0] - r1.mat1.z * mat[5 * 6 + 0];
                mat[1 * 6 + 1] = r0.mat1.y - r1.mat1.x * mat[3 * 6 + 1] - r1.mat1.y * mat[4 * 6 + 1] - r1.mat1.z * mat[5 * 6 + 1];
                mat[1 * 6 + 2] = r0.mat1.z - r1.mat1.x * mat[3 * 6 + 2] - r1.mat1.y * mat[4 * 6 + 2] - r1.mat1.z * mat[5 * 6 + 2];
                mat[2 * 6 + 0] = r0.mat2.x - r1.mat2.x * mat[3 * 6 + 0] - r1.mat2.y * mat[4 * 6 + 0] - r1.mat2.z * mat[5 * 6 + 0];
                mat[2 * 6 + 1] = r0.mat2.y - r1.mat2.x * mat[3 * 6 + 1] - r1.mat2.y * mat[4 * 6 + 1] - r1.mat2.z * mat[5 * 6 + 1];
                mat[2 * 6 + 2] = r0.mat2.z - r1.mat2.x * mat[3 * 6 + 2] - r1.mat2.y * mat[4 * 6 + 2] - r1.mat2.z * mat[5 * 6 + 2];

                // m1 = r1 * r3;
                mat[0 * 6 + 3] = r1.mat0.x * r3.mat0.x + r1.mat0.y * r3.mat1.x + r1.mat0.z * r3.mat2.x;
                mat[0 * 6 + 4] = r1.mat0.x * r3.mat0.y + r1.mat0.y * r3.mat1.y + r1.mat0.z * r3.mat2.y;
                mat[0 * 6 + 5] = r1.mat0.x * r3.mat0.z + r1.mat0.y * r3.mat1.z + r1.mat0.z * r3.mat2.z;
                mat[1 * 6 + 3] = r1.mat1.x * r3.mat0.x + r1.mat1.y * r3.mat1.x + r1.mat1.z * r3.mat2.x;
                mat[1 * 6 + 4] = r1.mat1.x * r3.mat0.y + r1.mat1.y * r3.mat1.y + r1.mat1.z * r3.mat2.y;
                mat[1 * 6 + 5] = r1.mat1.x * r3.mat0.z + r1.mat1.y * r3.mat1.z + r1.mat1.z * r3.mat2.z;
                mat[2 * 6 + 3] = r1.mat2.x * r3.mat0.x + r1.mat2.y * r3.mat1.x + r1.mat2.z * r3.mat2.x;
                mat[2 * 6 + 4] = r1.mat2.x * r3.mat0.y + r1.mat2.y * r3.mat1.y + r1.mat2.z * r3.mat2.y;
                mat[2 * 6 + 5] = r1.mat2.x * r3.mat0.z + r1.mat2.y * r3.mat1.z + r1.mat2.z * r3.mat2.z;

                // m3 = -r3;
                mat[3 * 6 + 3] = -r3.mat0.x;
                mat[3 * 6 + 4] = -r3.mat0.y;
                mat[3 * 6 + 5] = -r3.mat0.z;
                mat[4 * 6 + 3] = -r3.mat1.x;
                mat[4 * 6 + 4] = -r3.mat1.y;
                mat[4 * 6 + 5] = -r3.mat1.z;
                mat[5 * 6 + 3] = -r3.mat2.x;
                mat[5 * 6 + 4] = -r3.mat2.y;
                mat[5 * 6 + 5] = -r3.mat2.z;
            }

            return true;
        }

        public const int Dimension = 36;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Fixed<T>(FloatPtr<T> callback)
            => mat0.Fixed(callback);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fixed(FloatPtr callback)
            => mat0.Fixed(callback);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(int precision = 2)
        {
            fixed (float* _ = mat0.p) return FloatArrayToString(_, Dimension, precision);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe partial struct MatrixX
    {
        const int MATX_MAX_TEMP = 1024;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] static int MATX_QUAD(int x) => ((x) + 3) & ~3;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] void MATX_CLEAREND() { int s = numRows * numColumns; while (s < ((s + 3) & ~3)) { mat[s++] = 0f; } }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] internal static float[] MATX_ALLOCA(int n) => new float[MATX_QUAD(n)]; //:_alloc16

        public const float INVERSE_EPSILON = 1e-14F;
        public const float EPSILON = 1e-6F;

        int numRows;               // number of rows
        int numColumns;           // number of columns
        int alloced;              // floats allocated, if -1 then mat points to data set with SetData
        internal float[] mat;               // memory the matrix is stored

        static float[] temp; // = new float[MATX_MAX_TEMP + 4];   // used to store intermediate results
                             // static float[] tempPtr = temp; //(float*)(((intptr_t)idMatX::temp + 15) & ~15);              // pointer to 16 byte aligned temporary memory
        static int tempIndex = 0;                   // index into memory pool, wraps around

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MatrixX(in MatrixX a)
        {
            numRows = numColumns = alloced = 0;
            mat = null;
            SetSize(a.numRows, a.numColumns);
#if MATX_SIMD
            Simd.Copy16(mat, a.mat, a.numRows * a.numColumns);
#else
            fixed (float* mat = this.mat, a_mat = a.mat) Unsafe.CopyBlock(mat, a_mat, (uint)(a.numRows * a.numColumns * sizeof(float)));
#endif
            tempIndex = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MatrixX(int rows, int columns)
        {
            numRows = numColumns = alloced = 0;
            mat = null;
            SetSize(rows, columns);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MatrixX(int rows, int columns, float[] src)
        {
            numRows = numColumns = alloced = 0;
            mat = null;
            SetData(rows, columns, src);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int rows, int columns, float[] src)
        {
            SetSize(rows, columns);
            fixed (void* mat = this.mat, src_ = src) Unsafe.CopyBlock(mat, src_, (uint)(rows * columns * sizeof(float)));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(in Matrix3x3 m1, in Matrix3x3 m2)
        {
            SetSize(3, 6);
            for (var i = 0; i < 3; i++)
                for (var j = 0; j < 3; j++)
                {
                    mat[(i + 0) * numColumns + (j + 0)] = m1[i][j];
                    mat[(i + 0) * numColumns + (j + 3)] = m2[i][j];
                }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(in Matrix3x3 m1, in Matrix3x3 m2, in Matrix3x3 m3, in Matrix3x3 m4)
        {
            SetSize(6, 6);
            for (var i = 0; i < 3; i++)
                for (var j = 0; j < 3; j++)
                {
                    mat[(i + 0) * numColumns + (j + 0)] = m1[i][j];
                    mat[(i + 0) * numColumns + (j + 3)] = m2[i][j];
                    mat[(i + 3) * numColumns + (j + 0)] = m3[i][j];
                    mat[(i + 3) * numColumns + (j + 3)] = m4[i][j];
                }
        }

        public Span<float> this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Debug.Assert(index >= 0 && index < numRows);
                return mat.AsSpan(index * numColumns);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MatrixX operator *(in MatrixX _, float a)
        {
            var m = new MatrixX();
            m.SetTempSize(_.numRows, _.numColumns);
#if MATX_SIMD
            Simd.Mul16(m.mat, mat, a, numRows * numColumns);
#else
            var s = _.numRows * _.numColumns;
            for (var i = 0; i < s; i++) m.mat[i] = _.mat[i] * a;
#endif
            return m;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorX operator *(in MatrixX _, in VectorX vec)
        {
            Debug.Assert(_.numColumns == vec.Size);
            VectorX dst = new();
            dst.SetTempSize(_.numRows);
#if MATX_SIMD
            Simd.MatX_MultiplyVecX(dst, *this, vec);
#else
            _.Multiply(dst, vec);
#endif
            return dst;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MatrixX operator *(in MatrixX _, in MatrixX a)
        {
            Debug.Assert(_.numColumns == a.numRows);
            MatrixX dst = new();
            dst.SetTempSize(_.numRows, a.numColumns);
#if MATX_SIMD
            Simd.MatX_MultiplyMatX(dst, *this, a);
#else
            _.Multiply(dst, a);
#endif
            return dst;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MatrixX operator +(in MatrixX _, in MatrixX a)
        {
            Debug.Assert(_.numRows == a.numRows && _.numColumns == a.numColumns);
            MatrixX m = new();
            m.SetTempSize(_.numRows, _.numColumns);
#if MATX_SIMD
            Simd.Add16(m.mat, mat, a.mat, numRows * numColumns);
#else
            var s = _.numRows * _.numColumns;
            for (var i = 0; i < s; i++) m.mat[i] = _.mat[i] + a.mat[i];
#endif
            return m;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MatrixX operator -(in MatrixX _, in MatrixX a)
        {
            Debug.Assert(_.numRows == a.numRows && _.numColumns == a.numColumns);
            MatrixX m = new();
            m.SetTempSize(_.numRows, _.numColumns);
#if MATX_SIMD
            Simd.Sub16(m.mat, mat, a.mat, numRows * numColumns);
#else
            var s = _.numRows * _.numColumns;
            for (var i = 0; i < s; i++) m.mat[i] = _.mat[i] - a.mat[i];
#endif
            return m;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MatrixX operator *(float a, in MatrixX m)
        => m * a;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorX operator *(in VectorX vec, in MatrixX m)
        => m * vec;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in MatrixX a)                             // exact compare, no epsilon
        {
            Debug.Assert(numRows == a.numRows && numColumns == a.numColumns);
            var s = numRows * numColumns;
            for (var i = 0; i < s; i++) if (mat[i] != a.mat[i]) return false;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(in MatrixX a, float epsilon)            // compare with epsilon
        {
            Debug.Assert(numRows == a.numRows && numColumns == a.numColumns);
            var s = numRows * numColumns;
            for (var i = 0; i < s; i++)
                if (MathX.Fabs(mat[i] - a.mat[i]) > epsilon)
                    return false;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in MatrixX _, in MatrixX a)                         // exact compare, no epsilon
        => _.Compare(a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in MatrixX _, in MatrixX a)                         // exact compare, no epsilon
        => !_.Compare(a);
        public override bool Equals(object obj)
            => obj is MatrixX q && Compare(q);
        public override int GetHashCode()
            => base.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSize(int rows, int columns)                                // set the number of rows/columns
        {
            //Debug.Assert(mat < tempPtr || mat > tempPtr + MATX_MAX_TEMP);
            var alloc = (rows * columns + 3) & ~3;
            if (alloc > alloced && alloced != -1) { mat = new float[alloc]; alloced = alloc; }
            numRows = rows;
            numColumns = columns;
            MATX_CLEAREND();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetTempSize(int rows, int columns)
        {
            var newSize = (rows * columns + 3) & ~3;
            Debug.Assert(newSize < MATX_MAX_TEMP);
            if (tempIndex + newSize > MATX_MAX_TEMP) tempIndex = 0;
            mat = new float[newSize]; // tempPtr + tempIndex;
            tempIndex += newSize;
            alloced = newSize;
            numRows = rows;
            numColumns = columns;
            MATX_CLEAREND();
        }

        public void ChangeSize(int rows, int columns, bool makeZero = false)      // change the size keeping data intact where possible
        {
            var alloc = (rows * columns + 3) & ~3;
            if (alloc > alloced && alloced != -1)
            {
                var oldMat = mat;
                mat = new float[alloc];
                alloced = alloc;
                if (oldMat != null)
                {
                    var minRow = Math.Min(numRows, rows);
                    var minColumn = Math.Min(numColumns, columns);
                    for (var i = 0; i < minRow; i++) for (var j = 0; j < minColumn; j++) mat[i * columns + j] = oldMat[i * numColumns + j];
                }
            }
            else
            {
                if (columns < numColumns)
                {
                    var minRow = Math.Min(numRows, rows);
                    for (var i = 0; i < minRow; i++) for (var j = 0; j < columns; j++) mat[i * columns + j] = mat[i * numColumns + j];
                }
                else if (columns > numColumns)
                    for (var i = Math.Min(numRows, rows) - 1; i >= 0; i--)
                    {
                        if (makeZero) for (var j = columns - 1; j >= numColumns; j--) mat[i * columns + j] = 0f;
                        for (var j = numColumns - 1; j >= 0; j--) mat[i * columns + j] = mat[i * numColumns + j];
                    }
                if (makeZero && rows > numRows) Array.Clear(mat, numRows * columns, (rows - numRows) * columns);
            }
            numRows = rows;
            numColumns = columns;
            MATX_CLEAREND();
        }

        public int NumRows => numRows;                    // get the number of rows
        public int NumColumns => numColumns;              // get the number of columns

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetData(int rows, int columns, float[] data)                   // set float array pointer
        {
            //Debug.Assert(mat < tempPtr || mat > tempPtr + MATX_MAX_TEMP);
            //Debug.Assert((((uintptr_t)data) & 15) == 0); // data must be 16 byte aligned
            mat = data;
            alloced = -1;
            numRows = rows;
            numColumns = columns;
            MATX_CLEAREND();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero()                                                   // clear matrix
        {
#if MATX_SIMD
            Simd.Zero16(mat, numRows * numColumns);
#else
            fixed (void* mat = this.mat) Unsafe.InitBlock(mat, 0, (uint)(numRows * numColumns * sizeof(float)));
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Zero(int rows, int columns)                                   // set size and clear matrix
        {
            SetSize(rows, columns);
#if MATX_SIMD
            Simd.Zero16(mat, numRows * numColumns);
#else
            fixed (void* mat = this.mat) Unsafe.InitBlock(mat, 0, (uint)(rows * columns * sizeof(float)));
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Identity()                                               // clear to identity matrix
        {
            Debug.Assert(numRows == numColumns);
#if MATX_SIMD
            Simd.Zero16(mat, numRows * numColumns);
#else
            fixed (void* mat = this.mat) Unsafe.InitBlock(mat, 0, (uint)(numRows * numColumns * sizeof(float)));
#endif
            for (var i = 0; i < numRows; i++) mat[i * numColumns + i] = 1f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Identity(int rows, int columns)                               // set size and clear to identity matrix
        {
            Debug.Assert(rows == columns);
            SetSize(rows, columns);
            Identity();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Diag(in VectorX v)                                      // create diagonal matrix from vector
        {
            Zero(v.Size, v.Size);
            for (var i = 0; i < v.Size; i++) mat[i * numColumns + i] = v[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Random(long seed, float l = 0f, float u = 1f)              // fill matrix with random values
        {
            var rnd = new RandomX(seed);
            var c = u - l;
            var s = numRows * numColumns;
            for (var i = 0; i < s; i++) mat[i] = l + rnd.RandomFloat() * c;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Random(int rows, int columns, long seed, float l = 0f, float u = 1f)
        {
            var rnd = new RandomX(seed);
            SetSize(rows, columns);
            var c = u - l;
            var s = numRows * numColumns;
            for (var i = 0; i < s; i++) mat[i] = l + rnd.RandomFloat() * c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Negate()                                                 // this = - this
        {
#if MATX_SIMD
            Simd.Negate16(mat, numRows * numColumns);
#else
            var s = numRows * numColumns;
            for (var i = 0; i < s; i++) mat[i] = -mat[i];
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clamp(float min, float max)                                   // clamp all values
        {
            var s = numRows * numColumns;
            for (var i = 0; i < s; i++)
            {
                if (mat[i] < min) mat[i] = min;
                else if (mat[i] > max) mat[i] = max;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MatrixX SwapRows(int r1, int r2)                                     // swap rows
        {
            var ptr = stackalloc float[numColumns + floatX.ALLOC16]; ptr = (float*)_alloca16(ptr);
            fixed (float* mat = this.mat)
            {
                Unsafe.CopyBlock(ptr, mat + r1 * numColumns, (uint)numColumns * sizeof(float));
                Unsafe.CopyBlock(mat + r1 * numColumns, mat + r2 * numColumns, (uint)numColumns * sizeof(float));
                Unsafe.CopyBlock(mat + r2 * numColumns, ptr, (uint)numColumns * sizeof(float));
                return this;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MatrixX SwapColumns(int r1, int r2)                                  // swap columns
        {
            fixed (float* mat = this.mat)
                for (var i = 0; i < numRows; i++)
                {
                    var ptr = mat + i * numColumns;
                    var tmp = ptr[r1];
                    ptr[r1] = ptr[r2];
                    ptr[r2] = tmp;
                }
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MatrixX SwapRowsColumns(int r1, int r2)                              // swap rows and columns
        {
            SwapRows(r1, r2);
            SwapColumns(r1, r2);
            return this;
        }

        public MatrixX RemoveRow(int r)                                             // remove a row
        {
            Debug.Assert(r < numRows);

            numRows--;

            fixed (float* mat = this.mat) for (var i = r; i < numRows; i++) Unsafe.CopyBlock(&mat[i * numColumns], &mat[(i + 1) * numColumns], (uint)numColumns * sizeof(float));

            return this;
        }

        public MatrixX RemoveColumn(int r)                                          // remove a column
        {
            int i;
            Debug.Assert(r < numColumns);

            numColumns--;

            fixed (float* mat = this.mat)
            {
                for (i = 0; i < numRows - 1; i++) UnsafeX.MoveBlock(&mat[i * numColumns + r], &mat[i * (numColumns + 1) + r + 1], (uint)numColumns * sizeof(float));
                UnsafeX.MoveBlock(&mat[i * numColumns + r], &mat[i * (numColumns + 1) + r + 1], (uint)(numColumns - r) * sizeof(float));
                return this;
            }
        }

        public MatrixX RemoveRowColumn(int r)                                       // remove a row and column
        {
            int i;
            Debug.Assert(r < numRows && r < numColumns);

            numRows--;
            numColumns--;

            fixed (float* mat = this.mat)
            {
                if (r > 0)
                {
                    for (i = 0; i < r - 1; i++) UnsafeX.MoveBlock(&mat[i * numColumns + r], &mat[i * (numColumns + 1) + r + 1], (uint)numColumns * sizeof(float));
                    UnsafeX.MoveBlock(&mat[i * numColumns + r], &mat[i * (numColumns + 1) + r + 1], (uint)(numColumns - r) * sizeof(float));
                }
                Unsafe.CopyBlock(&mat[r * numColumns], &mat[(r + 1) * (numColumns + 1)], (uint)r * sizeof(float));
                for (i = r; i < numRows - 1; i++) Unsafe.CopyBlock(&mat[i * numColumns + r], &mat[(i + 1) * (numColumns + 1) + r + 1], (uint)numColumns * sizeof(float));
                Unsafe.CopyBlock(&mat[i * numColumns + r], &mat[(i + 1) * (numColumns + 1) + r + 1], (uint)(numColumns - r) * sizeof(float));
                return this;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearUpperTriangle()                                     // clear the upper triangle
        {
            Debug.Assert(numRows == numColumns);

            fixed (float* mat = this.mat) for (var i = numRows - 2; i >= 0; i--) Unsafe.InitBlock(mat + i * numColumns + i + 1, 0, (uint)(numColumns - 1 - i) * sizeof(float));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void ClearLowerTriangle()                                     // clear the lower triangle
        {
            Debug.Assert(numRows == numColumns);

            fixed (float* mat = this.mat) for (var i = 1; i < numRows; i++) Unsafe.InitBlock(mat + i * numColumns, 0, (uint)i * sizeof(float));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SquareSubMatrix(in MatrixX m, int size)                  // get square sub-matrix from 0,0 to size,size
        {
            Debug.Assert(size <= m.numRows && size <= m.numColumns);

            SetSize(size, size);

            fixed (float* mat = this.mat, m_mat = m.mat) for (var i = 0; i < size; i++) Unsafe.CopyBlock(mat + i * numColumns, m_mat + i * m.numColumns, (uint)size * sizeof(float));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float MaxDifference(in MatrixX m)                          // return maximum element difference between this and m
        {
            Debug.Assert(numRows == m.numRows && numColumns == m.numColumns);

            var maxDiff = -1f;
            for (var i = 0; i < numRows; i++)
                for (var j = 0; j < numColumns; j++)
                {
                    var diff = MathX.Fabs(mat[i * numColumns + j] - m[i][j]);
                    if (maxDiff < 0f || diff > maxDiff) maxDiff = diff;
                }
            return maxDiff;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSquare()
            => numRows == numColumns;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsZero(float epsilon = EPSILON)
        {
            // returns true if this == Zero
            for (var i = 0; i < numRows; i++) for (var j = 0; j < numColumns; j++) if (MathX.Fabs(mat[i * numColumns + j]) > epsilon) return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsIdentity(float epsilon = EPSILON)
        {
            // returns true if this == Identity
            Debug.Assert(numRows == numColumns);

            for (var i = 0; i < numRows; i++) for (var j = 0; j < numColumns; j++) if (MathX.Fabs(mat[i * numColumns + j] - (i == j ? 1f : 0f)) > epsilon) return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDiagonal(float epsilon = EPSILON)
        {
            // returns true if all elements are zero except for the elements on the diagonal
            Debug.Assert(numRows == numColumns);

            for (var i = 0; i < numRows; i++) for (var j = 0; j < numColumns; j++) if (i != j && MathX.Fabs(mat[i * numColumns + j]) > epsilon) return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTriDiagonal(float epsilon = EPSILON)
        {
            // returns true if all elements are zero except for the elements on the diagonal plus or minus one column
            if (numRows != numColumns) return false;
            for (var i = 0; i < numRows - 2; i++)
                for (var j = i + 2; j < numColumns; j++)
                {
                    if (MathX.Fabs(this[i][j]) > epsilon) return false;
                    if (MathX.Fabs(this[j][i]) > epsilon) return false;
                }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSymmetric(float epsilon = EPSILON)
        {
            // this[i][j] == this[j][i]
            if (numRows != numColumns) return false;
            for (var i = 0; i < numRows; i++)
                for (var j = 0; j < numColumns; j++)
                    if (MathX.Fabs(mat[i * numColumns + j] - mat[j * numColumns + i]) > epsilon) return false;
            return true;
        }

        /// <summary>
        /// returns true if this * this.Transpose() == Identity
        /// </summary>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns>
        ///   <c>true</c> if the specified epsilon is orthogonal; otherwise, <c>false</c>.
        /// </returns>
        public bool IsOrthogonal(float epsilon = EPSILON)
        {
            if (!IsSquare()) return false;
            fixed (float* mat = this.mat)
            {
                var ptr1 = mat;
                for (var i = 0; i < numRows; i++)
                {
                    for (var j = 0; j < numColumns; j++)
                    {
                        var ptr2 = mat + j;
                        var sum = ptr1[0] * ptr2[0] - (i == j ? 1f : 0f);
                        for (var n = 1; n < numColumns; n++) { ptr2 += numColumns; sum += ptr1[n] * ptr2[0]; }
                        if (MathX.Fabs(sum) > epsilon) return false;
                    }
                    ptr1 += numColumns;
                }
                return true;
            }
        }
        /// <summary>
        /// returns true if this * this.Transpose() == Identity and the length of each column vector is 1
        /// </summary>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns>
        ///   <c>true</c> if the specified epsilon is orthonormal; otherwise, <c>false</c>.
        /// </returns>
        public bool IsOrthonormal(float epsilon = EPSILON)
        {
            if (!IsSquare()) return false;
            fixed (float* mat = this.mat)
            {
                var ptr1 = mat;
                for (var i = 0; i < numRows; i++)
                {
                    var colVecSum = 0f;
                    var colVecPtr = mat + i; // row 0 col i - don't worry, numRows == numColums because IsSquare()
                    for (var j = 0; j < numColumns; j++)
                    {
                        var ptr2 = mat + j;
                        var sum = ptr1[0] * ptr2[0] - (i == j ? 1f : 0f);
                        for (var n = 1; n < numColumns; n++) { ptr2 += numColumns; sum += ptr1[n] * ptr2[0]; }
                        if (MathX.Fabs(sum) > epsilon) return false;
                        // row j, col i - this works because numRows == numColumns
                        colVecSum += colVecPtr[0] * colVecPtr[0];
                        colVecPtr += numColumns; // next row, same column
                    }
                    ptr1 += numColumns;

                    // check that length of *column* vector i is 1 (no need for sqrt because sqrt(1)==1)
                    if (MathX.Fabs(colVecSum - 1f) > epsilon) return false;
                }
                return true;
            }
        }

        /// <summary>
        /// returns true if the matrix is a P-matrix
        /// A square matrix is a P-matrix if all its principal minors are positive.
        /// </summary>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns>
        ///   <c>true</c> if [is p matrix] [the specified epsilon]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPMatrix(float epsilon = EPSILON)
        {
            if (!IsSquare()) return false;
            if (numRows <= 0) return true;
            if (this[0][0] <= epsilon) return false;
            if (numRows <= 1) return true;

            var m = new MatrixX();
            m.SetData(numRows - 1, numColumns - 1, MATX_ALLOCA((numRows - 1) * (numColumns - 1)));

            int i, j;
            for (i = 1; i < numRows; i++) for (j = 1; j < numColumns; j++) m[i - 1][j - 1] = this[i][j];
            if (!m.IsPMatrix(epsilon)) return false;

            for (i = 1; i < numRows; i++)
            {
                var d = this[i][0] / this[0][0];
                for (j = 1; j < numColumns; j++) m[i - 1][j - 1] = this[i][j] - d * this[0][j];
            }

            return m.IsPMatrix(epsilon);
        }

        /// <summary>
        /// returns true if the matrix is a Z-matrix
        /// A square matrix M is a Z-matrix if M[i][j] <= 0 for all i != j.
        /// </summary>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns>
        ///   <c>true</c> if the matrix is a Z-matrix; otherwise, <c>false</c>.
        /// </returns>
        public bool IsZMatrix(float epsilon = EPSILON)
        {
            if (!IsSquare()) return false;
            for (var i = 0; i < numRows; i++) for (var j = 0; j < numColumns; j++) if (this[i][j] > epsilon && i != j) return false;
            return true;
        }

        /// <summary>
        /// returns true if the matrix is Positive Definite (PD)
        /// A square matrix M of order n is said to be PD if y'My > 0 for all vectors y of dimension n, y != 0.
        /// </summary>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns>
        ///   <c>true</c> if the matrix is Positive Definite (PD); otherwise, <c>false</c>.
        /// </returns>
        public bool IsPositiveDefinite(float epsilon = EPSILON)
        {
            // the matrix must be square
            if (!IsSquare()) return false;

            // copy matrix
            var m = new MatrixX();
            m.SetData(numRows, numColumns, MATX_ALLOCA(numRows * numColumns));
            m = this;

            // add transpose
            int i, j, k;
            for (i = 0; i < numRows; i++) for (j = 0; j < numColumns; j++) m[i][j] += this[j][i];

            // test Positive Definiteness with Gaussian pivot steps
            for (i = 0; i < numRows; i++)
            {
                for (j = i; j < numColumns; j++) if (m[j][j] <= epsilon) return false;
                var d = 1f / m[i][i];
                for (j = i + 1; j < numColumns; j++)
                {
                    var s = d * m[j][i];
                    m[j][i] = 0f;
                    for (k = i + 1; k < numRows; k++) m[j][k] -= s * m[i][k];
                }
            }

            return true;
        }

        /// <summary>
        /// returns true if the matrix is Symmetric Positive Definite (PD)
        /// A square matrix M of order n is said to be PSD if y'My >= 0 for all vectors y of dimension n, y != 0.
        /// </summary>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns>
        ///   <c>true</c> if the matrix is Positive Semi Definite (PSD); otherwise, <c>false</c>.
        /// </returns>
        public bool IsSymmetricPositiveDefinite(float epsilon = EPSILON)
        {
            // the matrix must be symmetric
            if (!IsSymmetric(epsilon)) return false;

            // copy matrix
            var m = new MatrixX();
            m.SetData(numRows, numColumns, MATX_ALLOCA(numRows * numColumns));
            m = this;

            // being able to obtain Cholesky factors is both a necessary and sufficient condition for positive definiteness
            return m.Cholesky_Factor();
        }

        /// <summary>
        /// returns true if the matrix is Positive Semi Definite (PSD)
        /// A square matrix M of order n is said to be PSD if y'My >= 0 for all vectors y of dimension n, y != 0.
        /// </summary>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns>
        ///   <c>true</c> if the matrix is Positive Semi Definite (PSD); otherwise, <c>false</c>.
        /// </returns>
        public bool IsPositiveSemiDefinite(float epsilon = EPSILON)
        {
            // the matrix must be square
            if (!IsSquare()) return false;

            // copy original matrix
            var m = new MatrixX();
            m.SetData(numRows, numColumns, MATX_ALLOCA(numRows * numColumns));
            m = this;

            // add transpose
            int i, j, k;
            for (i = 0; i < numRows; i++) for (j = 0; j < numColumns; j++) m[i][j] += this[j][i];

            // test Positive Semi Definiteness with Gaussian pivot steps
            for (i = 0; i < numRows; i++)
            {
                for (j = i; j < numColumns; j++)
                {
                    if (m[j][j] < -epsilon) return false;
                    if (m[j][j] > epsilon) continue;
                    for (k = 0; k < numRows; k++) if (MathX.Fabs(m[k][j]) > epsilon || MathX.Fabs(m[j][k]) > epsilon) return false;
                }

                if (m[i][i] <= epsilon) continue;

                var d = 1f / m[i][i];
                for (j = i + 1; j < numColumns; j++)
                {
                    var s = d * m[j][i];
                    m[j][i] = 0f;
                    for (k = i + 1; k < numRows; k++) m[j][k] -= s * m[i][k];
                }
            }

            return true;
        }

        /// <summary>
        /// returns true if the matrix is Symmetric Positive Semi Definite (PSD)
        /// </summary>
        /// <param name="epsilon">The epsilon.</param>
        /// <returns>
        ///   <c>true</c> if the matrix is Symmetric Positive Semi Definite (PSD); otherwise, <c>false</c>.
        /// </returns>
        public bool IsSymmetricPositiveSemiDefinite(float epsilon = EPSILON)
            // the matrix must be symmetric
            => IsSymmetric(epsilon) && IsPositiveSemiDefinite(epsilon);

        public float Trace
        {
            // returns product of diagonal elements
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Debug.Assert(numRows == numColumns);
                // sum of elements on the diagonal
                var trace = 0f;
                for (var i = 0; i < numRows; i++) trace += mat[i * numRows + i];
                return trace;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Determinant()                                     // returns determinant of matrix
        {
            Debug.Assert(numRows == numColumns);
            if (numRows == 1) return mat[0];
            fixed (float* mat_ = mat)
                return numRows switch
                {
                    2 => reinterpret.cast_mat2(mat_).Determinant(),
                    3 => reinterpret.cast_mat3(mat_).Determinant(),
                    4 => reinterpret.cast_mat4(mat_).Determinant(),
                    5 => reinterpret.cast_mat5(mat_).Determinant(),
                    6 => reinterpret.cast_mat6(mat_).Determinant(),
                    _ => DeterminantGeneric(),
                };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MatrixX Transpose()                                     // returns transpose
        {
            MatrixX m = new();
            m.SetTempSize(numColumns, numRows);
            for (var i = 0; i < numRows; i++) for (var j = 0; j < numColumns; j++) m.mat[j * m.numColumns + i] = mat[i * numColumns + j];
            return m;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MatrixX TransposeSelf()                                            // transposes the matrix itself
        {
            this = Transpose();
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MatrixX Inverse()                                           // returns the inverse ( m * m.Inverse() = identity )
        {
            MatrixX invMat = new();
            invMat.SetTempSize(numRows, numColumns);
            fixed (float* invMat_mat = invMat.mat, mat = this.mat) Unsafe.CopyBlock(invMat_mat, mat, (uint)(numRows * numColumns * sizeof(float)));
            var r = invMat.InverseSelf();
            Debug.Assert(r);
            return invMat;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool InverseSelf()                                            // returns false if determinant is zero
        {
            Debug.Assert(numRows == numColumns);
            if (numRows == 1)
            {
                if (MathX.Fabs(mat[0]) < INVERSE_EPSILON) return false;
                mat[0] = 1f / mat[0];
                return true;
            }
            fixed (float* mat_ = mat)
                return numRows switch
                {
                    2 => reinterpret.cast_mat2(mat_).InverseSelf(),
                    3 => reinterpret.cast_mat3(mat_).InverseSelf(),
                    4 => reinterpret.cast_mat4(mat_).InverseSelf(),
                    5 => reinterpret.cast_mat5(mat_).InverseSelf(),
                    6 => reinterpret.cast_mat6(mat_).InverseSelf(),
                    _ => InverseSelfGeneric(),
                };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MatrixX InverseFast()                                       // returns the inverse ( m * m.Inverse() = identity )
        {
            MatrixX invMat = new();
            invMat.SetTempSize(numRows, numColumns);
            fixed (float* invMat_mat = invMat.mat, mat = this.mat) Unsafe.CopyBlock(invMat_mat, mat, (uint)(numRows * numColumns * sizeof(float)));
            var r = invMat.InverseFastSelf();
            Debug.Assert(r);
            return invMat;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool InverseFastSelf()                                        // returns false if determinant is zero
        {
            Debug.Assert(numRows == numColumns);
            if (numRows == 1)
            {
                if (MathX.Fabs(mat[0]) < INVERSE_EPSILON) return false;
                mat[0] = 1f / mat[0];
                return true;
            }
            fixed (float* mat_ = mat)
                return numRows switch
                {
                    2 => reinterpret.cast_mat2(mat_).InverseFastSelf(),
                    3 => reinterpret.cast_mat3(mat_).InverseFastSelf(),
                    4 => reinterpret.cast_mat4(mat_).InverseFastSelf(),
                    5 => reinterpret.cast_mat5(mat_).InverseFastSelf(),
                    6 => reinterpret.cast_mat6(mat_).InverseFastSelf(),
                    _ => InverseSelfGeneric(),
                };
        }

        /// <summary>
        /// in-place inversion of the lower triangular matrix
        /// </summary>
        /// <returns>false if determinant is zero</returns>
        public bool LowerTriangularInverse()
        {
            int i, j, k; double d, sum;

            for (i = 0; i < numRows; i++)
            {
                d = this[i][i];
                if (d == 0f) return false;
                this[i][i] = (float)(d = 1f / d);
                for (j = 0; j < i; j++)
                {
                    sum = 0f;
                    for (k = j; k < i; k++) sum -= this[i][k] * this[k][j];
                    this[i][j] = (float)(sum * d);
                }
            }
            return true;
        }

        /// <summary>
        /// in-place inversion of the upper triangular matrix
        /// </summary>
        /// <returns>false if determinant is zero</returns>
        public bool UpperTriangularInverse()
        {
            int i, j, k; double d, sum;

            for (i = numRows - 1; i >= 0; i--)
            {
                d = this[i][i];
                if (d == 0f) return false;
                this[i][i] = (float)(d = 1f / d);
                for (j = numRows - 1; j > i; j--)
                {
                    sum = 0f;
                    for (k = j; k > i; k--) sum -= this[i][k] * this[k][j];
                    this[i][j] = (float)(sum * d);
                }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorX Multiply(in VectorX vec)                           // this * vec
        {
            Debug.Assert(numColumns == vec.Size);
            var dst = new VectorX();
            dst.SetTempSize(numRows);
#if MATX_SIMD
            Simd.MatX_MultiplyVecX(dst, *this, vec);
#else
            Multiply(dst, vec);
#endif
            return dst;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorX TransposeMultiply(in VectorX vec)                  // this.Transpose() * vec
        {
            Debug.Assert(numRows == vec.Size);
            var dst = new VectorX();
            dst.SetTempSize(numColumns);
#if MATX_SIMD
            Simd.MatX_TransposeMultiplyVecX(dst, *this, vec);
#else
            TransposeMultiply(dst, vec);
#endif
            return dst;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MatrixX Multiply(in MatrixX a)                             // this * a
        {
            Debug.Assert(numColumns == a.numRows);
            var dst = new MatrixX();
            dst.SetTempSize(numRows, a.numColumns);
#if MATX_SIMD
            Simd.MatX_MultiplyMatX(dst, *this, a);
#else
            Multiply(dst, a);
#endif
            return dst;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MatrixX TransposeMultiply(in MatrixX a)                        // this.Transpose() * a
        {
            Debug.Assert(numRows == a.numRows);
            var dst = new MatrixX();
            dst.SetTempSize(numColumns, a.numColumns);
#if MATX_SIMD
            Simd.MatX_TransposeMultiplyMatX(dst, *this, a);
#else
            TransposeMultiply(dst, a);
#endif
            return dst;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Multiply(in VectorX dst, in VectorX vec)             // dst = this * vec
        {
#if MATX_SIMD
            Simd.MatX_MultiplyVecX(dst, *this, vec);
#else
            fixed (float* mat = this.mat, dstp = dst.p, vecp = vec.p)
            {
                var mPtr = mat;
                var dstPtr = dstp + dst.pi;
                var vPtr = vecp + vec.pi;
                for (var i = 0; i < numRows; i++)
                {
                    var sum = mPtr[0] * vPtr[0];
                    for (var j = 1; j < numColumns; j++) sum += mPtr[j] * vPtr[j];
                    dstPtr[i] = sum;
                    mPtr += numColumns;
                }
            }
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MultiplyAdd(in VectorX dst, in VectorX vec)          // dst += this * vec
        {
#if MATX_SIMD
            Simd.MatX_MultiplyAddVecX(dst, *this, vec);
#else
            fixed (float* mat = this.mat, dstp = dst.p, vecp = vec.p)
            {
                var mPtr = mat;
                var dstPtr = dstp + dst.pi;
                var vPtr = vecp + vec.pi;
                for (var i = 0; i < numRows; i++)
                {
                    var sum = mPtr[0] * vPtr[0];
                    for (var j = 1; j < numColumns; j++) sum += mPtr[j] * vPtr[j];
                    dstPtr[i] += sum;
                    mPtr += numColumns;
                }
            }
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MultiplySub(in VectorX dst, in VectorX vec)          // dst -= this * vec
        {
#if MATX_SIMD
            Simd.MatX_MultiplySubVecX(dst, *this, vec);
#else
            fixed (float* mat = this.mat, dstp = dst.p, vecp = vec.p)
            {
                var mPtr = mat;
                var dstPtr = dstp + dst.pi;
                var vPtr = vecp + vec.pi;
                for (var i = 0; i < numRows; i++)
                {
                    var sum = mPtr[0] * vPtr[0];
                    for (var j = 1; j < numColumns; j++) sum += mPtr[j] * vPtr[j];
                    dstPtr[i] -= sum;
                    mPtr += numColumns;
                }
            }
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TransposeMultiply(in VectorX dst, in VectorX vec)        // dst = this.Transpose() * vec
        {
#if MATX_SIMD
            Simd.MatX_TransposeMultiplyVecX(dst, *this, vec);
#else
            fixed (float* mat = this.mat, dstp = dst.p, vecp = vec.p)
            {
                var dstPtr = dstp + dst.pi;
                var vPtr = vecp + vec.pi;
                for (var i = 0; i < numColumns; i++)
                {
                    var mPtr = mat + i;
                    var sum = mPtr[0] * vPtr[0];
                    for (var j = 1; j < numRows; j++) { mPtr += numColumns; sum += mPtr[0] * vPtr[j]; }
                    dstPtr[i] = sum;
                }
            }
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TransposeMultiplyAdd(in VectorX dst, in VectorX vec) // dst += this.Transpose() * vec
        {
#if MATX_SIMD
            Simd.MatX_TransposeMultiplyAddVecX(dst, *this, vec);
#else
            fixed (float* mat = this.mat, dstp = dst.p, vecp = vec.p)
            {
                var dstPtr = dstp + dst.pi;
                var vPtr = vecp + vec.pi;
                for (var i = 0; i < numColumns; i++)
                {
                    var mPtr = mat + i;
                    var sum = mPtr[0] * vPtr[0];
                    for (var j = 1; j < numRows; j++) { mPtr += numColumns; sum += mPtr[0] * vPtr[j]; }
                    dstPtr[i] += sum;
                }
            }
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TransposeMultiplySub(in VectorX dst, in VectorX vec) // dst -= this.Transpose() * vec
        {
#if MATX_SIMD
            Simd.MatX_TransposeMultiplySubVecX(dst, *this, vec);
#else
            fixed (float* mat = this.mat, dstp = dst.p, vecp = vec.p)
            {
                var dstPtr = dstp + dst.pi;
                var vPtr = vecp + vec.pi;
                for (var i = 0; i < numColumns; i++)
                {
                    var mPtr = mat + i;
                    var sum = mPtr[0] * vPtr[0];
                    for (var j = 1; j < numRows; j++) { mPtr += numColumns; sum += mPtr[0] * vPtr[j]; }
                    dstPtr[i] -= sum;
                }
            }
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Multiply(in MatrixX dst, in MatrixX a)                   // dst = this * a
        {
#if MATX_SIMD
            Simd.MatX_MultiplyMatX(dst, *this, a);
#else
            Debug.Assert(numColumns == a.numRows);
            int i, j, k, l, n; double sum;

            fixed (float* dstp = dst.mat, mat = this.mat, a_mat = a.mat)
            {
                var dstPtr = dstp;
                var m1Ptr = mat;
                var m2Ptr = a_mat;
                k = numRows;
                l = a.NumColumns;

                for (i = 0; i < k; i++)
                {
                    for (j = 0; j < l; j++)
                    {
                        m2Ptr = a_mat + j;
                        sum = m1Ptr[0] * m2Ptr[0];
                        for (n = 1; n < numColumns; n++) { m2Ptr += l; sum += m1Ptr[n] * m2Ptr[0]; }
                        *dstPtr++ = (float)sum;
                    }
                    m1Ptr += numColumns;
                }
            }
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TransposeMultiply(in MatrixX dst, in MatrixX a)      // dst = this.Transpose() * a
        {
#if MATX_SIMD
            Simd.MatX_TransposeMultiplyMatX(dst, *this, a);
#else
            Debug.Assert(numRows == a.numRows);
            int i, j, k, l, n; double sum;

            fixed (float* dstp = dst.mat, mat = this.mat, a_mat = a.mat)
            {
                var dstPtr = dstp;
                var m1Ptr = mat;
                k = numColumns;
                l = a.numColumns;

                for (i = 0; i < k; i++)
                {
                    for (j = 0; j < l; j++)
                    {
                        m1Ptr = mat + i;
                        var m2Ptr = a_mat + j;
                        sum = m1Ptr[0] * m2Ptr[0];
                        for (n = 1; n < numRows; n++) { m1Ptr += numColumns; m2Ptr += a.numColumns; sum += m1Ptr[0] * m2Ptr[0]; }
                        *dstPtr++ = (float)sum;
                    }
                }
            }
#endif
        }

        public int Dimension
        {                                      // returns total number of values in matrix
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => numRows * numColumns;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Vector6 SubVec6(int row)                                       // interpret beginning of row as a Vector6
        {
            Debug.Assert(numColumns >= 6 && row >= 0 && row < numRows);
            fixed (float* mat = &this.mat[0]) return ref reinterpret.cast_vec6(mat, row * numColumns);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VectorX SubVecX(int row)                                     // interpret complete row as a VectorX
        {
            Debug.Assert(row >= 0 && row < numRows);
            var v = new VectorX();
            v.SetData(numColumns, mat, row * numColumns);
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Fixed<T>(FloatPtr<T> callback)
        {
            fixed (float* _ = mat) return callback(_);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fixed(FloatPtr callback)
        {
            fixed (float* _ = mat) callback(_);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(int precision = 2)
        {
            var dimension = Dimension;
            fixed (float* _ = mat) return FloatArrayToString(_, dimension, precision);
        }

        float DeterminantGeneric()
        {
            var index = stackalloc int[numRows + floatX.ALLOC16]; index = (int*)_alloca16(index);
            var tmp = new MatrixX();
            tmp.SetData(numRows, numColumns, MATX_ALLOCA(numRows * numColumns));
            tmp = this;

            if (!tmp.LU_Factor(index, out var det)) return 0f;
            return det;
        }

        bool InverseSelfGeneric()
        {
            var index = stackalloc int[numRows + floatX.ALLOC16]; index = (int*)_alloca16(index);
            var tmp = new MatrixX();
            tmp.SetData(numRows, numColumns, MATX_ALLOCA(numRows * numColumns));
            tmp = this;

            if (!tmp.LU_Factor(index)) return false;
            VectorX x = new(), b = new();
            x.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            b.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            b.Zero();

            for (var i = 0; i < numRows; i++)
            {
                b[i] = 1f;
                tmp.LU_Solve(ref x, b, index);
                for (var j = 0; j < numRows; j++) this[j][i] = x[j];
                b[i] = 0f;
            }
            return true;
        }
    }
}
