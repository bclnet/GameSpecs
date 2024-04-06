using System.Runtime.InteropServices;

namespace OggVorbis
{
    public unsafe struct ogg_iovec_t
    {
        public void* iov_base;
        public nuint iov_len;
    }

    public unsafe struct oggpack_buffer
    {
        public nint endbyte;
        public int endbit;

        public byte* buffer;
        public byte* ptr;
        public nint storage;
    }

    // ogg_page is used to encapsulate the data in one Ogg bitstream page
    public unsafe struct ogg_page
    {
        public byte* header;
        public nint header_len;
        public byte* body;
        public nint body_len;
    }

    // ogg_stream_state contains the current encode/decode state of a logical Ogg bitstream
    public unsafe struct ogg_stream_state
    {
        public byte* body_data;         // bytes from packet bodies
        public nint body_storage;       // storage elements allocated
        public nint body_fill;          // elements stored; fill mark
        public nint body_returned;      // elements of fill returned

        public int* lacing_vals;        // The values that will go to the segment table
        public long* granule_vals;      // granulepos values for headers. Not compact this way, but it is simple coupled to the lacing fifo
        public nint lacing_storage;
        public nint lacing_fill;
        public nint lacing_packet;
        public nint lacing_returned;

        public fixed byte header[282];  // working space for header encode
        public int header_fill;

        public int e_o_s;               // set when we have buffered the last packet in the logical bitstream
        public int b_o_s;               // set after we've written the initial page of a logical bitstream
        public nint serialno;
        public nint pageno;
        public long packetno;           // sequence number for decode; the framing knows where there's a hole in the data, but we need coupling so that the codec (which is in a separate abstraction layer) also knows about the gap
        public long granulepos;
    }

    // ogg_packet is used to encapsulate the data and metadata belonging to a single raw Ogg/Vorbis packet
    public unsafe struct ogg_packet
    {
        public byte* packet;
        public nint bytes;
        public nint b_o_s;
        public nint e_o_s;

        public long granulepos;

        public long packetno;           // sequence number for decode; the framing knows where there's a hole in the data, but we need coupling so that the codec (which is in a separate abstraction layer) also knows about the gap
    }

    public unsafe struct ogg_sync_state
    {
        public byte* data;
        public int storage;
        public int fill;
        public int returned;
        public int unsynced;
        public int headerbytes;
        public int bodybytes;
    }

    public static unsafe class Ogg
    {
        const string LibraryName = "ogg";

        // Ogg BITSTREAM PRIMITIVES: bitstream
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpack_writeinit(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int oggpack_writecheck(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpack_writetrunc(oggpack_buffer* b, nint bits);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpack_writealign(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpack_writecopy(oggpack_buffer* b, void* source, nint bits);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpack_reset(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpack_writeclear(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpack_readinit(oggpack_buffer* b, byte* buf, int bytes);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpack_write(oggpack_buffer* b, nuint value, int bits);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint oggpack_look(oggpack_buffer* b, int bits);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint oggpack_look1(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpack_adv(oggpack_buffer* b, int bits);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpack_adv1(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint oggpack_read(oggpack_buffer* b, int bits);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint oggpack_read1(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint oggpack_bytes(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint oggpack_bits(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern byte* oggpack_get_buffer(oggpack_buffer* b);

        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpackB_writeinit(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int oggpackB_writecheck(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpackB_writetrunc(oggpack_buffer* b, nint bits);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpackB_writealign(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpackB_writecopy(oggpack_buffer* b, void* source, nint bits);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpackB_reset(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpackB_writeclear(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpackB_readinit(oggpack_buffer* b, byte* buf, int bytes);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpackB_write(oggpack_buffer* b, nuint value, int bits);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint oggpackB_look(oggpack_buffer* b, int bits);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint oggpackB_look1(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpackB_adv(oggpack_buffer* b, int bits);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void oggpackB_adv1(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint oggpackB_read(oggpack_buffer* b, int bits);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint oggpackB_read1(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint oggpackB_bytes(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint oggpackB_bits(oggpack_buffer* b);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern byte* oggpackB_get_buffer(oggpack_buffer* b);

        // Ogg BITSTREAM PRIMITIVES: encoding
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_stream_packetin(ogg_stream_state* os, ogg_packet* op);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_stream_iovecin(ogg_stream_state* os, ogg_iovec_t* iov, int count, nint e_o_s, long granulepos);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_stream_pageout(ogg_stream_state* os, ogg_page* og);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_stream_pageout_fill(ogg_stream_state* os, ogg_page* og, int nfill);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_stream_flush(ogg_stream_state* os, ogg_page* og);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_stream_flush_fill(ogg_stream_state* os, ogg_page* og, int nfill);

        // Ogg BITSTREAM PRIMITIVES: decoding
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_sync_init(ogg_sync_state* oy);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_sync_clear(ogg_sync_state* oy);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_sync_reset(ogg_sync_state* oy);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_sync_destroy(ogg_sync_state* oy);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_sync_check(ogg_sync_state* oy);

        [DllImport(LibraryName, ExactSpelling = true)] public static extern byte* ogg_sync_buffer(ogg_sync_state* oy, nint size);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_sync_wrote(ogg_sync_state* oy, nint bytes);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint ogg_sync_pageseek(ogg_sync_state* oy, ogg_page* og);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_sync_pageout(ogg_sync_state* oy, ogg_page* og);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_stream_pagein(ogg_stream_state* os, ogg_page* og);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_stream_packetout(ogg_stream_state* os, ogg_packet* op);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_stream_packetpeek(ogg_stream_state* os, ogg_packet* op);

        // Ogg BITSTREAM PRIMITIVES: general
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_stream_init(ogg_stream_state* os, int serialno);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_stream_clear(ogg_stream_state* os);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_stream_reset(ogg_stream_state* os);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_stream_reset_serialno(ogg_stream_state* os, int serialno);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_stream_destroy(ogg_stream_state* os);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_stream_check(ogg_stream_state* os);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_stream_eos(ogg_stream_state* os);

        [DllImport(LibraryName, ExactSpelling = true)] public static extern void ogg_page_checksum_set(ogg_page* og);

        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_page_version(ogg_page* og);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_page_continued(ogg_page* og);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_page_bos(ogg_page* og);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_page_eos(ogg_page* og);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern long ogg_page_granulepos(ogg_page* og);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_page_serialno(ogg_page* og);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint ogg_page_pageno(ogg_page* og);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ogg_page_packets(ogg_page* og);

        [DllImport(LibraryName, ExactSpelling = true)] public static extern void ogg_packet_clear(ogg_packet* op);
    }
}