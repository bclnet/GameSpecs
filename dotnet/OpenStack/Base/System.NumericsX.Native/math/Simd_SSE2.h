#ifndef __SIMD_SSE2_H__
#define __SIMD_SSE2_H__

#include "Simd_SSE.h"

#if defined(__GNUC__) && defined(__SSE2__)
const char* VPCALL SIMD_SSE2_GetName(void);
void VPCALL SIMD_SSE2_CmpLT(byte* dst, const byte bitNum, const float* src0, const float constant, const int count);

#elif defined(_MSC_VER)
const char* VPCALL SIMD_SSE2_GetName(void);

void VPCALL SIMD_SSE2_MixedSoundToSamples(short* samples, const float* mixBuffer, const int numSamples);
#endif

#endif
