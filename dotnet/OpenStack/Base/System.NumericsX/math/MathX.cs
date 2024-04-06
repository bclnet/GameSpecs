using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.NumericsX
{
    // token types
    public unsafe static class MathX
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float DEG2RAD(float a) => a * M_DEG2RAD;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float RAD2DEG(float a) => a * M_RAD2DEG;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double RAD2DEG(double a) => a * M_RAD2DEG;

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int SEC2MS(float t) => FtoiFast(t * M_SEC2MS);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float MS2SEC(int t) => t * M_MS2SEC;

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int ANGLE2SHORT(float x) => FtoiFast(x * 65536f / 360f) & 65535;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float SHORT2ANGLE(int x) => x * (360f / 65536f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int ANGLE2BYTE(float x) => FtoiFast(x * 256f / 360f) & 255;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float BYTE2ANGLE(int x) => x * (360f / 256f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool FLOATSIGNBITSET(float f) => ((*(uint*)&f) >> 31) != 0;[MethodImpl(MethodImplOptions.AggressiveInlining)] public static  int FLOATSIGNBITSET_(float f) => ((*(int*)&f) >> 31);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool FLOATSIGNBITNOTSET(float f) => ((~*(uint*)&f) >> 31) != 0;[MethodImpl(MethodImplOptions.AggressiveInlining)] public static  int FLOATSIGNBITNOTSET_(float f) => (~*(int*)&f) >> 31;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool FLOATNOTZERO(float f) => ((*(uint*)&f) & ~(1 << 31)) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool INTSIGNBITSET(int i) => (((uint)i) >> 31) != 0;[MethodImpl(MethodImplOptions.AggressiveInlining)] public static  int INTSIGNBITSET_(int i) => i >> 31;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool INTSIGNBITNOTSET(int i) => ((~(uint)i) >> 31) != 0;[MethodImpl(MethodImplOptions.AggressiveInlining)] public static  int INTSIGNBITNOTSET_(int i) => (~i) >> 31;

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool FLOAT_IS_NAN(float x) => ((*(uint*)&x) & 0x7f800000) == 0x7f800000;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool FLOAT_IS_INF(float x) => ((*(uint*)&x) & 0x7fffffff) == 0x7f800000;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool FLOAT_IS_IND(float x) => (*(uint*)&x) == 0xffc00000;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool FLOAT_IS_DENORMAL(float x) => ((*(uint*)&x) & 0x7f800000) == 0x00000000 && ((*(uint*)&x) & 0x007fffff) != 0x00000000;

        public const int IEEE_FLT_MANTISSA_BITS = 23;
        public const int IEEE_FLT_EXPONENT_BITS = 8;
        public const int IEEE_FLT_EXPONENT_BIAS = 127;
        public const int IEEE_FLT_SIGN_BIT = 31;

        public const int IEEE_DBL_MANTISSA_BITS = 52;
        public const int IEEE_DBL_EXPONENT_BITS = 11;
        public const int IEEE_DBL_EXPONENT_BIAS = 1023;
        public const int IEEE_DBL_SIGN_BIT = 63;

        public const int IEEE_DBLE_MANTISSA_BITS = 63;
        public const int IEEE_DBLE_EXPONENT_BITS = 15;
        public const int IEEE_DBLE_EXPONENT_BIAS = 0;
        public const int IEEE_DBLE_SIGN_BIT = 79;

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int MaxIndex(int x, int y) => x > y ? 0 : 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int MinIndex(int x, int y) => x < y ? 0 : 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float MaxIndex(float x, float y) => x > y ? 0 : 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float MinIndex(float x, float y) => x < y ? 0 : 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int Max3(int x, int y, int z) => x > y ? (x > z ? x : z) : (y > z ? y : z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int Min3(int x, int y, int z) => x < y ? (x < z ? x : z) : (y < z ? y : z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int Max3Index(int x, int y, int z) => x > y ? (x > z ? 0 : 2) : (y > z ? 1 : 2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int Min3Index(int x, int y, int z) => x < y ? (x < z ? 0 : 2) : (y < z ? 1 : 2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Max3(float x, float y, float z) => x > y ? (x > z ? x : z) : (y > z ? y : z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Min3(float x, float y, float z) => x < y ? (x < z ? x : z) : (y < z ? y : z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int Max3Index(float x, float y, float z) => x > y ? (x > z ? 0 : 2) : (y > z ? 1 : 2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int Min3Index(float x, float y, float z) => x < y ? (x < z ? 0 : 2) : (y < z ? 1 : 2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int Sign(int f) => f > 0 ? 1 : (f < 0 ? -1 : 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int Square(int x) => x * x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int Cube(int x) => x * x * x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Sign(float f) => f > 0 ? 1 : (f < 0 ? -1 : 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Square(float x) => x * x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Cube(float x) => x * x * x;

        public static void Init()
        {
            reinterpret.F2ui fi = new(), fo = new();
            for (var i = 0; i < SQRT_TABLE_SIZE; i++)
            {
                fi.u = (uint)(((EXP_BIAS - 1) << EXP_POS) | (i << LOOKUP_POS));
                fo.f = (float)(1.0 / Math.Sqrt(fi.f));
                iSqrt[i] = (((fo.u + (1 << (SEED_POS - 2))) >> SEED_POS) & 0xFF) << SEED_POS;
            }
            iSqrt[SQRT_TABLE_SIZE / 2] = (uint)0xFF << SEED_POS;
            initialized = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RSqrt(float x)            // reciprocal square root, returns huge number when x == 0.0
        {
            float y = x * 0.5f;
            int i = reinterpret.cast_int(x);
            i = 0x5f3759df - (i >> 1);
            float r = reinterpret.cast_float(i);
            r *= 1.5f - r * r * y;
            return r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InvSqrt(float x)          // inverse square root with 32 bits precision, returns huge number when x == 0.0
        {
            Debug.Assert(initialized);
            uint a = new reinterpret.F2ui { f = x }.u;
            reinterpret.F2ui seed = new();

            double y = x * 0.5f;
            seed.u = (((3 * EXP_BIAS - 1 - ((a >> EXP_POS) & 0xFF)) >> 1) << EXP_POS) | iSqrt[(a >> (EXP_POS - LOOKUP_BITS)) & LOOKUP_MASK];
            double r = seed.f;
            r *= 1.5f - r * r * y;
            r *= 1.5f - r * r * y;
            return (float)r;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InvSqrt16(float x)        // inverse square root with 16 bits precision, returns huge number when x == 0.0
        {
            Debug.Assert(initialized);
            uint a = new reinterpret.F2ui { f = x }.u;
            reinterpret.F2ui seed = new();

            double y = x * 0.5f;
            seed.u = (((3 * EXP_BIAS - 1 - ((a >> EXP_POS) & 0xFF)) >> 1) << EXP_POS) | iSqrt[(a >> (EXP_POS - LOOKUP_BITS)) & LOOKUP_MASK];
            double r = seed.f;
            r *= 1.5f - r * r * y;
            return (float)r;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double InvSqrt64(float x)       // inverse square root with 64 bits precision, returns huge number when x == 0.0
        {
            Debug.Assert(initialized);
            uint a = new reinterpret.F2ui { f = x }.u;
            reinterpret.F2ui seed = new();

            double y = x * 0.5f;
            seed.u = (((3 * EXP_BIAS - 1 - ((a >> EXP_POS) & 0xFF)) >> 1) << EXP_POS) | iSqrt[(a >> (EXP_POS - LOOKUP_BITS)) & LOOKUP_MASK];
            double r = seed.f;
            r *= 1.5f - r * r * y;
            r *= 1.5f - r * r * y;
            r *= 1.5f - r * r * y;
            return r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sqrt(float x)          // square root with 32 bits precision
            => x * InvSqrt(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sqrt16(float x)           // square root with 16 bits precision
            => x * InvSqrt16(x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sqrt64(float x)          // square root with 64 bits precision
            => x * InvSqrt64(x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin(float a)             // sine with 32 bits precision
            => (float)Math.Sin(a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin16(float a)            // sine with 16 bits precision, maximum absolute error is 2.3082e-09
        {
            float s;

            if ((a < 0f) || (a >= TWO_PI)) a -= (float)Math.Floor(a / TWO_PI) * TWO_PI;
#if true
            if (a < PI)
            {
                if (a > HALF_PI) a = PI - a;
            }
            else
            {
                if (a > PI + HALF_PI) a -= TWO_PI;
                else a = PI - a;
            }
#else
            a = PI - a;
            if (Math.Fabs(a) >= HALF_PI) a = ((a < 0f) ? -PI : PI) - a;
#endif
            s = a * a;
            return a * (((((-2.39e-08f * s + 2.7526e-06f) * s - 1.98409e-04f) * s + 8.3333315e-03f) * s - 1.666666664e-01f) * s + 1f);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Sin64(float a)          // sine with 64 bits precision
            => Math.Sin(a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos(float a)              // cosine with 32 bits precision
            => (float)Math.Cos(a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos16(float a)            // cosine with 16 bits precision, maximum absolute error is 2.3082e-09
        {
            float s, d;

            if ((a < 0f) || (a >= TWO_PI)) a -= (float)Math.Floor(a / TWO_PI) * TWO_PI;
#if true
            if (a < PI)
            {
                if (a > HALF_PI) { a = PI - a; d = -1f; }
                else d = 1f;
            }
            else
            {
                if (a > PI + HALF_PI) { a -= TWO_PI; d = 1f; }
                else { a = PI - a; d = -1f; }
            }
#else
            a = PI - a;
            if (fabs(a) >= HALF_PI) { a = ((a < 0f) ? -PI : PI) - a; d = 1f; }
            else d = -1f;
#endif
            s = a * a;
            return d * (((((-2.605e-07f * s + 2.47609e-05f) * s - 1.3888397e-03f) * s + 4.16666418e-02f) * s - 4.999999963e-01f) * s + 1f);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Cos64(float a)           // cosine with 64 bits precision
             => Math.Cos(a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SinCos(float a, out float s, out float c)       // sine and cosine with 32 bits precision
        {
#if _M_IX86
	_asm {
		fld		a
		fsincos
		mov		ecx, c
		mov		edx, s
		fstp	dword ptr [ecx]
		fstp	dword ptr [edx]
	}
#else
            s = (float)Math.Sin(a);
            c = (float)Math.Cos(a);
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SinCos16(float a, out float s, out float c) // sine and cosine with 16 bits precision
        {
            float t, d;

            if ((a < 0f) || (a >= TWO_PI))
                a -= (float)Math.Floor(a / TWO_PI) * TWO_PI;
#if true
            if (a < PI)
            {
                if (a > HALF_PI) { a = PI - a; d = -1f; }
                else d = 1f;
            }
            else
            {
                if (a > PI + HALF_PI) { a -= TWO_PI; d = 1f; }
                else { a = PI - a; d = -1f; }
            }
#else
            a = PI - a;
            if (fabs(a) >= HALF_PI) { a = ((a < 0f) ? -PI : PI) - a; d = 1f; }
            else d = -1f;
#endif
            t = a * a;
            s = a * (((((-2.39e-08f * t + 2.7526e-06f) * t - 1.98409e-04f) * t + 8.3333315e-03f) * t - 1.666666664e-01f) * t + 1f);
            c = d * (((((-2.605e-07f * t + 2.47609e-05f) * t - 1.3888397e-03f) * t + 4.16666418e-02f) * t - 4.999999963e-01f) * t + 1f);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SinCos64(float a, out double s, out double c) // sine and cosine with 64 bits precision
        {
#if _M_IX86
	_asm {
		fld		a
		fsincos
		mov		ecx, c
		mov		edx, s
		fstp	qword ptr [ecx]
		fstp	qword ptr [edx]
	}
#else
            s = Math.Sin(a);
            c = Math.Cos(a);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Tan(float a)               // tangent with 32 bits precision
            => (float)Math.Tan(a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Tan16(float a)            // tangent with 16 bits precision, maximum absolute error is 1.8897e-08
        {
            float s; bool reciprocal;

            if ((a < 0f) || (a >= PI))
                a -= (float)Math.Floor(a / PI) * PI;
#if true
            if (a < HALF_PI)
            {
                if (a > ONEFOURTH_PI) { a = HALF_PI - a; reciprocal = true; }
                else reciprocal = false;
            }
            else
            {
                if (a > HALF_PI + ONEFOURTH_PI) { a -= PI; reciprocal = false; }
                else { a = HALF_PI - a; reciprocal = true; }
            }
#else
            a = HALF_PI - a;
            if (fabs(a) >= ONEFOURTH_PI) { a = ((a < 0f) ? -HALF_PI : HALF_PI) - a; reciprocal = false; }
            else reciprocal = true;
#endif
            s = a * a;
            s = a * ((((((9.5168091e-03f * s + 2.900525e-03f) * s + 2.45650893e-02f) * s + 5.33740603e-02f) * s + 1.333923995e-01f) * s + 3.333314036e-01f) * s + 1f);
            if (reciprocal) return 1f / s;
            else return s;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Tan64(float a)            // tangent with 64 bits precision
            => Math.Tan(a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ASin(float a)         // arc sine with 32 bits precision, input is clamped to [-1, 1] to avoid a silent NaN
        {
            if (a <= -1f) return -HALF_PI;
            if (a >= 1f) return HALF_PI;
            return (float)Math.Asin(a);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ASin16(float a)           // arc sine with 16 bits precision, maximum absolute error is 6.7626e-05
        {
            if (FLOATSIGNBITSET(a))
            {
                if (a <= -1f) return -HALF_PI;
                a = Math.Abs(a);
                return (((-0.0187293f * a + 0.0742610f) * a - 0.2121144f) * a + 1.5707288f) * (float)Math.Sqrt(1f - a) - HALF_PI;
            }
            else
            {
                if (a >= 1f) return HALF_PI;
                return HALF_PI - (((-0.0187293f * a + 0.0742610f) * a - 0.2121144f) * a + 1.5707288f) * (float)Math.Sqrt(1f - a);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ASin64(float a)          // arc sine with 64 bits precision
        {
            if (a <= -1f) return -HALF_PI;
            if (a >= 1f) return HALF_PI;
            return Math.Asin(a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ACos(float a)         // arc cosine with 32 bits precision, input is clamped to [-1, 1] to avoid a silent NaN
        {
            if (a <= -1f) return PI;
            if (a >= 1f) return 0f;
            return (float)Math.Acos(a);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ACos16(float a)           // arc cosine with 16 bits precision, maximum absolute error is 6.7626e-05
        {
            if (FLOATSIGNBITSET(a))
            {
                if (a <= -1f) return PI;
                a = Math.Abs(a);
                return PI - (((-0.0187293f * a + 0.0742610f) * a - 0.2121144f) * a + 1.5707288f) * (float)Math.Sqrt(1f - a);
            }
            else
            {
                if (a >= 1f) return 0f;
                return (((-0.0187293f * a + 0.0742610f) * a - 0.2121144f) * a + 1.5707288f) * (float)Math.Sqrt(1f - a);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ACos64(float a)          // arc cosine with 64 bits precision
        {
            if (a <= -1f) return PI;
            if (a >= 1f) return 0f;
            return Math.Acos(a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ATan(float a)         // arc tangent with 32 bits precision
            => (float)Math.Atan(a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ATan16(float a)           // arc tangent with 16 bits precision, maximum absolute error is 1.3593e-08
        {
            float s;

            if (Math.Abs(a) > 1f)
            {
                a = 1f / a;
                s = a * a;
                s = -(((((((((0.0028662257f * s - 0.0161657367f) * s + 0.0429096138f) * s - 0.0752896400f)
                    * s + 0.1065626393f) * s - 0.1420889944f) * s + 0.1999355085f) * s - 0.3333314528f) * s) + 1f) * a;
                if (FLOATSIGNBITSET(a)) return s - HALF_PI;
                else return s + HALF_PI;
            }
            else
            {
                s = a * a;
                return (((((((((0.0028662257f * s - 0.0161657367f) * s + 0.0429096138f) * s - 0.0752896400f)
                    * s + 0.1065626393f) * s - 0.1420889944f) * s + 0.1999355085f) * s - 0.3333314528f) * s) + 1f) * a;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ATan64(float a)           // arc tangent with 64 bits precision
            => Math.Atan(a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ATan(float y, float x)     // arc tangent with 32 bits precision
            => (float)Math.Atan2(y, x);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ATan16(float y, float x)  // arc tangent with 16 bits precision, maximum absolute error is 1.3593e-08
        {
            float a, s;

            if (Math.Abs(y) > Math.Abs(x))
            {
                a = x / y;
                s = a * a;
                s = -(((((((((0.0028662257f * s - 0.0161657367f) * s + 0.0429096138f) * s - 0.0752896400f)
                        * s + 0.1065626393f) * s - 0.1420889944f) * s + 0.1999355085f) * s - 0.3333314528f) * s) + 1f) * a;
                if (FLOATSIGNBITSET(a)) return s - HALF_PI;
                else return s + HALF_PI;
            }
            else
            {
                a = y / x;
                s = a * a;
                return (((((((((0.0028662257f * s - 0.0161657367f) * s + 0.0429096138f) * s - 0.0752896400f)
                    * s + 0.1065626393f) * s - 0.1420889944f) * s + 0.1999355085f) * s - 0.3333314528f) * s) + 1f) * a;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double ATan64(float y, float x) // arc tangent with 64 bits precision
            => Math.Atan2(y, x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Pow(float x, float y)  // x raised to the power y with 32 bits precision
            => (float)Math.Pow(x, y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Pow16(float x, float y)   // x raised to the power y with 16 bits precision
             => Exp16(y * Log16(x));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Pow64(float x, float y)  // x raised to the power y with 64 bits precision
             => Math.Pow(x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Exp(float f)              // e raised to the power f with 32 bits precision
             => (float)Math.Exp(f);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Exp16(float f)            // e raised to the power f with 16 bits precision
        {
            int i, s, e, m, exponent;
            float x, x2, y, p, q;

            x = f * 1.44269504088896340f;       // multiply with ( 1 / log( 2 ) )
#if true
            i = reinterpret.cast_int(x);
            s = (i >> IEEE_FLT_SIGN_BIT);
            e = ((i >> IEEE_FLT_MANTISSA_BITS) & ((1 << IEEE_FLT_EXPONENT_BITS) - 1)) - IEEE_FLT_EXPONENT_BIAS;
            m = (i & ((1 << IEEE_FLT_MANTISSA_BITS) - 1)) | (1 << IEEE_FLT_MANTISSA_BITS);
            i = ((m >> (IEEE_FLT_MANTISSA_BITS - e)) & ~(e >> 31)) ^ s;
#else
            i = (int)x;
            if (x < 0f) i--;
#endif
            exponent = (i + IEEE_FLT_EXPONENT_BIAS) << IEEE_FLT_MANTISSA_BITS;
            y = reinterpret.cast_float(exponent);
            x -= i;
            if (x >= 0.5f) { x -= 0.5f; y *= 1.4142135623730950488f; } // multiply with sqrt( 2 )
            x2 = x * x;
            p = x * (7.2152891511493f + x2 * 0.0576900723731f);
            q = 20.8189237930062f + x2;
            x = y * (q + p) / (q - p);
            return x;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Exp64(float f)           // e raised to the power f with 64 bits precision
             => Math.Exp(f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Log(float f)              // natural logarithm with 32 bits precision
            => (float)Math.Log(f);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Log16(float f)            // natural logarithm with 16 bits precision
        {
            int i, exponent;
            float y, y2;

            i = reinterpret.cast_int(f);
            exponent = ((i >> IEEE_FLT_MANTISSA_BITS) & ((1 << IEEE_FLT_EXPONENT_BITS) - 1)) - IEEE_FLT_EXPONENT_BIAS;
            i -= (exponent + 1) << IEEE_FLT_MANTISSA_BITS;  // get value in the range [.5, 1>
            y = reinterpret.cast_float(i);
            y *= 1.4142135623730950488f;                        // multiply with sqrt( 2 )
            y = (y - 1f) / (y + 1f);
            y2 = y * y;
            y *= (2.000000000046727f + y2 * (0.666666635059382f + y2 * (0.4000059794795f + y2 * (0.28525381498f + y2 * 0.2376245609f))));
            y += 0.693147180559945f * (exponent + 0.5f);
            return y;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Log64(float f)           // natural logarithm with 64 bits precision
            => Math.Log(f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IPow(int x, int y)      // integral x raised to the power y
        {
            int r; for (r = x; y > 1; y--) { r *= x; }
            return r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ILog2(float f)         // integral base-2 logarithm of the floating point value
            => ((reinterpret.cast_int(f) >> IEEE_FLT_MANTISSA_BITS) & ((1 << IEEE_FLT_EXPONENT_BITS) - 1)) - IEEE_FLT_EXPONENT_BIAS;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ILog2(int i)                // integral base-2 logarithm of the integer value
             => ILog2((float)i);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitsForFloat(float f)   // minumum number of bits required to represent ceil( f )
            => ILog2(f) + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitsForInteger(int i)   // minumum number of bits required to represent i
            => ILog2((float)i) + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MaskForFloatSign(float f)// returns 0x00000000 if x >= 0f and returns 0xFFFFFFFF if x <= -0f
            => (reinterpret.cast_int(f) >> 31);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MaskForIntegerSign(int i)// returns 0x00000000 if x >= 0 and returns 0xFFFFFFFF if x < 0
            => (i >> 31);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorPowerOfTwo(int x)  // round x down to the nearest power of 2
            => CeilPowerOfTwo(x) >> 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CeilPowerOfTwo(int x)   // round x up to the nearest power of 2
        {
            x--;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            x++;
            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(int x)        // returns true if x is a power of 2
            => (x & (x - 1)) == 0 && x > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitCount(int x)         // returns the number of 1 bits in x
        {
            x -= ((x >> 1) & 0x55555555);
            x = (((x >> 2) & 0x33333333) + (x & 0x33333333));
            x = (((x >> 4) + x) & 0x0f0f0f0f);
            x += (x >> 8);
            return (x + (x >> 16)) & 0x0000003f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitReverse(int x)       // returns the bit reverse of x
        {
            x = (((x >> 1) & 0x55555555) | ((x & 0x55555555) << 1));
            x = (((x >> 2) & 0x33333333) | ((x & 0x33333333) << 2));
            x = (((x >> 4) & 0x0f0f0f0f) | ((x & 0x0f0f0f0f) << 4));
            x = (((x >> 8) & 0x00ff00ff) | ((x & 0x00ff00ff) << 8));
            return (x >> 16) | (x << 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(int x)              // returns the absolute value of the integer value (for reference only)
        {
            var y = x >> 31;
            return (x ^ y) - y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Fabs(float f)         // returns the absolute value of the floating point value
        {
            var tmp = reinterpret.cast_int(f);
            tmp &= 0x7FFFFFFF;
            return reinterpret.cast_float(tmp);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Fabs(double f)         // returns the absolute value of the floating point value
        {
            var tmp = reinterpret.cast_int((float)f);
            tmp &= 0x7FFFFFFF;
            return reinterpret.cast_float(tmp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Floor(float f)            // returns the largest integer that is less than or equal to the given value
            => (float)Math.Floor(f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Ceil(float f)         // returns the smallest integer that is greater than or equal to the given value
            => (float)Math.Ceiling(f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Rint(float f)         // returns the nearest integer
            => (float)Math.Floor(f + 0.5f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Ftoi(float f)           // float to int conversion
            => (int)f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FtoiFast(float f)       // fast float to int conversion but uses current FPU round mode (default round nearest)
        {
#if _M_IX86
	int i;
	__asm fld		f
	__asm fistp		i		// use default rouding mode (round nearest)
	return i;
#else
            return (int)f;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Ftol(float f)          // float to int conversion
            => (uint)f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Ftol(double f)          // float to int conversion
            => (uint)f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint FtolFast(float f)           // fast float to int conversion but uses current FPU round mode (default round nearest)
        {
#if _M_IX86
	// FIXME: this overflows on 31bits still .. same as FtoiFast
	unsigned int i;
	__asm fld		f
	__asm fistp		i		// use default rouding mode (round nearest)
	return i;
#else
            return (uint)f;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ClampChar(int i)
        {
            if (i < -128) return -128;
            if (i > 127) return 127;
            return (sbyte)i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ClampShort(int i)
        {
            if (i < -32768) return -32768;
            if (i > 32767) return 32767;
            return (short)i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ClampInt(int min, int max, int value)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ClampFloat(float min, float max, float value)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleNormalize360(float angle)
        {
            if ((angle >= 360f) || (angle < 0f)) angle -= (float)Math.Floor(angle / 360f) * 360f;
            return angle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleNormalize180(float angle)
        {
            angle = AngleNormalize360(angle);
            if (angle > 180f) angle -= 360f;
            return angle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AngleDelta(float angle1, float angle2)
            => AngleNormalize180(angle1 - angle2);

        public static int FloatToBits(float f, int exponentBits, int mantissaBits)
        {
            int i, sign, exponent, mantissa, value;
            Debug.Assert(exponentBits >= 2 && exponentBits <= 8);
            Debug.Assert(mantissaBits >= 2 && mantissaBits <= 23);

            var maxBits = (((1 << (exponentBits - 1)) - 1) << mantissaBits) | ((1 << mantissaBits) - 1);
            var minBits = (((1 << exponentBits) - 2) << mantissaBits) | 1;

            var max = BitsToFloat(maxBits, exponentBits, mantissaBits);
            var min = BitsToFloat(minBits, exponentBits, mantissaBits);

            if (f >= 0f)
            {
                if (f >= max) return maxBits;
                else if (f <= min) return minBits;
            }
            else
            {
                if (f <= -max) return maxBits | (1 << (exponentBits + mantissaBits));
                else if (f >= -min) return minBits | (1 << (exponentBits + mantissaBits));
            }

            exponentBits--;
            i = reinterpret.cast_int(f);
            sign = (i >> IEEE_FLT_SIGN_BIT) & 1;
            exponent = ((i >> IEEE_FLT_MANTISSA_BITS) & ((1 << IEEE_FLT_EXPONENT_BITS) - 1)) - IEEE_FLT_EXPONENT_BIAS;
            mantissa = i & ((1 << IEEE_FLT_MANTISSA_BITS) - 1);
            value = sign << (1 + exponentBits + mantissaBits);
            value |= ((INTSIGNBITSET(exponent) ? 1 : 0 << exponentBits) | (Math.Abs(exponent) & ((1 << exponentBits) - 1))) << mantissaBits;
            value |= mantissa >> (IEEE_FLT_MANTISSA_BITS - mantissaBits);
            return value;
        }

        static int[] BitsToFloat_exponentSign = new[] { 1, -1 };
        public static float BitsToFloat(int i, int exponentBits, int mantissaBits)
        {
            int sign, exponent, mantissa, value;
            Debug.Assert(exponentBits >= 2 && exponentBits <= 8);
            Debug.Assert(mantissaBits >= 2 && mantissaBits <= 23);

            exponentBits--;
            sign = i >> (1 + exponentBits + mantissaBits);
            exponent = ((i >> mantissaBits) & ((1 << exponentBits) - 1)) * BitsToFloat_exponentSign[(i >> (exponentBits + mantissaBits)) & 1];
            mantissa = (i & ((1 << mantissaBits) - 1)) << (IEEE_FLT_MANTISSA_BITS - mantissaBits);
            value = sign << IEEE_FLT_SIGN_BIT | (exponent + IEEE_FLT_EXPONENT_BIAS) << IEEE_FLT_MANTISSA_BITS | mantissa;
            return reinterpret.cast_float(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloatHash(float[] array, int numFloats)
        {
            throw new NotImplementedException();
            //int i, hash = 0;
            //const int* ptr;

            //ptr = reinterpret_cast_int(array);
            //for (i = 0; i < numFloats; i++) hash ^= ptr[i];
            //return hash;
        }

        public const float PI = 3.14159265358979323846f;                          // pi
        public const float TWO_PI = 2f * PI;                      // pi * 2
        public const float HALF_PI = 0.5f * PI;                 // pi / 2
        public const float ONEFOURTH_PI = 0.25f * PI;                // pi / 4
        public const float E = 2.71828182845904523536f;                           // e
        public const float SQRT_TWO = 1.41421356237309504880f;                   // sqrt( 2 )
        public const float SQRT_THREE = 1.73205080756887729352f;                  // sqrt( 3 )
        public const float SQRT_1OVER2 = 0.70710678118654752440f;             // sqrt( 1 / 2 )
        public const float SQRT_1OVER3 = 0.57735026918962576450f;             // sqrt( 1 / 3 )
        public const float M_DEG2RAD = PI / 180f;                   // degrees to radians multiplier
        public const float M_RAD2DEG = 180f / PI;                   // radians to degrees multiplier
        public const float M_SEC2MS = 1000f;                    // seconds to milliseconds multiplier
        public const float M_MS2SEC = 0.001f;                   // milliseconds to seconds multiplier
        public const float INFINITY = 1e30f;                    // huge number which should be larger than any valid number used
        public const float FLT_EPSILON = 1.192092896e-07f;             // smallest positive number such that 1.0+FLT_EPSILON != 1.0

        const int LOOKUP_BITS = 8;
        const int EXP_POS = 23;
        const int EXP_BIAS = 127;
        const int LOOKUP_POS = EXP_POS - LOOKUP_BITS;
        const int SEED_POS = EXP_POS - 8;
        const int SQRT_TABLE_SIZE = 2 << LOOKUP_BITS;
        const int LOOKUP_MASK = SQRT_TABLE_SIZE - 1;
        static readonly uint[] iSqrt = new uint[SQRT_TABLE_SIZE];		// inverse square root lookup table
        static bool initialized = false;


        // Old 3D vector macros, should no longer be used.

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float DotProduct(in Vector3 a, float* b) => a.x * b[0] + a.y * b[1] + a.z * b[2];
        //[MethodImpl(MethodImplOptions.AggressiveInlining)] public static float DotProduct(in Vector3 a, Span<float> b) => a.x * b[0] + a.y * b[1] + a.z * b[2];
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float DotProduct(in Plane a, float* b) => a.a * b[0] + a.b * b[1] + a.c * b[2];
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float DotProduct(in Plane a, Span<float> b) => a.a * b[0] + a.b * b[1] + a.c * b[2];

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void VectorSubtract(in Vector3 a, float* b, ref Vector3 c) { c.x = a.x - b[0]; c.y = a.y - b[1]; c.z = a.z - b[2]; }
        //[MethodImpl(MethodImplOptions.AggressiveInlining)] public static void VectorSubtract(in Vector3 a, Span<float> b, ref Vector3 c) { c.x = a.x - b[0]; c.y = a.y - b[1]; c.z = a.z - b[2]; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void VectorSubtract(in Vector3 a, in Vector3 b, float[] c) { c[0] = a.x - b.x; c[1] = a.y - b.y; c[2] = a.z - b.z; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void VectorSubtract(in Vector3 a, in Vector3 b, float* c) { c[0] = a.x - b.x; c[1] = a.y - b.y; c[2] = a.z - b.z; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void VectorAdd(in Vector3 a, in Vector3 b, float[] c) { c[0] = a.x + b.x; c[1] = a.y + b.y; c[2] = a.z + b.z; }
        //#define VectorScale( v, s, o )		((o)[0]=(v)[0]*(s),(o)[1]=(v)[1]*(s),(o)[2]=(v)[2]*(s))
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void VectorMA(in Vector3 v, float s, in Vector3 b, out Vector3 o) { o.x = v.x + b.x * s; o.y = v.y + b.y * s; o.z = v.z + b.z * s; }
        //#define VectorCopy( a, b )			((b)[0]=(a)[0],(b)[1]=(a)[1],(b)[2]=(a)[2])
    }
}