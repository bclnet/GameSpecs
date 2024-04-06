;:ref https ://www.cs.virginia.edu/~evans/cs216/guides/x86.html

XPTR typedef DWORD
XWORD typedef QWORD

_alloca16 macro symbol
add symbol, 15
and symbol, NOT 15
endm