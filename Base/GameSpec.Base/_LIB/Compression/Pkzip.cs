using System;

// https://github.com/karablin/arx-unpacker/blob/master/src/blast.c#L90
namespace Compression
{
    internal unsafe class Pkzip
    {
        const int MAXBITS = 13;              // maximum code length
        const int MAXWIN = 4096;             // maximum window size

        // Huffman code decoding tables.  count[1..MAXBITS] is the number of symbols of each length, which for a canonical code are stepped through in order.
        // symbol[] are the symbol values in canonical order, where the number of entries is the sum of the counts in count[].  The decoding process can be
        // seen in the function decode() below.
        struct huffman
        {
            public short[] count;       // number of symbols of each length
            public short[] symbol;      // canonically ordered symbols
        }

        static int virgin = 1;                              // build tables once
        static short[] litcnt = new short[MAXBITS + 1], litsym = new short[256];      // litcode memory
        static short[] lencnt = new short[MAXBITS + 1], lensym = new short[16];       // lencode memory
        static short[] distcnt = new short[MAXBITS + 1], distsym = new short[64];     // distcode memory
        static huffman litcode = new() { count = litcnt, symbol = litsym };        // length code
        static huffman lencode = new() { count = lencnt, symbol = lensym };        // length code
        static huffman distcode = new() { count = distcnt, symbol = distsym };     // distance code
        static readonly byte[] litlen = {
            11, 124, 8, 7, 28, 7, 188, 13, 76, 4, 10, 8, 12, 10, 12, 10, 8, 23, 8,
            9, 7, 6, 7, 8, 7, 6, 55, 8, 23, 24, 12, 11, 7, 9, 11, 12, 6, 7, 22, 5,
            7, 24, 6, 11, 9, 6, 7, 22, 7, 11, 38, 7, 9, 8, 25, 11, 8, 11, 9, 12,
            8, 12, 5, 38, 5, 38, 5, 11, 7, 5, 6, 21, 6, 10, 53, 8, 7, 24, 10, 27,
            44, 253, 253, 253, 252, 252, 252, 13, 12, 45, 12, 45, 12, 61, 12, 45,
            44, 173}; // bit lengths of literal codes
        static readonly byte[] lenlen = { 2, 35, 36, 53, 38, 23 }; // // bit lengths of length codes 0..15
        static readonly byte[] distlen = { 2, 20, 53, 230, 247, 151, 248 }; // bit lengths of distance codes 0..63
        static readonly short[] basex = { 3, 2, 4, 5, 6, 7, 8, 9, 10, 12, 16, 24, 40, 72, 136, 264 }; // base for length codes 
        static readonly byte[] extra = { 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8 }; // extra bits for length codes

        public static int Decompress(byte[] input, byte[] output)
        {
            return 0;
        }
    }
}
