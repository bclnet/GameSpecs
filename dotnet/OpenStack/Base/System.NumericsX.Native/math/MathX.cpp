#include "system/Platform.h"
#include "MathX.h"

const float	MathX::PI = 3.14159265358979323846f;
const float	MathX::TWO_PI = 2.0f * PI;
const float	MathX::HALF_PI = 0.5f * PI;
//const float	MathX::ONEFOURTH_PI = 0.25f * PI;
//const float MathX::E = 2.71828182845904523536f;
//const float MathX::SQRT_TWO = 1.41421356237309504880f;
//const float MathX::SQRT_THREE = 1.73205080756887729352f;
//const float	MathX::SQRT_1OVER2 = 0.70710678118654752440f;
//const float	MathX::SQRT_1OVER3 = 0.57735026918962576450f;
//const float	MathX::M_DEG2RAD = PI / 180.0f;
//const float	MathX::M_RAD2DEG = 180.0f / PI;
//const float	MathX::M_SEC2MS = 1000.0f;
//const float	MathX::M_MS2SEC = 0.001f;
const float	MathX::INFINITY = 1e30f;
//const float MathX::FLT_EPSILON = 1.192092896e-07f;

bool		MathX::initialized = false;
dword		MathX::iSqrt[SQRT_TABLE_SIZE];		// inverse square root lookup table

void MathX::Init(void) {
	union _flint fi, fo;

	for (int i = 0; i < SQRT_TABLE_SIZE; i++) {
		fi.i = ((EXP_BIAS - 1) << EXP_POS) | (i << LOOKUP_POS);
		fo.f = (float)(1.0 / sqrt(fi.f));
		iSqrt[i] = ((dword)(((fo.i + (1 << (SEED_POS - 2))) >> SEED_POS) & 0xFF)) << SEED_POS;
	}

	iSqrt[SQRT_TABLE_SIZE / 2] = ((dword)(0xFF)) << (SEED_POS);

	initialized = true;
}
