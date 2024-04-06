#if defined(_MSC_VER)
#include <intrin.h>
#include "system/Platform.h"
#include "Simd_MMX.h"

const char* VPCALL SIMD_MMX_GetName(void) {
	return "MMX";
}

extern "C" void MMX_Memcpy8B(void* dest, const void* src, const int count);
extern "C" void MMX_Memcpy64B(void* dest, const void* src, const int count); // 165MB/sec
extern "C" void MMX_Memcpy2kB(void* dest, const void* src, const int count); // 240MB/sec

// optimized memory copy routine that handles all alignment cases and block sizes efficiently
void VPCALL SIMD_MMX_Memcpy(void* dest0, const void* src0, const int count0) {
	// if copying more than 16 bytes and we can copy 8 byte aligned
	if (count0 > 16 && !(((int)dest0 ^ (int)src0) & 7)) {
		byte* dest = (byte*)dest0;
		byte* src = (byte*)src0;

		// copy up to the first 8 byte aligned boundary
		int count = ((int)dest) & 7;
		memcpy(dest, src, count);
		dest += count;
		src += count;
		count = count0 - count;

		// if there are multiple blocks of 2kB
		if (count & ~4095) {
			MMX_Memcpy2kB(dest, src, count);
			src += (count & ~2047);
			dest += (count & ~2047);
			count &= 2047;
		}

		// if there are blocks of 64 bytes
		if (count & ~63) {
			MMX_Memcpy64B(dest, src, count);
			src += (count & ~63);
			dest += (count & ~63);
			count &= 63;
		}

		// if there are blocks of 8 bytes
		if (count & ~7) {
			MMX_Memcpy8B(dest, src, count);
			src += (count & ~7);
			dest += (count & ~7);
			count &= 7;
		}

		// copy any remaining bytes
		memcpy(dest, src, count);
	}
	// use the regular one if we cannot copy 8 byte aligned
	else memcpy(dest0, src0, count0);

	// the MMX_Memcpy* functions use MOVNTQ, issue a fence operation
	_mm_sfence(); //__asm { sfence }
}

extern "C" void MMX_Memset8B(void* dest, const int val, const int count);
extern "C" void MMX_Memset64B(void* dest, const int val, const int count);

void VPCALL SIMD_MMX_Memset(void* dest0, const int val, const int count0) {
	union {
		byte	bytes[8];
		word	words[4];
		dword	dwords[2];
	} dat;

	byte* dest = (byte*)dest0;
	int count = count0;

	while (count > 0 && (((int)dest) & 7)) {
		*dest = val;
		dest++;
		count--;
	}
	if (!count) return;

	dat.bytes[0] = val;
	dat.bytes[1] = val;
	dat.words[1] = dat.words[0];
	dat.dwords[1] = dat.dwords[0];

	if (count >= 64) {
		MMX_Memset64B(dest, val, count);
		dest += (count & ~63);
		count &= 63;
	}

	if (count >= 8) {
		MMX_Memset8B(dest, val, count);
		dest += (count & ~7);
		count &= 7;
	}

	while (count > 0) {
		*dest = val;
		dest++;
		count--;
	}

#ifdef _M_IX86
	_mm_empty();
#endif

	// the MMX_Memcpy* functions use MOVNTQ, issue a fence operation
	_mm_sfence();
}

#endif
