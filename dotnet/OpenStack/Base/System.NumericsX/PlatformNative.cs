using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace System.NumericsX
{
    public static partial class PlatformNative
    {
        const string Lib = nameof(PlatformNative);

        [Flags]
        public enum CPUID
        {
            NONE = 0x00000,
            UNSUPPORTED = 0x00001,    // unsupported (386/486)
            GENERIC = 0x00002,    // unrecognized processor
            MMX = 0x00010,    // Multi Media Extensions
            _3DNOW = 0x00020,  // 3DNow!
            SSE = 0x00040,    // Streaming SIMD Extensions
            SSE2 = 0x00080,   // Streaming SIMD Extensions 2
            SSE3 = 0x00100,   // Streaming SIMD Extentions 3 aka Prescott's New Instructions
            ALTIVEC = 0x00200 // AltiVec
        }

        // returns a selection of the CPUID_* flags
        [DllImport(Lib, EntryPoint = "GetProcessorId", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)] public static extern CPUID GetProcessorId();

        // sets the FPU precision
        [DllImport(Lib, EntryPoint = "FPU_SetPrecision", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)] public static extern void FPU_SetPrecision();

        // sets Flush-To-Zero mode
        [DllImport(Lib, EntryPoint = "FPU_SetFTZ", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)] public static extern void FPU_SetFTZ(bool enable);

        // sets Denormals-Are-Zero mode
        [DllImport(Lib, EntryPoint = "FPU_SetDAZ", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)] public static extern void FPU_SetDAZ(bool enable);

        static string NativeLibraryArch
            => RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "64",
                Architecture.X86 => "32",
                _ => throw new NotSupportedException($"The library arch couldn't be resolved for the given platform ('{RuntimeInformation.ProcessArchitecture}')."),
            };

        static string NativeLibraryName
            => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? $"numericsx{NativeLibraryArch}.so.1" :
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"numericsx{NativeLibraryArch}.dll" :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"numericsx{NativeLibraryArch}.so.1" :
            throw new NotSupportedException($"The library name couldn't be resolved for the given platform ('{RuntimeInformation.OSDescription}').");

        static PlatformNative()
            => RegisterDllResolver();

        static bool RegisterDllResolver_Registered = false;
        internal static void RegisterDllResolver()
        {
            if (RegisterDllResolver_Registered == false)
            {
                NativeLibrary.SetDllImportResolver(typeof(PlatformNative).Assembly, DllImportResolver);
                RegisterDllResolver_Registered = true;
            }
        }

        static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName == Lib)
            {
                var nativeLibraryName = NativeLibraryName;
                if (!NativeLibrary.TryLoad(nativeLibraryName, assembly, searchPath, out var handle))
                    throw new DllNotFoundException($"Could not load the dll '{nativeLibraryName}' (this load is intercepted, specified in DllImport as '{libraryName}').");
                return handle;
            }
            else return NativeLibrary.Load(libraryName, assembly, searchPath);
        }

        public static TDelegate LoadDelegate<TDelegate>(string name) where TDelegate : Delegate
        {
            var ptr = NativeLibrary.GetExport(IntPtr.Zero, name);
            return ptr != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer<TDelegate>(ptr) : null;
        }

        public static TDelegate LoadRequiredDelegate<TDelegate>(string name) where TDelegate : Delegate
        {
            var ptr = NativeLibrary.GetExport(IntPtr.Zero, name);
            if (ptr != IntPtr.Zero) return Marshal.GetDelegateForFunctionPointer<TDelegate>(ptr);

            // If we can't load the function for whatever reason we dynamically generate a delegate to give the user an error message that is actually understandable.
            var invoke = typeof(TDelegate).GetMethod("Invoke");
            var returnType = invoke.ReturnType;
            var parameters = invoke.GetParameters().Select(p => p.ParameterType).ToArray();
            var method = new DynamicMethod("NumericsX_GetProcAddress_Exception_Delegate_" + Guid.NewGuid(), returnType, parameters);

            // Here we are generating a delegate that looks like this: ((<the arguments that the delegate type takes>) => throw new Exception(<error string>);
            var generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldstr, $"This NumericsX function could not be loaded. This likely means that this extension isn't present in the current context.");
            generator.Emit(OpCodes.Newobj, typeof(Exception).GetConstructor(new[] { typeof(string) }));
            generator.Emit(OpCodes.Throw);

            return (TDelegate)method.CreateDelegate(typeof(TDelegate));
        }
    }
}