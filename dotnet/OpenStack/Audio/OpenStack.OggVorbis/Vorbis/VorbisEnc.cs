using System;
using System.Runtime.InteropServices;

namespace OggVorbis
{
    public unsafe static partial class Vorbis
    {
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_encode_init(vorbis_info* vi, nint channels, nint rate, nint max_bitrate, nint nominal_bitrate, nint min_bitrate);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_encode_setup_managed(vorbis_info* vi, nint channels, nint rate, nint max_bitrate, nint nominal_bitrate, nint min_bitrate);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_encode_setup_vbr(vorbis_info* vi, nint channels, nint rate, float quality);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_encode_init_vbr(vorbis_info* vi, nint channels, nint rate, float base_quality);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_encode_setup_init(vorbis_info* vi);
        [DllImport(LibraryName, ExactSpelling = true)] public static extern int vorbis_encode_ctl(vorbis_info* vi, int number, void* args);
    }

    [Obsolete("This is a deprecated interface. Please use vorbis_encode_ctl() with the \ref ovectl_ratemanage2_arg struct and OV_ECTL_RATEMANAGE2_GET and \ref OV_ECTL_RATEMANAGE2_SET calls in new code.")]
    public unsafe struct ovectl_ratemanage_arg
    {
        public int management_active;
        public nint bitrate_hard_min;
        public nint bitrate_hard_max;
        public double bitrate_hard_window;
        public nint bitrate_av_lo;
        public nint bitrate_av_hi;
        public nint bitrate_av_window;
        public double bitrate_av_window_center;
    }

    public unsafe struct ovectl_ratemanage2_arg
    {
        public int management_active;
        public nint bitrate_limit_min_kbps;
        public nint bitrate_limit_max_kbps;
        public nint bitrate_limit_reservoir_bits;
        public double bitrate_limit_reservoir_bias;
        public nint bitrate_average_kbps;
        public double bitrate_average_damping;
    }

    public unsafe static partial class Vorbis
    {
        public const int OV_ECTL_RATEMANAGE2_GET = 0x14;
        public const int OV_ECTL_RATEMANAGE2_SET = 0x15;
        public const int OV_ECTL_LOWPASS_GET = 0x20;
        public const int OV_ECTL_LOWPASS_SET = 0x21;
        public const int OV_ECTL_IBLOCK_GET = 0x30;
        public const int OV_ECTL_IBLOCK_SET = 0x31;
        public const int OV_ECTL_COUPLING_GET = 0x40;
        public const int OV_ECTL_COUPLING_SET = 0x41;
        [Obsolete("Please use OV_ECTL_RATEMANAGE2_SET instead.")] public const int OV_ECTL_RATEMANAGE_GET = 0x10;
        [Obsolete("Please use OV_ECTL_RATEMANAGE2_SET instead.")] public const int OV_ECTL_RATEMANAGE_SET = 0x11;
        [Obsolete("Please use OV_ECTL_RATEMANAGE2_SET instead.")] public const int OV_ECTL_RATEMANAGE_AVG = 0x12;
        [Obsolete("Please use OV_ECTL_RATEMANAGE2_SET instead.")] public const int OV_ECTL_RATEMANAGE_HARD = 0x13;
    }
}
