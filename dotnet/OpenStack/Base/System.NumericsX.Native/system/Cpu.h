#ifndef _CPU_H
#define _CPU_H

#if defined(_MSC_VER) && (_MSC_VER >= 1500) && (defined(_M_IX86) || defined(_M_X64))
#include <intrin.h>
#ifndef _WIN64
#define __MMX__
#define __3DNOW__
#endif
#define __SSE__
#define __SSE2__
#elif defined(__MINGW64_VERSION_MAJOR)
#include <intrin.h>
#else
#ifdef __ALTIVEC__
#if HAVE_ALTIVEC_H && !defined(__APPLE_ALTIVEC__)
#include <altivec.h>
#undef pixel
#endif
#endif
#ifdef __MMX__
#include <mmintrin.h>
#endif
#ifdef __3DNOW__
#include <mm3dnow.h>
#endif
#ifdef __SSE__
#include <xmmintrin.h>
#endif
#ifdef __SSE2__
#include <emmintrin.h>
#endif
#endif

/* This is a guess for the cacheline size used for padding. Most x86 processors have a 64 byte cache line.
 * The 64-bit PowerPC processors have a 128 byte cache line. We'll use the larger value to be generally safe.
 */
#define CPU_CACHELINE_DEFAULTSIZE  128

 // This function returns the number of CPU cores available.
int CPU_GetCPUCount(void);

//  This function returns the L1 cache line size of the CPU
//
//  This is useful for determining multi-threaded structure padding or SIMD prefetch sizes.
int CPU_GetCPUCacheLineSize(void);

// This function returns true if the CPU has the RDTSC instruction.
bool CPU_HasRDTSC(void);

//  This function returns true if the CPU has AltiVec features.
bool CPU_HasAltiVec(void);

// This function returns true if the CPU has MMX features.
bool CPU_HasMMX(void);

// This function returns true if the CPU has 3DNow! features.
bool CPU_Has3DNow(void);

// This function returns true if the CPU has SSE features.
bool CPU_HasSSE(void);

// This function returns true if the CPU has SSE2 features.
bool CPU_HasSSE2(void);

// This function returns true if the CPU has SSE3 features.
bool CPU_HasSSE3(void);

// This function returns true if the CPU has SSE4.1 features.
bool CPU_HasSSE41(void);

// This function returns true if the CPU has SSE4.2 features.
bool CPU_HasSSE42(void);

// This function returns true if the CPU has AVX features.
bool CPU_HasAVX(void);

// This function returns true if the CPU has AVX2 features.
bool CPU_HasAVX2(void);

// This function returns the amount of RAM configured in the system, in MB.
int CPU_GetSystemRAM(void);

#endif
