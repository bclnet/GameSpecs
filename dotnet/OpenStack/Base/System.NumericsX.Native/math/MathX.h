#ifndef __MATHX_H__
#define __MATHX_H__

#ifdef INFINITY
#undef INFINITY
#endif

#define FLOATSIGNBITSET(f)		((*(const unsigned int *)&(f)) >> 31)

class MathX {
public:
	static void					Init(void);

	static float RSqrt(float x);			// reciprocal square root, returns huge number when x == 0.0

	static float				InvSqrt(float x);			// inverse square root with 32 bits precision, returns huge number when x == 0.0
	//static float				InvSqrt16(float x);		// inverse square root with 16 bits precision, returns huge number when x == 0.0
	//static double				InvSqrt64(float x);		// inverse square root with 64 bits precision, returns huge number when x == 0.0

	//static float				Sqrt(float x);			// square root with 32 bits precision
	//static float				Sqrt16(float x);			// square root with 16 bits precision
	//static double				Sqrt64(float x);			// square root with 64 bits precision

	//static float				Sin(float a);				// sine with 32 bits precision
	static float				Sin16(float a);			// sine with 16 bits precision, maximum absolute error is 2.3082e-09
	//static double				Sin64(float a);			// sine with 64 bits precision

	//static float				Cos(float a);				// cosine with 32 bits precision
	//static float				Cos16(float a);			// cosine with 16 bits precision, maximum absolute error is 2.3082e-09
	//static double				Cos64(float a);			// cosine with 64 bits precision

	//static void					SinCos(float a, float& s, float& c);		// sine and cosine with 32 bits precision
	//static void					SinCos16(float a, float& s, float& c);	// sine and cosine with 16 bits precision
	//static void					SinCos64(float a, double& s, double& c);	// sine and cosine with 64 bits precision

	//static float				Tan(float a);				// tangent with 32 bits precision
	//static float				Tan16(float a);			// tangent with 16 bits precision, maximum absolute error is 1.8897e-08
	//static double				Tan64(float a);			// tangent with 64 bits precision

	//static float				ASin(float a);			// arc sine with 32 bits precision, input is clamped to [-1, 1] to avoid a silent NaN
	//static float				ASin16(float a);			// arc sine with 16 bits precision, maximum absolute error is 6.7626e-05
	//static double				ASin64(float a);			// arc sine with 64 bits precision

	//static float				ACos(float a);			// arc cosine with 32 bits precision, input is clamped to [-1, 1] to avoid a silent NaN
	//static float				ACos16(float a);			// arc cosine with 16 bits precision, maximum absolute error is 6.7626e-05
	//static double				ACos64(float a);			// arc cosine with 64 bits precision

	//static float				ATan(float a);			// arc tangent with 32 bits precision
	//static float				ATan16(float a);			// arc tangent with 16 bits precision, maximum absolute error is 1.3593e-08
	//static double				ATan64(float a);			// arc tangent with 64 bits precision

	//static float				ATan(float y, float x);	// arc tangent with 32 bits precision
	static float				ATan16(float y, float x);	// arc tangent with 16 bits precision, maximum absolute error is 1.3593e-08
	//static double				ATan64(float y, float x);	// arc tangent with 64 bits precision

	//static float				Pow(float x, float y);	// x raised to the power y with 32 bits precision
	//static float				Pow16(float x, float y);	// x raised to the power y with 16 bits precision
	//static double				Pow64(float x, float y);	// x raised to the power y with 64 bits precision

	//static float				Exp(float f);				// e raised to the power f with 32 bits precision
	//static float				Exp16(float f);			// e raised to the power f with 16 bits precision
	//static double				Exp64(float f);			// e raised to the power f with 64 bits precision

	//static float				Log(float f);				// natural logarithm with 32 bits precision
	//static float				Log16(float f);			// natural logarithm with 16 bits precision
	//static double				Log64(float f);			// natural logarithm with 64 bits precision

	//static int					IPow(int x, int y);		// integral x raised to the power y
	//static int					ILog2(float f);			// integral base-2 logarithm of the floating point value
	//static int					ILog2(int i);				// integral base-2 logarithm of the integer value

	//static int					BitsForFloat(float f);	// minumum number of bits required to represent ceil( f )
	//static int					BitsForInteger(int i);	// minumum number of bits required to represent i
	//static int					MaskForFloatSign(float f);// returns 0x00000000 if x >= 0.0f and returns 0xFFFFFFFF if x <= -0.0f
	//static int					MaskForIntegerSign(int i);// returns 0x00000000 if x >= 0 and returns 0xFFFFFFFF if x < 0
	//static int					FloorPowerOfTwo(int x);	// round x down to the nearest power of 2
	//static int					CeilPowerOfTwo(int x);	// round x up to the nearest power of 2
	//static bool					IsPowerOfTwo(int x);		// returns true if x is a power of 2
	//static int					BitCount(int x);			// returns the number of 1 bits in x
	//static int					BitReverse(int x);		// returns the bit reverse of x

	//static int					Abs(int x);				// returns the absolute value of the integer value (for reference only)
	//static float				Fabs(float f);			// returns the absolute value of the floating point value
	//static float				Floor(float f);			// returns the largest integer that is less than or equal to the given value
	//static float				Ceil(float f);			// returns the smallest integer that is greater than or equal to the given value
	//static float				Rint(float f);			// returns the nearest integer
	//static int					Ftoi(float f);			// float to int conversion
	//static int					FtoiFast(float f);		// fast float to int conversion but uses current FPU round mode (default round nearest)
	//static unsigned int			Ftol(float f);			// float to int conversion
	//static unsigned int			FtolFast(float);			// fast float to int conversion but uses current FPU round mode (default round nearest)

	//static signed char			ClampChar(int i);
	//static signed short			ClampShort(int i);
	//static int					ClampInt(int min, int max, int value);
	//static float				ClampFloat(float min, float max, float value);

	//static float				AngleNormalize360(float angle);
	//static float				AngleNormalize180(float angle);
	//static float				AngleDelta(float angle1, float angle2);

	//static int					FloatToBits(float f, int exponentBits, int mantissaBits);
	//static float				BitsToFloat(int i, int exponentBits, int mantissaBits);

	//static int					FloatHash(const float* array, const int numFloats);

	static const float			PI;							// pi
	static const float			TWO_PI;						// pi * 2
	static const float			HALF_PI;					// pi / 2
	//static const float			ONEFOURTH_PI;				// pi / 4
	//static const float			E;							// e
	//static const float			SQRT_TWO;					// sqrt( 2 )
	//static const float			SQRT_THREE;					// sqrt( 3 )
	//static const float			SQRT_1OVER2;				// sqrt( 1 / 2 )
	//static const float			SQRT_1OVER3;				// sqrt( 1 / 3 )
	//static const float			M_DEG2RAD;					// degrees to radians multiplier
	//static const float			M_RAD2DEG;					// radians to degrees multiplier
	//static const float			M_SEC2MS;					// seconds to milliseconds multiplier
	//static const float			M_MS2SEC;					// milliseconds to seconds multiplier
	static const float			INFINITY;					// huge number which should be larger than any valid number used
	//static const float			FLT_EPSILON;				// smallest positive number such that 1.0+FLT_EPSILON != 1.0

private:
	enum {
		LOOKUP_BITS = 8,
		EXP_POS = 23,
		EXP_BIAS = 127,
		LOOKUP_POS = (EXP_POS - LOOKUP_BITS),
		SEED_POS = (EXP_POS - 8),
		SQRT_TABLE_SIZE = (2 << LOOKUP_BITS),
		LOOKUP_MASK = (SQRT_TABLE_SIZE - 1)
	};

	union _flint {
		dword					i;
		float					f;
	};

	static dword				iSqrt[SQRT_TABLE_SIZE];
	static bool					initialized;
};

ID_INLINE float MathX::RSqrt(float x) {

	int i; float y, r;

	y = x * 0.5f;
	i = *reinterpret_cast<int*>(&x);
	i = 0x5f3759df - (i >> 1);
	r = *reinterpret_cast<float*>(&i);
	r = r * (1.5f - r * r * y);
	return r;
}

ID_INLINE float MathX::InvSqrt(float x) {

	dword a = ((union _flint*)(&x))->i;
	union _flint seed;

	assert(initialized);

	double y = x * 0.5f;
	seed.i = ((((3 * EXP_BIAS - 1) - ((a >> EXP_POS) & 0xFF)) >> 1) << EXP_POS) | iSqrt[(a >> (EXP_POS - LOOKUP_BITS)) & LOOKUP_MASK];
	double r = seed.f;
	r = r * (1.5f - r * r * y);
	r = r * (1.5f - r * r * y);
	return (float)r;
}

ID_INLINE float MathX::Sin16(float a) {
	float s;

	if ((a < 0.0f) || (a >= TWO_PI)) {
		a -= floorf(a / TWO_PI) * TWO_PI;
	}
#if 1
	if (a < PI) {
		if (a > HALF_PI) {
			a = PI - a;
		}
	}
	else {
		if (a > PI + HALF_PI) {
			a = a - TWO_PI;
		}
		else {
			a = PI - a;
		}
	}
#else
	a = PI - a;
	if (fabs(a) >= HALF_PI) {
		a = ((a < 0.0f) ? -PI : PI) - a;
	}
#endif
	s = a * a;
	return a * (((((-2.39e-08f * s + 2.7526e-06f) * s - 1.98409e-04f) * s + 8.3333315e-03f) * s - 1.666666664e-01f) * s + 1.0f);
}

ID_INLINE float MathX::ATan16(float y, float x) {
	float a, s;

	if (fabs(y) > fabs(x)) {
		a = x / y;
		s = a * a;
		s = -(((((((((0.0028662257f * s - 0.0161657367f) * s + 0.0429096138f) * s - 0.0752896400f)
			* s + 0.1065626393f) * s - 0.1420889944f) * s + 0.1999355085f) * s - 0.3333314528f) * s) + 1.0f) * a;
		if (FLOATSIGNBITSET(a)) {
			return s - HALF_PI;
		}
		else {
			return s + HALF_PI;
		}
	}
	else {
		a = y / x;
		s = a * a;
		return (((((((((0.0028662257f * s - 0.0161657367f) * s + 0.0429096138f) * s - 0.0752896400f)
			* s + 0.1065626393f) * s - 0.1420889944f) * s + 0.1999355085f) * s - 0.3333314528f) * s) + 1.0f) * a;
	}
}

#endif
