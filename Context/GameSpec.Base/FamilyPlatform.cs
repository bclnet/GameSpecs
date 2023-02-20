﻿using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace GameSpec
{
    /// <summary>
    /// FamilyPlatform
    /// </summary>
    public static class FamilyPlatform
    {
        /// <summary>
        /// The platform stats.
        /// </summary>
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

        /// <summary>
        /// The platform type.
        /// </summary>
        public enum Type { Unknown, OpenGL, Unity, Unreal, Vulken, Test, Other }

        /// <summary>
        /// The platform OS.
        /// </summary>
        public enum OS { Windows, OSX, Linux, Android }

        static FamilyPlatform()
        {
            PlatformOS = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OS.Windows
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OS.OSX
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? true ? OS.Linux : OS.Android
                : throw new ArgumentOutOfRangeException(nameof(RuntimeInformation.IsOSPlatform));
        }

        /// <summary>
        /// Gets or sets the platform.
        /// </summary>
        public static Type Platform = Type.Unknown;

        /// <summary>
        /// Gets or sets the platform tag.
        /// </summary>
        public static string PlatformTag;

        /// <summary>
        /// Gets the platform os.
        /// </summary>
        public static readonly OS PlatformOS;

        /// <summary>
        /// Gets or sets the platform graphics factory.
        /// </summary>
        public static Func<PakFile, IOpenGraphic> GraphicFactory;

        /// <summary>
        /// Gets the platform startups.
        /// </summary>
        public static readonly List<Func<bool>> Startups = new();

        /// <summary>
        /// Determines if in a test host.
        /// </summary>
        public static bool InTestHost
            => AppDomain.CurrentDomain.GetAssemblies().Any(x => x.FullName.StartsWith("testhost,"));
    }
}