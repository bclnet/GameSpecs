.model flat, c
include Simd_msc_x86.asm.h

; extrn printf:proc
; extrn exit:proc
_text segment

; void MMX_Memcpy8B(void* dest, const void* src, const int count);
MMX_Memcpy8B proc PUBLIC, dest:XPTR, src:XPTR, count:DWORD
	mov		esi, DWORD PTR src
	mov		edi, DWORD PTR dest
	mov		ecx, count
	shr		ecx, 3			; 8 bytes per iteration

loop1:
	movq	mm1, 0[esi]		; Read in source data
	movntq	0[edi], mm1		; Non-temporal stores

	add		esi, 8
	add		edi, 8
	dec		ecx
	jnz		loop1

	emms
	ret
MMX_Memcpy8B endp

; 165MB/sec
; void MMX_Memcpy64B(void* dest, const void* src, const int count);
MMX_Memcpy64B proc PUBLIC, dest:XPTR, src:XPTR, count:DWORD
	mov		esi, src
	mov		edi, dest
	mov		ecx, count
	shr		ecx, 6		; 64 bytes per iteration

loop1:
	prefetchnta 64[esi]	; Prefetch next loop, non-temporal
	prefetchnta 96[esi]

	movq mm1, 0[esi]	; Read in source data
	movq mm2, 8[esi]
	movq mm3, 16[esi]
	movq mm4, 24[esi]
	movq mm5, 32[esi]
	movq mm6, 40[esi]
	movq mm7, 48[esi]
	movq mm0, 56[esi]

	movntq  0[edi], mm1	; Non-temporal stores
	movntq  8[edi], mm2
	movntq 16[edi], mm3
	movntq 24[edi], mm4
	movntq 32[edi], mm5
	movntq 40[edi], mm6
	movntq 48[edi], mm7
	movntq 56[edi], mm0

	add		esi, 64
	add		edi, 64
	dec		ecx
	jnz		loop1

	emms
	ret
MMX_Memcpy64B endp

; 240MB/sec
; void MMX_Memcpy2kB(void* dest, const void* src, const int count);
MMX_Memcpy2kB proc PUBLIC, dest:XPTR, src:XPTR, count:DWORD
	
	local tbuf[2048+15]:XPTR
	_alloca16 tbuf

	push	ebx
	mov		esi, src
	mov		ebx, count
	shr		ebx, 11		; 2048 bytes at a time
	mov		edi, dest

loop2k:
	push	edi			; copy 2k into temporary buffer
	mov		edi, tbuf
	mov		ecx, 32

loopMemToL1:
	prefetchnta 64[esi] ; Prefetch next loop, non-temporal
	prefetchnta 96[esi]

	movq mm1, 0[esi]	; Read in source data
	movq mm2, 8[esi]
	movq mm3, 16[esi]
	movq mm4, 24[esi]
	movq mm5, 32[esi]
	movq mm6, 40[esi]
	movq mm7, 48[esi]
	movq mm0, 56[esi]

	movq  0[edi], mm1	; Store into L1
	movq  8[edi], mm2
	movq 16[edi], mm3
	movq 24[edi], mm4
	movq 32[edi], mm5
	movq 40[edi], mm6
	movq 48[edi], mm7
	movq 56[edi], mm0
	add		esi, 64
	add		edi, 64
	dec		ecx
	jnz		loopMemToL1

	pop		edi			; Now copy from L1 to system memory
	push	esi
	mov		esi, tbuf
	mov		ecx, 32

loopL1ToMem:
	movq mm1, 0[esi]	; Read in source data from L1
	movq mm2, 8[esi]
	movq mm3, 16[esi]
	movq mm4, 24[esi]
	movq mm5, 32[esi]
	movq mm6, 40[esi]
	movq mm7, 48[esi]
	movq mm0, 56[esi]

	movntq 0[edi], mm1	; Non-temporal stores
	movntq 8[edi], mm2
	movntq 16[edi], mm3
	movntq 24[edi], mm4
	movntq 32[edi], mm5
	movntq 40[edi], mm6
	movntq 48[edi], mm7
	movntq 56[edi], mm0

	add		esi, 64
	add		edi, 64
	dec		ecx
	jnz		loopL1ToMem

	pop		esi			; Do next 2k block
	dec		ebx
	jnz		loop2k
	pop		ebx

	emms
	ret
MMX_Memcpy2kB endp

;DAT_U union
;	bytes db ? dup(8);
;	words dw ? dup(4);
;	dwords dd ? dup(2);
;DAT_U ends

; void MMX_Memset64B(void* dest, const int val, const int count);
MMX_Memset64B proc PUBLIC, dest:XPTR, dat:XWORD, count:DWORD
	mov edi, dest
	mov ecx, count
	shr ecx, 3				; 8 bytes per iteration
	movq mm1, dat			; Read in source data
loop2:
	movntq  0[edi], mm1		; Non-temporal stores

	add edi, 8
	dec ecx
	jnz loop2

	ret
MMX_Memset64B endp

; void MMX_Memset8B(void* dest, const int val, const int count);
MMX_Memset8B proc PUBLIC, dest:XPTR, dat:QWORD, count:DWORD
	mov edi, dest
	mov ecx, count
	shr ecx, 6				; 64 bytes per iteration
	movq mm1, dat			; Read in source data
	movq mm2, mm1
	movq mm3, mm1
	movq mm4, mm1
	movq mm5, mm1
	movq mm6, mm1
	movq mm7, mm1
	movq mm0, mm1
loop1:
	movntq  0[edi], mm1		; Non-temporal stores
	movntq  8[edi], mm2
	movntq 16[edi], mm3
	movntq 24[edi], mm4
	movntq 32[edi], mm5
	movntq 40[edi], mm6
	movntq 48[edi], mm7
	movntq 56[edi], mm0

	add edi, 64
	dec ecx
	jnz loop1

	ret
MMX_Memset8B endp

_text ends
end