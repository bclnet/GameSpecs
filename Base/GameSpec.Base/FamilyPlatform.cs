using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace GameSpec
{
    public static class FamilyPlatform
    {
        public const string PlatformWindows = "Windows";
        public const string PlatformUnknown = "";

        public static string Platform = "";
        public static Func<PakFile, IOpenGraphic> GraphicFactory;

        public static readonly List<Func<bool>> Startups = new List<Func<bool>>();

        public class Stats
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
            public static bool VR { get; private set; }
        }

        public enum PlatformType
        {
            Windows,
            OSX,
            Linux,
            Android
        }

        public static PlatformType GetPlatformType()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { return PlatformType.Windows; }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) { return PlatformType.OSX; }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return true ? PlatformType.Linux : PlatformType.Android;
            }
            else throw new ArgumentOutOfRangeException(nameof(RuntimeInformation.IsOSPlatform));
        }

        public static bool InTestHost
            => AppDomain.CurrentDomain.GetAssemblies().Any(x => x.FullName.StartsWith("testhost,"));
    }
}