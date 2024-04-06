//#define TEST_MAIN
#include <stdint.h>
#include <string.h>
#if defined(_WIN32)
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#endif
#ifdef HAVE_SYSCONF
#include <unistd.h>
#endif
#ifdef HAVE_SYSCTLBYNAME
#include <sys/types.h>
#include <sys/sysctl.h>
#endif
#if defined(__MACOSX__) && (defined(__ppc__) || defined(__ppc64__))
#include <sys/sysctl.h>         /* For AltiVec check */
#elif defined(__OpenBSD__) && defined(__powerpc__)
#include <sys/param.h>
#include <sys/sysctl.h> /* For AltiVec check */
#include <machine/cpu.h>
#elif CPU_ALTIVEC_BLITTERS && HAVE_SETJMP
#include <signal.h>
#include <setjmp.h>
#endif
#include "Cpu.h"

#define CPU_HAS_RDTSC   0x00000001
#define CPU_HAS_ALTIVEC 0x00000002
#define CPU_HAS_MMX     0x00000004
#define CPU_HAS_3DNOW   0x00000008
#define CPU_HAS_SSE     0x00000010
#define CPU_HAS_SSE2    0x00000020
#define CPU_HAS_SSE3    0x00000040
#define CPU_HAS_SSE41   0x00000100
#define CPU_HAS_SSE42   0x00000200
#define CPU_HAS_AVX     0x00000400
#define CPU_HAS_AVX2    0x00000800

#if CPU_ALTIVEC_BLITTERS && HAVE_SETJMP && !__MACOSX__ && !__OpenBSD__
static jmp_buf jmpbuf;
static void illegal_instruction(int sig) { longjmp(jmpbuf, 1); }
#endif

static int CPU_haveCPUID(void) {
	int has_CPUID = 0;
#ifndef CPU_CPUINFO_DISABLED
#if defined(__GNUC__) && defined(i386)
	__asm__(
		"        pushfl                      # Get original EFLAGS             \n"
		"        popl    %%eax                                                 \n"
		"        movl    %%eax,%%ecx                                           \n"
		"        xorl    $0x200000,%%eax     # Flip ID bit in EFLAGS           \n"
		"        pushl   %%eax               # Save new EFLAGS value on stack  \n"
		"        popfl                       # Replace current EFLAGS value    \n"
		"        pushfl                      # Get new EFLAGS                  \n"
		"        popl    %%eax               # Store new EFLAGS in EAX         \n"
		"        xorl    %%ecx,%%eax         # Can not toggle ID bit,          \n"
		"        jz      1f                  # Processor=80486                 \n"
		"        movl    $1,%0               # We have CPUID support           \n"
		"1:                                                                    \n"
		: "=m" (has_CPUID)
		:
		: "%eax", "%ecx"
	);
#elif defined(__GNUC__) && defined(__x86_64__)
	__asm__(
		"        pushfq                      # Get original EFLAGS             \n"
		"        popq    %%rax                                                 \n"
		"        movq    %%rax,%%rcx                                           \n"
		"        xorl    $0x200000,%%eax     # Flip ID bit in EFLAGS           \n"
		"        pushq   %%rax               # Save new EFLAGS value on stack  \n"
		"        popfq                       # Replace current EFLAGS value    \n"
		"        pushfq                      # Get new EFLAGS                  \n"
		"        popq    %%rax               # Store new EFLAGS in EAX         \n"
		"        xorl    %%ecx,%%eax         # Can not toggle ID bit,          \n"
		"        jz      1f                  # Processor=80486                 \n"
		"        movl    $1,%0               # We have CPUID support           \n"
		"1:                                                                    \n"
		: "=m" (has_CPUID)
		:
		: "%rax", "%rcx"
	);
#elif (defined(_MSC_VER) && defined(_M_IX86)) || defined(__WATCOMC__)
	__asm {
		pushfd; Get original EFLAGS
		pop     eax
		mov     ecx, eax
		xor eax, 200000h; Flip ID bit in EFLAGS
		push    eax; Save new EFLAGS value on stack
		popfd; Replace current EFLAGS value
		pushfd; Get new EFLAGS
		pop     eax; Store new EFLAGS in EAX
		xor eax, ecx; Can not toggle ID bit,
		jz      done; Processor = 80486
		mov     has_CPUID, 1; We have CPUID support
		done :
	}
#elif defined(_MSC_VER) && defined(_M_X64)
	has_CPUID = 1;
#elif defined(__sun) && defined(__i386)
	__asm (
	"       pushfl                 \n"
		"       popl    %eax           \n"
		"       movl    %eax,%ecx      \n"
		"       xorl    $0x200000,%eax \n"
		"       pushl   %eax           \n"
		"       popfl                  \n"
		"       pushfl                 \n"
		"       popl    %eax           \n"
		"       xorl    %ecx,%eax      \n"
		"       jz      1f             \n"
		"       movl    $1,-8(%ebp)    \n"
		"1:                            \n"
		);
#elif defined(__sun) && defined(__amd64)
	__asm (
	"       pushfq                 \n"
		"       popq    %rax           \n"
		"       movq    %rax,%rcx      \n"
		"       xorl    $0x200000,%eax \n"
		"       pushq   %rax           \n"
		"       popfq                  \n"
		"       pushfq                 \n"
		"       popq    %rax           \n"
		"       xorl    %ecx,%eax      \n"
		"       jz      1f             \n"
		"       movl    $1,-8(%rbp)    \n"
		"1:                            \n"
		);
#endif
#endif
	return has_CPUID;
}

#if defined(__GNUC__) && defined(i386)
#define cpuid(func, a, b, c, d) \
    __asm__ __volatile__ ( \
"        pushl %%ebx        \n" \
"        xorl %%ecx,%%ecx   \n" \
"        cpuid              \n" \
"        movl %%ebx, %%esi  \n" \
"        popl %%ebx         \n" : \
            "=a" (a), "=S" (b), "=c" (c), "=d" (d) : "a" (func))
#elif defined(__GNUC__) && defined(__x86_64__)
#define cpuid(func, a, b, c, d) \
    __asm__ __volatile__ ( \
"        pushq %%rbx        \n" \
"        xorq %%rcx,%%rcx   \n" \
"        cpuid              \n" \
"        movq %%rbx, %%rsi  \n" \
"        popq %%rbx         \n" : \
            "=a" (a), "=S" (b), "=c" (c), "=d" (d) : "a" (func))
#elif (defined(_MSC_VER) && defined(_M_IX86)) || defined(__WATCOMC__)
#define cpuid(func, a, b, c, d) \
    __asm { \
        __asm mov eax, func \
        __asm xor ecx, ecx \
        __asm cpuid \
        __asm mov a, eax \
        __asm mov b, ebx \
        __asm mov c, ecx \
        __asm mov d, edx \
}
#elif defined(_MSC_VER) && defined(_M_X64)
#define cpuid(func, a, b, c, d) \
{ \
    int CPUInfo[4]; \
    __cpuid(CPUInfo, func); \
    a = CPUInfo[0]; \
    b = CPUInfo[1]; \
    c = CPUInfo[2]; \
    d = CPUInfo[3]; \
}
#else
#define cpuid(func, a, b, c, d) \
    a = b = c = d = 0
#endif

static int CPU_getCPUIDFeatures(void) {
	int features = 0; int a, b, c, d;
	cpuid(0, a, b, c, d);
	if (a >= 1) { cpuid(1, a, b, c, d); features = d; }
	return features;
}

static bool CPU_OSSavesYMM(void) {
	int a, b, c, d;
	// Check to make sure we can call xgetbv
	cpuid(0, a, b, c, d);
	if (a < 1) return false;
	cpuid(1, a, b, c, d);
	if (!(c & 0x08000000)) return false;

	// Call xgetbv to see if YMM register state is saved
	a = 0;
#if defined(__GNUC__) && (defined(i386) || defined(__x86_64__))
	asm(".byte 0x0f, 0x01, 0xd0" : "=a" (a) : "c" (0) : "%edx");
#elif defined(_MSC_VER) && (defined(_M_IX86) || defined(_M_X64)) && (_MSC_FULL_VER >= 160040219)
	a = (int)_xgetbv(0);
#elif (defined(_MSC_VER) && defined(_M_IX86)) || defined(__WATCOMC__)
	__asm
	{
		xor ecx, ecx
		_asm _emit 0x0f _asm _emit 0x01 _asm _emit 0xd0
		mov a, eax
	}
#endif
	return (a & 6) == 6;
}

static bool CPU_haveRDTSC(void) { return CPU_haveCPUID() && (CPU_getCPUIDFeatures() & 0x00000010); }

static bool CPU_haveAltiVec(void) {
	volatile bool altivec = false;
#ifndef CPU_CPUINFO_DISABLED
#if (defined(__MACOSX__) && (defined(__ppc__) || defined(__ppc64__))) || (defined(__OpenBSD__) && defined(__powerpc__))
#ifdef __OpenBSD__
	int selectors[2] = { CTL_MACHDEP, CPU_ALTIVEC };
#else
	int selectors[2] = { CTL_HW, HW_VECTORUNIT };
#endif
	int hasVectorUnit = 0;
	size_t length = sizeof(hasVectorUnit);
	int error = sysctl(selectors, 2, &hasVectorUnit, &length, NULL, 0);
	if (error == 0) altivec = (hasVectorUnit != 0);
#elif CPU_ALTIVEC_BLITTERS && HAVE_SETJMP
	void (*handler) (int sig);
	handler = signal(SIGILL, illegal_instruction);
	if (setjmp(jmpbuf) == 0) { asm volatile ("mtspr 256, %0\n\t" "vand %%v0, %%v0, %%v0"::"r" (-1)); altivec = true; }
	signal(SIGILL, handler);
#endif
#endif
	return altivec;
}

static bool CPU_haveMMX(void) { return CPU_haveCPUID() && (CPU_getCPUIDFeatures() & 0x00800000); }

static bool CPU_have3DNow(void) {
	if (CPU_haveCPUID()) {
		int a, b, c, d;
		cpuid(0x80000000, a, b, c, d);
		if (a >= 0x80000001) { cpuid(0x80000001, a, b, c, d); return (d & 0x80000000); }
	}
	return false;
}

static bool CPU_haveSSE(void) { return CPU_haveCPUID() && (CPU_getCPUIDFeatures() & 0x02000000); }

static bool CPU_haveSSE2(void) { return CPU_haveCPUID() && (CPU_getCPUIDFeatures() & 0x04000000); }

static bool CPU_haveSSE3(void) {
	if (CPU_haveCPUID()) {
		int a, b, c, d;
		cpuid(0, a, b, c, d);
		if (a >= 1) { cpuid(1, a, b, c, d); return (c & 0x00000001); }
	}
	return false;
}

static bool CPU_haveSSE41(void) {
	if (CPU_haveCPUID()) {
		int a, b, c, d;
		cpuid(0, a, b, c, d);
		if (a >= 1) { cpuid(1, a, b, c, d); return (c & 0x00080000); }
	}
	return false;
}

static bool CPU_haveSSE42(void) {
	if (CPU_haveCPUID()) {
		int a, b, c, d;
		cpuid(0, a, b, c, d);
		if (a >= 1) { cpuid(1, a, b, c, d); return (c & 0x00100000); }
	}
	return false;
}

static bool CPU_haveAVX(void) {
	if (CPU_haveCPUID() && CPU_OSSavesYMM()) {
		int a, b, c, d;
		cpuid(0, a, b, c, d);
		if (a >= 1) { cpuid(1, a, b, c, d); return (c & 0x10000000); }
	}
	return false;
}

static bool CPU_haveAVX2(void) {
	if (CPU_haveCPUID() && CPU_OSSavesYMM()) {
		int a, b, c, d;
		cpuid(0, a, b, c, d);
		if (a >= 7) { cpuid(7, a, b, c, d); return (b & 0x00000020); }
	}
	return false;
}

static int CPU_CPUCount = 0;

int CPU_GetCPUCount(void) {
	if (!CPU_CPUCount) {
#ifndef CPU_CPUINFO_DISABLED
#if defined(HAVE_SYSCONF) && defined(_SC_NPROCESSORS_ONLN)
		if (CPU_CPUCount <= 0) CPU_CPUCount = (int)sysconf(_SC_NPROCESSORS_ONLN);
#endif
#ifdef HAVE_SYSCTLBYNAME
		if (CPU_CPUCount <= 0) { size_t size = sizeof(CPU_CPUCount); sysctlbyname("hw.ncpu", &CPU_CPUCount, &size, NULL, 0); }
#endif
#ifdef __WIN32__
		if (CPU_CPUCount <= 0) { SYSTEM_INFO info; GetSystemInfo(&info); CPU_CPUCount = info.dwNumberOfProcessors; }
#endif
#endif
		if (CPU_CPUCount <= 0)  CPU_CPUCount = 1;
	}
	return CPU_CPUCount;
}

static const char* CPU_GetCPUType(void) {
	static char CPU_CPUType[13];

	if (!CPU_CPUType[0]) {
		int i = 0;

		if (CPU_haveCPUID()) {
			int a, b, c, d;
			cpuid(0x00000000, a, b, c, d);
			(void)a;
			CPU_CPUType[i++] = (char)(b & 0xff); b >>= 8;
			CPU_CPUType[i++] = (char)(b & 0xff); b >>= 8;
			CPU_CPUType[i++] = (char)(b & 0xff); b >>= 8;
			CPU_CPUType[i++] = (char)(b & 0xff);

			CPU_CPUType[i++] = (char)(d & 0xff); d >>= 8;
			CPU_CPUType[i++] = (char)(d & 0xff); d >>= 8;
			CPU_CPUType[i++] = (char)(d & 0xff); d >>= 8;
			CPU_CPUType[i++] = (char)(d & 0xff);

			CPU_CPUType[i++] = (char)(c & 0xff); c >>= 8;
			CPU_CPUType[i++] = (char)(c & 0xff); c >>= 8;
			CPU_CPUType[i++] = (char)(c & 0xff); c >>= 8;
			CPU_CPUType[i++] = (char)(c & 0xff);
		}
		if (!CPU_CPUType[0]) strncpy(CPU_CPUType, "Unknown", sizeof(CPU_CPUType));
	}
	return CPU_CPUType;
}

#ifdef TEST_MAIN
static const char* CPU_GetCPUName(void) {
	static char CPU_CPUName[48];

	if (!CPU_CPUName[0]) {
		int i = 0;
		int a, b, c, d;

		if (CPU_haveCPUID()) {
			cpuid(0x80000000, a, b, c, d);
			if (a >= 0x80000004) {
				cpuid(0x80000002, a, b, c, d);
				CPU_CPUName[i++] = (char)(a & 0xff); a >>= 8;
				CPU_CPUName[i++] = (char)(a & 0xff); a >>= 8;
				CPU_CPUName[i++] = (char)(a & 0xff); a >>= 8;
				CPU_CPUName[i++] = (char)(a & 0xff); a >>= 8;
				CPU_CPUName[i++] = (char)(b & 0xff); b >>= 8;
				CPU_CPUName[i++] = (char)(b & 0xff); b >>= 8;
				CPU_CPUName[i++] = (char)(b & 0xff); b >>= 8;
				CPU_CPUName[i++] = (char)(b & 0xff); b >>= 8;
				CPU_CPUName[i++] = (char)(c & 0xff); c >>= 8;
				CPU_CPUName[i++] = (char)(c & 0xff); c >>= 8;
				CPU_CPUName[i++] = (char)(c & 0xff); c >>= 8;
				CPU_CPUName[i++] = (char)(c & 0xff); c >>= 8;
				CPU_CPUName[i++] = (char)(d & 0xff); d >>= 8;
				CPU_CPUName[i++] = (char)(d & 0xff); d >>= 8;
				CPU_CPUName[i++] = (char)(d & 0xff); d >>= 8;
				CPU_CPUName[i++] = (char)(d & 0xff); d >>= 8;
				cpuid(0x80000003, a, b, c, d);
				CPU_CPUName[i++] = (char)(a & 0xff); a >>= 8;
				CPU_CPUName[i++] = (char)(a & 0xff); a >>= 8;
				CPU_CPUName[i++] = (char)(a & 0xff); a >>= 8;
				CPU_CPUName[i++] = (char)(a & 0xff); a >>= 8;
				CPU_CPUName[i++] = (char)(b & 0xff); b >>= 8;
				CPU_CPUName[i++] = (char)(b & 0xff); b >>= 8;
				CPU_CPUName[i++] = (char)(b & 0xff); b >>= 8;
				CPU_CPUName[i++] = (char)(b & 0xff); b >>= 8;
				CPU_CPUName[i++] = (char)(c & 0xff); c >>= 8;
				CPU_CPUName[i++] = (char)(c & 0xff); c >>= 8;
				CPU_CPUName[i++] = (char)(c & 0xff); c >>= 8;
				CPU_CPUName[i++] = (char)(c & 0xff); c >>= 8;
				CPU_CPUName[i++] = (char)(d & 0xff); d >>= 8;
				CPU_CPUName[i++] = (char)(d & 0xff); d >>= 8;
				CPU_CPUName[i++] = (char)(d & 0xff); d >>= 8;
				CPU_CPUName[i++] = (char)(d & 0xff); d >>= 8;
				cpuid(0x80000004, a, b, c, d);
				CPU_CPUName[i++] = (char)(a & 0xff); a >>= 8;
				CPU_CPUName[i++] = (char)(a & 0xff); a >>= 8;
				CPU_CPUName[i++] = (char)(a & 0xff); a >>= 8;
				CPU_CPUName[i++] = (char)(a & 0xff); a >>= 8;
				CPU_CPUName[i++] = (char)(b & 0xff); b >>= 8;
				CPU_CPUName[i++] = (char)(b & 0xff); b >>= 8;
				CPU_CPUName[i++] = (char)(b & 0xff); b >>= 8;
				CPU_CPUName[i++] = (char)(b & 0xff); b >>= 8;
				CPU_CPUName[i++] = (char)(c & 0xff); c >>= 8;
				CPU_CPUName[i++] = (char)(c & 0xff); c >>= 8;
				CPU_CPUName[i++] = (char)(c & 0xff); c >>= 8;
				CPU_CPUName[i++] = (char)(c & 0xff); c >>= 8;
				CPU_CPUName[i++] = (char)(d & 0xff); d >>= 8;
				CPU_CPUName[i++] = (char)(d & 0xff); d >>= 8;
				CPU_CPUName[i++] = (char)(d & 0xff); d >>= 8;
				CPU_CPUName[i++] = (char)(d & 0xff); d >>= 8;
			}
		}
		if (!CPU_CPUName[0]) strncpy(CPU_CPUName, "Unknown", sizeof(CPU_CPUName));
	}
	return CPU_CPUName;
}
#endif

int CPU_GetCPUCacheLineSize(void) {
	const char* cpuType = CPU_GetCPUType();
	int a, b, c, d;
	(void)a; (void)b; (void)c; (void)d;
	if (strcmp(cpuType, "GenuineIntel") == 0) {
		cpuid(0x00000001, a, b, c, d);
		return (((b >> 8) & 0xff) * 8);
	}
	else if (strcmp(cpuType, "AuthenticAMD") == 0) {
		cpuid(0x80000005, a, b, c, d);
		return (c & 0xff);
	}
	else return CPU_CACHELINE_DEFAULTSIZE;
}

static uint32_t CPU_CPUFeatures = 0xFFFFFFFF;

static uint32_t CPU_GetCPUFeatures(void) {
	if (CPU_CPUFeatures == 0xFFFFFFFF) {
		CPU_CPUFeatures = 0;
		if (CPU_haveRDTSC()) CPU_CPUFeatures |= CPU_HAS_RDTSC;
		if (CPU_haveAltiVec()) CPU_CPUFeatures |= CPU_HAS_ALTIVEC;
		if (CPU_haveMMX()) CPU_CPUFeatures |= CPU_HAS_MMX;
		if (CPU_have3DNow()) CPU_CPUFeatures |= CPU_HAS_3DNOW;
		if (CPU_haveSSE()) CPU_CPUFeatures |= CPU_HAS_SSE;
		if (CPU_haveSSE2()) CPU_CPUFeatures |= CPU_HAS_SSE2;
		if (CPU_haveSSE3()) CPU_CPUFeatures |= CPU_HAS_SSE3;
		if (CPU_haveSSE41()) CPU_CPUFeatures |= CPU_HAS_SSE41;
		if (CPU_haveSSE42()) CPU_CPUFeatures |= CPU_HAS_SSE42;
		if (CPU_haveAVX()) CPU_CPUFeatures |= CPU_HAS_AVX;
		if (CPU_haveAVX2()) CPU_CPUFeatures |= CPU_HAS_AVX2;
	}
	return CPU_CPUFeatures;
}

bool CPU_HasRDTSC(void) { return CPU_GetCPUFeatures() & CPU_HAS_RDTSC; }

bool CPU_HasAltiVec(void) { return CPU_GetCPUFeatures() & CPU_HAS_ALTIVEC; }

bool CPU_HasMMX(void) { return CPU_GetCPUFeatures() & CPU_HAS_MMX; }

bool CPU_Has3DNow(void) { return CPU_GetCPUFeatures() & CPU_HAS_3DNOW; }

bool CPU_HasSSE(void) { return CPU_GetCPUFeatures() & CPU_HAS_SSE; }

bool CPU_HasSSE2(void) { return CPU_GetCPUFeatures() & CPU_HAS_SSE2; }

bool CPU_HasSSE3(void) { return CPU_GetCPUFeatures() & CPU_HAS_SSE3; }

bool CPU_HasSSE41(void) { return CPU_GetCPUFeatures() & CPU_HAS_SSE41; }

bool CPU_HasSSE42(void) { return CPU_GetCPUFeatures() & CPU_HAS_SSE42; }

bool CPU_HasAVX(void) { return CPU_GetCPUFeatures() & CPU_HAS_AVX; }

bool CPU_HasAVX2(void) { return CPU_GetCPUFeatures() & CPU_HAS_AVX2; }

static int CPU_SystemRAM = 0;

int CPU_GetSystemRAM(void) {
	if (!CPU_SystemRAM) {
#ifndef CPU_CPUINFO_DISABLED
#if defined(HAVE_SYSCONF) && defined(_SC_PHYS_PAGES) && defined(_SC_PAGESIZE)
		if (CPU_SystemRAM <= 0) CPU_SystemRAM = (int)((Sint64)sysconf(_SC_PHYS_PAGES) * sysconf(_SC_PAGESIZE) / (1024 * 1024));
#endif
#ifdef HAVE_SYSCTLBYNAME
		if (CPU_SystemRAM <= 0) {
#if defined(__FreeBSD__) || defined(__FreeBSD_kernel__) || defined(__NetBSD__)
#ifdef HW_REALMEM
			int mib[2] = { CTL_HW, HW_REALMEM };
#else
			/* might only report up to 2 GiB */
			int mib[2] = { CTL_HW, HW_PHYSMEM };
#endif /* HW_REALMEM */
#else
			int mib[2] = { CTL_HW, HW_MEMSIZE };
#endif /* __FreeBSD__ || __FreeBSD_kernel__ */
			Uint64 memsize = 0;
			size_t len = sizeof(memsize);

			if (sysctl(mib, 2, &memsize, &len, NULL, 0) == 0) CPU_SystemRAM = (int)(memsize / (1024 * 1024));
		}
#endif
#ifdef _WIN32
		if (CPU_SystemRAM <= 0) {
			MEMORYSTATUSEX stat;
			stat.dwLength = sizeof(stat);
			if (GlobalMemoryStatusEx(&stat)) CPU_SystemRAM = (int)(stat.ullTotalPhys / (1024 * 1024));
		}
#endif
#endif
	}
	return CPU_SystemRAM;
}


#ifdef TEST_MAIN

#include <stdio.h>
int main() {
	printf("CPU count: %d\n", CPU_GetCPUCount());
	printf("CPU type: %s\n", CPU_GetCPUType());
	printf("CPU name: %s\n", CPU_GetCPUName());
	printf("CacheLine size: %d\n", CPU_GetCPUCacheLineSize());
	printf("RDTSC: %d\n", CPU_HasRDTSC());
	printf("Altivec: %d\n", CPU_HasAltiVec());
	printf("MMX: %d\n", CPU_HasMMX());
	printf("3DNow: %d\n", CPU_Has3DNow());
	printf("SSE: %d\n", CPU_HasSSE());
	printf("SSE2: %d\n", CPU_HasSSE2());
	printf("SSE3: %d\n", CPU_HasSSE3());
	printf("SSE4.1: %d\n", CPU_HasSSE41());
	printf("SSE4.2: %d\n", CPU_HasSSE42());
	printf("AVX: %d\n", CPU_HasAVX());
	printf("AVX2: %d\n", CPU_HasAVX2());
	printf("RAM: %d MB\n", CPU_GetSystemRAM());
	return 0;
}

#endif
