using System;
using System.Runtime.InteropServices;

namespace OggVorbis
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ov_callbacks
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate nint ReadFuncDelegate(byte* ptr, nint size, nint nmemb, object datasource);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate int SeekFuncDelegate(object datasource, long offset, int whence);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate int CloseFuncDelegate(object datasource);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate nint TellFuncDelegate(object datasource);

        public IntPtr read_func;
        public IntPtr seek_func;
        public IntPtr close_func;
        public IntPtr tell_func;
    }

    public unsafe static partial class Vorbis
    {
        public const int NOTOPEN = 0;
        public const int PARTOPEN = 1;
        public const int OPENED = 2;
        public const int STREAMSET = 3;
        public const int INITSET = 4;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe class OggVorbis_File
    {
        public void* datasource;         // Pointer to a FILE *, etc.
        public int seekable;
        public long offset;
        public long end;
        public ogg_sync_state oy;

        // If the FILE handle isn't seekable (eg, a pipe), only the current stream appears
        public int links;
        public long* offsets;
        public long* dataoffsets;
        public uint* serialnos;
        public long* pcmlengths;        // overloaded to maintain binary compatibility; x2 size, stores both beginning and end values
        public vorbis_info* vi;
        public vorbis_comment* vc;

        // Decoding working state local storage
        public long pcm_offset;
        public int ready_state;
        public uint current_serialno;
        public int current_link;

        public double bittrack;
        public double samptrack;

        public ogg_stream_state os;     // take physical pages, weld into a logical stream of packets
        public vorbis_dsp_state vd;     // central working state for the packet->PCM decoder
        public vorbis_block vb;         // local working space for packet->PCM decode

        public ov_callbacks callbacks;

        public void memset()
        {
            throw new NotImplementedException();
        }
    }

    public unsafe static partial class Vorbis
    {
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_clear(OggVorbis_File vf);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_fopen(string path, OggVorbis_File vf);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_open(IntPtr f, OggVorbis_File vf, byte* initial, nint ibytes);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_open_callbacks(object datasource, OggVorbis_File vf, byte* initial, nint ibytes, ov_callbacks callbacks);

        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_test(IntPtr f, OggVorbis_File vf, byte* initial, nint ibytes);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_test_callbacks(object datasource, OggVorbis_File vf, byte* initial, nint ibytes, ov_callbacks callbacks);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_test_open(OggVorbis_File vf);

        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint ov_bitrate(OggVorbis_File vf, int i);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint ov_bitrate_instant(OggVorbis_File vf);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint ov_streams(OggVorbis_File vf);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint ov_seekable(OggVorbis_File vf);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint ov_serialnumber(OggVorbis_File vf, int i);

        [DllImport(LibraryName, ExactSpelling = true)] public static extern long ov_raw_total(OggVorbis_File vf, int i);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern long ov_pcm_total(OggVorbis_File vf, int i);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern double ov_time_total(OggVorbis_File vf, int i);

        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_raw_seek(OggVorbis_File vf, long pos);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_pcm_seek(OggVorbis_File vf, long pos);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_pcm_seek_page(OggVorbis_File vf, long pos);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_time_seek(OggVorbis_File vf, double pos);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_time_seek_page(OggVorbis_File vf, double pos);

        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_raw_seek_lap(OggVorbis_File vf, long pos);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_pcm_seek_lap(OggVorbis_File vf, long pos);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_pcm_seek_page_lap(OggVorbis_File vf, long pos);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_time_seek_lap(OggVorbis_File vf, double pos);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_time_seek_page_lap(OggVorbis_File vf, double pos);

        [DllImport(LibraryName, ExactSpelling = true)] public static extern long ov_raw_tell(OggVorbis_File vf);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern long ov_pcm_tell(OggVorbis_File vf);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern double ov_time_tell(OggVorbis_File vf);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public unsafe delegate void FilterProc(float** pcm, nint channels, nint samples, void* filter_param);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern vorbis_info* ov_info(OggVorbis_File vf, int link);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint ov_read_float(OggVorbis_File vf, float*** pcm_channels, int samples, int* bitstream);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint ov_read_filter(OggVorbis_File vf, byte* buffer, int length, int bigendianp, int word, int sgned, int* bitstream, FilterProc filter, void* filter_param);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern nint ov_read(OggVorbis_File vf, byte* buffer, int length, int bigendianp, int word, int sgned, int* bitstream);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_crosslap(OggVorbis_File vf1, OggVorbis_File vf2);

        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_halfrate(OggVorbis_File vf, int flag);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int ov_halfrate_p(OggVorbis_File vf);
    }
}