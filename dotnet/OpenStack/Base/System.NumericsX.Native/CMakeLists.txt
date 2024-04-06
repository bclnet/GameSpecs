cmake_minimum_required (VERSION 3.8)
project("numericsx" CXX ASM_MASM)
if(CMAKE_SIZEOF_VOID_P GREATER 4)
set(BIN_NATIVE "numericsx64")
else()
set(BIN_NATIVE "numericsx32")
endif()

# SYSTEM
include(CheckCXXCompilerFlag)
include(GNUInstallDirs OPTIONAL RESULT_VARIABLE GNUINSTALLDIRS)

if(NOT CMAKE_SYSTEM_PROCESSOR)
	message(FATAL_ERROR "No target CPU architecture set")
endif()

if(NOT CMAKE_SYSTEM_NAME)
	message(FATAL_ERROR "No target OS set")
endif()

# target cpu
set(cpu ${CMAKE_SYSTEM_PROCESSOR})
if(cpu STREQUAL "powerpc")
	set(cpu "ppc")
elseif(cpu MATCHES "i.86")
	set(cpu "x86")
endif()

if(MSVC AND CMAKE_CL_64)
	set(cpu "amd64")
endif()

# target os
if(APPLE)
	set(os "macosx")
else()
	string(TOLOWER "${CMAKE_SYSTEM_NAME}" os)
endif()

# build type
if(NOT CMAKE_BUILD_TYPE)
	set(CMAKE_BUILD_TYPE "RelWithDebInfo")
endif()

# compiler specific flags
if(CMAKE_COMPILER_IS_GNUCC OR CMAKE_C_COMPILER_ID STREQUAL "Clang")
	#add_compile_options(-pipe)
	#add_compile_options(-Wall)
	#if(NOT CMAKE_CROSSCOMPILING AND ONATIVE)
	#	add_compile_options(-march=native)
	#elseif(NOT APPLE AND cpu STREQUAL "x86")
	#	add_compile_options(-march=pentium3)
	#endif()
	#set(CMAKE_C_FLAGS_DEBUG "-g -ggdb -D_DEBUG -O0")
	#set(CMAKE_C_FLAGS_DEBUGALL "-g -ggdb -D_DEBUG")
	#set(CMAKE_C_FLAGS_PROFILE "-g -ggdb -D_DEBUG -O1 -fno-omit-frame-pointer")
	#set(CMAKE_C_FLAGS_RELEASE "-O2 -fno-unsafe-math-optimizations -fno-math-errno -fno-trapping-math -fomit-frame-pointer")
	#set(CMAKE_C_FLAGS_RELWITHDEBINFO "-g -ggdb -O2 -fno-unsafe-math-optimizations -fno-math-errno -fno-trapping-math -fno-omit-frame-pointer")
	#set(CMAKE_C_FLAGS_MINSIZEREL "-Os -fno-unsafe-math-optimizations -fno-math-errno -fno-trapping-math -fomit-frame-pointer")

	set(CMAKE_CXX_FLAGS_DEBUGALL ${CMAKE_C_FLAGS_DEBUGALL})
	set(CMAKE_CXX_FLAGS_PROFILE ${CMAKE_C_FLAGS_PROFILE})

	add_compile_options(-fno-strict-aliasing)

	#if(NOT AROS)
	#	CHECK_CXX_COMPILER_FLAG("-fvisibility=hidden" cxx_has_fvisibility)
	#	if(NOT cxx_has_fvisibility)
	#		message(FATAL_ERROR "Compiler does not support -fvisibility")
	#	endif()
	#	add_compile_options(-fvisibility=hidden)
	#endif()

	# TODO fix these warnings
	add_compile_options(-Wno-sign-compare)
	add_compile_options(-Wno-switch)
	add_compile_options(-Wno-format-security)

	CHECK_CXX_COMPILER_FLAG("-Woverloaded-virtual" cxx_has_Woverload_virtual)
	if(cxx_has_Woverload_virtual)
		add_compile_options(-Woverloaded-virtual)
	endif()

	if(AROS)
		set(CMAKE_SHARED_LIBRARY_SUFFIX ".aros-${cpu}")
		add_definitions(-DIOAPI_NO_64)
	elseif(APPLE)
		add_definitions(-DMACOS_X=1)

		if(cpu STREQUAL "x86_64")
			add_compile_options(-arch x86_64 -mmacosx-version-min=10.9)
			set(ldflags "${ldflags} -arch x86_64 -mmacosx-version-min=10.9")
		elseif(cpu STREQUAL "x86")
			CHECK_CXX_COMPILER_FLAG("-arch i386" cxx_has_arch_i386)
			if(cxx_has_arch_i386)
				add_compile_options(-arch i386)
				set(ldflags "${ldflags} -arch i386")
			endif()

			add_compile_options(-mmacosx-version-min=10.4)
			set(ldflags "${ldflags} -mmacosx-version-min=10.4")
		elseif(cpu STREQUAL "ppc")
			CHECK_CXX_COMPILER_FLAG("-arch ppc" cxx_has_arch_ppc)
			if(cxx_has_arch_ppc)
				add_compile_options(-arch ppc)
				set(ldflags "${ldflags} -arch ppc")
			endif()

			add_compile_options(-mmacosx-version-min=10.4)
			set(ldflags "${ldflags} -mmacosx-version-min=10.4")
		else()
			message(FATAL_ERROR "Unsupported CPU architecture for OSX")
		endif()

		set(sys_libs ${sys_libs} "-framework Carbon -framework Cocoa -framework IOKit")
	elseif(WIN32)
		set(ldflags "${ldflags} -static-libgcc -static-libstdc++")
	elseif(os STREQUAL "linux")
		set(sys_libs ${sys_libs} dl)
	elseif(os STREQUAL "freebsd")
		find_library(EXECINFO_LIBRARIES execinfo /usr/lib /usr/local/lib)
		set(sys_libs ${sys_libs} ${EXECINFO_LIBRARIES})
	endif()
elseif(MSVC)
	add_compile_options(/MP)	 # parallel build (use all cores, or as many as configured in VS)
	
	#add_compile_options(/W4)
	#add_compile_options(/we4840) # treat as error when passing a class to a vararg-function (probably printf-like)
	#add_compile_options(/wd4100) # unreferenced formal parameter
	#add_compile_options(/wd4127) # conditional expression is constant
	add_compile_options(/wd4244) # possible loss of data
	#add_compile_options(/wd4245) # signed/unsigned mismatch
	#add_compile_options(/wd4267) # possible loss of data
	#add_compile_options(/wd4714) # 'function' marked as __forceinline not inlined
	add_compile_options(/wd4996) # 'function': was declared deprecated
	#add_compile_options(/wd4068) # unknown pragma
	#add_compile_options(/wd4458) # declaration of 'variable' hides class member
	#add_definitions(-D_ALLOW_KEYWORD_MACROS) # because of the "#define private public" and "#define protected public" in TypeInfo.cpp
	#set(CMAKE_C_FLAGS_DEBUG "-D_DEBUG /Od /Zi /MDd")
	#set(CMAKE_C_FLAGS_RELEASE "/Ox /Oy /MD")
	#set(CMAKE_C_FLAGS_RELWITHDEBINFO "/Ox /Oy /Zi /MD")
	#set(CMAKE_C_FLAGS_MINSIZEREL "/Ox /Oy /Os /MD")
else()
	message(FATAL_ERROR "Unsupported compiler")
endif()

set(CMAKE_CXX_FLAGS_DEBUG ${CMAKE_C_FLAGS_DEBUG})
set(CMAKE_CXX_FLAGS_RELWITHDEBINFO ${CMAKE_C_FLAGS_RELWITHDEBINFO})
# disable assertions in release builds
set(CMAKE_CXX_FLAGS_RELEASE "-DNDEBUG ${CMAKE_C_FLAGS_RELEASE}")
set(CMAKE_CXX_FLAGS_MINSIZEREL "-DNDEBUG ${CMAKE_C_FLAGS_MINSIZEREL}")
set(CMAKE_ASM_FLAGS "${CFLAGS}")


# SOURCE
function(add_globbed_headers SRCLIST PATHPREFIX)
	file(GLOB_RECURSE tmp_hdrs RELATIVE "${CMAKE_SOURCE_DIR}" "${PATHPREFIX}/*.h")
	set(${SRCLIST} ${tmp_hdrs} ${${SRCLIST}} PARENT_SCOPE)
endfunction()

set(src_lib
	geometry/JointTransform.cpp
	math/MathX.cpp
	math/Quat.cpp
	math/Vector.cpp
	math/Simd_3DNow_gnu.cpp
	#math/Simd_3DNow_msc_x64.cpp
	math/Simd_3DNow_msc_x86.cpp
	math/Simd_Generic.cpp
	math/Simd_MMX_gnu.cpp
	math/Simd_MMX_msc.cpp
	math/Simd_SSE_gnu.cpp
	#math/Simd_SSE_msc_x64.cpp
	math/Simd_SSE_msc_x86.cpp
	math/Simd_SSE2_gnu.cpp
	#math/Simd_SSE2_msc_x64.cpp
	math/Simd_SSE2_msc_x86.cpp
	math/Simd_SSE3_gnu.cpp
	#math/Simd_SSE3_msc_x64.cpp
	math/Simd_SSE3_msc_x86.cpp
	system/Cpu.cpp
	system/Platform.cpp
	)
if(MSVC)
if(CMAKE_SIZEOF_VOID_P GREATER 4)
	set(src_lib_asm
		#math/Simd_3DNow_msc_x64.asm
		math/Simd_MMX_msc_x64.asm
		#math/Simd_SSE_msc_x64.asm
		#math/Simd_SSE2_msc_x64.asm
		#math/Simd_SSE3_msc_x64.asm
		)
else()
	set(src_lib_asm
		#math/Simd_3DNow_msc_x64.asm
		math/Simd_MMX_msc_x86.asm
		#math/Simd_SSE_msc_x64.asm
		#math/Simd_SSE2_msc_x64.asm
		#math/Simd_SSE3_msc_x64.asm
		)
endif()
endif()
add_globbed_headers(src_lib "geometry")
add_globbed_headers(src_lib "math")
add_globbed_headers(src_lib "system")
source_group(TREE ${CMAKE_CURRENT_SOURCE_DIR} FILES ${src_lib} ${src_lib_asm})


# COMPILE
message(STATUS "Building ${CMAKE_BUILD_TYPE} for ${os}-${cpu}")
include_directories(${CMAKE_BINARY_DIR})
include_directories(${CMAKE_SOURCE_DIR})

add_library (${BIN_NATIVE} SHARED
	${src_lib}
	${src_lib_asm}
	System.NumericsX.Native.cpp
	System.NumericsX.Native.h
	)
set_target_properties(${BIN_NATIVE} PROPERTIES COMPILE_DEFINITIONS "__NUMERICSX__")
set_target_properties(${BIN_NATIVE} PROPERTIES LINK_FLAGS "${ldflags}")
