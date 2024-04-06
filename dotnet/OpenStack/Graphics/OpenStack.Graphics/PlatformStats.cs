using System;
using System.Diagnostics;

namespace OpenStack
{
    public static class PlatformStats
    {
        static readonly bool _HighRes = Stopwatch.IsHighResolution;
        static readonly double _HighFrequency = 1000.0 / Stopwatch.Frequency;
        static readonly double _LowFrequency = 1000.0 / TimeSpan.TicksPerSecond;
        static bool _UseHRT = false;

        public static bool UsingHighResolutionTiming => _UseHRT && _HighRes && !Unix;
        public static long TickCount => (long)Ticks;
        public static double Ticks => _UseHRT && _HighRes && !Unix ? Stopwatch.GetTimestamp() * _HighFrequency : DateTime.UtcNow.Ticks * _LowFrequency;

        public static readonly bool Is64Bit = Environment.Is64BitProcess;
        public static bool MultiProcessor { get; private set; }
        public static int ProcessorCount { get; private set; }
        public static bool Unix { get; private set; }

        public static int MaxTextureMaxAnisotropy { get; set; }
    }
}