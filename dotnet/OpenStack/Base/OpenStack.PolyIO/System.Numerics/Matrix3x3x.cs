////:ref https://referencesource.microsoft.com/#System.Numerics/System/Numerics/Matrix4x4.cs,48ce53b7e55d0436
//namespace System.Numerics
//{
//    /// <summary>
//    /// A structure encapsulating a 3x3 matrix.
//    /// </summary>
//    public struct Matrix3x3x : IEquatable<Matrix3x3x>
//    {
//        public float M11;
//        public float M12;
//        public float M13;
//        public float M21;
//        public float M22;
//        public float M23;
//        public float M31;
//        public float M32;
//        public float M33;

//        static readonly Matrix3x3x _identity = new Matrix3x3x(
//            1f, 0f, 0f,
//            0f, 1f, 0f,
//            0f, 0f, 1f
//        );

//        public static Matrix3x3x Identity
//            => _identity;

//        public Matrix3x3x(float m11, float m12, float m13,
//                         float m21, float m22, float m23,
//                         float m31, float m32, float m33)
//        {
//            M11 = m11;
//            M12 = m12;
//            M13 = m13;

//            M21 = m21;
//            M22 = m22;
//            M23 = m23;

//            M31 = m31;
//            M32 = m32;
//            M33 = m33;
//        }

//        /// <summary>
//        /// Creates a rotation matrix from the given Quaternion rotation value.
//        /// </summary>
//        /// <param name="quaternion">The source Quaternion.</param>
//        /// <returns>The rotation matrix.</returns>
//        public static Matrix3x3 CreateFromQuaternion(Quaternion quaternion)
//            => quaternion.ConvertToRotationMatrix();

//        /// <summary>
//        /// Transposes the rows and columns of a matrix.
//        /// </summary>
//        /// <param name="matrix">The source matrix.</param>
//        /// <returns>The transposed matrix.</returns>
//        public static Matrix3x3x Transpose(Matrix3x3x matrix)
//        {
//            Matrix3x3x result;

//            result.M11 = matrix.M11;
//            result.M12 = matrix.M21;
//            result.M13 = matrix.M31;
//            result.M21 = matrix.M12;
//            result.M22 = matrix.M22;
//            result.M23 = matrix.M32;
//            result.M31 = matrix.M13;
//            result.M32 = matrix.M23;
//            result.M33 = matrix.M33;

//            return result;
//        }

//        public static Matrix3x3x Mult(Matrix3x3x value1, Matrix3x3x value2)
//        {
//            Matrix3x3x m;

//            m.M11 = (value1.M11 * value2.M11) + (value1.M12 * value2.M21) + (value1.M13 * value2.M31);
//            m.M12 = (value1.M11 * value2.M12) + (value1.M12 * value2.M22) + (value1.M13 * value2.M32);
//            m.M13 = (value1.M11 * value2.M13) + (value1.M12 * value2.M23) + (value1.M13 * value2.M33);
//            m.M21 = (value1.M21 * value2.M11) + (value1.M22 * value2.M21) + (value1.M23 * value2.M31);
//            m.M22 = (value1.M21 * value2.M12) + (value1.M22 * value2.M22) + (value1.M23 * value2.M32);
//            m.M23 = (value1.M21 * value2.M13) + (value1.M22 * value2.M23) + (value1.M23 * value2.M33);
//            m.M31 = (value1.M31 * value2.M11) + (value1.M32 * value2.M21) + (value1.M33 * value2.M31);
//            m.M32 = (value1.M31 * value2.M12) + (value1.M32 * value2.M22) + (value1.M33 * value2.M32);
//            m.M33 = (value1.M31 * value2.M13) + (value1.M32 * value2.M23) + (value1.M33 * value2.M33);

//            return m;
//        }

//        public static Vector3 Mult(Matrix3x3x matrix, Vector3 vector)
//            // Multiply the 3x3 matrix by a Vector 3 to get the rotation
//            => new Vector3
//            {
//                X = ((vector.X * matrix.M11) + (vector.Y * matrix.M21) + (vector.Z * matrix.M31)),
//                Y = ((vector.X * matrix.M12) + (vector.Y * matrix.M22) + (vector.Z * matrix.M32)),
//                Z = ((vector.X * matrix.M13) + (vector.Y * matrix.M23) + (vector.Z * matrix.M33))
//            };

//        public static Matrix3x3x operator *(Matrix3x3x lhs, Matrix3x3x rhs)
//            => Mult(lhs, rhs);

//        public static Vector3 operator *(Matrix3x3x matrix, Vector3 vector)
//            => Mult(matrix, vector);

//        public override string ToString()
//            => $"[[{M11:F6}, {M12:F6}, {M13:F6}], [{M21:F6}, {M22:F6}, {M23:F6}], [{M31:F6}, {M32:F6}, {M33:F6}]]";

//        /// <summary>
//        /// Returns a boolean indicating whether this matrix instance is equal to the other given matrix.
//        /// </summary>
//        /// <param name="other">The matrix to compare this instance to.</param>
//        /// <returns>True if the matrices are equal; False otherwise.</returns>
//        public bool Equals(Matrix3x3x other)
//            => M11 == other.M11 && M22 == other.M22 && M33 == other.M33 && // Check diagonal element first for early out.
//            M12 == other.M12 && M13 == other.M13 &&
//            M21 == other.M21 && M23 == other.M23 &&
//            M31 == other.M31 && M32 == other.M32;
//    }
//}
