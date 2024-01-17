import os
from io import BytesIO
from typing import Callable

MAXBITS = 13
MAXWIN = 4096
CHUNK = 16384

# Huffman
class Huffman:
    def __init__(self, counts: int, symbols: int, rep: bytes):
        self.count = [0]*counts       # number of symbols of each length
        self.symbol = [0]*symbols     # canonically ordered symbols
        n = len(rep)

        repi = 0
        offs = [0]*(MAXBITS + 1)
        length = [0]*(256)

        # convert compact repeat counts into symbol bit length list
        symbol = 0
        while True:
            lenx = rep[repi]; repi += 1
            left = (lenx >> 4) + 1
            lenx &= 15
            while True:
                length[symbol] = lenx; symbol += 1
                left -= 1
                if left == 0: break
            n -= 1
            if n == 0: break
        n = symbol

        # count number of codes of each length
        for lenx in range(MAXBITS+1): self.count[lenx] = 0
        for symbol in range(n): self.count[length[symbol]] += 1             # assumes lengths are within bounds
        if self.count[0] == n: return 0                                     # no codes! complete, but decode() will fail

        # check for an over-subscribed or incomplete set of lengths
        left = 1                                                            # one possible code of zero length
        for lenx in range(1, MAXBITS+1):
            left <<= 1                      # one more bit, double codes left
            left -= self.count[lenx]         # deduct count from possible codes
            if left < 0: return left        # over-subscribed--return negative
                                            # left > 0 means incomplete
        
        # generate offsets into symbol table for each length for sorting
        offs[1] = 0
        for lenx in range(1, MAXBITS): offs[lenx + 1] = (offs[lenx] + self.count[lenx]) & 0xffff

        # put symbols in table sorted by length, by symbol order within each length
        for symbol in range(n):
            if length[symbol] != 0: self.symbol[offs[length[symbol]]] = symbol; offs[length[symbol]] += 1

litcode = Huffman(MAXBITS + 1, 256, bytes([
    11, 124, 8, 7, 28, 7, 188, 13, 76, 4, 10, 8, 12, 10, 12, 10, 8, 23, 8,
    9, 7, 6, 7, 8, 7, 6, 55, 8, 23, 24, 12, 11, 7, 9, 11, 12, 6, 7, 22, 5,
    7, 24, 6, 11, 9, 6, 7, 22, 7, 11, 38, 7, 9, 8, 25, 11, 8, 11, 9, 12,
    8, 12, 5, 38, 5, 38, 5, 11, 7, 5, 6, 21, 6, 10, 53, 8, 7, 24, 10, 27,
    44, 253, 253, 253, 252, 252, 252, 13, 12, 45, 12, 45, 12, 61, 12, 45,
    44, 173])) # bit lengths of literal codes 
lencode = Huffman(MAXBITS + 1, 16, bytes([2, 35, 36, 53, 38, 23])) # bit lengths of length codes 0..15
distcode = Huffman(MAXBITS + 1, 64, bytes([2, 20, 53, 230, 247, 151, 248])) # bit lengths of distance codes 0..63
basex = [3, 2, 4, 5, 6, 7, 8, 9, 10, 12, 16, 24, 40, 72, 136, 264] # base for length codes 
extra = [0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8] # extra bits for length codes

# Blast
class Blast:
    # input state
    infun: Callable                     # input function provided by user
    inhow: object                       # opaque information passed to infun()
    inxs: bytes
    inx: int                            # next input location
    left: int                           # available input at in
    bitbuf: int                         # bit buffer
    bitcnt: int                         # number of bits in bit buffer

    # output state
    outfun: Callable                    # output function provided by user
    outhow: object                      # opaque information passed to outfun()
    nextx: int                          # index of next write location in out[]
    first: int                          # true to check distances (for first 4K)
    outx: bytearray = bytearray(MAXWIN) # output buffer and sliding window

    def bits(self, need: int) -> int:
        val: int # bit accumulator

        # load at least need bits into val
        val = self.bitbuf
        while self.bitcnt < need:
            if self.left == 0:
                def assign(x): self.inxs = x; self.inx = 0
                self.left = self.infun(self.inhow, assign)
                if self.left == 0: raise Exception('EOF') # out of input
            val |= self.inxs[self.inx] << self.bitcnt; self.inx += 1 # load eight bits
            self.left -= 1
            self.bitcnt += 8

        # drop need bits and update buffer, always zero to seven bits left
        self.bitbuf = val >> need
        self.bitcnt -= need

        # return need bits, zeroing the bits above that
        return val & ((1 << need) - 1)

    def decode(self, h: Huffman) -> int:
        lenx: int           # current number of bits in code
        code: int           # len bits being decoded
        first: int          # first code of length len
        count: int          # number of codes of length len
        index: int          # index of first code of length len in symbol table
        bitbuf: int         # bits from stream
        left: int           # bits left in next or left to process
        nextx: int          # next number of codes

        bitbuf = self.bitbuf
        left = self.bitcnt
        code = first = index = 0
        lenx = 1

        nexts = h.count; nextx = 1
        while True:
            while left != 0:
                left -= 1
                code |= (bitbuf & 1) ^ 1 # invert code
                bitbuf >>= 1
                count = nexts[nextx]; nextx += 1
                if code < first + count:
                    # if length len, return symbol
                    self.bitbuf = bitbuf
                    self.bitcnt = (self.bitcnt - lenx) & 7
                    return h.symbol[index + (code - first)]
                index += count # else update for next length
                first += count
                first <<= 1
                code <<= 1
                lenx += 1
            left = MAXBITS + 1 - lenx
            if left == 0: break
            if self.left == 0:
                self.left = self.infun(self.inhow, self.inx)
                if self.left == 0: raise Exception('EOF') # out of input
            bitbuf = self.inxs[self.inx]; self.inx += 1
            self.left -= 1
            if left > 8: left = 8
        return -9 # ran out of codes

    def decomp(self) -> int:
        lit: int                # true if literals are coded
        dictx: int              # log2(dictionary size) - 6
        symbol: int             # decoded symbol, extra bits for distance
        lenx: int               # length for copy
        dist: int               # distance for copy
        copy: int               # copy counter
        fromx: int; to: int     # copy pointers

        # read header
        lit = self.bits(8)
        if lit > 1: return -1
        dictx = self.bits(8)
        if dictx < 4 or dictx > 6: return -2

        # decode literals and length/distance pairs
        outx = self.outx; out_ = 0
        while True:
            if self.bits(1) != 0:
                # get length
                symbol = self.decode(lencode)
                lenx = basex[symbol] + self.bits(extra[symbol])
                if lenx == 519: break # end code

                # get distance
                symbol = 2 if lenx == 2 else dictx
                dist = self.decode(distcode) << symbol
                dist += self.bits(symbol)
                dist += 1
                if self.first != 0 and dist > self.next: return -3 # distance too far back

                # copy length bytes from distance bytes back
                while True:
                    to = out_ + self.next
                    fromx = to - dist
                    copy = MAXWIN
                    if self.next < dist:
                        fromx += copy
                        copy = dist
                    copy -= self.next
                    if copy > lenx: copy = lenx
                    lenx -= copy
                    self.next += copy
                    while True:
                        outx[to] = outx[fromx]; to += 1; fromx += 1
                        copy -= 1
                        if copy == 0: break
                    if self.next == MAXWIN:
                        if self.outfun(self.outhow, outx, out_, self.next) != 0: return 1
                        self.next = 0
                        self.first = 0
                    if lenx == 0: break

            else:
                 # get literal and write it
                symbol = self.decode(litcode) if lit != 0 else self.bits(8)
                outx[out_ + self.next] = symbol & 0xff; self.next += 1
                if self.next == MAXWIN:
                    if self.outfun(self.outhow, outx, out_, self.next) != 0: return 1
                    self.next = 0
                    self.first = 0
        return 0

    def blast(self, infun: Callable, inhow: object, outfun: Callable, outhow: object) -> int:
        # initialize input state
        self.infun = infun
        self.inhow = inhow
        self.left = 0
        self.bitbuf = 0
        self.bitcnt = 0

        # initialize output state
        self.outfun = outfun
        self.outhow = outhow
        self.next = 0
        self.first = 1

        # return if bits() or decode() tries to read past available input
        err = 0
        try:
            err = self.decomp() # decompress
        except: err = 2         # if came back here via longjmp(), then skip decomp(), return error

        # write any leftover output and update the error code if needed
        outx = self.outx; out_ = 0
        if err != 1 and self.next != 0 and self.outfun(self.outhow, outx, out_, self.next) != 0 and err == 0: err = 1
        if err != 0: raise Exception(f'blast error: {err}')

    def decompress(self, inputx: bytes, output: bytearray) -> int:
        hold = bytearray(CHUNK); holdPtr = 0
        inputLen = len(inputx); inputPtr = 0
        outputLen = len(output); outputPtr = 0
        def inf(how: object, buf: Callable) -> int:
            nonlocal inputLen, inputPtr
            if inputLen <= 0: return 0
            buf(hold)
            lenx = min(inputLen, CHUNK)
            hold[holdPtr:holdPtr+lenx] = inputx[inputPtr:inputPtr+lenx]
            inputPtr += lenx
            inputLen -= lenx
            return lenx
        def outf(how: object, buf: bytes, bufPtr: int, length: int) -> int:
            nonlocal outputLen, outputPtr
            if outputLen <= 0: return 0
            output[outputPtr:outputPtr+length] = buf[bufPtr:bufPtr+length]
            outputPtr += length
            outputLen -= length
            return 0
        # decompress
        self.blast(inf, inputx, outf, output)