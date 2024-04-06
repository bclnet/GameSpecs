using System.Runtime.InteropServices;

namespace System.NumericsX.OpenAL
{
    /// <summary>
    /// Contains the library name of OpenAL.
    /// </summary>
    public class OpenALLibraryNameContainer
    {
        /// <summary>
        /// Gets the library name to use on Windows.
        /// </summary>
        public string Windows => "openal32.dll";

        /// <summary>
        /// Gets the library name to use on Linux.
        /// </summary>
        public string Linux => "libopenal.so.1";

        /// <summary>
        /// Gets the library name to use on MacOS.
        /// </summary>
        public string MacOS => "/System/Library/Frameworks/OpenAL.framework/OpenAL";

        /// <summary>
        /// Gets the library name to use on Android.
        /// </summary>
        public string Android => Linux;

        /// <summary>
        /// Gets the library name to use on iOS.
        /// </summary>
        public string IOS => MacOS;

        public string GetLibraryName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID")) ? Android : Linux;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return Windows;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS")) ? IOS : MacOS;
            else throw new NotSupportedException($"The library name couldn't be resolved for the given platform ('{RuntimeInformation.OSDescription}').");
        }
    }
}
