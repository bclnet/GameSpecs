//using MathNet.Numerics.LinearAlgebra;
//using System;
//using static GameEstate.EstateDebug;

//namespace GameEstate.Cry.Formats
//{
//    /// <summary>
//    /// Vector in 3D space {x,y,z}
//    /// </summary>
//    public struct Vector3
//    {
//        public float x;
//        public float y;
//        public float z;
//        public float w; // Currently Unused

//        public Vector3(float x, float y, float z) : this() { this.x = x; this.y = y; this.z = z; }
//        public Vector3 Add(Vector3 vector) => new Vector3 { x = vector.x + x, y = vector.y + y, z = vector.z + z };
//        public static Vector3 operator +(Vector3 lhs, Vector3 rhs) => new Vector3 { x = lhs.x + rhs.x, y = lhs.y + rhs.y, z = lhs.z + rhs.z };
//        public static Vector3 operator -(Vector3 lhs, Vector3 rhs) => new Vector3 { x = lhs.x - rhs.x, y = lhs.y - rhs.y, z = lhs.z - rhs.z };
//        public Vector4 ToVector4() => new Vector4 { x = x, y = y, z = z, w = 1 };
//    }

//    public struct Vector4
//    {
//        public float x;
//        public float y;
//        public float z;
//        public float w;

//        public Vector4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
//        public Vector3 ToVector3() { var r = new Vector3(); if (w == 0) { r.x = x; r.y = y; r.z = z; } else { r.x = x / w; r.y = y / w; r.z = z / w; } return r; }
//    }

//    public struct Matrix3x3    // a 3x3 transformation matrix
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

//        /// <summary>
//        /// Determines whether this instance is identity.
//        /// </summary>
//        /// <returns>
//        ///   <c>true</c> if this instance is identity; otherwise, <c>false</c>.
//        /// </returns>
//        public bool IsIdentity() =>
//            Math.Abs(M11 - 1.0) > 0.00001 ||
//            Math.Abs(M12) > 0.00001 ||
//            Math.Abs(M13) > 0.00001 ||
//            Math.Abs(M21) > 0.00001 ||
//            Math.Abs(M22 - 1.0) > 0.00001 ||
//            Math.Abs(M23) > 0.00001 ||
//            Math.Abs(M31) > 0.00001 ||
//            Math.Abs(M32) > 0.00001 ||
//            Math.Abs(M33 - 1.0) > 0.00001
//                ? false
//                : true;

//        /// <summary>
//        /// Gets the copy.
//        /// </summary>
//        /// <returns>copy of the matrix33</returns>
//        public Matrix3x3 GetCopy() => new Matrix3x3
//        {
//            M11 = M11,
//            M12 = M12,
//            M13 = M13,
//            M21 = M21,
//            M22 = M22,
//            M23 = M23,
//            M31 = M31,
//            M32 = M32,
//            M33 = M33
//        };

//        public float GetDeterminant() =>
//            M11 * M22 * M33
//            + M12 * M23 * M31
//            + M13 * M21 * M32
//            - M31 * M22 * M13
//            - M21 * M12 * M33
//            - M11 * M32 * M23;

//        /// <summary>
//        /// Gets the transpose.
//        /// </summary>
//        /// <returns>copy of the matrix33</returns>
//        public Matrix3x3 GetTranspose() => new Matrix3x3
//        {
//            M11 = M11,
//            M12 = M21,
//            M13 = M31,
//            M21 = M12,
//            M22 = M22,
//            M23 = M32,
//            M31 = M13,
//            M32 = M23,
//            M33 = M33
//        };

//        public Matrix3x3 Mult(Matrix3x3 mat) => new Matrix3x3
//        {
//            M11 = (M11 * mat.M11) + (M12 * mat.M21) + (M13 * mat.M31),
//            M12 = (M11 * mat.M12) + (M12 * mat.M22) + (M13 * mat.M32),
//            M13 = (M11 * mat.M13) + (M12 * mat.M23) + (M13 * mat.M33),
//            M21 = (M21 * mat.M11) + (M22 * mat.M21) + (M23 * mat.M31),
//            M22 = (M21 * mat.M12) + (M22 * mat.M22) + (M23 * mat.M32),
//            M23 = (M21 * mat.M13) + (M22 * mat.M23) + (M23 * mat.M33),
//            M31 = (M31 * mat.M11) + (M32 * mat.M21) + (M33 * mat.M31),
//            M32 = (M31 * mat.M12) + (M32 * mat.M22) + (M33 * mat.M32),
//            M33 = (M31 * mat.M13) + (M32 * mat.M23) + (M33 * mat.M33)
//        };

//        public static Matrix3x3 operator *(Matrix3x3 lhs, Matrix3x3 rhs) => lhs.Mult(rhs);

//        /// <summary>
//        /// Multiply the 3x3 matrix by a Vector 3 to get the rotation
//        /// </summary>
//        /// <param name="vector">The vector.</param>
//        /// <returns></returns>
//        public Vector3 Mult3x1(Vector3 vector) => new Vector3
//        {
//            x = (vector.x * M11) + (vector.y * M21) + (vector.z * M31),
//            y = (vector.x * M12) + (vector.y * M22) + (vector.z * M32),
//            z = (vector.x * M13) + (vector.y * M23) + (vector.z * M33)
//        };

//        public static Vector3 operator *(Matrix3x3 rhs, Vector3 lhs) => rhs.Mult3x1(lhs);

//        /// <summary>
//        /// Determines whether the matrix decomposes nicely into scale * rotation.
//        /// </summary>
//        /// <returns>
//        ///   <c>true</c> if [is scale rotation]; otherwise, <c>false</c>.
//        /// </returns>
//        public bool IsScaleRotation()
//        {
//            var transpose = GetTranspose();
//            var mat = Mult(transpose);
//            if (Math.Abs(mat.M12) + Math.Abs(mat.M13)
//                + Math.Abs(mat.M21) + Math.Abs(mat.M23)
//                + Math.Abs(mat.M31) + Math.Abs(mat.M32) > 0.01) { Log(" is a Scale_Rot matrix"); return false; }
//            Log(" is not a Scale_Rot matrix");
//            return true;
//        }

//        /// <summary>
//        /// Get the scale, assuming IsScaleRotation is true
//        /// </summary>
//        /// <returns></returns>
//        public Vector3 GetScale()
//        {
//            var mat = Mult(GetTranspose());
//            var scale = new Vector3
//            {
//                x = (float)Math.Pow(mat.M11, 0.5f),
//                y = (float)Math.Pow(mat.M22, 0.5f),
//                z = (float)Math.Pow(mat.M33, 0.5f)
//            };
//            if (GetDeterminant() < 0)
//            {
//                scale.x = 0 - scale.x;
//                scale.y = 0 - scale.y;
//                scale.z = 0 - scale.z;
//                return scale;
//            }
//            return scale;
//        }

//        /// <summary>
//        /// Gets the scale, should also return the rotation matrix, but..eh...
//        /// </summary>
//        /// <returns></returns>
//        public Vector3 GetScaleRotation() => GetScale();

//        public bool IsRotation()
//        {
//            // NOTE: 0.01 instead of CgfFormat.EPSILON to work around bad files
//            if (!IsScaleRotation()) return false;
//            var scale = GetScale();
//            return Math.Abs(scale.x - 1.0f) > 0.01f || Math.Abs(scale.y - 1.0f) > 0.01f || Math.Abs(scale.z - 1.0f) > 0.1f ? false : true;
//        }

//        public float Determinant() => this.ToMathMatrix().Determinant();
//        public Matrix3x3 Inverse() => this.ToMathMatrix().Inverse().ToMatrix3x3();
//        public Matrix3x3 Conjugate() => this.ToMathMatrix().Conjugate().ToMatrix3x3();
//        public Matrix3x3 ConjugateTranspose() => this.ToMathMatrix().ConjugateTranspose().ToMatrix3x3();
//        public Matrix3x3 ConjugateTransposeThisAndMultiply(Matrix3x3 inputMatrix) => this.ToMathMatrix().ConjugateTransposeThisAndMultiply(inputMatrix.ToMathMatrix()).ToMatrix3x3();
//        public Vector3 Diagonal() => new Vector3().ToVector3(this.ToMathMatrix().Diagonal());
//    }

//    /// <summary>
//    /// A 4x4 Transformation matrix.  These are row major matrices (M24 is first row, 3rd column). [first value is row, second is column.]
//    /// </summary>
//    public struct Matrix4x4
//    {
//        public float M11;
//        public float M12;
//        public float M13;
//        public float M14;
//        public float M21;
//        public float M22;
//        public float M23;
//        public float M24;
//        public float M31;
//        public float M32;
//        public float M33;
//        public float M34;
//        public float M41;
//        public float M42;
//        public float M43;
//        public float M44;

//        /// <summary>
//        /// Pass the matrix a Vector4 (4x1) vector to get the transform of the vector
//        /// </summary>
//        /// <param name="vector">The vector.</param>
//        /// <returns></returns>
//        public Vector4 Mult4x1(Vector4 vector) => new Vector4
//        {
//            x = (M11 * vector.x) + (M21 * vector.y) + (M31 * vector.z) + M41 / 100f,
//            y = (M12 * vector.x) + (M22 * vector.y) + (M32 * vector.z) + M42 / 100f,
//            z = (M13 * vector.x) + (M23 * vector.y) + (M33 * vector.z) + M43 / 100f,
//            w = (M14 * vector.x) + (M24 * vector.y) + (M34 * vector.z) + M44 / 100f
//        };

//        public static Vector4 operator *(Matrix4x4 lhs, Vector4 vector) => new Vector4
//        {
//            x = (lhs.M11 * vector.x) + (lhs.M21 * vector.y) + (lhs.M31 * vector.z) + lhs.M41 / 100f,
//            y = (lhs.M12 * vector.x) + (lhs.M22 * vector.y) + (lhs.M32 * vector.z) + lhs.M42 / 100f,
//            z = (lhs.M13 * vector.x) + (lhs.M23 * vector.y) + (lhs.M33 * vector.z) + lhs.M43 / 100f,
//            w = (lhs.M14 * vector.x) + (lhs.M24 * vector.y) + (lhs.M34 * vector.z) + lhs.M44 / 100f
//        };

//        public static Matrix4x4 operator *(Matrix4x4 lhs, Matrix4x4 rhs) => new Matrix4x4
//        {
//            // First row
//            M11 = (lhs.M11 * rhs.M11) + (lhs.M12 * rhs.M21) + (lhs.M13 * rhs.M31) + (lhs.M14 * rhs.M41),
//            M12 = (lhs.M11 * rhs.M12) + (lhs.M12 * rhs.M22) + (lhs.M13 * rhs.M32) + (lhs.M14 * rhs.M42),
//            M13 = (lhs.M11 * rhs.M13) + (lhs.M12 * rhs.M23) + (lhs.M13 * rhs.M33) + (lhs.M14 * rhs.M43),
//            M14 = (lhs.M11 * rhs.M14) + (lhs.M12 * rhs.M24) + (lhs.M13 * rhs.M34) + (lhs.M14 * rhs.M44),
//            // second row
//            M21 = (lhs.M21 * rhs.M11) + (lhs.M22 * rhs.M21) + (lhs.M23 * rhs.M31) + (lhs.M24 * rhs.M41),
//            M22 = (lhs.M21 * rhs.M12) + (lhs.M22 * rhs.M22) + (lhs.M23 * rhs.M32) + (lhs.M24 * rhs.M42),
//            M23 = (lhs.M21 * rhs.M13) + (lhs.M22 * rhs.M23) + (lhs.M23 * rhs.M33) + (lhs.M24 * rhs.M43),
//            M24 = (lhs.M21 * rhs.M14) + (lhs.M22 * rhs.M24) + (lhs.M23 * rhs.M34) + (lhs.M24 * rhs.M44),
//            // third row
//            M31 = (lhs.M31 * rhs.M11) + (lhs.M32 * rhs.M21) + (lhs.M33 * rhs.M31) + (lhs.M34 * rhs.M41),
//            M32 = (lhs.M31 * rhs.M12) + (lhs.M32 * rhs.M22) + (lhs.M33 * rhs.M32) + (lhs.M34 * rhs.M42),
//            M33 = (lhs.M31 * rhs.M13) + (lhs.M32 * rhs.M23) + (lhs.M33 * rhs.M33) + (lhs.M34 * rhs.M43),
//            M34 = (lhs.M31 * rhs.M14) + (lhs.M32 * rhs.M24) + (lhs.M33 * rhs.M34) + (lhs.M34 * rhs.M44),
//            // fourth row
//            M41 = (lhs.M41 * rhs.M11) + (lhs.M42 * rhs.M21) + (lhs.M43 * rhs.M31) + (lhs.M44 * rhs.M41),
//            M42 = (lhs.M41 * rhs.M12) + (lhs.M42 * rhs.M22) + (lhs.M43 * rhs.M32) + (lhs.M44 * rhs.M42),
//            M43 = (lhs.M41 * rhs.M13) + (lhs.M42 * rhs.M23) + (lhs.M43 * rhs.M33) + (lhs.M44 * rhs.M43),
//            M44 = (lhs.M41 * rhs.M14) + (lhs.M42 * rhs.M24) + (lhs.M43 * rhs.M34) + (lhs.M44 * rhs.M44)
//        };

//        public Vector3 GetTranslation() => new Vector3
//        {
//            x = M14,
//            y = M24,
//            z = M34
//        };

//        /// <summary>
//        /// Gets the Rotation portion of a Transform Matrix44 (upper left).
//        /// </summary>
//        /// <returns>New Matrix33 with the rotation component.</returns>
//        public Matrix3x3 GetRotation() => new Matrix3x3()
//        {
//            M11 = M11,
//            M12 = M12,
//            M13 = M13,
//            M21 = M21,
//            M22 = M22,
//            M23 = M23,
//            M31 = M31,
//            M32 = M32,
//            M33 = M33,
//        };

//        public Vector3 GetScale() => new Vector3
//        {
//            x = M41 / 100f,
//            y = M42 / 100f,
//            z = M43 / 100f
//        };

//        public Vector3 GetBoneTranslation() => new Vector3
//        {
//            x = M14,
//            y = M24,
//            z = M34
//        };

//        public float[,] ConvertTo4x4Array()
//        {
//            var r = new float[4, 4];
//            r[0, 0] = M11;
//            r[0, 1] = M12;
//            r[0, 2] = M13;
//            r[0, 3] = M14;
//            r[1, 0] = M21;
//            r[1, 1] = M22;
//            r[1, 2] = M23;
//            r[1, 3] = M24;
//            r[2, 0] = M31;
//            r[2, 1] = M32;
//            r[2, 2] = M33;
//            r[2, 3] = M34;
//            r[3, 0] = M41;
//            r[3, 1] = M42;
//            r[3, 2] = M43;
//            r[3, 3] = M44;
//            return r;
//        }

//        public Matrix4x4 Inverse() => this.ToMathMatrix().Inverse().ToMatrix4x4();

//        public Matrix4x4 GetTransformFromParts(Vector3 localTranslation, Matrix3x3 localRotation, Vector3 localScale) => new Matrix4x4
//        {
//            // For Node Chunks, the translation appears to be along the bottom of the matrix, and scale on right side.
//            // Translation part
//            M41 = localTranslation.x,
//            M42 = localTranslation.y,
//            M43 = localTranslation.z,
//            // Rotation part
//            M11 = localRotation.M11,
//            M12 = localRotation.M12,
//            M13 = localRotation.M13,
//            M21 = localRotation.M21,
//            M22 = localRotation.M22,
//            M23 = localRotation.M23,
//            M31 = localRotation.M31,
//            M32 = localRotation.M32,
//            M33 = localRotation.M33,
//            // Scale part
//            M14 = localScale.x,
//            M24 = localScale.y,
//            M34 = localScale.z,
//            // Set final row
//            M44 = 1
//        };

//        public static Matrix4x4 Identity() => new Matrix4x4()
//        {
//            M11 = 1,
//            M12 = 0,
//            M13 = 0,
//            M14 = 0,
//            M21 = 0,
//            M22 = 1,
//            M23 = 0,
//            M24 = 0,
//            M31 = 0,
//            M32 = 0,
//            M33 = 1,
//            M34 = 0,
//            M41 = 0,
//            M42 = 0,
//            M43 = 0,
//            M44 = 1
//        };
//    }

//    /// <summary>
//    /// A quaternion (x,y,z,w)
//    /// </summary>
//    public struct Quat
//    {
//        public float x;
//        public float y;
//        public float z;
//        public float w;
//    }
//}