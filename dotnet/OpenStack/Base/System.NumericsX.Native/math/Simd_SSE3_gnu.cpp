#if defined(__GNUC__) && defined(__SSE3__)
#include "system/Platform.h"
#include "Simd_SSE3.h"

const char* VPCALL SIMD_SSE3_GetName(void) {
	return "MMX & SSE & SSE2 & SSE3";
}

#endif
