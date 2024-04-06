#if defined(_MSC_VER) && defined(_M_IX86)
#include "system/Platform.h"
#include "Simd_SSE2.h"

#include <xmmintrin.h>

#include "geometry/JointTransform.h"
#include "math/MathX.h"

#define SHUFFLEPS( x, y, z, w )		(( (x) & 3 ) << 6 | ( (y) & 3 ) << 4 | ( (z) & 3 ) << 2 | ( (w) & 3 ))
#define R_SHUFFLEPS( x, y, z, w )	(( (w) & 3 ) << 6 | ( (z) & 3 ) << 4 | ( (y) & 3 ) << 2 | ( (x) & 3 ))
#define SHUFFLEPD( x, y )			(( (x) & 1 ) << 1 | ( (y) & 1 ))
#define R_SHUFFLEPD( x, y )			(( (y) & 1 ) << 1 | ( (x) & 1 ))

#define ALIGN4_INIT1( X, INIT )				ALIGN16( static X[4] ) = { INIT, INIT, INIT, INIT }
#define ALIGN4_INIT4( X, I0, I1, I2, I3 )	ALIGN16( static X[4] ) = { I0, I1, I2, I3 }
#define ALIGN8_INIT1( X, INIT )				ALIGN16( static X[8] ) = { INIT, INIT, INIT, INIT, INIT, INIT, INIT, INIT }

ALIGN8_INIT1(unsigned short SIMD_W_zero, 0);
ALIGN8_INIT1(unsigned short SIMD_W_maxShort, 1 << 15);

ALIGN4_INIT4(unsigned int SIMD_SP_singleSignBitMask, (unsigned int)(1 << 31), 0, 0, 0);
ALIGN4_INIT1(unsigned int SIMD_SP_signBitMask, (unsigned int)(1 << 31));
ALIGN4_INIT1(unsigned int SIMD_SP_absMask, (unsigned int)~(1 << 31));
ALIGN4_INIT1(unsigned int SIMD_SP_infinityMask, (unsigned int)~(1 << 23));

ALIGN4_INIT1(float SIMD_SP_zero, 0.0f);
ALIGN4_INIT1(float SIMD_SP_one, 1.0f);
ALIGN4_INIT1(float SIMD_SP_two, 2.0f);
ALIGN4_INIT1(float SIMD_SP_three, 3.0f);
ALIGN4_INIT1(float SIMD_SP_four, 4.0f);
ALIGN4_INIT1(float SIMD_SP_maxShort, (1 << 15));
ALIGN4_INIT1(float SIMD_SP_tiny, 1e-10f);
ALIGN4_INIT1(float SIMD_SP_PI, MathX::PI);
ALIGN4_INIT1(float SIMD_SP_halfPI, MathX::HALF_PI);
ALIGN4_INIT1(float SIMD_SP_twoPI, MathX::TWO_PI);
ALIGN4_INIT1(float SIMD_SP_oneOverTwoPI, 1.0f / MathX::TWO_PI);
ALIGN4_INIT1(float SIMD_SP_infinity, MathX::INFINITY);

const char* VPCALL SIMD_SSE2_GetName(void) {
	return "MMX & SSE & SSE2";
}

void VPCALL SIMD_SSE2_MixedSoundToSamples(short* samples, const float* mixBuffer, const int numSamples) {

	assert((numSamples % MIXBUFFER_SAMPLES) == 0);

	__asm {

		mov			eax, numSamples
		mov			edi, mixBuffer
		mov			esi, samples
		shl			eax, 2
		add			edi, eax
		neg			eax

		loop16 :

		movaps		xmm0, [edi + eax + 0 * 16]
			movaps		xmm1, [edi + eax + 1 * 16]
			movaps		xmm2, [edi + eax + 2 * 16]
			movaps		xmm3, [edi + eax + 3 * 16]

			add			esi, 4 * 4 * 2

			cvtps2dq	xmm4, xmm0
			cvtps2dq	xmm5, xmm1
			cvtps2dq	xmm6, xmm2
			cvtps2dq	xmm7, xmm3

			prefetchnta[edi + eax + 128]

			packssdw	xmm4, xmm5
			packssdw	xmm6, xmm7

			add			eax, 4 * 16

			movlps[esi - 4 * 4 * 2], xmm4		// FIXME: should not use movlps/movhps to move integer data
			movhps[esi - 3 * 4 * 2], xmm4
			movlps[esi - 2 * 4 * 2], xmm6
			movhps[esi - 1 * 4 * 2], xmm6

			jl			loop16
	}
}

#endif
