#include <float.h>
#include "Cpu.h"

// MSVC header intrin.h uses strcmp and errors out when not set
#define IDSTR_NO_REDIRECT
#include "Platform.h"

#ifdef NO_CPUID
#undef NO_CPUID
#endif

#if defined(__GNUC__)
#if !defined(__i386__) && !defined(__x86_64__)
#define NO_CPUID
#endif
#elif defined(_MSC_VER)
//#if !defined(_M_IX86)
//#define NO_CPUID
//#endif
#else
#error unsupported compiler
#endif

#ifdef NO_CPUID
void FPU_SetDAZ(bool enable) {}

void FPU_SetFTZ(bool enable) {}
#else

#if defined(__GNUC__)
static inline void CPUid(int index, int* a, int* b, int* c, int* d) {
#if __x86_64__
#define REG_b "rbx"
#define REG_S "rsi"
#elif __i386__
#define REG_b "ebx"
#define REG_S "esi"
#endif
	* a = *b = *c = *d = 0;

	__asm__ volatile
		("mov %%" REG_b ", %%" REG_S "\n\t"
			"cpuid\n\t"
			"xchg %%" REG_b ", %%" REG_S
			:"=a" (*a), "=S" (*b),
			"=c" (*c), "=d" (*d)
			: "0" (index));
}
#elif defined(_MSC_VER)
#include <intrin.h>
static inline void CPUid(int index, int* a, int* b, int* c, int* d) {
	int info[4] = { };
	__cpuid(info, index);
	*a = info[0];
	*b = info[1];
	*c = info[2];
	*d = info[3];
}
#else
#error unsupported compiler
#endif

inline void Printf(const char* fmt) { printf(fmt); }
inline void Printf(const char* fmt, const char* arg1) { printf(fmt, arg1); }

#define c_SSE3		(1 << 0)
#define d_FXSAVE	(1 << 24)

static inline bool HasDAZ() {
	int a, b, c, d;
	CPUid(0, &a, &b, &c, &d);
	if (a < 1) return false;
	CPUid(1, &a, &b, &c, &d);
	return (d & d_FXSAVE) == d_FXSAVE;
}

static inline bool HasSSE3() {
	int a, b, c, d;
	CPUid(0, &a, &b, &c, &d);
	if (a < 1) return false;
	CPUid(1, &a, &b, &c, &d);
	return (c & c_SSE3) == c_SSE3;
}

#define MXCSR_DAZ (1 << 6)
#define MXCSR_FTZ (1 << 15)

//:ref http://www.info.univ-angers.fr/~richer/ens/l3info/ao/intel_intrinsics.pdf
static void EnableMXCSRFlag(int flag, bool enable, const char* name) {
	int sse_mode = _mm_getcsr();
	if (enable && (sse_mode & flag) == flag) { Printf("%s mode is already enabled\n", name); return; }
	if (!enable && (sse_mode & flag) == 0) { Printf("%s mode is already disabled\n", name); return; }
	if (enable) { Printf("enabling %s mode\n", name); sse_mode |= flag; }
	else { Printf("disabling %s mode\n", name); sse_mode &= ~flag; }
	_mm_setcsr(sse_mode);
}

void FPU_SetDAZ(bool enable) {
	if (!HasDAZ()) { Printf("this CPU doesn't support Denormals-Are-Zero\n"); return; }
	EnableMXCSRFlag(MXCSR_DAZ, enable, "Denormals-Are-Zero");
}

void FPU_SetFTZ(bool enable) {
	EnableMXCSRFlag(MXCSR_FTZ, enable, "Flush-To-Zero");
}
#endif

int GetProcessorId(void) {
	int flags = CPUID_GENERIC;
	if (CPU_HasMMX()) flags |= CPUID_MMX;
	if (CPU_Has3DNow()) flags |= CPUID_3DNOW;
	if (CPU_HasSSE()) flags |= CPUID_SSE;
	if (CPU_HasSSE2()) flags |= CPUID_SSE2;
#ifndef NO_CPUID
	// there is no SDL_HasSSE3() in SDL 1.2
	if (HasSSE3()) flags |= CPUID_SSE3;
#endif
	if (CPU_HasAltiVec()) flags |= CPUID_ALTIVEC;
	return flags;
}

void FPU_SetPrecision() {
#if defined(_MSC_VER)
	_controlfp(_PC_64, _MCW_PC);
#endif
}
