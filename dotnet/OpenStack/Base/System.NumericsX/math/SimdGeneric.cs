//#define DERIVE_UNSMOOTHED_BITANGENT
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.NumericsX
{
    public unsafe static class SimdGeneric
    {
        public static void Activate() { }

        const byte True = 1;
        const byte False = 0;

        public static int CpuId => 0;
        public static string Name => "generic code";

        // dst[i] = constant + src[i];
        public static void Add(float* dst, float constant, float* src, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src[_IX + 0] + constant;
                dst[_IX + 1] = src[_IX + 1] + constant;
                dst[_IX + 2] = src[_IX + 2] + constant;
                dst[_IX + 3] = src[_IX + 3] + constant;
            }
            for (; _IX < count; _IX++) { dst[_IX] = src[_IX] + constant; }
        }
        // dst[i] = src0[i] + src1[i];
        public static void Addv(float* dst, float* src0, float* src1, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src0[_IX + 0] + src1[_IX + 0];
                dst[_IX + 1] = src0[_IX + 1] + src1[_IX + 1];
                dst[_IX + 2] = src0[_IX + 2] + src1[_IX + 2];
                dst[_IX + 3] = src0[_IX + 3] + src1[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] = src0[_IX] + src1[_IX]; }
        }
        // dst[i] = constant - src[i];
        public static void Sub(float* dst, float constant, float* src, int count)
        {
            var c = constant; //: double
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = c - src[_IX + 0];
                dst[_IX + 1] = c - src[_IX + 1];
                dst[_IX + 2] = c - src[_IX + 2];
                dst[_IX + 3] = c - src[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] = c - src[_IX]; }
        }
        // dst[i] = src0[i] - src1[i];
        public static void Subv(float* dst, float* src0, float* src1, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src0[_IX + 0] - src1[_IX + 0];
                dst[_IX + 1] = src0[_IX + 1] - src1[_IX + 1];
                dst[_IX + 2] = src0[_IX + 2] - src1[_IX + 2];
                dst[_IX + 3] = src0[_IX + 3] - src1[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] = src0[_IX] - src1[_IX]; }
        }
        // dst[i] = constant * src[i];
        public static void Mul(float* dst, float constant, float* src, int count)
        {
            var c = constant; //: double
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = c * src[_IX + 0];
                dst[_IX + 1] = c * src[_IX + 1];
                dst[_IX + 2] = c * src[_IX + 2];
                dst[_IX + 3] = c * src[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] = c * src[_IX]; }
        }
        // dst[i] = src0[i] * src1[i];
        public static void Mulv(float* dst, float* src0, float* src1, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src0[_IX + 0] * src1[_IX + 0];
                dst[_IX + 1] = src0[_IX + 1] * src1[_IX + 1];
                dst[_IX + 2] = src0[_IX + 2] * src1[_IX + 2];
                dst[_IX + 3] = src0[_IX + 3] * src1[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] = src0[_IX] * src1[_IX]; }
        }
        // dst[i] = constant / src[i];
        public static void Div(float* dst, float constant, float* src, int count)
        {
            var c = constant; //: double
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = c / src[_IX + 0];
                dst[_IX + 1] = c / src[_IX + 1];
                dst[_IX + 2] = c / src[_IX + 2];
                dst[_IX + 3] = c / src[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] = c / src[_IX]; }
        }
        // dst[i] = src0[i] / src1[i];
        public static void Divv(float* dst, float* src0, float* src1, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src0[_IX + 0] / src1[_IX + 0];
                dst[_IX + 1] = src0[_IX + 1] / src1[_IX + 1];
                dst[_IX + 2] = src0[_IX + 2] / src1[_IX + 2];
                dst[_IX + 3] = src0[_IX + 3] / src1[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] = src0[_IX] / src1[_IX]; }
        }
        // dst[i] += constant * src[i];
        public static void MulAdd(float* dst, float constant, float* src, int count)
        {
            var c = constant; //: double
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] += c * src[_IX + 0];
                dst[_IX + 1] += c * src[_IX + 1];
                dst[_IX + 2] += c * src[_IX + 2];
                dst[_IX + 3] += c * src[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] += c * src[_IX]; }
        }
        // dst[i] += src0[i] * src1[i];
        public static void MulAddv(float* dst, float* src0, float* src1, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] += src0[_IX + 0] * src1[_IX + 0];
                dst[_IX + 1] += src0[_IX + 1] * src1[_IX + 1];
                dst[_IX + 2] += src0[_IX + 2] * src1[_IX + 2];
                dst[_IX + 3] += src0[_IX + 3] * src1[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] += src0[_IX] * src1[_IX]; }
        }
        // dst[i] -= constant * src[i];
        public static void MulSub(float* dst, float constant, float* src, int count)
        {
            var c = constant; //: double
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] -= c * src[_IX + 0];
                dst[_IX + 1] -= c * src[_IX + 1];
                dst[_IX + 2] -= c * src[_IX + 2];
                dst[_IX + 3] -= c * src[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] -= c * src[_IX]; }
        }
        // dst[i] -= src0[i] * src1[i];
        public static void MulSubv(float* dst, float* src0, float* src1, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] -= src0[_IX + 0] * src1[_IX + 0];
                dst[_IX + 1] -= src0[_IX + 1] * src1[_IX + 1];
                dst[_IX + 2] -= src0[_IX + 2] * src1[_IX + 2];
                dst[_IX + 3] -= src0[_IX + 3] * src1[_IX + 3];
            }
            for (; _IX < count; _IX++) { dst[_IX] -= src0[_IX] * src1[_IX]; }
        }

        // dst[i] = constant * src[i];
        public static void Dotcv(float* dst, Vector3 constant, Vector3* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = constant * src[_IX];
            }
        }
        // dst[i] = constant * src[i].Normal() + src[i][3];
        public static void Dotcp(float* dst, Vector3 constant, Plane* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = constant * src[_IX].Normal + src[_IX].d;
            }
        }
        // dst[i] = constant * src[i].xyz;
        public static void Dotcd(float* dst, Vector3 constant, DrawVert* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = constant * src[_IX].xyz;
            }
        }
        // dst[i] = constant.Normal() * src[i] + constant[3];
        public static void Dotpv(float* dst, Plane constant, Vector3* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = constant.Normal * src[_IX] + constant.d;
            }
        }
        // dst[i] = constant.Normal() * src[i].Normal() + constant[3] * src[i][3];
        public static void Dotpp(float* dst, Plane constant, Plane* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = constant.Normal * src[_IX].Normal + constant.d * src[_IX].d;
            }
        }
        // dst[i] = constant.Normal() * src[i].xyz + constant[3];
        public static void Dotpd(float* dst, Plane constant, DrawVert* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = constant.Normal * src[_IX].xyz + constant.d;
            }
        }
        // dst[i] = src0[i] * src1[i];
        public static void Dotvv(float* dst, Vector3* src0, Vector3* src1, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = src0[_IX] * src1[_IX];
            }
        }
        // dot = src1[0] * src2[0] + src1[1] * src2[1] + src1[2] * src2[2] + ...
        public static void Dotff(out float dot, float* src1, float* src2, int count)
        {
            switch (count)
            {
                case 0: dot = 0f; return;
                case 1: dot = src1[0] * src2[0]; return;
                case 2: dot = src1[0] * src2[0] + src1[1] * src2[1]; return;
                case 3: dot = src1[0] * src2[0] + src1[1] * src2[1] + src1[2] * src2[2]; return;
                default:
                    {
                        int i; double s0, s1, s2, s3;
                        s0 = src1[0] * src2[0];
                        s1 = src1[1] * src2[1];
                        s2 = src1[2] * src2[2];
                        s3 = src1[3] * src2[3];
                        for (i = 4; i < count - 7; i += 8)
                        {
                            s0 += src1[i + 0] * src2[i + 0];
                            s1 += src1[i + 1] * src2[i + 1];
                            s2 += src1[i + 2] * src2[i + 2];
                            s3 += src1[i + 3] * src2[i + 3];
                            s0 += src1[i + 4] * src2[i + 4];
                            s1 += src1[i + 5] * src2[i + 5];
                            s2 += src1[i + 6] * src2[i + 6];
                            s3 += src1[i + 7] * src2[i + 7];
                        }
                        switch (count - i)
                        {
                            default: Debug.Assert(false); goto case 7;
                            case 7: s0 += src1[i + 6] * src2[i + 6]; goto case 6;
                            case 6: s1 += src1[i + 5] * src2[i + 5]; goto case 5;
                            case 5: s2 += src1[i + 4] * src2[i + 4]; goto case 4;
                            case 4: s3 += src1[i + 3] * src2[i + 3]; goto case 3;
                            case 3: s0 += src1[i + 2] * src2[i + 2]; goto case 2;
                            case 2: s1 += src1[i + 1] * src2[i + 1]; goto case 1;
                            case 1: s2 += src1[i + 0] * src2[i + 0]; goto case 0;
                            case 0: break;
                        }
                        double sum;
                        sum = s3;
                        sum += s2;
                        sum += s1;
                        sum += s0;
                        dot = (float)sum;
                        return;
                    }
            }
        }

        // dst[i] = src0[i] > constant;
        public static void CmpGT(byte* dst, float* src0, float constant, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src0[_IX + 0] > constant ? True : False;
                dst[_IX + 1] = src0[_IX + 1] > constant ? True : False;
                dst[_IX + 2] = src0[_IX + 2] > constant ? True : False;
                dst[_IX + 3] = src0[_IX + 3] > constant ? True : False;
            }
            for (; _IX < count; _IX++) { dst[_IX] = src0[_IX] > constant ? True : False; }
        }
        // dst[i] |= (src0[i] > constant) << bitNum;
        public static void CmpGTb(byte* dst, byte bitNum, float* src0, float constant, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] |= (byte)((src0[_IX + 0] > constant ? True : False) << bitNum);
                dst[_IX + 1] |= (byte)((src0[_IX + 1] > constant ? True : False) << bitNum);
                dst[_IX + 2] |= (byte)((src0[_IX + 2] > constant ? True : False) << bitNum);
                dst[_IX + 3] |= (byte)((src0[_IX + 3] > constant ? True : False) << bitNum);
            }
            for (; _IX < count; _IX++) { dst[_IX] |= (byte)((src0[_IX] > constant ? 1 : 0) << bitNum); }
        }
        // dst[i] = src0[i] >= constant;
        public static void CmpGE(byte* dst, float* src0, float constant, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src0[_IX + 0] >= constant ? True : False;
                dst[_IX + 1] = src0[_IX + 1] >= constant ? True : False;
                dst[_IX + 2] = src0[_IX + 2] >= constant ? True : False;
                dst[_IX + 3] = src0[_IX + 3] >= constant ? True : False;
            }
            for (; _IX < count; _IX++) { dst[_IX] = src0[_IX] >= constant ? True : False; }
        }
        // dst[i] |= (src0[i] >= constant) << bitNum;
        public static void CmpGEb(byte* dst, byte bitNum, float* src0, float constant, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] |= (byte)((src0[_IX + 0] >= constant ? True : False) << bitNum);
                dst[_IX + 1] |= (byte)((src0[_IX + 1] >= constant ? True : False) << bitNum);
                dst[_IX + 2] |= (byte)((src0[_IX + 2] >= constant ? True : False) << bitNum);
                dst[_IX + 3] |= (byte)((src0[_IX + 3] >= constant ? True : False) << bitNum);
            }
            for (; _IX < count; _IX++) { dst[_IX] |= (byte)((src0[_IX] >= constant ? True : False) << bitNum); }
        }
        // dst[i] = src0[i] < constant;
        public static void CmpLT(byte* dst, float* src0, float constant, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src0[_IX + 0] < constant ? True : False;
                dst[_IX + 1] = src0[_IX + 1] < constant ? True : False;
                dst[_IX + 2] = src0[_IX + 2] < constant ? True : False;
                dst[_IX + 3] = src0[_IX + 3] < constant ? True : False;
            }
            for (; _IX < count; _IX++) { dst[_IX] = src0[_IX] < constant ? True : False; }
        }
        // dst[i] |= (src0[i] < constant) << bitNum;
        public static void CmpLTb(byte* dst, byte bitNum, float* src0, float constant, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] |= (byte)((src0[_IX + 0] < constant ? True : False) << bitNum);
                dst[_IX + 1] |= (byte)((src0[_IX + 1] < constant ? True : False) << bitNum);
                dst[_IX + 2] |= (byte)((src0[_IX + 2] < constant ? True : False) << bitNum);
                dst[_IX + 3] |= (byte)((src0[_IX + 3] < constant ? True : False) << bitNum);
            }
            for (; _IX < count; _IX++) { dst[_IX] |= (byte)((src0[_IX] < constant ? True : False) << bitNum); }
        }
        // dst[i] = src0[i] <= constant;
        public static void CmpLE(byte* dst, float* src0, float constant, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] = src0[_IX + 0] <= constant ? True : False;
                dst[_IX + 1] = src0[_IX + 1] <= constant ? True : False;
                dst[_IX + 2] = src0[_IX + 2] <= constant ? True : False;
                dst[_IX + 3] = src0[_IX + 3] <= constant ? True : False;
            }
            for (; _IX < count; _IX++) { dst[_IX] = src0[_IX] <= constant ? True : False; }
        }
        // dst[i] |= (src0[i] <= constant) << bitNum;
        public static void CmpLEb(byte* dst, byte bitNum, float* src0, float constant, int count)
        {
            int _IX, _NM = (int)(count & 0xfffffffc); for (_IX = 0; _IX < _NM; _IX += 4)
            {
                dst[_IX + 0] |= (byte)((src0[_IX + 0] <= constant ? True : False) << bitNum);
                dst[_IX + 1] |= (byte)((src0[_IX + 1] <= constant ? True : False) << bitNum);
                dst[_IX + 2] |= (byte)((src0[_IX + 2] <= constant ? True : False) << bitNum);
                dst[_IX + 3] |= (byte)((src0[_IX + 3] <= constant ? True : False) << bitNum);
            }
            for (; _IX < count; _IX++) { dst[_IX] |= (byte)((src0[_IX] <= constant ? True : False) << bitNum); }
        }

        public static void MinMaxf(out float min, out float max, float* src, int count)
        {
            min = MathX.INFINITY; max = -MathX.INFINITY;
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                if (src[_IX] < min) { min = src[_IX]; }
                if (src[_IX] > max) { max = src[_IX]; }
            }
        }
        public static void MinMax2(out Vector2 min, out Vector2 max, Vector2* src, int count)
        {
            min.x = min.y = MathX.INFINITY; max.x = max.y = -MathX.INFINITY;
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                var v = src[_IX];
                if (v.x < min.x) { min.x = v.x; }
                if (v.x > max.x) { max.x = v.x; }
                if (v.y < min.y) { min.y = v.y; }
                if (v.y > max.y) { max.y = v.y; }
            }
        }
        public static void MinMax3(out Vector3 min, out Vector3 max, Vector3* src, int count)
        {
            min.x = min.y = min.z = MathX.INFINITY; max.x = max.y = max.z = -MathX.INFINITY;
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                var v = src[_IX];
                if (v.x < min.x) { min.x = v.x; }
                if (v.x > max.x) { max.x = v.x; }
                if (v.y < min.y) { min.y = v.y; }
                if (v.y > max.y) { max.y = v.y; }
                if (v.z < min.z) { min.z = v.z; }
                if (v.z > max.z) { max.z = v.z; }
            }
        }
        public static void MinMaxd(out Vector3 min, out Vector3 max, DrawVert* src, int count)
        {
            min.x = min.y = min.z = MathX.INFINITY; max.x = max.y = max.z = -MathX.INFINITY;
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                var v = src[_IX].xyz;
                if (v.x < min.x) { min.x = v.x; }
                if (v.x > max.x) { max.x = v.x; }
                if (v.y < min.y) { min.y = v.y; }
                if (v.y > max.y) { max.y = v.y; }
                if (v.z < min.z) { min.z = v.z; }
                if (v.z > max.z) { max.z = v.z; }
            }
        }
        public static void MinMaxdi(out Vector3 min, out Vector3 max, DrawVert* src, int* indexes, int count)
        {
            min.x = min.y = min.z = MathX.INFINITY; max.x = max.y = max.z = -MathX.INFINITY;
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                var v = src[indexes[_IX]].xyz;
                if (v.x < min.x) { min.x = v.x; }
                if (v.x > max.x) { max.x = v.x; }
                if (v.y < min.y) { min.y = v.y; }
                if (v.y > max.y) { max.y = v.y; }
                if (v.z < min.z) { min.z = v.z; }
                if (v.z > max.z) { max.z = v.z; }
            }
        }
        public static void MinMaxds(out Vector3 min, out Vector3 max, DrawVert* src, short* indexes, int count)
        {
            min.x = min.y = min.z = MathX.INFINITY; max.x = max.y = max.z = -MathX.INFINITY;
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                var v = src[indexes[_IX]].xyz;
                if (v.x < min.x) { min.x = v.x; }
                if (v.x > max.x) { max.x = v.x; }
                if (v.y < min.y) { min.y = v.y; }
                if (v.y > max.y) { max.y = v.y; }
                if (v.z < min.z) { min.z = v.z; }
                if (v.z > max.z) { max.z = v.z; }
            }
        }

        public static void Clamp(float* dst, float* src, float min, float max, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = src[_IX] < min ? min : src[_IX] > max ? max : src[_IX];
            }
        }
        public static void ClampMin(float* dst, float* src, float min, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = src[_IX] < min ? min : src[_IX];
            }
        }
        public static void ClampMax(float* dst, float* src, float max, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = src[_IX] > max ? max : src[_IX];
            }
        }

        public static void Memcpy(void* dst, void* src, int count)
            => Unsafe.CopyBlock((byte*)dst, (byte*)src, (uint)count);
        public static void Memset(void* dst, int val, int count)
            => Unsafe.InitBlock((byte*)dst, (byte)val, (uint)count);

        public static void Zero16(float* dst, int count)
            => Unsafe.InitBlock((byte*)dst, 0, (uint)(count * sizeof(float)));
        public static void Negate16(float* dst, int count)
        {
            var ptr = (uint*)dst;
            unchecked
            {
                int _IX; for (_IX = 0; _IX < count; _IX++)
                {
                    ptr[_IX] ^= (uint)1 << 31;        // IEEE 32 bits float sign bit
                }
            }
        }
        public static void Copy16(float* dst, float* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = src[_IX];
            }
        }
        public static void Add16(float* dst, float* src1, float* src2, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = src1[_IX] + src2[_IX];
            }
        }
        public static void Sub16(float* dst, float* src1, float* src2, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = src1[_IX] - src2[_IX];
            }
        }
        public static void Mul16(float* dst, float* src1, float constant, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] = src1[_IX] * constant;
            }
        }
        public static void AddAssign16(float* dst, float* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] += src[_IX];
            }
        }
        public static void SubAssign16(float* dst, float* src, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] -= src[_IX];
            }
        }
        public static void MulAssign16(float* dst, float constant, int count)
        {
            int _IX; for (_IX = 0; _IX < count; _IX++)
            {
                dst[_IX] *= constant;
            }
        }

        public static void MatX_MultiplyVecX(VectorX dst, MatrixX mat, VectorX vec)
        {
            int i, j;

            Debug.Assert(vec.Size >= mat.NumColumns);
            Debug.Assert(dst.Size >= mat.NumRows);

            fixed (float* matF = mat.mat, vPtr = vec.p, dstPtr = dst.p)
            {
                var mPtr = matF;
                var numRows = mat.NumRows;
                switch (mat.NumColumns)
                {
                    case 1:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] = mPtr[0] * vPtr[0];
                            mPtr++;
                        }
                        break;
                    case 2:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] = mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1];
                            mPtr += 2;
                        }
                        break;
                    case 3:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] = mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2];
                            mPtr += 3;
                        }
                        break;
                    case 4:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] = mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] + mPtr[3] * vPtr[3];
                            mPtr += 4;
                        }
                        break;
                    case 5:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] = mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] + mPtr[3] * vPtr[3] + mPtr[4] * vPtr[4];
                            mPtr += 5;
                        }
                        break;
                    case 6:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] = mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] + mPtr[3] * vPtr[3] + mPtr[4] * vPtr[4] + mPtr[5] * vPtr[5];
                            mPtr += 6;
                        }
                        break;
                    default:
                        var numColumns = mat.NumColumns;
                        for (i = 0; i < numRows; i++)
                        {
                            var sum = mPtr[0] * vPtr[0];
                            for (j = 1; j < numColumns; j++)
                                sum += mPtr[j] * vPtr[j];
                            dstPtr[i] = sum;
                            mPtr += numColumns;
                        }
                        break;
                }
            }
        }
        public static void MatX_MultiplyAddVecX(VectorX dst, MatrixX mat, VectorX vec)
        {
            int i, j;

            Debug.Assert(vec.Size >= mat.NumColumns);
            Debug.Assert(dst.Size >= mat.NumRows);

            fixed (float* matF = mat.mat, vPtr = vec.p, dstPtr = dst.p)
            {
                var mPtr = matF;
                var numRows = mat.NumRows;
                switch (mat.NumColumns)
                {
                    case 1:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] += mPtr[0] * vPtr[0];
                            mPtr++;
                        }
                        break;
                    case 2:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] += mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1];
                            mPtr += 2;
                        }
                        break;
                    case 3:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] += mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2];
                            mPtr += 3;
                        }
                        break;
                    case 4:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] += mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] + mPtr[3] * vPtr[3];
                            mPtr += 4;
                        }
                        break;
                    case 5:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] += mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] + mPtr[3] * vPtr[3] + mPtr[4] * vPtr[4];
                            mPtr += 5;
                        }
                        break;
                    case 6:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] += mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] + mPtr[3] * vPtr[3] + mPtr[4] * vPtr[4] + mPtr[5] * vPtr[5];
                            mPtr += 6;
                        }
                        break;
                    default:
                        var numColumns = mat.NumColumns;
                        for (i = 0; i < numRows; i++)
                        {
                            var sum = mPtr[0] * vPtr[0];
                            for (j = 1; j < numColumns; j++)
                                sum += mPtr[j] * vPtr[j];
                            dstPtr[i] += sum;
                            mPtr += numColumns;
                        }
                        break;
                }
            }
        }
        public static void MatX_MultiplySubVecX(VectorX dst, MatrixX mat, VectorX vec)
        {
            int i, j;

            Debug.Assert(vec.Size >= mat.NumColumns);
            Debug.Assert(dst.Size >= mat.NumRows);

            fixed (float* matF = mat.mat, vPtr = vec.p, dstPtr = dst.p)
            {
                var mPtr = matF;
                var numRows = mat.NumRows;
                switch (mat.NumColumns)
                {
                    case 1:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] -= mPtr[0] * vPtr[0];
                            mPtr++;
                        }
                        break;
                    case 2:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] -= mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1];
                            mPtr += 2;
                        }
                        break;
                    case 3:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] -= mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2];
                            mPtr += 3;
                        }
                        break;
                    case 4:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] -= mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] + mPtr[3] * vPtr[3];
                            mPtr += 4;
                        }
                        break;
                    case 5:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] -= mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] + mPtr[3] * vPtr[3] + mPtr[4] * vPtr[4];
                            mPtr += 5;
                        }
                        break;
                    case 6:
                        for (i = 0; i < numRows; i++)
                        {
                            dstPtr[i] -= mPtr[0] * vPtr[0] + mPtr[1] * vPtr[1] + mPtr[2] * vPtr[2] + mPtr[3] * vPtr[3] + mPtr[4] * vPtr[4] + mPtr[5] * vPtr[5];
                            mPtr += 6;
                        }
                        break;
                    default:
                        var numColumns = mat.NumColumns;
                        for (i = 0; i < numRows; i++)
                        {
                            var sum = mPtr[0] * vPtr[0];
                            for (j = 1; j < numColumns; j++)
                                sum += mPtr[j] * vPtr[j];
                            dstPtr[i] -= sum;
                            mPtr += numColumns;
                        }
                        break;
                }
            }
        }
        public static void MatX_TransposeMultiplyVecX(VectorX dst, MatrixX mat, VectorX vec)
        {
            int i, j;

            Debug.Assert(vec.Size >= mat.NumRows);
            Debug.Assert(dst.Size >= mat.NumColumns);

            fixed (float* matF = mat.mat, vPtr = vec.p, dstPtr = dst.p)
            {
                var mPtr = matF;
                var numColumns = mat.NumColumns;
                switch (mat.NumRows)
                {
                    case 1:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] = *(mPtr) * vPtr[0];
                            mPtr++;
                        }
                        break;
                    case 2:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] = *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1];
                            mPtr++;
                        }
                        break;
                    case 3:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] = *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2];
                            mPtr++;
                        }
                        break;
                    case 4:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] = *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] + *(mPtr + 3 * numColumns) * vPtr[3];
                            mPtr++;
                        }
                        break;
                    case 5:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] = *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] + *(mPtr + 3 * numColumns) * vPtr[3] + *(mPtr + 4 * numColumns) * vPtr[4];
                            mPtr++;
                        }
                        break;
                    case 6:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] = *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] + *(mPtr + 3 * numColumns) * vPtr[3] + *(mPtr + 4 * numColumns) * vPtr[4] + *(mPtr + 5 * numColumns) * vPtr[5];
                            mPtr++;
                        }
                        break;
                    default:
                        var numRows = mat.NumRows;
                        for (i = 0; i < numColumns; i++)
                        {
                            mPtr = matF + i;
                            var sum = mPtr[0] * vPtr[0];
                            for (j = 1; j < numRows; j++)
                            {
                                mPtr += numColumns;
                                sum += mPtr[0] * vPtr[j];
                            }
                            dstPtr[i] = sum;
                        }
                        break;
                }
            }
        }
        public static void MatX_TransposeMultiplyAddVecX(VectorX dst, MatrixX mat, VectorX vec)
        {
            int i, j;

            Debug.Assert(vec.Size >= mat.NumRows);
            Debug.Assert(dst.Size >= mat.NumColumns);

            fixed (float* matF = mat.mat, vPtr = vec.p, dstPtr = dst.p)
            {
                var mPtr = matF;
                var numColumns = mat.NumColumns;
                switch (mat.NumRows)
                {
                    case 1:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] += *(mPtr) * vPtr[0];
                            mPtr++;
                        }
                        break;
                    case 2:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] += *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1];
                            mPtr++;
                        }
                        break;
                    case 3:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] += *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2];
                            mPtr++;
                        }
                        break;
                    case 4:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] += *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] + *(mPtr + 3 * numColumns) * vPtr[3];
                            mPtr++;
                        }
                        break;
                    case 5:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] += *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] + *(mPtr + 3 * numColumns) * vPtr[3] + *(mPtr + 4 * numColumns) * vPtr[4];
                            mPtr++;
                        }
                        break;
                    case 6:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] += *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] + *(mPtr + 3 * numColumns) * vPtr[3] + *(mPtr + 4 * numColumns) * vPtr[4] + *(mPtr + 5 * numColumns) * vPtr[5];
                            mPtr++;
                        }
                        break;
                    default:
                        var numRows = mat.NumRows;
                        for (i = 0; i < numColumns; i++)
                        {
                            mPtr = matF + i;
                            var sum = mPtr[0] * vPtr[0];
                            for (j = 1; j < numRows; j++)
                            {
                                mPtr += numColumns;
                                sum += mPtr[0] * vPtr[j];
                            }
                            dstPtr[i] += sum;
                        }
                        break;
                }
            }
        }
        public static void MatX_TransposeMultiplySubVecX(VectorX dst, MatrixX mat, VectorX vec)
        {
            int i;

            Debug.Assert(vec.Size >= mat.NumRows);
            Debug.Assert(dst.Size >= mat.NumColumns);

            fixed (float* matF = mat.mat, vPtr = vec.p, dstPtr = dst.p)
            {
                var mPtr = matF;
                var numColumns = mat.NumColumns;
                switch (mat.NumRows)
                {
                    case 1:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] -= *(mPtr) * vPtr[0];
                            mPtr++;
                        }
                        break;
                    case 2:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] -= *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1];
                            mPtr++;
                        }
                        break;
                    case 3:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] -= *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2];
                            mPtr++;
                        }
                        break;
                    case 4:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] -= *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] + *(mPtr + 3 * numColumns) * vPtr[3];
                            mPtr++;
                        }
                        break;
                    case 5:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] -= *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] + *(mPtr + 3 * numColumns) * vPtr[3] + *(mPtr + 4 * numColumns) * vPtr[4];
                            mPtr++;
                        }
                        break;
                    case 6:
                        for (i = 0; i < numColumns; i++)
                        {
                            dstPtr[i] -= *(mPtr) * vPtr[0] + *(mPtr + numColumns) * vPtr[1] + *(mPtr + 2 * numColumns) * vPtr[2] + *(mPtr + 3 * numColumns) * vPtr[3] + *(mPtr + 4 * numColumns) * vPtr[4] + *(mPtr + 5 * numColumns) * vPtr[5];
                            mPtr++;
                        }
                        break;
                    default:
                        var numRows = mat.NumRows;
                        for (i = 0; i < numColumns; i++)
                        {
                            mPtr = matF + i;
                            var sum = mPtr[0] * vPtr[0];
                            for (var j = 1; j < numRows; j++)
                            {
                                mPtr += numColumns;
                                sum += mPtr[0] * vPtr[j];
                            }
                            dstPtr[i] -= sum;
                        }
                        break;
                }
            }
        }
        // optimizes the following matrix multiplications:
        //
        // NxN * Nx6
        // 6xN * Nx6
        // Nx6 * 6xN
        // 6x6 * 6xN
        // 
        // with N in the range [1-6].
        public static void MatX_MultiplyMatX(MatrixX dst, MatrixX m1, MatrixX m2)
        {
            int i, j, n; double sum;

            Debug.Assert(m1.NumColumns == m2.NumRows);

            fixed (float* dstF = dst.mat, m1F = m1.mat, m2F = m2.mat)
            {
                var dstPtr = dstF;
                var m1Ptr = m1F;
                var m2Ptr = m2F;
                var k = m1.NumRows;
                var l = m2.NumColumns;

                switch (m1.NumColumns)
                {
                    case 1:
                        if (l == 6)
                        {
                            for (i = 0; i < k; i++)
                            {   // Nx1 * 1x6
                                *dstPtr++ = m1Ptr[i] * m2Ptr[0];
                                *dstPtr++ = m1Ptr[i] * m2Ptr[1];
                                *dstPtr++ = m1Ptr[i] * m2Ptr[2];
                                *dstPtr++ = m1Ptr[i] * m2Ptr[3];
                                *dstPtr++ = m1Ptr[i] * m2Ptr[4];
                                *dstPtr++ = m1Ptr[i] * m2Ptr[5];
                            }
                            return;
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2F;
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0];
                                m2Ptr++;
                            }
                            m1Ptr++;
                        }
                        break;
                    case 2:
                        if (l == 6)
                        {
                            for (i = 0; i < k; i++)
                            {   // Nx2 * 2x6
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[6];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[1] + m1Ptr[1] * m2Ptr[7];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[2] + m1Ptr[1] * m2Ptr[8];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[3] + m1Ptr[1] * m2Ptr[9];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[4] + m1Ptr[1] * m2Ptr[10];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[5] + m1Ptr[1] * m2Ptr[11];
                                m1Ptr += 2;
                            }
                            return;
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2F;
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[l];
                                m2Ptr++;
                            }
                            m1Ptr += 2;
                        }
                        break;
                    case 3:
                        if (l == 6)
                        {
                            for (i = 0; i < k; i++)
                            {   // Nx3 * 3x6
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[6] + m1Ptr[2] * m2Ptr[12];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[1] + m1Ptr[1] * m2Ptr[7] + m1Ptr[2] * m2Ptr[13];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[2] + m1Ptr[1] * m2Ptr[8] + m1Ptr[2] * m2Ptr[14];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[3] + m1Ptr[1] * m2Ptr[9] + m1Ptr[2] * m2Ptr[15];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[4] + m1Ptr[1] * m2Ptr[10] + m1Ptr[2] * m2Ptr[16];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[5] + m1Ptr[1] * m2Ptr[11] + m1Ptr[2] * m2Ptr[17];
                                m1Ptr += 3;
                            }
                            return;
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2F;
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[l] + m1Ptr[2] * m2Ptr[2 * l];
                                m2Ptr++;
                            }
                            m1Ptr += 3;
                        }
                        break;
                    case 4:
                        if (l == 6)
                        {
                            for (i = 0; i < k; i++)
                            {   // Nx4 * 4x6
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[6] + m1Ptr[2] * m2Ptr[12] + m1Ptr[3] * m2Ptr[18];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[1] + m1Ptr[1] * m2Ptr[7] + m1Ptr[2] * m2Ptr[13] + m1Ptr[3] * m2Ptr[19];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[2] + m1Ptr[1] * m2Ptr[8] + m1Ptr[2] * m2Ptr[14] + m1Ptr[3] * m2Ptr[20];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[3] + m1Ptr[1] * m2Ptr[9] + m1Ptr[2] * m2Ptr[15] + m1Ptr[3] * m2Ptr[21];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[4] + m1Ptr[1] * m2Ptr[10] + m1Ptr[2] * m2Ptr[16] + m1Ptr[3] * m2Ptr[22];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[5] + m1Ptr[1] * m2Ptr[11] + m1Ptr[2] * m2Ptr[17] + m1Ptr[3] * m2Ptr[23];
                                m1Ptr += 4;
                            }
                            return;
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2F;
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[l] + m1Ptr[2] * m2Ptr[2 * l] + m1Ptr[3] * m2Ptr[3 * l];
                                m2Ptr++;
                            }
                            m1Ptr += 4;
                        }
                        break;
                    case 5:
                        if (l == 6)
                        {
                            for (i = 0; i < k; i++)
                            {   // Nx5 * 5x6
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[6] + m1Ptr[2] * m2Ptr[12] + m1Ptr[3] * m2Ptr[18] + m1Ptr[4] * m2Ptr[24];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[1] + m1Ptr[1] * m2Ptr[7] + m1Ptr[2] * m2Ptr[13] + m1Ptr[3] * m2Ptr[19] + m1Ptr[4] * m2Ptr[25];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[2] + m1Ptr[1] * m2Ptr[8] + m1Ptr[2] * m2Ptr[14] + m1Ptr[3] * m2Ptr[20] + m1Ptr[4] * m2Ptr[26];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[3] + m1Ptr[1] * m2Ptr[9] + m1Ptr[2] * m2Ptr[15] + m1Ptr[3] * m2Ptr[21] + m1Ptr[4] * m2Ptr[27];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[4] + m1Ptr[1] * m2Ptr[10] + m1Ptr[2] * m2Ptr[16] + m1Ptr[3] * m2Ptr[22] + m1Ptr[4] * m2Ptr[28];
                                *dstPtr++ = m1Ptr[0] * m2Ptr[5] + m1Ptr[1] * m2Ptr[11] + m1Ptr[2] * m2Ptr[17] + m1Ptr[3] * m2Ptr[23] + m1Ptr[4] * m2Ptr[29];
                                m1Ptr += 5;
                            }
                            return;
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2F;
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[l] + m1Ptr[2] * m2Ptr[2 * l] + m1Ptr[3] * m2Ptr[3 * l] + m1Ptr[4] * m2Ptr[4 * l];
                                m2Ptr++;
                            }
                            m1Ptr += 5;
                        }
                        break;
                    case 6:
                        switch (k)
                        {
                            case 1:
                                if (l == 1)
                                {   // 1x6 * 6x1
                                    dstPtr[0] = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[1] + m1Ptr[2] * m2Ptr[2] + m1Ptr[3] * m2Ptr[3] + m1Ptr[4] * m2Ptr[4] + m1Ptr[5] * m2Ptr[5];
                                    return;
                                }
                                break;
                            case 2:
                                if (l == 2)
                                {   // 2x6 * 6x2
                                    for (i = 0; i < 2; i++)
                                    {
                                        for (j = 0; j < 2; j++)
                                        {
                                            *dstPtr = m1Ptr[0] * m2Ptr[0 * 2 + j]
                                                    + m1Ptr[1] * m2Ptr[1 * 2 + j]
                                                    + m1Ptr[2] * m2Ptr[2 * 2 + j]
                                                    + m1Ptr[3] * m2Ptr[3 * 2 + j]
                                                    + m1Ptr[4] * m2Ptr[4 * 2 + j]
                                                    + m1Ptr[5] * m2Ptr[5 * 2 + j];
                                            dstPtr++;
                                        }
                                        m1Ptr += 6;
                                    }
                                    return;
                                }
                                break;
                            case 3:
                                if (l == 3)
                                {   // 3x6 * 6x3
                                    for (i = 0; i < 3; i++)
                                    {
                                        for (j = 0; j < 3; j++)
                                        {
                                            *dstPtr = m1Ptr[0] * m2Ptr[0 * 3 + j]
                                                    + m1Ptr[1] * m2Ptr[1 * 3 + j]
                                                    + m1Ptr[2] * m2Ptr[2 * 3 + j]
                                                    + m1Ptr[3] * m2Ptr[3 * 3 + j]
                                                    + m1Ptr[4] * m2Ptr[4 * 3 + j]
                                                    + m1Ptr[5] * m2Ptr[5 * 3 + j];
                                            dstPtr++;
                                        }
                                        m1Ptr += 6;
                                    }
                                    return;
                                }
                                break;
                            case 4:
                                if (l == 4)
                                {   // 4x6 * 6x4
                                    for (i = 0; i < 4; i++)
                                    {
                                        for (j = 0; j < 4; j++)
                                        {
                                            *dstPtr = m1Ptr[0] * m2Ptr[0 * 4 + j]
                                                    + m1Ptr[1] * m2Ptr[1 * 4 + j]
                                                    + m1Ptr[2] * m2Ptr[2 * 4 + j]
                                                    + m1Ptr[3] * m2Ptr[3 * 4 + j]
                                                    + m1Ptr[4] * m2Ptr[4 * 4 + j]
                                                    + m1Ptr[5] * m2Ptr[5 * 4 + j];
                                            dstPtr++;
                                        }
                                        m1Ptr += 6;
                                    }
                                    return;
                                }
                                break;
                            case 5:
                                if (l == 5)
                                {   // 5x6 * 6x5
                                    for (i = 0; i < 5; i++)
                                    {
                                        for (j = 0; j < 5; j++)
                                        {
                                            *dstPtr = m1Ptr[0] * m2Ptr[0 * 5 + j]
                                                    + m1Ptr[1] * m2Ptr[1 * 5 + j]
                                                    + m1Ptr[2] * m2Ptr[2 * 5 + j]
                                                    + m1Ptr[3] * m2Ptr[3 * 5 + j]
                                                    + m1Ptr[4] * m2Ptr[4 * 5 + j]
                                                    + m1Ptr[5] * m2Ptr[5 * 5 + j];
                                            dstPtr++;
                                        }
                                        m1Ptr += 6;
                                    }
                                    return;
                                }
                                break;
                            case 6:
                                switch (l)
                                {
                                    case 1:
                                        {   // 6x6 * 6x1
                                            for (i = 0; i < 6; i++)
                                            {
                                                *dstPtr = m1Ptr[0] * m2Ptr[0 * 1]
                                                        + m1Ptr[1] * m2Ptr[1 * 1]
                                                        + m1Ptr[2] * m2Ptr[2 * 1]
                                                        + m1Ptr[3] * m2Ptr[3 * 1]
                                                        + m1Ptr[4] * m2Ptr[4 * 1]
                                                        + m1Ptr[5] * m2Ptr[5 * 1];
                                                dstPtr++;
                                                m1Ptr += 6;
                                            }
                                            return;
                                        }
                                    case 2:
                                        {       // 6x6 * 6x2
                                            for (i = 0; i < 6; i++)
                                            {
                                                for (j = 0; j < 2; j++)
                                                {
                                                    *dstPtr = m1Ptr[0] * m2Ptr[0 * 2 + j]
                                                            + m1Ptr[1] * m2Ptr[1 * 2 + j]
                                                            + m1Ptr[2] * m2Ptr[2 * 2 + j]
                                                            + m1Ptr[3] * m2Ptr[3 * 2 + j]
                                                            + m1Ptr[4] * m2Ptr[4 * 2 + j]
                                                            + m1Ptr[5] * m2Ptr[5 * 2 + j];
                                                    dstPtr++;
                                                }
                                                m1Ptr += 6;
                                            }
                                            return;
                                        }
                                    case 3:
                                        {   // 6x6 * 6x3
                                            for (i = 0; i < 6; i++)
                                            {
                                                for (j = 0; j < 3; j++)
                                                {
                                                    *dstPtr = m1Ptr[0] * m2Ptr[0 * 3 + j]
                                                            + m1Ptr[1] * m2Ptr[1 * 3 + j]
                                                            + m1Ptr[2] * m2Ptr[2 * 3 + j]
                                                            + m1Ptr[3] * m2Ptr[3 * 3 + j]
                                                            + m1Ptr[4] * m2Ptr[4 * 3 + j]
                                                            + m1Ptr[5] * m2Ptr[5 * 3 + j];
                                                    dstPtr++;
                                                }
                                                m1Ptr += 6;
                                            }
                                            return;
                                        }
                                    case 4:
                                        {   // 6x6 * 6x4
                                            for (i = 0; i < 6; i++)
                                            {
                                                for (j = 0; j < 4; j++)
                                                {
                                                    *dstPtr = m1Ptr[0] * m2Ptr[0 * 4 + j]
                                                            + m1Ptr[1] * m2Ptr[1 * 4 + j]
                                                            + m1Ptr[2] * m2Ptr[2 * 4 + j]
                                                            + m1Ptr[3] * m2Ptr[3 * 4 + j]
                                                            + m1Ptr[4] * m2Ptr[4 * 4 + j]
                                                            + m1Ptr[5] * m2Ptr[5 * 4 + j];
                                                    dstPtr++;
                                                }
                                                m1Ptr += 6;
                                            }
                                            return;
                                        }
                                    case 5:
                                        {   // 6x6 * 6x5
                                            for (i = 0; i < 6; i++)
                                            {
                                                for (j = 0; j < 5; j++)
                                                {
                                                    *dstPtr = m1Ptr[0] * m2Ptr[0 * 5 + j]
                                                            + m1Ptr[1] * m2Ptr[1 * 5 + j]
                                                            + m1Ptr[2] * m2Ptr[2 * 5 + j]
                                                            + m1Ptr[3] * m2Ptr[3 * 5 + j]
                                                            + m1Ptr[4] * m2Ptr[4 * 5 + j]
                                                            + m1Ptr[5] * m2Ptr[5 * 5 + j];
                                                    dstPtr++;
                                                }
                                                m1Ptr += 6;
                                            }
                                            return;
                                        }
                                    case 6:
                                        {   // 6x6 * 6x6
                                            for (i = 0; i < 6; i++)
                                            {
                                                for (j = 0; j < 6; j++)
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
                                            return;
                                        }
                                }
                                break;
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2F;
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[1] * m2Ptr[l] + m1Ptr[2] * m2Ptr[2 * l] + m1Ptr[3] * m2Ptr[3 * l] + m1Ptr[4] * m2Ptr[4 * l] + m1Ptr[5] * m2Ptr[5 * l];
                                m2Ptr++;
                            }
                            m1Ptr += 6;
                        }
                        break;
                    default:
                        for (i = 0; i < k; i++)
                        {
                            for (j = 0; j < l; j++)
                            {
                                m2Ptr = m2F + j;
                                sum = m1Ptr[0] * m2Ptr[0];
                                for (n = 1; n < m1.NumColumns; n++)
                                {
                                    m2Ptr += l;
                                    sum += m1Ptr[n] * m2Ptr[0];
                                }
                                *dstPtr++ = (float)sum;
                            }
                            m1Ptr += m1.NumColumns;
                        }
                        break;
                }
            }
        }
        // optimizes the following tranpose matrix multiplications:
        // 
        // Nx6 * NxN
        // 6xN * 6x6
        // 
        // with N in the range [1-6].
        public static void MatX_TransposeMultiplyMatX(MatrixX dst, MatrixX m1, MatrixX m2)
        {
            int i, j, n; double sum;

            Debug.Assert(m1.NumRows == m2.NumRows);

            fixed (float* dstF = dst.mat, m1F = m1.mat, m2F = m2.mat)
            {
                var dstPtr = dstF;
                var m1Ptr = m1F;
                var m2Ptr = m2F;
                var k = m1.NumColumns;
                var l = m2.NumColumns;

                switch (m1.NumRows)
                {
                    case 1:
                        if (k == 6 && l == 1)
                        {   // 1x6 * 1x1
                            for (i = 0; i < 6; i++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0];
                                m1Ptr++;
                            }
                            return;
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2F;
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0];
                                m2Ptr++;
                            }
                            m1Ptr++;
                        }
                        break;
                    case 2:
                        if (k == 6 && l == 2)
                        {   // 2x6 * 2x2
                            for (i = 0; i < 6; i++)
                            {
                                *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 2 + 0] + m1Ptr[1 * 6] * m2Ptr[1 * 2 + 0];
                                *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 2 + 1] + m1Ptr[1 * 6] * m2Ptr[1 * 2 + 1];
                                m1Ptr++;
                            }
                            return;
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2F;
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[k] * m2Ptr[l];
                                m2Ptr++;
                            }
                            m1Ptr++;
                        }
                        break;
                    case 3:
                        if (k == 6 && l == 3)
                        {   // 3x6 * 3x3
                            for (i = 0; i < 6; i++)
                            {
                                *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 3 + 0] + m1Ptr[1 * 6] * m2Ptr[1 * 3 + 0] + m1Ptr[2 * 6] * m2Ptr[2 * 3 + 0];
                                *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 3 + 1] + m1Ptr[1 * 6] * m2Ptr[1 * 3 + 1] + m1Ptr[2 * 6] * m2Ptr[2 * 3 + 1];
                                *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 3 + 2] + m1Ptr[1 * 6] * m2Ptr[1 * 3 + 2] + m1Ptr[2 * 6] * m2Ptr[2 * 3 + 2];
                                m1Ptr++;
                            }
                            return;
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2F;
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[k] * m2Ptr[l] + m1Ptr[2 * k] * m2Ptr[2 * l];
                                m2Ptr++;
                            }
                            m1Ptr++;
                        }
                        break;
                    case 4:
                        if (k == 6 && l == 4)
                        {   // 4x6 * 4x4
                            for (i = 0; i < 6; i++)
                            {
                                *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 4 + 0] + m1Ptr[1 * 6] * m2Ptr[1 * 4 + 0] + m1Ptr[2 * 6] * m2Ptr[2 * 4 + 0] + m1Ptr[3 * 6] * m2Ptr[3 * 4 + 0];
                                *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 4 + 1] + m1Ptr[1 * 6] * m2Ptr[1 * 4 + 1] + m1Ptr[2 * 6] * m2Ptr[2 * 4 + 1] + m1Ptr[3 * 6] * m2Ptr[3 * 4 + 1];
                                *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 4 + 2] + m1Ptr[1 * 6] * m2Ptr[1 * 4 + 2] + m1Ptr[2 * 6] * m2Ptr[2 * 4 + 2] + m1Ptr[3 * 6] * m2Ptr[3 * 4 + 2];
                                *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 4 + 3] + m1Ptr[1 * 6] * m2Ptr[1 * 4 + 3] + m1Ptr[2 * 6] * m2Ptr[2 * 4 + 3] + m1Ptr[3 * 6] * m2Ptr[3 * 4 + 3];
                                m1Ptr++;
                            }
                            return;
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2F;
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[k] * m2Ptr[l] + m1Ptr[2 * k] * m2Ptr[2 * l] + m1Ptr[3 * k] * m2Ptr[3 * l];
                                m2Ptr++;
                            }
                            m1Ptr++;
                        }
                        break;
                    case 5:
                        if (k == 6 && l == 5)
                        {   // 5x6 * 5x5
                            for (i = 0; i < 6; i++)
                            {
                                *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 5 + 0] + m1Ptr[1 * 6] * m2Ptr[1 * 5 + 0] + m1Ptr[2 * 6] * m2Ptr[2 * 5 + 0] + m1Ptr[3 * 6] * m2Ptr[3 * 5 + 0] + m1Ptr[4 * 6] * m2Ptr[4 * 5 + 0];
                                *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 5 + 1] + m1Ptr[1 * 6] * m2Ptr[1 * 5 + 1] + m1Ptr[2 * 6] * m2Ptr[2 * 5 + 1] + m1Ptr[3 * 6] * m2Ptr[3 * 5 + 1] + m1Ptr[4 * 6] * m2Ptr[4 * 5 + 1];
                                *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 5 + 2] + m1Ptr[1 * 6] * m2Ptr[1 * 5 + 2] + m1Ptr[2 * 6] * m2Ptr[2 * 5 + 2] + m1Ptr[3 * 6] * m2Ptr[3 * 5 + 2] + m1Ptr[4 * 6] * m2Ptr[4 * 5 + 2];
                                *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 5 + 3] + m1Ptr[1 * 6] * m2Ptr[1 * 5 + 3] + m1Ptr[2 * 6] * m2Ptr[2 * 5 + 3] + m1Ptr[3 * 6] * m2Ptr[3 * 5 + 3] + m1Ptr[4 * 6] * m2Ptr[4 * 5 + 3];
                                *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 5 + 4] + m1Ptr[1 * 6] * m2Ptr[1 * 5 + 4] + m1Ptr[2 * 6] * m2Ptr[2 * 5 + 4] + m1Ptr[3 * 6] * m2Ptr[3 * 5 + 4] + m1Ptr[4 * 6] * m2Ptr[4 * 5 + 4];
                                m1Ptr++;
                            }
                            return;
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2F;
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[k] * m2Ptr[l] + m1Ptr[2 * k] * m2Ptr[2 * l] + m1Ptr[3 * k] * m2Ptr[3 * l] + m1Ptr[4 * k] * m2Ptr[4 * l];
                                m2Ptr++;
                            }
                            m1Ptr++;
                        }
                        break;
                    case 6:
                        if (l == 6)
                        {
                            switch (k)
                            {
                                case 1: // 6x1 * 6x6
                                    m2Ptr = m2F;
                                    for (j = 0; j < 6; j++)
                                    {
                                        *dstPtr++ = m1Ptr[0 * 1] * m2Ptr[0 * 6] +
                                                    m1Ptr[1 * 1] * m2Ptr[1 * 6] +
                                                    m1Ptr[2 * 1] * m2Ptr[2 * 6] +
                                                    m1Ptr[3 * 1] * m2Ptr[3 * 6] +
                                                    m1Ptr[4 * 1] * m2Ptr[4 * 6] +
                                                    m1Ptr[5 * 1] * m2Ptr[5 * 6];
                                        m2Ptr++;
                                    }
                                    return;
                                case 2: // 6x2 * 6x6
                                    for (i = 0; i < 2; i++)
                                    {
                                        m2Ptr = m2F;
                                        for (j = 0; j < 6; j++)
                                        {
                                            *dstPtr++ = m1Ptr[0 * 2] * m2Ptr[0 * 6] +
                                                        m1Ptr[1 * 2] * m2Ptr[1 * 6] +
                                                        m1Ptr[2 * 2] * m2Ptr[2 * 6] +
                                                        m1Ptr[3 * 2] * m2Ptr[3 * 6] +
                                                        m1Ptr[4 * 2] * m2Ptr[4 * 6] +
                                                        m1Ptr[5 * 2] * m2Ptr[5 * 6];
                                            m2Ptr++;
                                        }
                                        m1Ptr++;
                                    }
                                    return;
                                case 3: // 6x3 * 6x6
                                    for (i = 0; i < 3; i++)
                                    {
                                        m2Ptr = m2F;
                                        for (j = 0; j < 6; j++)
                                        {
                                            *dstPtr++ = m1Ptr[0 * 3] * m2Ptr[0 * 6] +
                                                        m1Ptr[1 * 3] * m2Ptr[1 * 6] +
                                                        m1Ptr[2 * 3] * m2Ptr[2 * 6] +
                                                        m1Ptr[3 * 3] * m2Ptr[3 * 6] +
                                                        m1Ptr[4 * 3] * m2Ptr[4 * 6] +
                                                        m1Ptr[5 * 3] * m2Ptr[5 * 6];
                                            m2Ptr++;
                                        }
                                        m1Ptr++;
                                    }
                                    return;
                                case 4: // 6x4 * 6x6
                                    for (i = 0; i < 4; i++)
                                    {
                                        m2Ptr = m2F;
                                        for (j = 0; j < 6; j++)
                                        {
                                            *dstPtr++ = m1Ptr[0 * 4] * m2Ptr[0 * 6] +
                                                        m1Ptr[1 * 4] * m2Ptr[1 * 6] +
                                                        m1Ptr[2 * 4] * m2Ptr[2 * 6] +
                                                        m1Ptr[3 * 4] * m2Ptr[3 * 6] +
                                                        m1Ptr[4 * 4] * m2Ptr[4 * 6] +
                                                        m1Ptr[5 * 4] * m2Ptr[5 * 6];
                                            m2Ptr++;
                                        }
                                        m1Ptr++;
                                    }
                                    return;
                                case 5: // 6x5 * 6x6
                                    for (i = 0; i < 5; i++)
                                    {
                                        m2Ptr = m2F;
                                        for (j = 0; j < 6; j++)
                                        {
                                            *dstPtr++ = m1Ptr[0 * 5] * m2Ptr[0 * 6] +
                                                        m1Ptr[1 * 5] * m2Ptr[1 * 6] +
                                                        m1Ptr[2 * 5] * m2Ptr[2 * 6] +
                                                        m1Ptr[3 * 5] * m2Ptr[3 * 6] +
                                                        m1Ptr[4 * 5] * m2Ptr[4 * 6] +
                                                        m1Ptr[5 * 5] * m2Ptr[5 * 6];
                                            m2Ptr++;
                                        }
                                        m1Ptr++;
                                    }
                                    return;
                                case 6: // 6x6 * 6x6
                                    for (i = 0; i < 6; i++)
                                    {
                                        m2Ptr = m2F;
                                        for (j = 0; j < 6; j++)
                                        {
                                            *dstPtr++ = m1Ptr[0 * 6] * m2Ptr[0 * 6] +
                                                        m1Ptr[1 * 6] * m2Ptr[1 * 6] +
                                                        m1Ptr[2 * 6] * m2Ptr[2 * 6] +
                                                        m1Ptr[3 * 6] * m2Ptr[3 * 6] +
                                                        m1Ptr[4 * 6] * m2Ptr[4 * 6] +
                                                        m1Ptr[5 * 6] * m2Ptr[5 * 6];
                                            m2Ptr++;
                                        }
                                        m1Ptr++;
                                    }
                                    return;
                            }
                        }
                        for (i = 0; i < k; i++)
                        {
                            m2Ptr = m2F;
                            for (j = 0; j < l; j++)
                            {
                                *dstPtr++ = m1Ptr[0] * m2Ptr[0] + m1Ptr[k] * m2Ptr[l] + m1Ptr[2 * k] * m2Ptr[2 * l] + m1Ptr[3 * k] * m2Ptr[3 * l] + m1Ptr[4 * k] * m2Ptr[4 * l] + m1Ptr[5 * k] * m2Ptr[5 * l];
                                m2Ptr++;
                            }
                            m1Ptr++;
                        }
                        break;
                    default:
                        for (i = 0; i < k; i++)
                            for (j = 0; j < l; j++)
                            {
                                m1Ptr = m1F + i;
                                m2Ptr = m2F + j;
                                sum = m1Ptr[0] * m2Ptr[0];
                                for (n = 1; n < m1.NumRows; n++)
                                {
                                    m1Ptr += k;
                                    m2Ptr += l;
                                    sum += m1Ptr[0] * m2Ptr[0];
                                }
                                *dstPtr++ = (float)sum;
                            }
                        break;
                }
            }
        }
        // solves x in Lx = b for the n * n sub-matrix of L if skip > 0 the first skip elements of x are assumed to be valid already
        // L has to be a lower triangular matrix with(implicit) ones on the diagonal x == b is allowed
        public static void MatX_LowerTriangularSolve(MatrixX L, float* x, float* b, int n, int skip = 0)
        {
            if (skip >= n)
                return;
            fixed (float* LF = L.mat)
            {
                var lptr = LF;
                var nc = L.NumColumns;

                // unrolled cases for n < 8
                if (n < 8)
                {
                    switch (n << 3 | skip & 7)
                    {
                        case 1 << 3 | 0 & 7:
                            x[0] = b[0];
                            return;
                        case 2 << 3 | 0 & 7: x[0] = b[0]; goto case 2 << 3 | 1 & 7;
                        case 2 << 3 | 1 & 7:
                            x[1] = b[1] - lptr[1 * nc + 0] * x[0];
                            return;
                        case 3 << 3 | 0 & 7: x[0] = b[0]; goto case 3 << 3 | 1 & 7;
                        case 3 << 3 | 1 & 7: x[1] = b[1] - lptr[1 * nc + 0] * x[0]; goto case 3 << 3 | 2 & 7;
                        case 3 << 3 | 2 & 7:
                            x[2] = b[2] - lptr[2 * nc + 0] * x[0] - lptr[2 * nc + 1] * x[1];
                            return;
                        case 4 << 3 | 0 & 7: x[0] = b[0]; goto case 4 << 3 | 1 & 7;
                        case 4 << 3 | 1 & 7: x[1] = b[1] - lptr[1 * nc + 0] * x[0]; goto case 4 << 3 | 2 & 7;
                        case 4 << 3 | 2 & 7: x[2] = b[2] - lptr[2 * nc + 0] * x[0] - lptr[2 * nc + 1] * x[1]; goto case 4 << 3 | 3 & 7;
                        case 4 << 3 | 3 & 7:
                            x[3] = b[3] - lptr[3 * nc + 0] * x[0] - lptr[3 * nc + 1] * x[1] - lptr[3 * nc + 2] * x[2];
                            return;
                        case 5 << 3 | 0 & 7: x[0] = b[0]; goto case 5 << 3 | 1 & 7;
                        case 5 << 3 | 1 & 7: x[1] = b[1] - lptr[1 * nc + 0] * x[0]; goto case 5 << 3 | 2 & 7;
                        case 5 << 3 | 2 & 7: x[2] = b[2] - lptr[2 * nc + 0] * x[0] - lptr[2 * nc + 1] * x[1]; goto case 5 << 3 | 3 & 7;
                        case 5 << 3 | 3 & 7: x[3] = b[3] - lptr[3 * nc + 0] * x[0] - lptr[3 * nc + 1] * x[1] - lptr[3 * nc + 2] * x[2]; goto case 5 << 3 | 4 & 7;
                        case 5 << 3 | 4 & 7:
                            x[4] = b[4] - lptr[4 * nc + 0] * x[0] - lptr[4 * nc + 1] * x[1] - lptr[4 * nc + 2] * x[2] - lptr[4 * nc + 3] * x[3];
                            return;
                        case 6 << 3 | 0 & 7: x[0] = b[0]; goto case 6 << 3 | 1 & 7;
                        case 6 << 3 | 1 & 7: x[1] = b[1] - lptr[1 * nc + 0] * x[0]; goto case 6 << 3 | 2 & 7;
                        case 6 << 3 | 2 & 7: x[2] = b[2] - lptr[2 * nc + 0] * x[0] - lptr[2 * nc + 1] * x[1]; goto case 6 << 3 | 3 & 7;
                        case 6 << 3 | 3 & 7: x[3] = b[3] - lptr[3 * nc + 0] * x[0] - lptr[3 * nc + 1] * x[1] - lptr[3 * nc + 2] * x[2]; goto case 6 << 3 | 4 & 7;
                        case 6 << 3 | 4 & 7: x[4] = b[4] - lptr[4 * nc + 0] * x[0] - lptr[4 * nc + 1] * x[1] - lptr[4 * nc + 2] * x[2] - lptr[4 * nc + 3] * x[3]; goto case 6 << 3 | 5 & 7;
                        case 6 << 3 | 5 & 7:
                            x[5] = b[5] - lptr[5 * nc + 0] * x[0] - lptr[5 * nc + 1] * x[1] - lptr[5 * nc + 2] * x[2] - lptr[5 * nc + 3] * x[3] - lptr[5 * nc + 4] * x[4];
                            return;
                        case 7 << 3 | 0 & 7: x[0] = b[0]; goto case 7 << 3 | 1 & 7;
                        case 7 << 3 | 1 & 7: x[1] = b[1] - lptr[1 * nc + 0] * x[0]; goto case 7 << 3 | 2 & 7;
                        case 7 << 3 | 2 & 7: x[2] = b[2] - lptr[2 * nc + 0] * x[0] - lptr[2 * nc + 1] * x[1]; goto case 7 << 3 | 3 & 7;
                        case 7 << 3 | 3 & 7: x[3] = b[3] - lptr[3 * nc + 0] * x[0] - lptr[3 * nc + 1] * x[1] - lptr[3 * nc + 2] * x[2]; goto case 7 << 3 | 4 & 7;
                        case 7 << 3 | 4 & 7: x[4] = b[4] - lptr[4 * nc + 0] * x[0] - lptr[4 * nc + 1] * x[1] - lptr[4 * nc + 2] * x[2] - lptr[4 * nc + 3] * x[3]; goto case 7 << 3 | 5 & 7;
                        case 7 << 3 | 5 & 7: x[5] = b[5] - lptr[5 * nc + 0] * x[0] - lptr[5 * nc + 1] * x[1] - lptr[5 * nc + 2] * x[2] - lptr[5 * nc + 3] * x[3] - lptr[5 * nc + 4] * x[4]; goto case 7 << 3 | 6 & 7;
                        case 7 << 3 | 6 & 7:
                            x[6] = b[6] - lptr[6 * nc + 0] * x[0] - lptr[6 * nc + 1] * x[1] - lptr[6 * nc + 2] * x[2] - lptr[6 * nc + 3] * x[3] - lptr[6 * nc + 4] * x[4] - lptr[6 * nc + 5] * x[5];
                            return;
                    }
                    return;
                }

                // process first 4 rows
                switch (skip)
                {
                    case 0: x[0] = b[0]; goto case 1;
                    case 1: x[1] = b[1] - lptr[1 * nc + 0] * x[0]; goto case 2;
                    case 2: x[2] = b[2] - lptr[2 * nc + 0] * x[0] - lptr[2 * nc + 1] * x[1]; goto case 3;
                    case 3: x[3] = b[3] - lptr[3 * nc + 0] * x[0] - lptr[3 * nc + 1] * x[1] - lptr[3 * nc + 2] * x[2]; skip = 4; break;
                }

                lptr = LF + skip;

                int i, j; double s0, s1, s2, s3; //: register

                for (i = skip; i < n; i++)
                {
                    s0 = lptr[0] * x[0];
                    s1 = lptr[1] * x[1];
                    s2 = lptr[2] * x[2];
                    s3 = lptr[3] * x[3];
                    for (j = 4; j < i - 7; j += 8)
                    {
                        s0 += lptr[j + 0] * x[j + 0];
                        s1 += lptr[j + 1] * x[j + 1];
                        s2 += lptr[j + 2] * x[j + 2];
                        s3 += lptr[j + 3] * x[j + 3];
                        s0 += lptr[j + 4] * x[j + 4];
                        s1 += lptr[j + 5] * x[j + 5];
                        s2 += lptr[j + 6] * x[j + 6];
                        s3 += lptr[j + 7] * x[j + 7];
                    }
                    switch (i - j)
                    {
                        default: Debug.Assert(false); break;
                        case 7: s0 += lptr[j + 6] * x[j + 6]; goto case 6;
                        case 6: s1 += lptr[j + 5] * x[j + 5]; goto case 5;
                        case 5: s2 += lptr[j + 4] * x[j + 4]; goto case 4;
                        case 4: s3 += lptr[j + 3] * x[j + 3]; goto case 3;
                        case 3: s0 += lptr[j + 2] * x[j + 2]; goto case 2;
                        case 2: s1 += lptr[j + 1] * x[j + 1]; goto case 1;
                        case 1: s2 += lptr[j + 0] * x[j + 0]; goto case 0;
                        case 0: break;
                    }
                    var sum = s3;
                    sum += s2;
                    sum += s1;
                    sum += s0;
                    sum -= b[i];
                    x[i] = (float)-sum;
                    lptr += nc;
                }
            }
        }
        //   solves x in L'x = b for the n * n sub-matrix of L
        // L has to be a lower triangular matrix with(implicit) ones on the diagonal
        // x == b is allowed
        public static void MatX_LowerTriangularSolveTranspose(MatrixX L, float* x, float* b, int n)
        {
            fixed (float* LF = L.mat)
            {
                var lptr = LF;
                var nc = L.NumColumns;

                // unrolled cases for n < 8
                if (n < 8)
                    switch (n)
                    {
                        case 0:
                            return;
                        case 1:
                            x[0] = b[0];
                            return;
                        case 2:
                            x[1] = b[1];
                            x[0] = b[0] - lptr[1 * nc + 0] * x[1];
                            return;
                        case 3:
                            x[2] = b[2];
                            x[1] = b[1] - lptr[2 * nc + 1] * x[2];
                            x[0] = b[0] - lptr[2 * nc + 0] * x[2] - lptr[1 * nc + 0] * x[1];
                            return;
                        case 4:
                            x[3] = b[3];
                            x[2] = b[2] - lptr[3 * nc + 2] * x[3];
                            x[1] = b[1] - lptr[3 * nc + 1] * x[3] - lptr[2 * nc + 1] * x[2];
                            x[0] = b[0] - lptr[3 * nc + 0] * x[3] - lptr[2 * nc + 0] * x[2] - lptr[1 * nc + 0] * x[1];
                            return;
                        case 5:
                            x[4] = b[4];
                            x[3] = b[3] - lptr[4 * nc + 3] * x[4];
                            x[2] = b[2] - lptr[4 * nc + 2] * x[4] - lptr[3 * nc + 2] * x[3];
                            x[1] = b[1] - lptr[4 * nc + 1] * x[4] - lptr[3 * nc + 1] * x[3] - lptr[2 * nc + 1] * x[2];
                            x[0] = b[0] - lptr[4 * nc + 0] * x[4] - lptr[3 * nc + 0] * x[3] - lptr[2 * nc + 0] * x[2] - lptr[1 * nc + 0] * x[1];
                            return;
                        case 6:
                            x[5] = b[5];
                            x[4] = b[4] - lptr[5 * nc + 4] * x[5];
                            x[3] = b[3] - lptr[5 * nc + 3] * x[5] - lptr[4 * nc + 3] * x[4];
                            x[2] = b[2] - lptr[5 * nc + 2] * x[5] - lptr[4 * nc + 2] * x[4] - lptr[3 * nc + 2] * x[3];
                            x[1] = b[1] - lptr[5 * nc + 1] * x[5] - lptr[4 * nc + 1] * x[4] - lptr[3 * nc + 1] * x[3] - lptr[2 * nc + 1] * x[2];
                            x[0] = b[0] - lptr[5 * nc + 0] * x[5] - lptr[4 * nc + 0] * x[4] - lptr[3 * nc + 0] * x[3] - lptr[2 * nc + 0] * x[2] - lptr[1 * nc + 0] * x[1];
                            return;
                        case 7:
                            x[6] = b[6];
                            x[5] = b[5] - lptr[6 * nc + 5] * x[6];
                            x[4] = b[4] - lptr[6 * nc + 4] * x[6] - lptr[5 * nc + 4] * x[5];
                            x[3] = b[3] - lptr[6 * nc + 3] * x[6] - lptr[5 * nc + 3] * x[5] - lptr[4 * nc + 3] * x[4];
                            x[2] = b[2] - lptr[6 * nc + 2] * x[6] - lptr[5 * nc + 2] * x[5] - lptr[4 * nc + 2] * x[4] - lptr[3 * nc + 2] * x[3];
                            x[1] = b[1] - lptr[6 * nc + 1] * x[6] - lptr[5 * nc + 1] * x[5] - lptr[4 * nc + 1] * x[4] - lptr[3 * nc + 1] * x[3] - lptr[2 * nc + 1] * x[2];
                            x[0] = b[0] - lptr[6 * nc + 0] * x[6] - lptr[5 * nc + 0] * x[5] - lptr[4 * nc + 0] * x[4] - lptr[3 * nc + 0] * x[3] - lptr[2 * nc + 0] * x[2] - lptr[1 * nc + 0] * x[1];
                            return;
                        default: return;
                    }

                int i, j; float* xptr; double s0, s1, s2, s3; //: register

                lptr = LF + n * nc + n - 4;
                xptr = x + n;

                // process 4 rows at a time
                for (i = n; i >= 4; i -= 4)
                {
                    s0 = b[i - 4];
                    s1 = b[i - 3];
                    s2 = b[i - 2];
                    s3 = b[i - 1];
                    // process 4x4 blocks
                    for (j = 0; j < n - i; j += 4)
                    {
                        s0 -= lptr[(j + 0) * nc + 0] * xptr[j + 0];
                        s1 -= lptr[(j + 0) * nc + 1] * xptr[j + 0];
                        s2 -= lptr[(j + 0) * nc + 2] * xptr[j + 0];
                        s3 -= lptr[(j + 0) * nc + 3] * xptr[j + 0];
                        s0 -= lptr[(j + 1) * nc + 0] * xptr[j + 1];
                        s1 -= lptr[(j + 1) * nc + 1] * xptr[j + 1];
                        s2 -= lptr[(j + 1) * nc + 2] * xptr[j + 1];
                        s3 -= lptr[(j + 1) * nc + 3] * xptr[j + 1];
                        s0 -= lptr[(j + 2) * nc + 0] * xptr[j + 2];
                        s1 -= lptr[(j + 2) * nc + 1] * xptr[j + 2];
                        s2 -= lptr[(j + 2) * nc + 2] * xptr[j + 2];
                        s3 -= lptr[(j + 2) * nc + 3] * xptr[j + 2];
                        s0 -= lptr[(j + 3) * nc + 0] * xptr[j + 3];
                        s1 -= lptr[(j + 3) * nc + 1] * xptr[j + 3];
                        s2 -= lptr[(j + 3) * nc + 2] * xptr[j + 3];
                        s3 -= lptr[(j + 3) * nc + 3] * xptr[j + 3];
                    }
                    // process left over of the 4 rows
                    s0 -= lptr[0 - 1 * nc] * s3;
                    s1 -= lptr[1 - 1 * nc] * s3;
                    s2 -= lptr[2 - 1 * nc] * s3;
                    s0 -= lptr[0 - 2 * nc] * s2;
                    s1 -= lptr[1 - 2 * nc] * s2;
                    s0 -= lptr[0 - 3 * nc] * s1;
                    // store result
                    xptr[-4] = (float)s0;
                    xptr[-3] = (float)s1;
                    xptr[-2] = (float)s2;
                    xptr[-1] = (float)s3;
                    // update pointers for next four rows
                    lptr -= 4 + 4 * nc;
                    xptr -= 4;
                }
                // process left over rows
                for (i--; i >= 0; i--)
                {
                    s0 = b[i];
                    lptr = LF + i;
                    for (j = i + 1; j < n; j++) s0 -= lptr[j * nc] * x[j];
                    x[i] = (float)s0;
                }
            }
        }
        // in-place factorization LDL' of the n * n sub-matrix of mat the reciprocal of the diagonal elements are stored in invDiag
        public static bool MatX_LDLTFactor(MatrixX mat, VectorX invDiag, int n)
        {
            int i, j, k, nc; double s0, s1, s2, s3, sum, d;
            //float* v, *diag, *mptr;

            var v = stackalloc float[n];
            var diag = stackalloc float[n];

            fixed (float* matF = mat.mat)
            {
                nc = mat.NumColumns;

                if (n <= 0) return true;

                var mptr = matF;

                sum = mptr[0];

                if (sum == 0f) return false;

                diag[0] = (float)sum;
                invDiag[0] = (float)(d = 1f / sum);

                if (n <= 1) return true;

                mptr = matF + 0;
                for (j = 1; j < n; j++)
                    mptr[j * nc + 0] = (float)(mptr[j * nc + 0] * d);

                mptr = matF + 1;

                v[0] = diag[0] * mptr[0]; s0 = v[0] * mptr[0];
                sum = mptr[1] - s0;

                if (sum == 0f) return false;

                mat[1][1] = (float)sum;
                diag[1] = (float)sum;
                invDiag[1] = (float)(d = 1f / sum);

                if (n <= 2) return true;

                mptr = matF + 0;
                for (j = 2; j < n; j++)
                    mptr[j * nc + 1] = (float)((mptr[j * nc + 1] - v[0] * mptr[j * nc + 0]) * d);

                mptr = matF + 2;

                v[0] = diag[0] * mptr[0]; s0 = v[0] * mptr[0];
                v[1] = diag[1] * mptr[1]; s1 = v[1] * mptr[1];
                sum = mptr[2] - s0 - s1;

                if (sum == 0f) return false;

                mat[2][2] = (float)sum;
                diag[2] = (float)sum;
                invDiag[2] = (float)(d = 1f / sum);

                if (n <= 3) return true;

                mptr = matF + 0;
                for (j = 3; j < n; j++)
                    mptr[j * nc + 2] = (float)((mptr[j * nc + 2] - v[0] * mptr[j * nc + 0] - v[1] * mptr[j * nc + 1]) * d);

                mptr = matF + 3;

                v[0] = diag[0] * mptr[0]; s0 = v[0] * mptr[0];
                v[1] = diag[1] * mptr[1]; s1 = v[1] * mptr[1];
                v[2] = diag[2] * mptr[2]; s2 = v[2] * mptr[2];
                sum = mptr[3] - s0 - s1 - s2;

                if (sum == 0f) return false;

                mat[3][3] = (float)sum;
                diag[3] = (float)sum;
                invDiag[3] = (float)(d = 1f / sum);

                if (n <= 4) return true;

                mptr = matF + 0;
                for (j = 4; j < n; j++)
                    mptr[j * nc + 3] = (float)((mptr[j * nc + 3] - v[0] * mptr[j * nc + 0] - v[1] * mptr[j * nc + 1] - v[2] * mptr[j * nc + 2]) * d);

                for (i = 4; i < n; i++)
                {
                    mptr = matF + i;

                    v[0] = diag[0] * mptr[0]; s0 = v[0] * mptr[0];
                    v[1] = diag[1] * mptr[1]; s1 = v[1] * mptr[1];
                    v[2] = diag[2] * mptr[2]; s2 = v[2] * mptr[2];
                    v[3] = diag[3] * mptr[3]; s3 = v[3] * mptr[3];
                    for (k = 4; k < i - 3; k += 4)
                    {
                        v[k + 0] = diag[k + 0] * mptr[k + 0]; s0 += v[k + 0] * mptr[k + 0];
                        v[k + 1] = diag[k + 1] * mptr[k + 1]; s1 += v[k + 1] * mptr[k + 1];
                        v[k + 2] = diag[k + 2] * mptr[k + 2]; s2 += v[k + 2] * mptr[k + 2];
                        v[k + 3] = diag[k + 3] * mptr[k + 3]; s3 += v[k + 3] * mptr[k + 3];
                    }
                    switch (i - k)
                    {
                        default: Debug.Assert(false); break;
                        case 3: v[k + 2] = diag[k + 2] * mptr[k + 2]; s0 += v[k + 2] * mptr[k + 2]; goto case 2;
                        case 2: v[k + 1] = diag[k + 1] * mptr[k + 1]; s1 += v[k + 1] * mptr[k + 1]; goto case 1;
                        case 1: v[k + 0] = diag[k + 0] * mptr[k + 0]; s2 += v[k + 0] * mptr[k + 0]; goto case 0;
                        case 0: break;
                    }
                    sum = s3;
                    sum += s2;
                    sum += s1;
                    sum += s0;
                    sum = mptr[i] - sum;

                    if (sum == 0f) return false;

                    mat[i][i] = (float)sum;
                    diag[i] = (float)sum;
                    invDiag[i] = (float)(d = 1f / sum);

                    if (i + 1 >= n) return true;

                    mptr = matF + i + 1;
                    for (j = i + 1; j < n; j++)
                    {
                        s0 = mptr[0] * v[0];
                        s1 = mptr[1] * v[1];
                        s2 = mptr[2] * v[2];
                        s3 = mptr[3] * v[3];
                        for (k = 4; k < i - 7; k += 8)
                        {
                            s0 += mptr[k + 0] * v[k + 0];
                            s1 += mptr[k + 1] * v[k + 1];
                            s2 += mptr[k + 2] * v[k + 2];
                            s3 += mptr[k + 3] * v[k + 3];
                            s0 += mptr[k + 4] * v[k + 4];
                            s1 += mptr[k + 5] * v[k + 5];
                            s2 += mptr[k + 6] * v[k + 6];
                            s3 += mptr[k + 7] * v[k + 7];
                        }
                        switch (i - k)
                        {
                            default: Debug.Assert(false); break;
                            case 7: s0 += mptr[k + 6] * v[k + 6]; goto case 6;
                            case 6: s1 += mptr[k + 5] * v[k + 5]; goto case 5;
                            case 5: s2 += mptr[k + 4] * v[k + 4]; goto case 4;
                            case 4: s3 += mptr[k + 3] * v[k + 3]; goto case 3;
                            case 3: s0 += mptr[k + 2] * v[k + 2]; goto case 2;
                            case 2: s1 += mptr[k + 1] * v[k + 1]; goto case 1;
                            case 1: s2 += mptr[k + 0] * v[k + 0]; goto case 0;
                            case 0: break;
                        }
                        sum = s3;
                        sum += s2;
                        sum += s1;
                        sum += s0;
                        mptr[i] = (float)((mptr[i] - sum) * d);
                        mptr += nc;
                    }
                }

                return true;
            }
        }

        public static void BlendJoints(JointQuat* joints, JointQuat* blendJoints, float lerp, int* index, int numJoints)
        {
            for (var i = 0; i < numJoints; i++)
            {
                var j = index[i];
                joints[j].q.Slerp(joints[j].q, blendJoints[j].q, lerp);
                joints[j].t.Lerp(joints[j].t, blendJoints[j].t, lerp);
            }
        }
        public static void ConvertJointQuatsToJointMats(JointMat* jointMats, JointQuat* jointQuats, int numJoints)
        {
            for (var i = 0; i < numJoints; i++)
            {
                jointMats[i].SetRotation(jointQuats[i].q.ToMat3());
                jointMats[i].SetTranslation(jointQuats[i].t);
            }
        }
        public static void ConvertJointMatsToJointQuats(JointQuat* jointQuats, JointMat* jointMats, int numJoints)
        {
            for (var i = 0; i < numJoints; i++)
            {
                jointQuats[i] = jointMats[i].ToJointQuat();
            }
        }
        public static void TransformJoints(JointMat* jointMats, int* parents, int firstJoint, int lastJoint)
        {
            for (var i = firstJoint; i <= lastJoint; i++)
            {
                Debug.Assert(parents[i] < i);
                jointMats[i] *= jointMats[parents[i]];
            }
        }
        public static void UntransformJoints(JointMat* jointMats, int* parents, int firstJoint, int lastJoint)
        {
            for (var i = lastJoint; i >= firstJoint; i--)
            {
                Debug.Assert(parents[i] < i);
                jointMats[i] /= jointMats[parents[i]];
            }
        }
        public static void TransformVerts(DrawVert* verts, int numVerts, JointMat* joints, Vector4* weights, int* index, int numWeights)
        {
            for (int j = 0, i = 0; i < numVerts; i++)
            {
                var jointIndex = index[j * 2 + 0] / sizeof(JointMat);
                var v = joints[jointIndex] * weights[j];
                while (index[j * 2 + 1] == 0) { j++; v += joints[jointIndex] * weights[j]; }
                j++;
                verts[i].xyz = v;
            }
        }
        public static void TracePointCull(byte* cullBits, out byte totalOr, float radius, Plane* planes, DrawVert* verts, int numVerts)
        {
            var tOr = (byte)0;

            for (var i = 0; i < numVerts; i++)
            {
                int bits; float d0, d1, d2, d3, t;
                ref Vector3 v = ref verts[i].xyz;

                d0 = planes[0].Distance(v);
                d1 = planes[1].Distance(v);
                d2 = planes[2].Distance(v);
                d3 = planes[3].Distance(v);

                t = d0 + radius; bits = MathX.FLOATSIGNBITSET_(t) << 0;
                t = d1 + radius; bits |= MathX.FLOATSIGNBITSET_(t) << 1;
                t = d2 + radius; bits |= MathX.FLOATSIGNBITSET_(t) << 2;
                t = d3 + radius; bits |= MathX.FLOATSIGNBITSET_(t) << 3;

                t = d0 - radius; bits |= MathX.FLOATSIGNBITSET_(t) << 4;
                t = d1 - radius; bits |= MathX.FLOATSIGNBITSET_(t) << 5;
                t = d2 - radius; bits |= MathX.FLOATSIGNBITSET_(t) << 6;
                t = d3 - radius; bits |= MathX.FLOATSIGNBITSET_(t) << 7;

                bits ^= 0x0F;       // flip lower four bits

                tOr |= (byte)bits;
                cullBits[i] = (byte)bits;
            }

            totalOr = tOr;
        }
        public static void DecalPointCull(byte* cullBits, Plane* planes, DrawVert* verts, int numVerts)
        {
            for (var i = 0; i < numVerts; i++)
            {
                int bits; float d0, d1, d2, d3, d4, d5;
                ref Vector3 v = ref verts[i].xyz;

                d0 = planes[0].Distance(v);
                d1 = planes[1].Distance(v);
                d2 = planes[2].Distance(v);
                d3 = planes[3].Distance(v);
                d4 = planes[4].Distance(v);
                d5 = planes[5].Distance(v);

                bits = MathX.FLOATSIGNBITSET_(d0) << 0;
                bits |= MathX.FLOATSIGNBITSET_(d1) << 1;
                bits |= MathX.FLOATSIGNBITSET_(d2) << 2;
                bits |= MathX.FLOATSIGNBITSET_(d3) << 3;
                bits |= MathX.FLOATSIGNBITSET_(d4) << 4;
                bits |= MathX.FLOATSIGNBITSET_(d5) << 5;

                cullBits[i] = (byte)(bits ^ 0x3F);      // flip lower 6 bits
            }
        }
        public static void OverlayPointCull(byte* cullBits, Vector2* texCoords, Plane* planes, DrawVert* verts, int numVerts)
        {
            for (var i = 0; i < numVerts; i++)
            {
                int bits; float d0, d1;
                ref Vector3 v = ref verts[i].xyz;

                texCoords[i][0] = d0 = planes[0].Distance(v);
                texCoords[i][1] = d1 = planes[1].Distance(v);

                bits = MathX.FLOATSIGNBITSET_(d0) << 0;
                d0 = 1f - d0;
                bits |= MathX.FLOATSIGNBITSET_(d1) << 1;
                d1 = 1f - d1;
                bits |= MathX.FLOATSIGNBITSET_(d0) << 2;
                bits |= MathX.FLOATSIGNBITSET_(d1) << 3;

                cullBits[i] = (byte)bits;
            }
        }
        // Derives a plane equation for each triangle.
        public static void DeriveTriPlanesi(Plane* planes, DrawVert* verts, int numVerts, int* indexes, int numIndexes)
        {
            for (var i = 0; i < numIndexes; i += 3)
            {
                DrawVert a, b, c; float d00, d01, d02, d10, d11, d12, f; Vector3 n;

                a = verts[indexes[i + 0]];
                b = verts[indexes[i + 1]];
                c = verts[indexes[i + 2]];

                d00 = b.xyz.x - a.xyz.x;
                d01 = b.xyz.y - a.xyz.y;
                d02 = b.xyz.z - a.xyz.z;

                d10 = c.xyz.x - a.xyz.x;
                d11 = c.xyz.y - a.xyz.y;
                d12 = c.xyz.z - a.xyz.z;

                n.x = d11 * d02 - d12 * d01;
                n.y = d12 * d00 - d10 * d02;
                n.z = d10 * d01 - d11 * d00;

                f = MathX.RSqrt(n.x * n.x + n.y * n.y + n.z * n.z);

                n.x *= f;
                n.y *= f;
                n.z *= f;

                planes->SetNormal(n);
                planes->FitThroughPoint(a.xyz);
                planes++;
            }
        }
        // Derives a plane equation for each triangle.
        public static void DeriveTriPlaness(Plane* planes, DrawVert* verts, int numVerts, short* indexes, int numIndexes)
        {
            for (var i = 0; i < numIndexes; i += 3)
            {
                DrawVert a, b, c; float d00, d01, d02, d10, d11, d12, f; Vector3 n;

                a = verts[indexes[i + 0]];
                b = verts[indexes[i + 1]];
                c = verts[indexes[i + 2]];

                d00 = b.xyz.x - a.xyz.x;
                d01 = b.xyz.y - a.xyz.y;
                d02 = b.xyz.z - a.xyz.z;

                d10 = c.xyz.x - a.xyz.x;
                d11 = c.xyz.y - a.xyz.y;
                d12 = c.xyz.z - a.xyz.z;

                n.x = d11 * d02 - d12 * d01;
                n.y = d12 * d00 - d10 * d02;
                n.z = d10 * d01 - d11 * d00;

                f = MathX.RSqrt(n.x * n.x + n.y * n.y + n.z * n.z);

                n.x *= f;
                n.y *= f;
                n.z *= f;

                planes->SetNormal(n);
                planes->FitThroughPoint(a.xyz);
                planes++;
            }
        }
        // Derives the normal and orthogonal tangent vectors for the triangle vertices.
        // For each vertex the normal and tangent vectors are derived from all triangles using the vertex which results in smooth tangents across the mesh.
        // In the process the triangle planes are calculated as well.
        public static void DeriveTangentsi(Plane* planes, DrawVert* verts, int numVerts, int* indexes, int numIndexes)
        {
            var used = stackalloc bool[numVerts];
            Unsafe.InitBlock(used, 0, (uint)(numVerts * sizeof(bool)));

            var planesPtr = planes;
            for (var i = 0; i < numIndexes; i += 3)
            {
                DrawVert a, b, c; uint signBit;
                float d00, d01, d02, d03, d04, d10, d11, d12, d13, d14, f, area; Vector3 n, t0, t1;

                var v0 = indexes[i + 0];
                var v1 = indexes[i + 1];
                var v2 = indexes[i + 2];

                a = verts[v0];
                b = verts[v1];
                c = verts[v2];

                d00 = b.xyz.x - a.xyz.x;
                d01 = b.xyz.y - a.xyz.y;
                d02 = b.xyz.z - a.xyz.z;
                d03 = b.st.x - a.st.x;
                d04 = b.st.y - a.st.y;

                d10 = c.xyz.x - a.xyz.x;
                d11 = c.xyz.y - a.xyz.y;
                d12 = c.xyz.z - a.xyz.z;
                d13 = c.st.x - a.st.x;
                d14 = c.st.y - a.st.y;

                // normal
                n.x = d11 * d02 - d12 * d01;
                n.y = d12 * d00 - d10 * d02;
                n.z = d10 * d01 - d11 * d00;

                f = MathX.RSqrt(n.x * n.x + n.y * n.y + n.z * n.z);

                n.x *= f;
                n.y *= f;
                n.z *= f;

                planesPtr->SetNormal(n);
                planesPtr->FitThroughPoint(a.xyz);
                planesPtr++;

                // area sign bit
                area = d03 * d14 - d04 * d13;
                signBit = (uint)((*(uint*)&area) & (1 << 31));

                // first tangent
                t0.x = d00 * d14 - d04 * d10;
                t0.y = d01 * d14 - d04 * d11;
                t0.z = d02 * d14 - d04 * d12;

                f = MathX.RSqrt(t0.x * t0.x + t0.y * t0.y + t0.z * t0.z);
                *(uint*)&f ^= signBit;

                t0.x *= f;
                t0.y *= f;
                t0.z *= f;

                // second tangent
                t1.x = d03 * d10 - d00 * d13;
                t1.y = d03 * d11 - d01 * d13;
                t1.z = d03 * d12 - d02 * d13;

                f = MathX.RSqrt(t1.x * t1.x + t1.y * t1.y + t1.z * t1.z);
                *(uint*)&f ^= signBit;

                t1.x *= f;
                t1.y *= f;
                t1.z *= f;

                if (used[v0])
                {
                    a.normal += n;
                    a.tangents0 += t0;
                    a.tangents1 += t1;
                }
                else
                {
                    a.normal = n;
                    a.tangents0 = t0;
                    a.tangents1 = t1;
                    used[v0] = true;
                }

                if (used[v1])
                {
                    b.normal += n;
                    b.tangents0 += t0;
                    b.tangents1 += t1;
                }
                else
                {
                    b.normal = n;
                    b.tangents0 = t0;
                    b.tangents1 = t1;
                    used[v1] = true;
                }

                if (used[v2])
                {
                    c.normal += n;
                    c.tangents0 += t0;
                    c.tangents1 += t1;
                }
                else
                {
                    c.normal = n;
                    c.tangents0 = t0;
                    c.tangents1 = t1;
                    used[v2] = true;
                }
            }
        }
        // Derives the normal and orthogonal tangent vectors for the triangle vertices.
        // For each vertex the normal and tangent vectors are derived from all triangles using the vertex which results in smooth tangents across the mesh.
        // In the process the triangle planes are calculated as well.
        public static void DeriveTangentss(Plane* planes, DrawVert* verts, int numVerts, short* indexes, int numIndexes)
        {
            var used = stackalloc bool[numVerts];
            Unsafe.InitBlock(used, 0, (uint)(numVerts * sizeof(bool)));

            var planesPtr = planes;
            for (var i = 0; i < numIndexes; i += 3)
            {
                DrawVert a, b, c; uint signBit;
                float d00, d01, d02, d03, d04, d10, d11, d12, d13, d14, f, area; Vector3 n, t0, t1;

                var v0 = indexes[i + 0];
                var v1 = indexes[i + 1];
                var v2 = indexes[i + 2];

                a = verts[v0];
                b = verts[v1];
                c = verts[v2];

                d00 = b.xyz.x - a.xyz.x;
                d01 = b.xyz.y - a.xyz.y;
                d02 = b.xyz.z - a.xyz.z;
                d03 = b.st.x - a.st.x;
                d04 = b.st.y - a.st.y;

                d10 = c.xyz.x - a.xyz.x;
                d11 = c.xyz.y - a.xyz.y;
                d12 = c.xyz.z - a.xyz.z;
                d13 = c.st.x - a.st.x;
                d14 = c.st.y - a.st.y;

                // normal
                n.x = d11 * d02 - d12 * d01;
                n.y = d12 * d00 - d10 * d02;
                n.z = d10 * d01 - d11 * d00;

                f = MathX.RSqrt(n.x * n.x + n.y * n.y + n.z * n.z);

                n.x *= f;
                n.y *= f;
                n.z *= f;

                planesPtr->SetNormal(n);
                planesPtr->FitThroughPoint(a.xyz);
                planesPtr++;

                // area sign bit
                area = d03 * d14 - d04 * d13;
                signBit = (uint)((*(uint*)&area) & (1 << 31));

                // first tangent
                t0.x = d00 * d14 - d04 * d10;
                t0.y = d01 * d14 - d04 * d11;
                t0.z = d02 * d14 - d04 * d12;

                f = MathX.RSqrt(t0.x * t0.x + t0.y * t0.y + t0.z * t0.z);
                *(uint*)&f ^= signBit;

                t0.x *= f;
                t0.y *= f;
                t0.z *= f;

                // second tangent
                t1.x = d03 * d10 - d00 * d13;
                t1.y = d03 * d11 - d01 * d13;
                t1.z = d03 * d12 - d02 * d13;

                f = MathX.RSqrt(t1.x * t1.x + t1.y * t1.y + t1.z * t1.z);
                *(uint*)&f ^= signBit;

                t1.x *= f;
                t1.y *= f;
                t1.z *= f;

                if (used[v0])
                {
                    a.normal += n;
                    a.tangents0 += t0;
                    a.tangents1 += t1;
                }
                else
                {
                    a.normal = n;
                    a.tangents0 = t0;
                    a.tangents1 = t1;
                    used[v0] = true;
                }

                if (used[v1])
                {
                    b.normal += n;
                    b.tangents0 += t0;
                    b.tangents1 += t1;
                }
                else
                {
                    b.normal = n;
                    b.tangents0 = t0;
                    b.tangents1 = t1;
                    used[v1] = true;
                }

                if (used[v2])
                {
                    c.normal += n;
                    c.tangents0 += t0;
                    c.tangents1 += t1;
                }
                else
                {
                    c.normal = n;
                    c.tangents0 = t0;
                    c.tangents1 = t1;
                    used[v2] = true;
                }
            }
        }
        // Derives the normal and orthogonal tangent vectors for the triangle vertices.
        // For each vertex the normal and tangent vectors are derived from a single dominant triangle.
        public static void DeriveUnsmoothedTangents(DrawVert* verts, DominantTri* dominantTris, int numVerts)
        {
            for (var i = 0; i < numVerts; i++)
            {
                DrawVert a, b, c;
#if !DERIVE_UNSMOOTHED_BITANGENT
                float d3, d8;
#endif
                float d0, d1, d2, d4;
                float d5, d6, d7, d9;
                float s0, s1, s2;
                float n0, n1, n2;
                float t0, t1, t2;
                float t3, t4, t5;

                DominantTri dt = dominantTris[i];

                a = verts[i];
                b = verts[dt.v2];
                c = verts[dt.v3];

                d0 = b.xyz.x - a.xyz.x;
                d1 = b.xyz.y - a.xyz.y;
                d2 = b.xyz.z - a.xyz.z;
#if !DERIVE_UNSMOOTHED_BITANGENT
                d3 = b.st.x - a.st.x;
#endif
                d4 = b.st.y - a.st.y;

                d5 = c.xyz.x - a.xyz.x;
                d6 = c.xyz.y - a.xyz.y;
                d7 = c.xyz.z - a.xyz.z;
#if !DERIVE_UNSMOOTHED_BITANGENT
                d8 = c.st.x - a.st.x;
#endif
                d9 = c.st.x - a.st.y;

                s0 = dt.normalizationScale[0];
                s1 = dt.normalizationScale[1];
                s2 = dt.normalizationScale[2];

                n0 = s2 * (d6 * d2 - d7 * d1);
                n1 = s2 * (d7 * d0 - d5 * d2);
                n2 = s2 * (d5 * d1 - d6 * d0);

                t0 = s0 * (d0 * d9 - d4 * d5);
                t1 = s0 * (d1 * d9 - d4 * d6);
                t2 = s0 * (d2 * d9 - d4 * d7);

#if !DERIVE_UNSMOOTHED_BITANGENT
                t3 = s1 * (d3 * d5 - d0 * d8);
                t4 = s1 * (d3 * d6 - d1 * d8);
                t5 = s1 * (d3 * d7 - d2 * d8);
#else
                t3 = s1 * (n2 * t1 - n1 * t2);
                t4 = s1 * (n0 * t2 - n2 * t0);
                t5 = s1 * (n1 * t0 - n0 * t1);
#endif

                a.normal.x = n0;
                a.normal.y = n1;
                a.normal.z = n2;

                a.tangents0.x = t0;
                a.tangents0.y = t1;
                a.tangents0.z = t2;

                a.tangents1.x = t3;
                a.tangents1.y = t4;
                a.tangents1.z = t5;
            }
        }
        // Normalizes each vertex normal and projects and normalizes the tangent vectors onto the plane orthogonal to the vertex normal.
        public static void NormalizeTangents(DrawVert* verts, int numVerts)
        {
            for (var i = 0; i < numVerts; i++)
            {
                ref Vector3 v = ref verts[i].normal; float f;
                f = MathX.RSqrt(v.x * v.x + v.y * v.y + v.z * v.z);
                v.x *= f; v.y *= f; v.z *= f;

                //: unroll
                ref Vector3 t = ref verts[i].tangents0;
                t -= t * v * v;
                f = MathX.RSqrt(t.x * t.x + t.y * t.y + t.z * t.z);
                t.x *= f; t.y *= f; t.z *= f;
                t = ref verts[i].tangents1;
                t -= t * v * v;
                f = MathX.RSqrt(t.x * t.x + t.y * t.y + t.z * t.z);
                t.x *= f; t.y *= f; t.z *= f;
            }
        }
        // Calculates light vectors in texture space for the given triangle vertices.
        // For each vertex the direction towards the light origin is projected onto texture space.
        // The light vectors are only calculated for the vertices referenced by the indexes.
        public static void CreateTextureSpaceLightVectors(Vector3* lightVectors, Vector3 lightOrigin, DrawVert* verts, int numVerts, int* indexes, int numIndexes)
        {
            var used = stackalloc bool[numVerts];
            Unsafe.InitBlock(used, 0, (uint)(numVerts * sizeof(bool)));
            for (var i = numIndexes - 1; i >= 0; i--)
                used[indexes[i]] = true;

            for (var i = 0; i < numVerts; i++)
            {
                if (!used[i]) continue;

                ref DrawVert v = ref verts[i];
                var lightDir = lightOrigin - v.xyz;
                lightVectors[i].x = lightDir * v.tangents0;
                lightVectors[i].y = lightDir * v.tangents1;
                lightVectors[i].z = lightDir * v.normal;
            }
        }
        // Calculates specular texture coordinates for the given triangle vertices.
        // For each vertex the normalized direction towards the light origin is added to the
        // normalized direction towards the view origin and the result is projected onto texture space.
        // The texture coordinates are only calculated for the vertices referenced by the indexes.
        public static void CreateSpecularTextureCoords(Vector4* texCoords, Vector3 lightOrigin, Vector3 viewOrigin, DrawVert* verts, int numVerts, int* indexes, int numIndexes)
        {
            var used = stackalloc bool[numVerts];
            Unsafe.InitBlock(used, 0, (uint)(numVerts * sizeof(bool)));
            for (var i = numIndexes - 1; i >= 0; i--)
                used[indexes[i]] = true;

            for (var i = 0; i < numVerts; i++)
            {
                if (!used[i])
                    continue;

                ref DrawVert v = ref verts[i];
                var lightDir = lightOrigin - v.xyz;
                var viewDir = viewOrigin - v.xyz;

                float ilength;
                ilength = MathX.RSqrt(lightDir * lightDir);
                lightDir[0] *= ilength;
                lightDir[1] *= ilength;
                lightDir[2] *= ilength;

                ilength = MathX.RSqrt(viewDir * viewDir);
                viewDir[0] *= ilength;
                viewDir[1] *= ilength;
                viewDir[2] *= ilength;

                lightDir += viewDir;
                texCoords[i].x = lightDir * v.tangents0;
                texCoords[i].y = lightDir * v.tangents1;
                texCoords[i].z = lightDir * v.normal;
                texCoords[i].w = 1f;
            }
        }
        public static int CreateShadowCache(Vector4* vertexCache, int* vertRemap, Vector3 lightOrigin, DrawVert* verts, int numVerts)
        {
            var outVerts = 0;

            for (var i = 0; i < numVerts; i++)
            {
                if (vertRemap[i] != 0) continue;
                float* v = &verts[i].xyz.x;
                {
                    vertexCache[outVerts + 0][0] = v[0];
                    vertexCache[outVerts + 0][1] = v[1];
                    vertexCache[outVerts + 0][2] = v[2];
                    vertexCache[outVerts + 0][3] = 1f;

                    // R_SetupProjection() builds the projection matrix with a slight crunch for depth, which keeps this w=0 division from rasterizing right at the
                    // wrap around point and causing depth fighting with the rear caps
                    vertexCache[outVerts + 1][0] = v[0] - lightOrigin[0];
                    vertexCache[outVerts + 1][1] = v[1] - lightOrigin[1];
                    vertexCache[outVerts + 1][2] = v[2] - lightOrigin[2];
                    vertexCache[outVerts + 1][3] = 0f;
                    vertRemap[i] = outVerts;
                    outVerts += 2;
                }
            }
            return outVerts;
        }
        public static int CreateVertexProgramShadowCache(Vector4* vertexCache, DrawVert* verts, int numVerts)
        {
            for (var i = 0; i < numVerts; i++)
            {
                var v = &verts[i].xyz.x;
                vertexCache[i * 2 + 0][0] = v[0];
                vertexCache[i * 2 + 1][0] = v[0];
                vertexCache[i * 2 + 0][1] = v[1];
                vertexCache[i * 2 + 1][1] = v[1];
                vertexCache[i * 2 + 0][2] = v[2];
                vertexCache[i * 2 + 1][2] = v[2];
                vertexCache[i * 2 + 0][3] = 1f;
                vertexCache[i * 2 + 1][3] = 0f;
            }
            return numVerts * 2;
        }

        // Duplicate samples for 44kHz output.
        public static void UpSamplePCMTo44kHz(float* dest, short* src, int numSamples, int kHz, int numChannels)
        {
            if (kHz == 11025)
            {
                if (numChannels == 1)
                    for (var i = 0; i < numSamples; i++)
                    {
                        dest[i * 4 + 0] = dest[i * 4 + 1] = dest[i * 4 + 2] = dest[i * 4 + 3] = src[i + 0];
                    }
                else
                    for (var i = 0; i < numSamples; i += 2)
                    {
                        dest[i * 4 + 0] = dest[i * 4 + 2] = dest[i * 4 + 4] = dest[i * 4 + 6] = src[i + 0];
                        dest[i * 4 + 1] = dest[i * 4 + 3] = dest[i * 4 + 5] = dest[i * 4 + 7] = src[i + 1];
                    }
            }
            else if (kHz == 22050)
            {
                if (numChannels == 1)
                    for (var i = 0; i < numSamples; i++)
                    {
                        dest[i * 2 + 0] = dest[i * 2 + 1] = src[i + 0];
                    }
                else
                    for (var i = 0; i < numSamples; i += 2)
                    {
                        dest[i * 2 + 0] = dest[i * 2 + 2] = src[i + 0];
                        dest[i * 2 + 1] = dest[i * 2 + 3] = src[i + 1];
                    }
            }
            else if (kHz == 44100)
            {
                for (var i = 0; i < numSamples; i++)
                    dest[i] = src[i];
            }
            else Debug.Assert(false);
        }
        // Duplicate samples for 44kHz output.
        public static void UpSampleOGGTo44kHz(float* dest, float** ogg, int numSamples, int kHz, int numChannels)
        {
            if (kHz == 11025)
            {
                if (numChannels == 1)
                    for (var i = 0; i < numSamples; i++)
                    {
                        dest[i * 4 + 0] = dest[i * 4 + 1] = dest[i * 4 + 2] = dest[i * 4 + 3] = ogg[0][i] * 32768f;
                    }
                else
                    for (var i = 0; i < numSamples >> 1; i++)
                    {
                        dest[i * 8 + 0] = dest[i * 8 + 2] = dest[i * 8 + 4] = dest[i * 8 + 6] = ogg[0][i] * 32768f;
                        dest[i * 8 + 1] = dest[i * 8 + 3] = dest[i * 8 + 5] = dest[i * 8 + 7] = ogg[1][i] * 32768f;
                    }
            }
            else if (kHz == 22050)
            {
                if (numChannels == 1)
                    for (var i = 0; i < numSamples; i++)
                    {
                        dest[i * 2 + 0] = dest[i * 2 + 1] = ogg[0][i] * 32768f;
                    }
                else
                    for (var i = 0; i < numSamples >> 1; i++)
                    {
                        dest[i * 4 + 0] = dest[i * 4 + 2] = ogg[0][i] * 32768f;
                        dest[i * 4 + 1] = dest[i * 4 + 3] = ogg[1][i] * 32768f;
                    }
            }
            else if (kHz == 44100)
            {
                if (numChannels == 1)
                    for (var i = 0; i < numSamples; i++)
                    {
                        dest[i * 1 + 0] = ogg[0][i] * 32768f;
                    }
                else
                    for (var i = 0; i < numSamples >> 1; i++)
                    {
                        dest[i * 2 + 0] = ogg[0][i] * 32768f;
                        dest[i * 2 + 1] = ogg[1][i] * 32768f;
                    }
            }
            else Debug.Assert(false);
        }
        public static void MixSoundTwoSpeakerMono(float* mixBuffer, float* samples, int numSamples, float[] lastV, float[] currentV)
        {
            var sL = lastV[0];
            var sR = lastV[1];
            var incL = (currentV[0] - lastV[0]) / Simd.MIXBUFFER_SAMPLES;
            var incR = (currentV[1] - lastV[1]) / Simd.MIXBUFFER_SAMPLES;

            Debug.Assert(numSamples == Simd.MIXBUFFER_SAMPLES);

            for (var j = 0; j < Simd.MIXBUFFER_SAMPLES; j++)
            {
                mixBuffer[j * 2 + 0] += samples[j] * sL;
                mixBuffer[j * 2 + 1] += samples[j] * sR;
                sL += incL;
                sR += incR;
            }
        }
        public static void MixSoundTwoSpeakerStereo(float* mixBuffer, float* samples, int numSamples, float[] lastV, float[] currentV)
        {
            var sL = lastV[0];
            var sR = lastV[1];
            var incL = (currentV[0] - lastV[0]) / Simd.MIXBUFFER_SAMPLES;
            var incR = (currentV[1] - lastV[1]) / Simd.MIXBUFFER_SAMPLES;

            Debug.Assert(numSamples == Simd.MIXBUFFER_SAMPLES);

            for (var j = 0; j < Simd.MIXBUFFER_SAMPLES; j++)
            {
                mixBuffer[j * 2 + 0] += samples[j * 2 + 0] * sL;
                mixBuffer[j * 2 + 1] += samples[j * 2 + 1] * sR;
                sL += incL;
                sR += incR;
            }
        }
        public static void MixSoundSixSpeakerMono(float* mixBuffer, float* samples, int numSamples, float[] lastV, float[] currentV)
        {
            var sL0 = lastV[0];
            var sL1 = lastV[1];
            var sL2 = lastV[2];
            var sL3 = lastV[3];
            var sL4 = lastV[4];
            var sL5 = lastV[5];

            var incL0 = (currentV[0] - lastV[0]) / Simd.MIXBUFFER_SAMPLES;
            var incL1 = (currentV[1] - lastV[1]) / Simd.MIXBUFFER_SAMPLES;
            var incL2 = (currentV[2] - lastV[2]) / Simd.MIXBUFFER_SAMPLES;
            var incL3 = (currentV[3] - lastV[3]) / Simd.MIXBUFFER_SAMPLES;
            var incL4 = (currentV[4] - lastV[4]) / Simd.MIXBUFFER_SAMPLES;
            var incL5 = (currentV[5] - lastV[5]) / Simd.MIXBUFFER_SAMPLES;

            Debug.Assert(numSamples == Simd.MIXBUFFER_SAMPLES);

            for (var i = 0; i < Simd.MIXBUFFER_SAMPLES; i++)
            {
                mixBuffer[i * 6 + 0] += samples[i] * sL0;
                mixBuffer[i * 6 + 1] += samples[i] * sL1;
                mixBuffer[i * 6 + 2] += samples[i] * sL2;
                mixBuffer[i * 6 + 3] += samples[i] * sL3;
                mixBuffer[i * 6 + 4] += samples[i] * sL4;
                mixBuffer[i * 6 + 5] += samples[i] * sL5;
                sL0 += incL0;
                sL1 += incL1;
                sL2 += incL2;
                sL3 += incL3;
                sL4 += incL4;
                sL5 += incL5;
            }
        }
        public static void MixSoundSixSpeakerStereo(float* mixBuffer, float* samples, int numSamples, float[] lastV, float[] currentV)
        {
            var sL0 = lastV[0];
            var sL1 = lastV[1];
            var sL2 = lastV[2];
            var sL3 = lastV[3];
            var sL4 = lastV[4];
            var sL5 = lastV[5];

            var incL0 = (currentV[0] - lastV[0]) / Simd.MIXBUFFER_SAMPLES;
            var incL1 = (currentV[1] - lastV[1]) / Simd.MIXBUFFER_SAMPLES;
            var incL2 = (currentV[2] - lastV[2]) / Simd.MIXBUFFER_SAMPLES;
            var incL3 = (currentV[3] - lastV[3]) / Simd.MIXBUFFER_SAMPLES;
            var incL4 = (currentV[4] - lastV[4]) / Simd.MIXBUFFER_SAMPLES;
            var incL5 = (currentV[5] - lastV[5]) / Simd.MIXBUFFER_SAMPLES;

            Debug.Assert(numSamples == Simd.MIXBUFFER_SAMPLES);

            for (var i = 0; i < Simd.MIXBUFFER_SAMPLES; i++)
            {
                mixBuffer[i * 6 + 0] += samples[i * 2 + 0] * sL0;
                mixBuffer[i * 6 + 1] += samples[i * 2 + 1] * sL1;
                mixBuffer[i * 6 + 2] += samples[i * 2 + 0] * sL2;
                mixBuffer[i * 6 + 3] += samples[i * 2 + 0] * sL3;
                mixBuffer[i * 6 + 4] += samples[i * 2 + 0] * sL4;
                mixBuffer[i * 6 + 5] += samples[i * 2 + 1] * sL5;
                sL0 += incL0;
                sL1 += incL1;
                sL2 += incL2;
                sL3 += incL3;
                sL4 += incL4;
                sL5 += incL5;
            }
        }
        public static void MixedSoundToSamples(short* samples, float* mixBuffer, int numSamples)
        {
            for (var i = 0; i < numSamples; i++)
                if (mixBuffer[i] <= -32768f) samples[i] = -32768;
                else if (mixBuffer[i] >= 32767f) samples[i] = 32767;
                else samples[i] = (short)mixBuffer[i];
        }
    }
}

