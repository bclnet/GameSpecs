#ifndef __SIMD_MMX_H__
#define __SIMD_MMX_H__

#include "Simd_Generic.h"

#if defined(__GNUC__) && defined(__MMX__)
EXTERN const char* VPCALL SIMD_MMX_GetName(void);

#elif defined(_MSC_VER)
EXTERN const char* VPCALL SIMD_MMX_GetName(void);

EXTERN void VPCALL SIMD_MMX_Memcpy(void* dst, const void* src, const int count);
EXTERN void VPCALL SIMD_MMX_Memset(void* dst, const int val, const int count);

#endif

#endif
