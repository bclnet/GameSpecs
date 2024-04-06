#ifndef __SIMD_3DNOW_H__
#define __SIMD_3DNOW_H__

#include "Simd_MMX.h"

#if defined(_MSC_VER)
const char* VPCALL SIMD_3DNow_GetName(void);

void VPCALL SIMD_3DNow_Memcpy(void* dst, const void* src, const int count);

#endif

#endif
