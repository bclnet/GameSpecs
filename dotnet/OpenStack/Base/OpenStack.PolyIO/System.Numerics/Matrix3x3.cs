//using MathNet.Numerics.LinearAlgebra;
using System.Globalization;

//:ref https://github.com/microsoft/referencesource/tree/master/System.Numerics/System/Numerics
namespace System.Numerics
{
    /// <summary>
    /// A structure encapsulating a 3x3 matrix.
    /// </summary>
    public struct Matrix3x3 : IEquatable<Matrix3x3>
    {
        #region Public Fields

        /// <summary>
        /// Value at row 1, column 1 of the matrix.
        /// </summary>
        public float M11;
        /// <summary>
        /// Value at row 1, column 2 of the matrix.
        /// </summary>
        public float M12;
        /// <summary>
        /// Value at row 1, column 3 of the matrix.
        /// </summary>
        public float M13;

        /// <summary>
        /// Value at row 2, column 1 of the matrix.
        /// </summary>
        public float M21;
        /// <summary>
        /// Value at row 2, column 2 of the matrix.
        /// </summary>
        public float M22;
        /// <summary>
        /// Value at row 2, column 3 of the matrix.
        /// </summary>
        public float M23;

        /// <summary>
        /// Value at row 3, column 1 of the matrix.
        /// </summary>
        public float M31;
        /// <summary>
        /// Value at row 3, column 2 of the matrix.
        /// </summary>
        public float M32;
        /// <summary>
        /// Value at row 3, column 3 of the matrix.
        /// </summary>
        public float M33;

        #endregion Public Fields

        #region Added

        /// <summary>
        /// Creates a rotation matrix from the given Quaternion rotation value.
        /// </summary>
        /// <param name="quaternion">The source Quaternion.</param>
        /// <returns>The rotation matrix.</returns>
        public static Matrix3x3 CreateFromQuaternion(Quaternion quaternion)
            => quaternion.ConvertToRotationMatrix();

        /// <summary>
        /// Gets the copy.
        /// </summary>
        /// <returns>copy of the matrix33</returns>
        public Matrix3x3 GetCopy() => new Matrix3x3
        {
            M11 = M11,
            M12 = M12,
            M13 = M13,
            M21 = M21,
            M22 = M22,
            M23 = M23,
            M31 = M31,
            M32 = M32,
            M33 = M33
        };

        /// <summary>
        /// Gets the transpose.
        /// </summary>
        /// <returns>copy of the matrix33</returns>
        public Matrix3x3 Transpose => new Matrix3x3
        {
            M11 = M11,
            M12 = M21,
            M13 = M31,
            M21 = M12,
            M22 = M22,
            M23 = M32,
            M31 = M13,
            M32 = M23,
            M33 = M33
        };

        /// <summary>
        /// Multiply the 3x3 matrix by a Vector 3 to get the rotation
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <returns></returns>
        public Vector3 Mult(Vector3 vector) => new Vector3
        {
            X = (vector.X * M11) + (vector.Y * M21) + (vector.Z * M31),
            Y = (vector.X * M12) + (vector.Y * M22) + (vector.Z * M32),
            Z = (vector.X * M13) + (vector.Y * M23) + (vector.Z * M33)
        };

        public static Vector3 operator *(Matrix3x3 rhs, Vector3 lhs) => rhs.Mult(lhs);

        /// <summary>
        /// Determines whether the matrix decomposes nicely into scale * rotation.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is scale rotation]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsScaleRotation
        {
            get
            {
                var mat = this * Transpose;
                if (Math.Abs(mat.M12) + Math.Abs(mat.M13)
                    + Math.Abs(mat.M21) + Math.Abs(mat.M23)
                    + Math.Abs(mat.M31) + Math.Abs(mat.M32) > 0.01) return false;
                return true;
            }
        }

        /// <summary>
        /// Get the scale, assuming IsScaleRotation is true
        /// </summary>
        /// <returns></returns>
        public Vector3 GetScale()
        {
            var mat = this * Transpose;
            var scale = new Vector3
            {
                X = (float)Math.Pow(mat.M11, 0.5f),
                Y = (float)Math.Pow(mat.M22, 0.5f),
                Z = (float)Math.Pow(mat.M33, 0.5f)
            };
            if (GetDeterminant() < 0)
            {
                scale.X = 0 - scale.X;
                scale.Y = 0 - scale.Y;
                scale.Z = 0 - scale.Z;
                return scale;
            }
            return scale;
        }

        /// <summary>
        /// Gets the scale, should also return the rotation matrix, but..eh...
        /// </summary>
        /// <returns></returns>
        public Vector3 GetScaleRotation() => GetScale();

        public bool IsRotation
        {
            get
            {
                // NOTE: 0.01 instead of CgfFormat.EPSILON to work around bad files
                if (!IsScaleRotation) return false;
                var scale = GetScale();
                return Math.Abs(scale.X - 1.0f) > 0.01f || Math.Abs(scale.Y - 1.0f) > 0.01f || Math.Abs(scale.Z - 1.0f) > 0.1f ? false : true;
            }
        }

        //public Matrix3x3 Inverse() => this.ToMathMatrix().Inverse().ToMatrix3x3();
        //public Matrix3x3 Conjugate() => this.ToMathMatrix().Conjugate().ToMatrix3x3();
        //public Matrix3x3 ConjugateTranspose() => this.ToMathMatrix().ConjugateTranspose().ToMatrix3x3();
        //public Matrix3x3 ConjugateTransposeThisAndMultiply(Matrix3x3 inputMatrix) => this.ToMathMatrix().ConjugateTransposeThisAndMultiply(inputMatrix.ToMathMatrix()).ToMatrix3x3();
        //public Vector3 Diagonal() => new Vector3().ToVector3(this.ToMathMatrix().Diagonal());

        #endregion

        static readonly Matrix3x3 _identity = new Matrix3x3
        (
            1f, 0f, 0f,
            0f, 1f, 0f,
            0f, 0f, 1f
        );

        /// <summary>
        /// Returns the multiplicative identity matrix.
        /// </summary>
        public static Matrix3x3 Identity => _identity;

        /// <summary>
        /// Returns whether the matrix is the identity matrix.
        /// </summary>
        public bool IsIdentity
            => M11 == 1f && M22 == 1f && M33 == 1f && // Check diagonal element first for early out.
            M12 == 0f && M13 == 0f &&
            M21 == 0f && M23 == 0f &&
            M31 == 0f && M32 == 0f;

        /// <summary>
        /// Gets or sets the translation component of this matrix.
        /// </summary>
        public Vector2 Translation
        {
            get => new Vector2(M31, M32);
            set
            {
                M31 = value.X;
                M32 = value.Y;
            }
        }

        /// <summary>
        /// Constructs a Matrix3x3 from the given components.
        /// </summary>
        public Matrix3x3(float m11, float m12, float m13,
                         float m21, float m22, float m23,
                         float m31, float m32, float m33)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;

            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;

            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
        }

        /// <summary>
        /// Constructs a Matrix3x3 from the given Matrix3x2.
        /// </summary>
        /// <param name="value">The source Matrix3x2.</param>
        public Matrix3x3(Matrix3x2 value)
        {
            M11 = value.M11;
            M12 = value.M12;
            M13 = 0f;
            M21 = value.M21;
            M22 = value.M22;
            M23 = 0f;
            M31 = 0f;
            M32 = 0f;
            M33 = 1f;
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="position">The amount to translate in each axis.</param>
        /// <returns>The translation matrix.</returns>
        public static Matrix3x3 CreateTranslation(Vector2 position)
        {
            Matrix3x3 result;

            result.M11 = 1.0f;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = 1.0f;
            result.M23 = 0.0f;
            result.M31 = position.X;
            result.M32 = position.Y;
            result.M33 = 1.0f;

            return result;
        }

        /// <summary>
        /// Creates a translation matrix.
        /// </summary>
        /// <param name="xPosition">The amount to translate on the X-axis.</param>
        /// <param name="yPosition">The amount to translate on the Y-axis.</param>
        /// <returns>The translation matrix.</returns>
        public static Matrix3x3 CreateTranslation(float xPosition, float yPosition)
        {
            Matrix3x3 result;

            result.M11 = 1.0f;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = 1.0f;
            result.M23 = 0.0f;
            result.M31 = xPosition;
            result.M32 = yPosition;
            result.M33 = 1.0f;

            return result;
        }

        /// <summary>
        /// Creates a scaling matrix.
        /// </summary>
        /// <param name="xScale">Value to scale by on the X-axis.</param>
        /// <param name="yScale">Value to scale by on the Y-axis.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix3x3 CreateScale(float xScale, float yScale)
        {
            Matrix3x3 result;

            result.M11 = xScale;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = yScale;
            result.M23 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;

            return result;
        }

        /// <summary>
        /// Creates a scaling matrix with a center point.
        /// </summary>
        /// <param name="xScale">Value to scale by on the X-axis.</param>
        /// <param name="yScale">Value to scale by on the Y-axis.</param>
        /// <param name="centerPoint">The center point.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix3x3 CreateScale(float xScale, float yScale, Vector2 centerPoint)
        {
            Matrix3x3 result;

            float tx = centerPoint.X * (1 - xScale);
            float ty = centerPoint.Y * (1 - yScale);

            result.M11 = xScale;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = yScale;
            result.M23 = 0.0f;
            result.M31 = tx;
            result.M32 = ty;
            result.M33 = 1.0f;

            return result;
        }

        /// <summary>
        /// Creates a scaling matrix.
        /// </summary>
        /// <param name="scales">The vector containing the amount to scale by on each axis.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix3x3 CreateScale(Vector2 scales)
        {
            Matrix3x3 result;

            result.M11 = scales.X;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = scales.Y;
            result.M23 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;

            return result;
        }

        /// <summary>
        /// Creates a scaling matrix with a center point.
        /// </summary>
        /// <param name="scales">The vector containing the amount to scale by on each axis.</param>
        /// <param name="centerPoint">The center point.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix3x3 CreateScale(Vector2 scales, Vector2 centerPoint)
        {
            Matrix3x3 result;

            float tx = centerPoint.X * (1 - scales.X);
            float ty = centerPoint.Y * (1 - scales.Y);

            result.M11 = scales.X;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = scales.Y;
            result.M23 = 0.0f;
            result.M31 = tx;
            result.M32 = ty;
            result.M33 = 1.0f;

            return result;
        }

        /// <summary>
        /// Creates a uniform scaling matrix that scales equally on each axis.
        /// </summary>
        /// <param name="scale">The uniform scaling factor.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix3x3 CreateScale(float scale)
        {
            Matrix3x3 result;

            result.M11 = scale;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = scale;
            result.M23 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = scale;

            return result;
        }

        /// <summary>
        /// Creates a uniform scaling matrix that scales equally on each axis with a center point.
        /// </summary>
        /// <param name="scale">The uniform scaling factor.</param>
        /// <param name="centerPoint">The center point.</param>
        /// <returns>The scaling matrix.</returns>
        public static Matrix3x3 CreateScale(float scale, Vector2 centerPoint)
        {
            Matrix3x3 result;

            float tx = centerPoint.X * (1 - scale);
            float ty = centerPoint.Y * (1 - scale);

            result.M11 = scale;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = scale;
            result.M23 = 0.0f;
            result.M31 = tx;
            result.M32 = ty;
            result.M33 = 1.0f;

            return result;
        }

        /// <summary>
        /// Creates a matrix for rotating points around the X-axis.
        /// </summary>
        /// <param name="radians">The amount, in radians, by which to rotate around the X-axis.</param>
        /// <returns>The rotation matrix.</returns>
        public static Matrix3x3 CreateRotationX(float radians)
        {
            Matrix3x3 result;

            float c = (float)Math.Cos(radians);
            float s = (float)Math.Sin(radians);

            // [  c  s  0 ]
            // [ -s  c  0 ]
            // [  0  0  1 ]
            result.M11 = c;
            result.M12 = s;
            result.M13 = 0.0f;
            result.M21 = -s;
            result.M22 = c;
            result.M23 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;

            return result;
        }

        /// <summary>
        /// Creates a matrix for rotating points around the X-axis, from a center point.
        /// </summary>
        /// <param name="radians">The amount, in radians, by which to rotate around the X-axis.</param>
        /// <param name="centerPoint">The center point.</param>
        /// <returns>The rotation matrix.</returns>
        public static Matrix3x3 CreateRotationX(float radians, Vector2 centerPoint)
        {
            Matrix3x3 result;

            float c = (float)Math.Cos(radians);
            float s = (float)Math.Sin(radians);

            float x = centerPoint.X * (1 - c) + centerPoint.Y * s;
            float y = centerPoint.Y * (1 - c) - centerPoint.X * s;

            // [  c  s  0 ]
            // [ -s  c  0 ]
            // [  x  y  1 ]
            result.M11 = c;
            result.M12 = s;
            result.M13 = 0.0f;
            result.M21 = -s;
            result.M22 = c;
            result.M23 = 0.0f;
            result.M31 = x;
            result.M32 = y;
            result.M33 = 1.0f;

            return result;
        }

        /// <summary>
        /// Calculates the determinant of the matrix.
        /// </summary>
        /// <returns>The determinant of the matrix.</returns>
        public float GetDeterminant()
        {
            var det2_12_01 = M21 * M22 - M22 * M31;
            var det2_12_02 = M21 * M23 - M23 * M31;
            var det2_12_12 = M22 * M23 - M23 * M32;
            return M11 * det2_12_12 - M12 * det2_12_02 + M13 * det2_12_01;
        }

        /// <summary>
        /// Attempts to calculate the inverse of the given matrix. If successful, result will contain the inverted matrix.
        /// </summary>
        /// <param name="matrix">The source matrix to invert.</param>
        /// <param name="result">If successful, contains the inverted matrix.</param>
        /// <returns>True if the source matrix could be inverted; False otherwise.</returns>
        public static bool Invert(Matrix3x3 matrix, out Matrix3x3 result)
        {
            // 18+3+9 = 30 multiplications
            //			 1 division
            result = new Matrix3x3(); double det, invDet;

            result.M11 = matrix.M22 * matrix.M33 - matrix.M23 * matrix.M32;
            result.M21 = matrix.M23 * matrix.M31 - matrix.M21 * matrix.M33;
            result.M31 = matrix.M21 * matrix.M32 - matrix.M22 * matrix.M31;

            det = matrix.M11 * result.M11 + matrix.M12 * result.M21 + matrix.M13 * result.M31;
            if (Math.Abs(det) < float.Epsilon) return false;

            result.M12 = matrix.M13 * matrix.M32 - matrix.M12 * matrix.M33;
            result.M13 = matrix.M12 * matrix.M23 - matrix.M13 * matrix.M22;
            result.M22 = matrix.M11 * matrix.M33 - matrix.M13 * matrix.M31;
            result.M23 = matrix.M13 * matrix.M21 - matrix.M11 * matrix.M23;
            result.M32 = matrix.M12 * matrix.M31 - matrix.M11 * matrix.M32;
            result.M33 = matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21;

            invDet = 1f / det;
            matrix.M11 = (float)(result.M11 * invDet); matrix.M12 = (float)(result.M12 * invDet); matrix.M13 = (float)(result.M13 * invDet);
            matrix.M21 = (float)(result.M21 * invDet); matrix.M22 = (float)(result.M22 * invDet); matrix.M23 = (float)(result.M23 * invDet);
            matrix.M31 = (float)(result.M31 * invDet); matrix.M32 = (float)(result.M32 * invDet); matrix.M33 = (float)(result.M33 * invDet);

            return true;
        }

        /// <summary>
        /// Returns a new matrix with the negated elements of the given matrix.
        /// </summary>
        /// <param name="value">The source matrix.</param>
        /// <returns>The negated matrix.</returns>
        public static Matrix3x3 Negate(Matrix3x3 value)
        {
            Matrix3x3 result;

            result.M11 = -value.M11;
            result.M12 = -value.M12;
            result.M13 = -value.M13;
            result.M21 = -value.M21;
            result.M22 = -value.M22;
            result.M23 = -value.M23;
            result.M31 = -value.M31;
            result.M32 = -value.M32;
            result.M33 = -value.M33;

            return result;
        }

        /// <summary>
        /// Adds two matrices together.
        /// </summary>
        /// <param name="value1">The first source matrix.</param>
        /// <param name="value2">The second source matrix.</param>
        /// <returns>The resulting matrix.</returns>
        public static Matrix3x3 Add(Matrix3x3 value1, Matrix3x3 value2)
        {
            Matrix3x3 result;

            result.M11 = value1.M11 + value2.M11;
            result.M12 = value1.M12 + value2.M12;
            result.M13 = value1.M13 + value2.M13;
            result.M21 = value1.M21 + value2.M21;
            result.M22 = value1.M22 + value2.M22;
            result.M23 = value1.M23 + value2.M23;
            result.M31 = value1.M31 + value2.M31;
            result.M32 = value1.M32 + value2.M32;
            result.M33 = value1.M33 + value2.M33;

            return result;
        }

        /// <summary>
        /// Subtracts the second matrix from the first.
        /// </summary>
        /// <param name="value1">The first source matrix.</param>
        /// <param name="value2">The second source matrix.</param>
        /// <returns>The result of the subtraction.</returns>
        public static Matrix3x3 Subtract(Matrix3x3 value1, Matrix3x3 value2)
        {
            Matrix3x3 result;

            result.M11 = value1.M11 - value2.M11;
            result.M12 = value1.M12 - value2.M12;
            result.M13 = value1.M13 - value2.M13;
            result.M21 = value1.M21 - value2.M21;
            result.M22 = value1.M22 - value2.M22;
            result.M23 = value1.M23 - value2.M23;
            result.M31 = value1.M31 - value2.M31;
            result.M32 = value1.M32 - value2.M32;
            result.M33 = value1.M33 - value2.M33;

            return result;
        }

        /// <summary>
        /// Multiplies a matrix by another matrix.
        /// </summary>
        /// <param name="value1">The first source matrix.</param>
        /// <param name="value2">The second source matrix.</param>
        /// <returns>The result of the multiplication.</returns>
        public static Matrix3x3 Multiply(Matrix3x3 value1, Matrix3x3 value2)
        {
            Matrix3x3 result;

            // First row
            result.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21 + value1.M13 * value2.M31;
            result.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22 + value1.M13 * value2.M32;
            result.M13 = value1.M11 * value2.M13 + value1.M12 * value2.M23 + value1.M13 * value2.M33;

            // Second row
            result.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21 + value1.M23 * value2.M31;
            result.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22 + value1.M23 * value2.M32;
            result.M23 = value1.M21 * value2.M13 + value1.M22 * value2.M23 + value1.M23 * value2.M33;

            // Third row
            result.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value1.M33 * value2.M31;
            result.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value1.M33 * value2.M32;
            result.M33 = value1.M31 * value2.M13 + value1.M32 * value2.M23 + value1.M33 * value2.M33;

            return result;
        }

        /// <summary>
        /// Multiplies a matrix by a scalar value.
        /// </summary>
        /// <param name="value1">The source matrix.</param>
        /// <param name="value2">The scaling factor.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix3x3 Multiply(Matrix3x3 value1, float value2)
        {
            Matrix3x3 result;

            result.M11 = value1.M11 * value2;
            result.M12 = value1.M12 * value2;
            result.M13 = value1.M13 * value2;
            result.M21 = value1.M21 * value2;
            result.M22 = value1.M22 * value2;
            result.M23 = value1.M23 * value2;
            result.M31 = value1.M31 * value2;
            result.M32 = value1.M32 * value2;
            result.M33 = value1.M33 * value2;

            return result;
        }

        /// <summary>
        /// Returns a new matrix with the negated elements of the given matrix.
        /// </summary>
        /// <param name="value">The source matrix.</param>
        /// <returns>The negated matrix.</returns>
        public static Matrix3x3 operator -(Matrix3x3 value)
        {
            Matrix3x3 m;

            m.M11 = -value.M11;
            m.M12 = -value.M12;
            m.M13 = -value.M13;
            m.M21 = -value.M21;
            m.M22 = -value.M22;
            m.M23 = -value.M23;
            m.M31 = -value.M31;
            m.M32 = -value.M32;
            m.M33 = -value.M33;

            return m;
        }

        /// <summary>
        /// Adds two matrices together.
        /// </summary>
        /// <param name="value1">The first source matrix.</param>
        /// <param name="value2">The second source matrix.</param>
        /// <returns>The resulting matrix.</returns>
        public static Matrix3x3 operator +(Matrix3x3 value1, Matrix3x3 value2)
        {
            Matrix3x3 m;

            m.M11 = value1.M11 + value2.M11;
            m.M12 = value1.M12 + value2.M12;
            m.M13 = value1.M13 + value2.M13;
            m.M21 = value1.M21 + value2.M21;
            m.M22 = value1.M22 + value2.M22;
            m.M23 = value1.M23 + value2.M23;
            m.M31 = value1.M31 + value2.M31;
            m.M32 = value1.M32 + value2.M32;
            m.M33 = value1.M33 + value2.M33;

            return m;
        }

        /// <summary>
        /// Subtracts the second matrix from the first.
        /// </summary>
        /// <param name="value1">The first source matrix.</param>
        /// <param name="value2">The second source matrix.</param>
        /// <returns>The result of the subtraction.</returns>
        public static Matrix3x3 operator -(Matrix3x3 value1, Matrix3x3 value2)
        {
            Matrix3x3 m;

            m.M11 = value1.M11 - value2.M11;
            m.M12 = value1.M12 - value2.M12;
            m.M13 = value1.M13 - value2.M13;
            m.M21 = value1.M21 - value2.M21;
            m.M22 = value1.M22 - value2.M22;
            m.M23 = value1.M23 - value2.M23;
            m.M31 = value1.M31 - value2.M31;
            m.M32 = value1.M32 - value2.M32;
            m.M33 = value1.M33 - value2.M33;

            return m;
        }

        /// <summary>
        /// Multiplies a matrix by another matrix.
        /// </summary>
        /// <param name="value1">The first source matrix.</param>
        /// <param name="value2">The second source matrix.</param>
        /// <returns>The result of the multiplication.</returns>
        public static Matrix3x3 operator *(Matrix3x3 value1, Matrix3x3 value2)
        {
            Matrix3x3 m;

            // First row
            m.M11 = value1.M11 * value2.M11 + value1.M12 * value2.M21 + value1.M13 * value2.M31;
            m.M12 = value1.M11 * value2.M12 + value1.M12 * value2.M22 + value1.M13 * value2.M32;
            m.M13 = value1.M11 * value2.M13 + value1.M12 * value2.M23 + value1.M13 * value2.M33;

            // Second row
            m.M21 = value1.M21 * value2.M11 + value1.M22 * value2.M21 + value1.M23 * value2.M31;
            m.M22 = value1.M21 * value2.M12 + value1.M22 * value2.M22 + value1.M23 * value2.M32;
            m.M23 = value1.M21 * value2.M13 + value1.M22 * value2.M23 + value1.M23 * value2.M33;

            // Third row
            m.M31 = value1.M31 * value2.M11 + value1.M32 * value2.M21 + value1.M33 * value2.M31;
            m.M32 = value1.M31 * value2.M12 + value1.M32 * value2.M22 + value1.M33 * value2.M32;
            m.M33 = value1.M31 * value2.M13 + value1.M32 * value2.M23 + value1.M33 * value2.M33;

            return m;
        }

        /// <summary>
        /// Multiplies a matrix by a scalar value.
        /// </summary>
        /// <param name="value1">The source matrix.</param>
        /// <param name="value2">The scaling factor.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix3x3 operator *(Matrix3x3 value1, float value2)
        {
            Matrix3x3 m;

            m.M11 = value1.M11 * value2;
            m.M12 = value1.M12 * value2;
            m.M13 = value1.M13 * value2;
            m.M21 = value1.M21 * value2;
            m.M22 = value1.M22 * value2;
            m.M23 = value1.M23 * value2;
            m.M31 = value1.M31 * value2;
            m.M32 = value1.M32 * value2;
            m.M33 = value1.M33 * value2;

            return m;
        }

        /// <summary>
        /// Returns a boolean indicating whether the given two matrices are equal.
        /// </summary>
        /// <param name="value1">The first matrix to compare.</param>
        /// <param name="value2">The second matrix to compare.</param>
        /// <returns>True if the given matrices are equal; False otherwise.</returns>
        public static bool operator ==(Matrix3x3 value1, Matrix3x3 value2)
            => value1.M11 == value2.M11 && value1.M22 == value2.M22 && value1.M33 == value2.M33 && // Check diagonal element first for early out.
            value1.M12 == value2.M12 && value1.M13 == value2.M13 &&
            value1.M21 == value2.M21 && value1.M23 == value2.M23 &&
            value1.M31 == value2.M31 && value1.M32 == value2.M32;

        /// <summary>
        /// Returns a boolean indicating whether the given two matrices are not equal.
        /// </summary>
        /// <param name="value1">The first matrix to compare.</param>
        /// <param name="value2">The second matrix to compare.</param>
        /// <returns>True if the given matrices are not equal; False if they are equal.</returns>
        public static bool operator !=(Matrix3x3 value1, Matrix3x3 value2)
            => value1.M11 != value2.M11 || value1.M12 != value2.M12 || value1.M13 != value2.M13 ||
            value1.M21 != value2.M21 || value1.M22 != value2.M22 || value1.M23 != value2.M23 ||
            value1.M31 != value2.M31 || value1.M32 != value2.M32 || value1.M33 != value2.M33;

        /// <summary>
        /// Returns a boolean indicating whether this matrix instance is equal to the other given matrix.
        /// </summary>
        /// <param name="other">The matrix to compare this instance to.</param>
        /// <returns>True if the matrices are equal; False otherwise.</returns>
        public bool Equals(Matrix3x3 other)
            => M11 == other.M11 && M22 == other.M22 && M33 == other.M33 && // Check diagonal element first for early out.
            M12 == other.M12 && M13 == other.M13 &&
            M21 == other.M21 && M23 == other.M23 &&
            M31 == other.M31 && M32 == other.M32;

        /// <summary>
        /// Returns a boolean indicating whether the given Object is equal to this matrix instance.
        /// </summary>
        /// <param name="obj">The Object to compare against.</param>
        /// <returns>True if the Object is equal to this matrix; False otherwise.</returns>
        public override bool Equals(object obj)
            => obj is Matrix3x3 x && Equals(x);

        /// <summary>
        /// Returns a String representing this matrix instance.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            var ci = CultureInfo.CurrentCulture;
            return string.Format(ci, "{{ {{M11:{0} M12:{1} M13:{2}}} {{M21:{3} M22:{4} M23:{5}}} {{M31:{6} M32:{7} M33:{8}}} }}",
            M11.ToString(ci), M12.ToString(ci), M13.ToString(ci),
            M21.ToString(ci), M22.ToString(ci), M23.ToString(ci),
            M31.ToString(ci), M32.ToString(ci), M33.ToString(ci));
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
            => M11.GetHashCode() + M12.GetHashCode() + M13.GetHashCode() +
            M21.GetHashCode() + M22.GetHashCode() + M23.GetHashCode() +
            M31.GetHashCode() + M32.GetHashCode() + M33.GetHashCode();
    }
}