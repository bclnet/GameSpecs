using System.Runtime.InteropServices;

namespace System.NumericsX.OpenAL.Extensions.SOFT.SourceLatency
{
    public class SourceLatency : ALBase
    {
        /// <summary>
        /// The name of this AL extension.
        /// </summary>
        public const string ExtensionName = "AL_SOFT_source_latency";

        // We need to register the resolver for OpenAL before we can DllImport functions.
        static SourceLatency()
            => RegisterOpenALResolver();

        SourceLatency() { }

        /// <summary>
        /// Checks if this extension is present.
        /// </summary>
        /// <returns>Whether the extension was present or not.</returns>
        public static bool IsExtensionPresent()
            => AL.IsExtensionPresent(ExtensionName);

#pragma warning disable SA1516 // Elements should be separated by blank line
        public static unsafe void GetSource(int source, SourceLatencyVector2i param, long* values) => _GetSourcei64vPtr(source, param, values);
        [UnmanagedFunctionPointer(AL.ALCallingConvention)] public unsafe delegate void GetSourcei64vPtrDelegate(int source, SourceLatencyVector2i param, long* values);
        static readonly GetSourcei64vPtrDelegate _GetSourcei64vPtr = LoadDelegate<GetSourcei64vPtrDelegate>("alGetSourcei64vSOFT");

        public static void GetSource(int source, SourceLatencyVector2i param, out long values) => _GetSourcei64vRef(source, param, out values);
        [UnmanagedFunctionPointer(AL.ALCallingConvention)] public delegate void GetSourcei64vRefDelegate(int source, SourceLatencyVector2i param, out long values);
        static readonly GetSourcei64vRefDelegate _GetSourcei64vRef = LoadDelegate<GetSourcei64vRefDelegate>("alGetSourcei64vSOFT");

        public static void GetSource(int source, SourceLatencyVector2i param, long[] values) => _GetSourcei64vArray(source, param, values);
        [UnmanagedFunctionPointer(AL.ALCallingConvention)] public delegate void GetSourcei64vArrayDelegate(int source, SourceLatencyVector2i param, long[] values);
        static readonly GetSourcei64vArrayDelegate _GetSourcei64vArray = LoadDelegate<GetSourcei64vArrayDelegate>("alGetSourcei64vSOFT");

        public static unsafe void GetSource(int source, SourceLatencyVector2d param, double* values) => _GetSourcedvPtr(source, param, values);
        [UnmanagedFunctionPointer(AL.ALCallingConvention)] public unsafe delegate void GetSourcedvPtrDelegate(int source, SourceLatencyVector2d param, double* values);
        static readonly GetSourcedvPtrDelegate _GetSourcedvPtr = LoadDelegate<GetSourcedvPtrDelegate>("alGetSourcedvSOFT");

        public static void GetSource(int source, SourceLatencyVector2d param, out double values) => _GetSourcedvRef(source, param, out values);
        [UnmanagedFunctionPointer(AL.ALCallingConvention)] public delegate void GetSourcedvRefDelegate(int source, SourceLatencyVector2d param, out double values);
        static readonly GetSourcedvRefDelegate _GetSourcedvRef = LoadDelegate<GetSourcedvRefDelegate>("alGetSourcedvSOFT");

        public static void GetSource(int source, SourceLatencyVector2d param, double[] values) => _GetSourcedvArray(source, param, values);
        [UnmanagedFunctionPointer(AL.ALCallingConvention)] public delegate void GetSourcedvArrayDelegate(int source, SourceLatencyVector2d param, double[] values);
        static readonly GetSourcedvArrayDelegate _GetSourcedvArray = LoadDelegate<GetSourcedvArrayDelegate>("alGetSourcedvSOFT");
#pragma warning restore SA1516 // Elements should be separated by blank line

        public static unsafe void GetSource(int source, SourceLatencyVector2i param, out long value1, out long value2)
        {
            var values = stackalloc long[2];
            GetSource(source, param, values);
            value1 = values[0];
            value2 = values[1];
        }

        public static unsafe void GetSource(int source, SourceLatencyVector2i param, Span<long> values)
           => GetSource(source, param, out values[0]);

        public static unsafe void GetSource(int source, SourceLatencyVector2i param, out int value1, out int value2, out long value3)
        {
            // FIXME: This might result in wrong values, though it seems to be somewhat correct...
            var values = stackalloc int[4];
            GetSource(source, param, (long*)values);
            value1 = values[0];
            value2 = values[1];
            value3 = ((long*)values)[2];
        }

        public static unsafe void GetSource(int source, SourceLatencyVector2d param, out double value1, out double value2)
        {
            var values = stackalloc double[2];
            GetSource(source, param, values);
            value1 = values[0];
            value2 = values[1];
        }

        public static unsafe void GetSource(int source, SourceLatencyVector2d param, Span<double> values)
            => GetSource(source, param, out values[0]);

        //public static void GetSource(int source, SourceLatencyVector2d param, out Vector2d values)
        //{
        //    values.y = default;
        //    GetSource(source, param, out values.x);
        //}
    }
}
