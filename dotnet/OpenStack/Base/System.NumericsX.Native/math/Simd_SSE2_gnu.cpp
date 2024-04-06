#if defined(__GNUC__) && defined(__SSE2__)
#include "system/Platform.h"
#include "Simd_SSE2.h"

#include <emmintrin.h>

#define SHUFFLEPS( x, y, z, w )		(( (x) & 3 ) << 6 | ( (y) & 3 ) << 4 | ( (z) & 3 ) << 2 | ( (w) & 3 ))
#define R_SHUFFLEPS( x, y, z, w )	(( (w) & 3 ) << 6 | ( (z) & 3 ) << 4 | ( (y) & 3 ) << 2 | ( (x) & 3 ))

const char* SIMD_SSE2_GetName(void) const {
	return "MMX & SSE & SSE2";
}

// dst[i] |= ( src0[i] < constant ) << bitNum;
void VPCALL SIMD_SSE2_CmpLT(byte* dst, const byte bitNum, const float* src0, const float constant, const int count) {
	int i, cnt, pre, post;
	float* aligned;
	__m128 xmm0, xmm1;
	__m128i xmm0i;
	int cnt_l;
	char* src0_p;
	char* constant_p;
	char* dst_p;
	int mask_l;
	int dst_l;

	/* if the float array is not aligned on a 4 byte boundary */
	if (ptrdiff_t(src0) & 3) {
		/* unaligned memory access */
		pre = 0;
		cnt = count >> 2;
		post = count - (cnt << 2);

		/*
			__asm	mov			edx, cnt
			__asm	test		edx, edx
			__asm	je			doneCmp
		*/
		cnt_l = cnt;
		if (cnt_l != 0) {
			/*
				__asm	push		ebx
				__asm	neg			edx
				__asm	mov			esi, src0
				__asm	prefetchnta	[esi+64]
				__asm	movss		xmm1, constant
				__asm	shufps		xmm1, xmm1, R_SHUFFLEPS( 0, 0, 0, 0 )
				__asm	mov			edi, dst
				__asm	mov			cl, bitNum
			*/
			cnt_l = -cnt_l;
			src0_p = (char*)src0;
			_mm_prefetch(src0_p + 64, _MM_HINT_NTA);
			constant_p = (char*)&constant;
			xmm1 = _mm_load_ss((float*)constant_p);
			xmm1 = _mm_shuffle_ps(xmm1, xmm1, R_SHUFFLEPS(0, 0, 0, 0));
			dst_p = (char*)dst;
			/*
					__asm loopNA:
			*/
			do {
				/*
					__asm	movups		xmm0, [esi]
					__asm	prefetchnta	[esi+128]
					__asm	cmpltps		xmm0, xmm1
					__asm	movmskps	eax, xmm0																												\
					__asm	mov			ah, al
					__asm	shr			ah, 1
					__asm	mov			bx, ax
					__asm	shl			ebx, 14
					__asm	mov			bx, ax
					__asm	and			ebx, 0x01010101
					__asm	shl			ebx, cl
					__asm	or			ebx, dword ptr [edi]
					__asm	mov			dword ptr [edi], ebx
					__asm	add			esi, 16
					__asm	add			edi, 4
					__asm	inc			edx
					__asm	jl			loopNA
					__asm	pop			ebx
				*/
				xmm0 = _mm_loadu_ps((float*)src0_p);
				_mm_prefetch(src0_p + 128, _MM_HINT_NTA);
				xmm0 = _mm_cmplt_ps(xmm0, xmm1);
				// Simplify using SSE2
				xmm0i = (__m128i) xmm0;
				xmm0i = _mm_packs_epi32(xmm0i, xmm0i);
				xmm0i = _mm_packs_epi16(xmm0i, xmm0i);
				mask_l = _mm_cvtsi128_si32(xmm0i);
				// End
				mask_l = mask_l & 0x01010101;
				mask_l = mask_l << bitNum;
				dst_l = *((int*)dst_p);
				mask_l = mask_l | dst_l;
				*((int*)dst_p) = mask_l;
				src0_p = src0_p + 16;
				dst_p = dst_p + 4;
				cnt_l = cnt_l + 1;
			} while (cnt_l < 0);
		}
	}
	else {
		/* aligned memory access */
		aligned = (float*)((ptrdiff_t(src0) + 15) & ~15);
		if (ptrdiff_t(aligned) > ptrdiff_t(src0) + count) {
			pre = count;
			post = 0;
		}
		else {
			pre = aligned - src0;
			cnt = (count - pre) >> 2;
			post = count - pre - (cnt << 2);
			/*
					__asm	mov			edx, cnt
					__asm	test		edx, edx
					__asm	je			doneCmp
			*/
			cnt_l = cnt;
			if (cnt_l != 0) {
				/*
						__asm	push		ebx
						__asm	neg			edx
						__asm	mov			esi, aligned
						__asm	prefetchnta	[esi+64]
						__asm	movss		xmm1, constant
						__asm	shufps		xmm1, xmm1, R_SHUFFLEPS( 0, 0, 0, 0 )
						__asm	mov			edi, dst
						__asm	add			edi, pre
						__asm	mov			cl, bitNum
				*/
				cnt_l = -cnt_l;
				src0_p = (char*)src0;
				_mm_prefetch(src0_p + 64, _MM_HINT_NTA);
				constant_p = (char*)&constant;
				xmm1 = _mm_load_ss((float*)constant_p);
				xmm1 = _mm_shuffle_ps(xmm1, xmm1, R_SHUFFLEPS(0, 0, 0, 0));
				dst_p = (char*)dst;
				dst_p = dst_p + pre;
				/*
						__asm loopA:
				*/
				do {
					/*
							__asm	movaps		xmm0, [esi]
							__asm	prefetchnta	[esi+128]
							__asm	cmpltps		xmm0, xmm1
							__asm	movmskps	eax, xmm0																											\
							__asm	mov			ah, al
							__asm	shr			ah, 1
							__asm	mov			bx, ax
							__asm	shl			ebx, 14
							__asm	mov			bx, ax
							__asm	and			ebx, 0x01010101
							__asm	shl			ebx, cl
							__asm	or			ebx, dword ptr [edi]
							__asm	mov			dword ptr [edi], ebx
							__asm	add			esi, 16
							__asm	add			edi, 4
							__asm	inc			edx
							__asm	jl			loopA
							__asm	pop			ebx
					*/
					xmm0 = _mm_load_ps((float*)src0_p);
					_mm_prefetch(src0_p + 128, _MM_HINT_NTA);
					xmm0 = _mm_cmplt_ps(xmm0, xmm1);
					// Simplify using SSE2
					xmm0i = (__m128i) xmm0;
					xmm0i = _mm_packs_epi32(xmm0i, xmm0i);
					xmm0i = _mm_packs_epi16(xmm0i, xmm0i);
					mask_l = _mm_cvtsi128_si32(xmm0i);
					// End
					mask_l = mask_l & 0x01010101;
					mask_l = mask_l << bitNum;
					dst_l = *((int*)dst_p);
					mask_l = mask_l | dst_l;
					*((int*)dst_p) = mask_l;
					src0_p = src0_p + 16;
					dst_p = dst_p + 4;
					cnt_l = cnt_l + 1;
				} while (cnt_l < 0);
			}
		}
	}
	/*
	doneCmp:
	*/
	float c = constant;
	for (i = 0; i < pre; i++) {
		dst[i] |= (src0[i] < c) << bitNum;
	}
	for (i = count - post; i < count; i++) {
		dst[i] |= (src0[i] < c) << bitNum;
	}
}


#endif
