using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Numerics
{
    // MARK Vector2

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Int2 { public int X; public int Y; public override string ToString() => $"{X},{Y}"; }

    // MARK Vector3

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Int3 { public int X; public int Y; public int Z; public Int3(int x, int y, int z) { X = x; Y = y; Z = z; } public override string ToString() => $"{X},{Y},{Z}"; }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Float3 { public float X; public float Y; public float Z; public override string ToString() => $"{X},{Y},{Z}"; public Vector3 ToVector3() => new Vector3(X, Y, Z); }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Byte3 { public byte X; public byte Y; public byte Z; public override string ToString() => $"{X},{Y},{Z}"; }

    public static class Polyfill
    {
        public static Vector3 ParseVector3(string input)
        {
            if (string.IsNullOrEmpty(input)) return default;
            var split = input.Split(' ');
            return split.Length == 3
                ? new Vector3(float.Parse(split[0], CultureInfo.InvariantCulture), float.Parse(split[1], CultureInfo.InvariantCulture), float.Parse(split[2], CultureInfo.InvariantCulture))
                : default;
        }

        public static Matrix4x4 ConvertToTransformationMatrix(string scale, string position, string angles)
            => scale == null || position == null || angles == null
                ? default
                : ConvertToTransformationMatrix(ParseVector3(scale), ParseVector3(position), ParseVector3(angles));

        public static Matrix4x4 ConvertToTransformationMatrix(Vector3 scale, Vector3 position, Vector3 pitchYawRoll)
        {
            var scaleMatrix = Matrix4x4.CreateScale(scale);
            var positionMatrix = Matrix4x4.CreateTranslation(position);
            var rollMatrix = Matrix4x4.CreateRotationX(pitchYawRoll.Z * ((float)Math.PI / 180f)); // Roll
            var pitchMatrix = Matrix4x4.CreateRotationY(pitchYawRoll.X * ((float)Math.PI / 180f)); // Pitch
            var yawMatrix = Matrix4x4.CreateRotationZ(pitchYawRoll.Y * ((float)Math.PI / 180f)); // Yaw

            var rotationMatrix = rollMatrix * pitchMatrix * yawMatrix;
            return scaleMatrix * rotationMatrix * positionMatrix;
        }

        //:ref https://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToMatrix/index.htm
        public static Matrix3x3 ConvertToRotationMatrix(this Quaternion q)
        {
            var rotationalMatrix = new Matrix3x3();
            float sqw = q.W * q.W, sqx = q.X * q.X, sqy = q.Y * q.Y, sqz = q.Z * q.Z;

            // invs (inverse square length) is only required if quaternion is not already normalised
            var invs = 1 / (sqx + sqy + sqz + sqw);
            rotationalMatrix.M11 = (sqx - sqy - sqz + sqw) * invs; // since sqw + sqx + sqy + sqz =1/invs*invs
            rotationalMatrix.M22 = (-sqx + sqy - sqz + sqw) * invs;
            rotationalMatrix.M33 = (-sqx - sqy + sqz + sqw) * invs;

            float tmp1 = q.X * q.Y, tmp2 = q.Z * q.W;
            rotationalMatrix.M21 = 2.0f * (tmp1 + tmp2) * invs;
            rotationalMatrix.M12 = 2.0f * (tmp1 - tmp2) * invs;

            tmp1 = q.X * q.Z; tmp2 = q.Y * q.W;
            rotationalMatrix.M31 = 2.0f * (tmp1 - tmp2) * invs;
            rotationalMatrix.M13 = 2.0f * (tmp1 + tmp2) * invs;
            tmp1 = q.Y * q.Z; tmp2 = q.X * q.W;
            rotationalMatrix.M32 = 2.0f * (tmp1 + tmp2) * invs;
            rotationalMatrix.M23 = 2.0f * (tmp1 - tmp2) * invs;

            return rotationalMatrix;
        }

        /// <summary>
        /// Gets the Rotation portion of a Transform Matrix44 (upper left).
        /// </summary>
        /// <returns>New Matrix33 with the rotation component.</returns>
        public static Matrix3x3 GetRotation(this Matrix4x4 source)
            => new Matrix3x3(
            m11: source.M11,
            m12: source.M12,
            m13: source.M13,
            m21: source.M21,
            m22: source.M22,
            m23: source.M23,
            m31: source.M31,
            m32: source.M32,
            m33: source.M33);


        /// <summary>
        /// Gets the Scale portion of a Transform Matrix44 (upper right).
        /// </summary>
        /// <returns>New Matrix33 with the rotation component.</returns>
        public static Vector3 GetScale(this Matrix4x4 source)
            => new Vector3(
            x: source.M41, //x: source.M41 / 100f,
            y: source.M42, //y: source.M42 / 100f,
            z: source.M43); //z: source.M43 / 100f);

        /// <summary>
        /// Gets the Translation portion of a Transform Matrix44 (lower left).
        /// </summary>
        /// <returns>New Matrix33 with the rotation component.</returns>
        public static Vector3 GetTranslation(this Matrix4x4 source)
            => new Vector3(
            x: source.M14,
            y: source.M24,
            z: source.M34);

        public static Matrix4x4 ToMatrix4x4(this Matrix3x4 source)
            => new Matrix4x4(
            m11: source.M11,
            m12: source.M12,
            m13: source.M13,
            m14: source.M14,
            m21: source.M21,
            m22: source.M22,
            m23: source.M23,
            m24: source.M24,
            m31: source.M31,
            m32: source.M32,
            m33: source.M33,
            m34: source.M34,
            m41: 0,
            m42: 0,
            m43: 0,
            m44: 1);

        public static Matrix4x4 GetRotationMatrix(this Matrix3x3 source)
            => new Matrix4x4(
            m11: source.M11,
            m12: source.M12,
            m13: source.M13,
            m14: 0.0f,
            m21: source.M21,
            m22: source.M22,
            m23: source.M23,
            m24: 0.0f,
            m31: source.M31,
            m32: source.M32,
            m33: source.M33,
            m34: 0.0f,
            m41: 0.0f,
            m42: 0.0f,
            m43: 0.0f,
            m44: 1.0f);

        public static Matrix4x4 CreateLocalTransform(Matrix4x4 parent, Matrix4x4 child)
            => parent * child;

        public static Matrix4x4 CreateTransformFromParts(Vector3 translation, Matrix3x3 rotation)
            => new Matrix4x4(
            m11: rotation.M11,
            m12: rotation.M12,
            m13: rotation.M13,
            m14: translation.X,
            m21: rotation.M21,
            m22: rotation.M22,
            m23: rotation.M23,
            m24: translation.Y,
            m31: rotation.M31,
            m32: rotation.M32,
            m33: rotation.M33,
            m34: translation.Z,
            m41: 0.0f,
            m42: 0.0f,
            m43: 0.0f,
            m44: 1.0f);

        /// <summary>
        /// Flatten an array of matrices to an array of floats.
        /// </summary>
        public static float[] Flatten(this Matrix4x4[] source)
        {
            var r = new float[source.Length * 16];
            for (var i = 0; i < source.Length; i++)
            {
                var s = source[i];
                r[i * 16] = s.M11;
                r[(i * 16) + 1] = s.M12;
                r[(i * 16) + 2] = s.M13;
                r[(i * 16) + 3] = s.M14;
                r[(i * 16) + 4] = s.M21;
                r[(i * 16) + 5] = s.M22;
                r[(i * 16) + 6] = s.M23;
                r[(i * 16) + 7] = s.M24;
                r[(i * 16) + 8] = s.M31;
                r[(i * 16) + 9] = s.M32;
                r[(i * 16) + 10] = s.M33;
                r[(i * 16) + 11] = s.M34;
                r[(i * 16) + 12] = s.M41;
                r[(i * 16) + 13] = s.M42;
                r[(i * 16) + 14] = s.M43;
                r[(i * 16) + 15] = s.M44;
            }
            return r;
        }

        public static readonly float EPSILON = 0.00019999999f;

        public static bool IsZero(this Vector3 v) => v.X == 0 && v.Y == 0 && v.Z == 0;

        public static bool IsZeroEpsilon(this Vector3 v) => Math.Abs(v.X) <= EPSILON && Math.Abs(v.Y) <= EPSILON && Math.Abs(v.Z) <= EPSILON;

        public static bool NearZero(this Vector3 v) => Math.Abs(v.X) <= 1.0f && Math.Abs(v.Y) <= 1.0f && Math.Abs(v.Z) <= 1.0f;

        public static float Get(this Matrix4x4 source, int row, int column)
        {
            if (row == 0)
            {
                if (column == 0) return source.M11;
                else if (column == 1) return source.M12;
                else if (column == 2) return source.M13;
                else if (column == 3) return source.M14;
                else throw new ArgumentOutOfRangeException(nameof(row));
            }
            else if (row == 1)
            {
                if (column == 0) return source.M21;
                else if (column == 1) return source.M22;
                else if (column == 2) return source.M23;
                else if (column == 3) return source.M24;
                else throw new ArgumentOutOfRangeException(nameof(row));
            }
            else if (row == 2)
            {
                if (column == 0) return source.M31;
                else if (column == 1) return source.M32;
                else if (column == 2) return source.M33;
                else if (column == 3) return source.M34;
                else throw new ArgumentOutOfRangeException(nameof(row));
            }
            else if (row == 3)
            {
                if (column == 0) return source.M41;
                else if (column == 1) return source.M42;
                else if (column == 2) return source.M43;
                else if (column == 3) return source.M44;
                else throw new ArgumentOutOfRangeException(nameof(row));
            }
            else throw new ArgumentOutOfRangeException(nameof(column));
        }

        public static void Set(this Matrix4x4 source, int row, int column, float value)
        {
            if (row == 0)
            {
                if (column == 0) source.M11 = value;
                else if (column == 1) source.M12 = value;
                else if (column == 2) source.M13 = value;
                else if (column == 3) source.M14 = value;
                else throw new ArgumentOutOfRangeException(nameof(row));
            }
            else if (row == 1)
            {
                if (column == 0) source.M21 = value;
                else if (column == 1) source.M22 = value;
                else if (column == 2) source.M23 = value;
                else if (column == 3) source.M24 = value;
                else throw new ArgumentOutOfRangeException(nameof(row));
            }
            else if (row == 2)
            {
                if (column == 0) source.M31 = value;
                else if (column == 1) source.M32 = value;
                else if (column == 2) source.M33 = value;
                else if (column == 3) source.M34 = value;
                else throw new ArgumentOutOfRangeException(nameof(row));
            }
            else if (row == 3)
            {
                if (column == 0) source.M41 = value;
                else if (column == 1) source.M42 = value;
                else if (column == 2) source.M43 = value;
                else if (column == 3) source.M44 = value;
                else throw new ArgumentOutOfRangeException(nameof(row));
            }
            else throw new ArgumentOutOfRangeException(nameof(column));
        }
    }
}