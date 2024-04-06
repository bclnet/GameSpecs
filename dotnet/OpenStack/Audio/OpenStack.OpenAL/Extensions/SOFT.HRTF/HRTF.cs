using System.Runtime.InteropServices;

namespace System.NumericsX.OpenAL.Extensions.SOFT.HRTF
{
    public class HRTF : ALBase
    {
        /// <summary>
        /// The name of this AL extension.
        /// </summary>
        public const string ExtensionName = "ALC_SOFT_HRTF";

        // We need to register the resolver for OpenAL before we can DllImport functions.
        static HRTF()
            => RegisterOpenALResolver();
        HRTF() { }

        /// <summary>
        /// Checks if this extension is present.
        /// </summary>
        /// <param name="device">The device to query.</param>
        /// <returns>Whether the extension was present or not.</returns>
        public static bool IsExtensionPresent(ALDevice device)
            => ALC.IsExtensionPresent(device, ExtensionName);

#pragma warning disable SA1516 // Elements should be separated by blank line
        public static unsafe bool ResetDeviceSoft(ALDevice device, int* attribs) => LoadDelegate<ResetDeviceSoftPtrDelegate>(device, "alcResetDeviceSOFT")(device, attribs);
        [UnmanagedFunctionPointer(AL.ALCallingConvention)] public unsafe delegate bool ResetDeviceSoftPtrDelegate(ALDevice device, int* attribs);

        public static unsafe bool ResetDeviceSoft(ALDevice device, ref int attribs) => LoadDelegate<ResetDeviceSoftRefDelegate>(device, "alcResetDeviceSOFT")(device, ref attribs);
        [UnmanagedFunctionPointer(AL.ALCallingConvention)] public unsafe delegate bool ResetDeviceSoftRefDelegate(ALDevice device, ref int attribs);

        public static unsafe bool ResetDeviceSoft(ALDevice device, int[] attribs) => LoadDelegate<ResetDeviceSoftArrayDelegate>(device, "alcResetDeviceSOFT")(device, attribs);
        [UnmanagedFunctionPointer(AL.ALCallingConvention)] public unsafe delegate bool ResetDeviceSoftArrayDelegate(ALDevice device, int[] attribs);
#pragma warning restore SA1516 // Elements should be separated by blank line
    }
}
