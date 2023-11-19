using System.Numerics;

namespace MathNet.Numerics.LinearAlgebra
{
    public static class NumericsExtensions
    {
        public static Vector<float> ToMathVector3(this Vector3 s) { var r = Vector<float>.Build.Dense(3); r[0] = s.X; r[1] = s.Y; r[2] = s.Z; return r; }
        public static Vector3 ToVector3(this Vector3 s, Vector<float> vector) => new Vector3 { X = vector[0], Y = vector[1], Z = vector[2] };

        public static Matrix<float> ToMathMatrix(this Matrix3x3 s)
        {
            var r = Matrix<float>.Build.Dense(3, 3);
            r[0, 0] = s.M11;
            r[0, 1] = s.M12;
            r[0, 2] = s.M13;
            r[1, 0] = s.M21;
            r[1, 1] = s.M22;
            r[1, 2] = s.M23;
            r[2, 0] = s.M31;
            r[2, 1] = s.M32;
            r[2, 2] = s.M33;
            return r;
        }
        public static Matrix3x3 ToMatrix3x3(this Matrix<float> matrix) => new Matrix3x3
        {
            M11 = matrix[0, 0],
            M12 = matrix[0, 1],
            M13 = matrix[0, 2],
            M21 = matrix[1, 0],
            M22 = matrix[1, 1],
            M23 = matrix[1, 2],
            M31 = matrix[2, 0],
            M32 = matrix[2, 1],
            M33 = matrix[2, 2]
        };

        public static Matrix<float> ToMathMatrix(this Matrix4x4 s)
        {
            var r = Matrix<float>.Build.Dense(4, 4);
            r[0, 0] = s.M11;
            r[0, 1] = s.M12;
            r[0, 2] = s.M13;
            r[0, 3] = s.M14;
            r[1, 0] = s.M21;
            r[1, 1] = s.M22;
            r[1, 2] = s.M23;
            r[1, 3] = s.M24;
            r[2, 0] = s.M31;
            r[2, 1] = s.M32;
            r[2, 2] = s.M33;
            r[2, 3] = s.M34;
            r[3, 0] = s.M41;
            r[3, 1] = s.M42;
            r[3, 2] = s.M43;
            r[3, 3] = s.M44;
            return r;
        }
        public static Matrix4x4 ToMatrix4x4(this Matrix<float> matrix) => new Matrix4x4
        {
            M11 = matrix[0, 0],
            M12 = matrix[0, 1],
            M13 = matrix[0, 2],
            M14 = matrix[0, 3],
            M21 = matrix[1, 0],
            M22 = matrix[1, 1],
            M23 = matrix[1, 2],
            M24 = matrix[1, 3],
            M31 = matrix[2, 0],
            M32 = matrix[2, 1],
            M33 = matrix[2, 2],
            M34 = matrix[2, 3],
            M41 = matrix[3, 0],
            M42 = matrix[3, 1],
            M43 = matrix[3, 2],
            M44 = matrix[3, 3],
        };
    }
}