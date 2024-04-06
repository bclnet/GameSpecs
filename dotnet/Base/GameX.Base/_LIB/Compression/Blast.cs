using System;
using System.IO;

namespace Compression
{
    internal unsafe class Blast
    {
        public delegate int blast_in(object how, ref byte* buf);
        public delegate int blast_out(object how, byte* buf, int length);

        const int MAXBITS = 13;              // maximum code length
        const int MAXWIN = 4096;             // maximum window size

        static short[] litcnt = new short[MAXBITS + 1], litsym = new short[256];      // litcode memory
        static short[] lencnt = new short[MAXBITS + 1], lensym = new short[16];       // lencode memory
        static short[] distcnt = new short[MAXBITS + 1], distsym = new short[64];     // distcode memory
        static Huffman litcode = new Huffman { count = litcnt, symbol = litsym };        // length code
        static Huffman lencode = new Huffman { count = lencnt, symbol = lensym };        // length code
        static Huffman distcode = new Huffman { count = distcnt, symbol = distsym };     // distance code
        static readonly byte[] litlen = {
            11, 124, 8, 7, 28, 7, 188, 13, 76, 4, 10, 8, 12, 10, 12, 10, 8, 23, 8,
            9, 7, 6, 7, 8, 7, 6, 55, 8, 23, 24, 12, 11, 7, 9, 11, 12, 6, 7, 22, 5,
            7, 24, 6, 11, 9, 6, 7, 22, 7, 11, 38, 7, 9, 8, 25, 11, 8, 11, 9, 12,
            8, 12, 5, 38, 5, 38, 5, 11, 7, 5, 6, 21, 6, 10, 53, 8, 7, 24, 10, 27,
            44, 253, 253, 253, 252, 252, 252, 13, 12, 45, 12, 45, 12, 61, 12, 45,
            44, 173 }; // bit lengths of literal codes
        static readonly byte[] lenlen = { 2, 35, 36, 53, 38, 23 }; // bit lengths of length codes 0..15
        static readonly byte[] distlen = { 2, 20, 53, 230, 247, 151, 248 }; // bit lengths of distance codes 0..63
        static readonly short[] basex = { 3, 2, 4, 5, 6, 7, 8, 9, 10, 12, 16, 24, 40, 72, 136, 264 }; // base for length codes 
        static readonly byte[] extra = { 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8 }; // extra bits for length codes

        static Blast()
        {
            // set up decoding tables once
            fixed (byte* litlen_ = litlen) litcode.construct(litlen_, litlen.Length);
            fixed (byte* lenlen_ = lenlen) lencode.construct(lenlen_, lenlen.Length);
            fixed (byte* distlen_ = distlen) distcode.construct(distlen_, distlen.Length);
        }

        // input state
        blast_in infun;                 // input function provided by user
        object inhow;                   // opaque information passed to infun()
        byte* inx;                      // next input location
        int left;                      // available input at in
        int bitbuf;                     // bit buffer
        int bitcnt;                     // number of bits in bit buffer

        // output state
        blast_out outfun;               // output function provided by user
        object outhow;                  // opaque information passed to outfun()
        int next;                       // index of next write location in out[]
        int first;                      // true to check distances (for first 4K)
        byte[] outx = new byte[MAXWIN]; // output buffer and sliding window

        // https://github.com/karablin/arx-unpacker/blob/master/src/blast.c#L66
        /// <summary>
        /// Return need bits from the input stream. This always leaves less than eight bits in the buffer. bits() works properly for need == 0.
        /// 
        /// Format notes:
        /// - Bits are stored in bytes from the least significant bit to the most significant bit. Therefore bits are dropped from the bottom of the bit
        ///   buffer, using shift right, and new bytes are appended to the top of the bit buffer, using shift left.
        /// </summary>
        /// <param name="need"></param>
        /// <returns></returns>
        int bits(int need)
        {
            int val; // bit accumulator

            // load at least need bits into val
            val = this.bitbuf;
            while (this.bitcnt < need)
            {
                if (this.left == 0)
                {
                    this.left = this.infun(this.inhow, ref this.inx);
                    if (this.left == 0) throw new Exception("EOF"); // out of input
                }
                val |= (*this.inx++) << this.bitcnt; // load eight bits
                this.left--;
                this.bitcnt += 8;
            }

            // drop need bits and update buffer, always zero to seven bits left
            this.bitbuf = val >> need;
            this.bitcnt -= need;

            // return need bits, zeroing the bits above that
            return val & ((1 << need) - 1);
        }

        // https://github.com/karablin/arx-unpacker/blob/master/src/blast.c#L123
        /// <summary>
        /// Decode a code from the stream s using huffman table h. Return the symbol or a negative value if there is an error. If all of the lengths are zero, i.e.
        /// an empty code, or if the code is incomplete and an invalid code is received, then -9 is returned after reading MAXBITS bits.
        ///
        /// Format notes:
        /// - The codes as stored in the compressed data are bit-reversed relative to a simple integer ordering of codes of the same lengths. Hence below the
        ///   bits are pulled from the compressed data one at a time and used to build the code value reversed from what is in the stream in order to
        ///   permit simple integer comparisons for decoding.
        ///
        /// - The first code for the shortest length is all ones. Subsequent codes of the same length are simply integer decrements of the previous code.  When
        ///   moving up a length, a one bit is appended to the code. For a complete code, the last code of the longest length will be all zeros.  To support
        ///   this ordering, the bits pulled during decoding are inverted to apply the more "natural" ordering starting with all zeros and incrementing.
        /// </summary>
        /// <param name="h"></param>
        /// <returns></returns>
        int decode(ref Huffman h)
        {
            int len;        // current number of bits in code
            int code;       // len bits being decoded
            int first;      // first code of length len
            int count;      // number of codes of length len
            int index;      // index of first code of length len in symbol table
            int bitbuf;     // bits from stream
            int left;       // bits left in next or left to process
            short* next;    // next number of codes

            bitbuf = this.bitbuf;
            left = this.bitcnt;
            code = first = index = 0;
            len = 1;

            fixed (short* _ = &h.count[1])
            {
                next = _;
                while (true)
                {
                    while (left-- != 0)
                    {
                        code |= (bitbuf & 1) ^ 1; // invert code
                        bitbuf >>= 1;
                        count = *next++;
                        if (code < first + count)
                        {
                            // if length len, return symbol
                            this.bitbuf = bitbuf;
                            this.bitcnt = (this.bitcnt - len) & 7;
                            return h.symbol[index + (code - first)];
                        }
                        index += count; // else update for next length
                        first += count;
                        first <<= 1;
                        code <<= 1;
                        len++;
                    }
                    left = MAXBITS + 1 - len;
                    if (left == 0) break;
                    if (this.left == 0)
                    {
                        this.left = this.infun(this.inhow, ref this.inx);
                        if (this.left == 0) throw new Exception("EOF"); // out of input
                    }
                    bitbuf = *this.inx++;
                    this.left--;
                    if (left > 8) left = 8;
                }
            }
            return -9; // ran out of codes
        }

        /// <summary>
        /// Huffman code decoding tables. count[1..MAXBITS] is the number of symbols of each length, which for a canonical code are stepped through in order.
        /// symbol[] are the symbol values in canonical order, where the number of entries is the sum of the counts in count[]. The decoding process can be
        /// seen in the function decode() below.
        /// </summary>
        struct Huffman
        {
            public short[] count;       // number of symbols of each length
            public short[] symbol;      // canonically ordered symbols

            // https://github.com/karablin/arx-unpacker/blob/master/src/blast.c#L185
            /// <summary>
            /// Given a list of repeated code lengths rep[0..n - 1], where each byte is a count(high four bits + 1) and a code length(low four bits), generate the
            /// list of code lengths.This compaction reduces the size of the object code. Then given the list of code lengths length[0..n-1] representing a canonical
            /// Huffman code for n symbols, construct the tables required to decode those codes. Those tables are the number of codes of each length, and the symbols
            /// sorted by length, retaining their original order within each length.The return value is zero for a complete code set, negative for an over-
            /// subscribed code set, and positive for an incomplete code set. The tables can be used if the return value is zero or positive, but they cannot be used
            /// if the return value is negative. If the return value is zero, it is not possible for decode() using that table to return an error--any stream of
            /// enough bits will resolve to a symbol.If the return value is positive, then it is possible for decode() using that table to return an error for received
            /// codes past the end of the incomplete lengths.
            /// </summary>
            /// <param name="rep"></param>
            /// <param name="n"></param>
            /// <returns></returns>
            public int construct(byte* rep, int n)
            {
                short symbol;                                   // current symbol when stepping through length[]
                short len;                                      // current length when stepping through h.count[]
                int left;                                       // number of possible codes left of current length
                short* offs = stackalloc short[MAXBITS + 1];    // offsets in symbol table for each length
                short* length = stackalloc short[256];          // code lengths

                // convert compact repeat counts into symbol bit length list
                symbol = 0;
                do
                {
                    len = *rep++;
                    left = (len >> 4) + 1;
                    len &= 15;
                    do { length[symbol++] = len; } while (--left != 0);
                } while (--n != 0);
                n = symbol;

                // count number of codes of each length
                for (len = 0; len <= MAXBITS; len++) this.count[len] = 0;
                for (symbol = 0; symbol < n; symbol++) this.count[length[symbol]]++;        // assumes lengths are within bounds
                if (this.count[0] == n) return 0;                                           // no codes! complete, but decode() will fail

                // check for an over-subscribed or incomplete set of lengths
                left = 1;                                                                   // one possible code of zero length
                for (len = 1; len <= MAXBITS; len++)
                {
                    left <<= 1;                     // one more bit, double codes left
                    left -= this.count[len];        // deduct count from possible codes
                    if (left < 0) return left;      // over-subscribed--return negative
                }                                   // left > 0 means incomplete

                // generate offsets into symbol table for each length for sorting
                offs[1] = 0;
                for (len = 1; len < MAXBITS; len++) offs[len + 1] = (short)(offs[len] + this.count[len]);

                // put symbols in table sorted by length, by symbol order within each length
                for (symbol = 0; symbol < n; symbol++) if (length[symbol] != 0) this.symbol[offs[length[symbol]]++] = symbol;

                // return zero for complete set, positive for incomplete set
                return left;
            }
        }

        // https://github.com/karablin/arx-unpacker/blob/master/src/blast.c#L276
        /// <summary>
        /// Decode PKWare Compression Library stream.
        ///
        /// Format notes:
        ///
        /// - First byte is 0 if literals are uncoded or 1 if they are coded. Second byte is 4, 5, or 6 for the number of extra bits in the distance code.
        ///   This is the base-2 logarithm of the dictionary size minus six.
        ///
        /// - Compressed data is a combination of literals and length/distance pairs terminated by an end code. Literals are either Huffman coded or
        ///   uncoded bytes. A length/distance pair is a coded length followed by a coded distance to represent a string that occurs earlier in the
        ///   uncompressed data that occurs again at the current location.
        ///
        /// - A bit preceding a literal or length/distance pair indicates which comes next, 0 for literals, 1 for length/distance.
        ///
        /// - If literals are uncoded, then the next eight bits are the literal, in the normal bit order in th stream, i.e. no bit-reversal is needed. Similarly,
        ///   no bit reversal is needed for either the length extra bits or the distance extra bits.
        ///
        /// - Literal bytes are simply written to the output. A length/distance pair is an instruction to copy previously uncompressed bytes to the output. The
        ///   copy is from distance bytes back in the output stream, copying for length bytes.
        ///
        /// - Distances pointing before the beginning of the output data are not permitted.
        ///
        /// - Overlapped copies, where the length is greater than the distance, are allowed and common. For example, a distance of one and a length of 518
        ///   simply copies the last byte 518 times. A distance of four and a length of twelve copies the last four bytes three times. A simple forward copy
        ///   ignoring whether the length is greater than the distance or not implements this correctly.
        /// </summary>
        /// <returns></returns>
        public int decomp()
        {
            int lit;            // true if literals are coded
            int dict;           // log2(dictionary size) - 6
            int symbol;         // decoded symbol, extra bits for distance
            int len;            // length for copy
            int dist;           // distance for copy
            int copy;           // copy counter
            byte* from, to;     // copy pointers

            // read header
            lit = bits(8);
            if (lit > 1) return -1;
            dict = bits(8);
            if (dict < 4 || dict > 6) return -2;

            // decode literals and length/distance pairs
            fixed (byte* out_ = this.outx)
                do
                {
                    if (bits(1) != 0)
                    {
                        // get length
                        symbol = decode(ref lencode);
                        len = basex[symbol] + bits(extra[symbol]);
                        if (len == 519) break; // end code

                        // get distance
                        symbol = len == 2 ? 2 : dict;
                        dist = decode(ref distcode) << symbol;
                        dist += bits(symbol);
                        dist++;
                        if (this.first != 0 && dist > this.next) return -3;              // distance too far back

                        // copy length bytes from distance bytes back
                        do
                        {
                            to = out_ + this.next;
                            from = to - dist;
                            copy = MAXWIN;
                            if (this.next < dist)
                            {
                                from += copy;
                                copy = dist;
                            }
                            copy -= this.next;
                            if (copy > len) copy = len;
                            len -= copy;
                            this.next += copy;
                            do
                            {
                                *to++ = *from++;
                            } while (--copy != 0);
                            if (this.next == MAXWIN)
                            {
                                if (this.outfun(this.outhow, out_, this.next) != 0) return 1;
                                this.next = 0;
                                this.first = 0;
                            }
                        } while (len != 0);
                    }
                    else
                    {
                        // get literal and write it
                        symbol = lit != 0 ? decode(ref litcode) : bits(8);
                        out_[this.next++] = (byte)symbol;
                        if (this.next == MAXWIN)
                        {
                            if (this.outfun(this.outhow, out_, this.next) != 0) return 1;
                            this.next = 0;
                            this.first = 0;
                        }
                    }
                } while (true);
            return 0;
        }

        // https://github.com/karablin/arx-unpacker/blob/master/src/blast.c#L377
        public int blast(blast_in infun, object inhow, blast_out outfun, object outhow)
        {
            // initialize input state
            this.infun = infun;
            this.inhow = inhow;
            this.left = 0;
            this.bitbuf = 0;
            this.bitcnt = 0;

            // initialize output state
            this.outfun = outfun;
            this.outhow = outhow;
            this.next = 0;
            this.first = 1;

            // return if bits() or decode() tries to read past available input
            int err;
            try { err = decomp(); } // decompress
            catch { err = 2; } // if came back here via longjmp(), then skip decomp(), return error

            // write any leftover output and update the error code if needed
            fixed (byte* out_ = this.outx)
                if (err != 1 && this.next != 0 && this.outfun(this.outhow, out_, this.next) != 0 && err == 0) err = 1;
            return err;
        }

        // Decompress a PKWare Compression Library stream from stdin to stdout
        // https://github.com/karablin/arx-unpacker/blob/master/src/blast.c#L428
        public int Decompress(byte[] inputBuffer, byte[] outputBuffer)
        {
            const int CHUNK = 16384;
            var hold = stackalloc byte[CHUNK];
            var holdPtr = hold;
            fixed (byte* input = inputBuffer, output = outputBuffer)
            {
                int inputLen = inputBuffer.Length, outputLen = outputBuffer.Length;
                IntPtr inputPtr = (IntPtr)input, outputPtr = (IntPtr)output;
                int inf(object how, ref byte* buf)
                {
                    if (inputLen <= 0) return 0;
                    buf = hold;
                    var len = Math.Min(inputLen, CHUNK);
                    UnsafeX.Memcpy(holdPtr, (void*)inputPtr, (uint)len);
                    inputPtr += len;
                    inputLen -= len;
                    return len;
                }
                int outf(object how, byte* buf, int length)
                {
                    if (outputLen <= 0) return 0;
                    UnsafeX.Memcpy((void*)outputPtr, (void*)buf, (uint)length);
                    outputPtr += length;
                    outputLen -= length;
                    return 0;
                }

                // decompress to stdout
                var ret = blast(inf, inputBuffer, outf, outputBuffer);
                if (ret != 0) throw new Exception($"blast error: {ret}");
                return 0;
            }
        }

        public int Decompress(byte[] inputBuffer, Stream outputBuffer)
        {
            const int CHUNK = 16384;
            var hold = stackalloc byte[CHUNK];
            var holdPtr = hold;
            fixed (byte* input = inputBuffer)
            {
                int inputLen = inputBuffer.Length;
                IntPtr inputPtr = (IntPtr)input;
                int inf(object how, ref byte* buf)
                {
                    if (inputLen <= 0) return 0;
                    buf = hold;
                    var len = Math.Min(inputLen, CHUNK);
                    UnsafeX.Memcpy(holdPtr, (void*)inputPtr, (uint)len);
                    inputPtr += len;
                    inputLen -= len;
                    return len;
                }
                int outf(object how, byte* buf, int length)
                {
                    outputBuffer.Write(new ReadOnlySpan<byte>(buf, length));
                    //if (outputLen <= 0) return 0;
                    //UnsafeX.Memcpy(outputPtr, (IntPtr)buf, (uint)length);
                    //outputPtr += length;
                    //outputLen -= length;
                    return 0;
                }
                // decompress
                var ret = blast(inf, inputBuffer, outf, outputBuffer);
                if (ret != 0) throw new Exception($"blast error: {ret}");
                return 0;
            }
        }
    }
}
