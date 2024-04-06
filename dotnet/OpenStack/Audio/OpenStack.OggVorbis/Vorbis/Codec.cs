using System.Runtime.InteropServices;

namespace OggVorbis
{
    // vorbis_info contains all the setup information specific to the specific compression/decompression mode in progress(eg,
    // psychoacoustic settings, channel setup, options, codebook etc). vorbis_info and substructures are in backends.h.
    public unsafe struct vorbis_info
    {
        public int version;
        public int channels;
        public nint rate;

        // The below bitrate declarations are *hints*.
        // Combinations of the three values carry the following implications:
        //
        // all three set to the same value: implies a fixed rate bitstream
        // only nominal set: implies a VBR stream that averages the nominal bitrate.  No hard upper/lower limit
        // upper and or lower set: implies a VBR bitstream that obeys the bitrate limits. nominal may also be set to give a nominal rate.
        // none set: the coder does not care to speculate.

        public nint bitrate_upper;
        public nint bitrate_nominal;
        public nint bitrate_lower;
        public nint bitrate_window;

        public void* codec_setup;
    }

    // vorbis_dsp_state buffers the current vorbis audio analysis/synthesis state.The DSP state belongs to a specific logical bitstream
    public unsafe struct vorbis_dsp_state
    {
        public int analysisp;
        public vorbis_info* vi;

        public float** pcm;
        public float** pcmret;
        public int pcm_storage;
        public int pcm_current;
        public int pcm_returned;

        public int preextrapolate;
        public int eofflag;

        public nint lW;
        public nint W;
        public nint nW;
        public nint centerW;

        public long granulepos;
        public long sequence;

        public long glue_bits;
        public long time_bits;
        public long floor_bits;
        public long res_bits;

        public void* backend_state;
    }

    public unsafe struct vorbis_block
    {
        // necessary stream state for linking to the framing abstraction
        public float** pcm;             // this is a pointer into local storage
        public oggpack_buffer obp;

        public nint lW;
        public nint W;
        public nint nW;
        public int pcmend;
        public int mode;

        public int eofflag;
        public long granunlepos;
        public long sequence;
        public vorbis_dsp_state* vd;    // For read-only access of configuration

        // local storage to avoid remallocing; it's up to the mapping to structure it
        public void* localstore;
        public nint localtop;
        public nint localalloc;
        public nint totaluse;
        public alloc_chain* reap;

        // bitmetrics for the frame
        public nint glue_bits;
        public nint time_bits;
        public nint floor_bits;
        public nint res_bits;

        public void* @internal;
    }

    // vorbis_block is a single block of data to be processed as part of the analysis/synthesis stream; it belongs to a specific logical
    // bitstream, but is independent from other vorbis_blocks belonging to that logical bitstream.
    public unsafe struct alloc_chain
    {
        public void* ptr;
        public alloc_chain* next;
    }

    // the comments are not part of vorbis_info so that vorbis_info can be static storage
    public unsafe struct vorbis_comment
    {
        public byte** user_comments;
        public int* comment_lengths;
        public int comments;
        public byte* vendor;
    }

    // libvorbis encodes in two abstraction layers; first we perform DSP and produce a packet (see docs/analysis.txt).  The packet is then
    // coded into a framed OggSquish bitstream by the second layer (see docs/framing.txt).  Decode is the reverse process; we sync/frame
    // the bitstream and extract individual packets, then decode the packet back into PCM audio.
    // 
    // The extra framing/packetizing is used in streaming formats, such as files.  Over the net (such as with UDP), the framing and
    // packetization aren't necessary as they're provided by the transport and the streaming layer is not used

    public unsafe static partial class Vorbis
    {
        const string LibraryName = "vorbis";

        // Vorbis PRIMITIVES: general
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void vorbis_info_init(vorbis_info* vi);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void vorbis_info_clear(vorbis_info* vi);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_info_blocksize(vorbis_info* vi, int zo);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void vorbis_comment_init(vorbis_comment* vc);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void vorbis_comment_add(vorbis_comment* vc, byte* comment);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void vorbis_comment_add_tag(vorbis_comment* vc, byte* tag, byte* contents);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern byte* vorbis_comment_query(vorbis_comment* vc, byte* tag, int count);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_comment_query_count(vorbis_comment* vc, byte* tag);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void vorbis_comment_clear(vorbis_comment* vc);

        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_block_init(vorbis_dsp_state* v, vorbis_block* vb);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_block_clear(vorbis_block* vb);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern void vorbis_dsp_clear(vorbis_dsp_state* v);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern double vorbis_granule_time(vorbis_dsp_state* v, long granulepos);

        [DllImport(LibraryName, ExactSpelling = true)] public static extern byte* vorbis_version_string();

        // Vorbis PRIMITIVES: analysis/DSP layer
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_analysis_init(vorbis_dsp_state* v, vorbis_info* vi);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_commentheader_out(vorbis_comment* vc, ogg_packet* op);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_analysis_headerout(vorbis_dsp_state* v, vorbis_comment* vc, ogg_packet* op, ogg_packet* op_comm, ogg_packet* op_code);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern float** vorbis_analysis_buffer(vorbis_dsp_state* v, int vals);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_analysis_wrote(vorbis_dsp_state* v, int vals);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_analysis_blockout(vorbis_dsp_state* v, vorbis_block* vb);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_analysis(vorbis_block* vb, ogg_packet* op);

        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_bitrate_addblock(vorbis_block* vb);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_bitrate_flushpacket(vorbis_dsp_state* vd, ogg_packet* op);

        // Vorbis PRIMITIVES: synthesis layer
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_synthesis_idheader(ogg_packet* op);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_synthesis_headerin(vorbis_info* vi, vorbis_comment* vc, ogg_packet* op);

        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_synthesis_init(vorbis_dsp_state* v, vorbis_info* vi);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_synthesis_restart(vorbis_dsp_state* v);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_synthesis(vorbis_block* vb, ogg_packet* op);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_synthesis_trackonly(vorbis_block* vb, ogg_packet* op);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_synthesis_blockin(vorbis_dsp_state* v, vorbis_block* vb);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_synthesis_pcmout(vorbis_dsp_state* v, float*** pcm);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_synthesis_lapout(vorbis_dsp_state* v, float*** pcm);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_synthesis_read(vorbis_dsp_state* v, int samples);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern long vorbis_packet_blocksize(vorbis_info* vi, ogg_packet* op);

        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_synthesis_halfrate(vorbis_info* v, int flag);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_synthesis_halfrate_p(vorbis_info* v);

        // Vorbis ERRORS and return codes
        public const int OV_FALSE = -1;
        public const int OV_EOF = -2;
        public const int OV_HOLE = -3;

        public const int OV_EREAD = -128;
        public const int OV_EFAULT = -129;
        public const int OV_EIMPL = -130;
        public const int OV_EINVAL = -131;
        public const int OV_ENOTVORBIS = -132;
        public const int OV_EBADHEADER = -133;
        public const int OV_EVERSION = -134;
        public const int OV_ENOTAUDIO = -135;
        public const int OV_EBADPACKET = -136;
        public const int OV_EBADLINK = -137;
        public const int OV_ENOSEEK = -138;
    }
}