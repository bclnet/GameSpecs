#ifndef __PLATFORM_H__
#define __PLATFORM_H__

// Win32
#if defined(WIN32) || defined(_WIN32)

#define _alloca16(x)				((void *)((((uintptr_t)_alloca( (x)+15 )) + 15) & ~15))

#ifdef _MSC_VER
#define ALIGN16(x)					__declspec(align(16)) x
#define PACKED
#define ID_INLINE					__forceinline
#else
#define ALIGN16(x)					x __attribute__ ((aligned (16)))
#define PACKED						__attribute__((packed))
#define ID_INLINE					inline
#endif

#endif

// Mac OSX
#if defined(MACOS_X) || defined(__APPLE__)

#define _alloca						alloca
#define _alloca16(x)				((void *)((((uintptr_t)alloca( (x)+15 )) + 15) & ~15))

#define ALIGN16(x)					__declspec(align(16)) x
#define PACKED
#define ID_INLINE					inline

#endif

// Unix
#ifdef __unix__

#define _alloca(x)					(({assert( (x)<600000 );}), alloca( (x) ))
#define _alloca16(x)				(({assert( (x)<600000 );}),((void *)((((uintptr_t)alloca( (x)+15 )) + 15) & ~15)))

#define ALIGN16(x)					x
#define PACKED						__attribute__((packed))
#define ID_INLINE					inline

#endif

#if !defined(_MSC_VER)
// MSVC does not provide this C99 header
#include <inttypes.h>
#endif
//#if defined(__MINGW32__)
#include <malloc.h>
//#endif
#include <stdio.h>
//#include <stdlib.h>
//#include <stdarg.h>
//#include <string.h>
#include <assert.h>
//#include <time.h>
//#include <ctype.h>
//#include <cstddef>
//#include <typeinfo>
//#include <errno.h>
#include <math.h>

#ifdef _WIN32
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#undef FindText								// stupid namespace poluting Microsoft monkeys
#endif

typedef unsigned char			byte;		// 8 bits
typedef unsigned short			word;		// 16 bits
typedef unsigned int			dword;		// 32 bits
typedef unsigned int			uint;
typedef unsigned long			ulong;

#ifndef NULL
#define NULL					((void *)0)
#endif

// PLATFORM

typedef enum {
	CPUID_NONE = 0x00000,
	CPUID_UNSUPPORTED = 0x00001,	// unsupported (386/486)
	CPUID_GENERIC = 0x00002,	// unrecognized processor
	CPUID_MMX = 0x00010,	// Multi Media Extensions
	CPUID_3DNOW = 0x00020,	// 3DNow!
	CPUID_SSE = 0x00040,	// Streaming SIMD Extensions
	CPUID_SSE2 = 0x00080,	// Streaming SIMD Extensions 2
	CPUID_SSE3 = 0x00100,	// Streaming SIMD Extentions 3 aka Prescott's New Instructions
	CPUID_ALTIVEC = 0x00200	// AltiVec
} CPUID;

// returns a selection of the CPUID_* flags
extern int GetProcessorId(void);

// sets the FPU precision
extern void FPU_SetPrecision();

// sets Flush-To-Zero mode
extern void FPU_SetFTZ(bool enable);

// sets Denormals-Are-Zero mode
extern void FPU_SetDAZ(bool enable);

#endif
