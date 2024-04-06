#if defined(__GNUC__) && defined(__MMX__)
#include "system/Platform.h"
#include "Simd_MMX.h"

const char* SIMD_MMX_GetName(void) const {
	return "MMX";
}

#endif
