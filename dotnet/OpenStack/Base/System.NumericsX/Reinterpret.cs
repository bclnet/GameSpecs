using System.Runtime.InteropServices;

namespace System.NumericsX
{
    public static class reinterpret
    {
        [StructLayout(LayoutKind.Explicit)]
        internal struct F2ui
        {
            [FieldOffset(0)] public float f;
            [FieldOffset(0)] public uint u;
        }

        [StructLayout(LayoutKind.Explicit)]
        public unsafe struct Color
        {
            [FieldOffset(0)] public int intVal;
            [FieldOffset(0)] public fixed byte color[4];
        }

        public static unsafe int cast_int(float v) => *(int*)&v;
        public static unsafe float cast_float(int v) => *(float*)&v;
        public static unsafe float cast_float(uint v) => *(float*)&v;

        public static unsafe ref Vector2 cast_vec2(Vector3 s) => ref *(Vector2*)&s;
        public static unsafe ref Vector2 cast_vec2(Vector4 s) => ref *(Vector2*)&s;

        public static unsafe ref Vector3 cast_vec3(Vector4 s) => ref *(Vector3*)&s;
        public static unsafe ref Vector3 cast_vec3(Vector5 s) => ref *(Vector3*)&s;
        public static unsafe ref Vector3 cast_vec3(Plane s) => ref *(Vector3*)&s;
        public static unsafe ref Vector3 cast_vec3(float* s, int index) => ref *(Vector3*)&s[index];
        public static unsafe ref Vector3 cast_vec3(float[] s, int index) { fixed (float* s_ = s) return ref *(Vector3*)&s_[index]; }

        public static unsafe ref Vector4 cast_vec4(Plane s) => ref *(Vector4*)&s;
        public static unsafe ref Vector4 cast_vec4(Rectangle s) => ref *(Vector4*)&s;

        public static unsafe ref Vector5 cast_vec5(Vector3 s) => ref *(Vector5*)&s;

        public static unsafe ref Vector6 cast_vec6(float* s, int index) => ref *(Vector6*)&s[index];

        public static unsafe ref Matrix2x2 cast_mat2(float* s) => ref *(Matrix2x2*)s;
        public static unsafe ref Matrix3x3 cast_mat3(float* s) => ref *(Matrix3x3*)s;
        public static unsafe ref Matrix4x4 cast_mat4(float* s) => ref *(Matrix4x4*)s;
        public static unsafe ref Matrix5x5 cast_mat5(float* s) => ref *(Matrix5x5*)s;
        public static unsafe ref Matrix6x6 cast_mat6(float* s) => ref *(Matrix6x6*)s;

    }
}