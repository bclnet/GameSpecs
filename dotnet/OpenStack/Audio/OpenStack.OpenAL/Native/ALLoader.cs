using System.Reflection;
using System.Runtime.InteropServices;

namespace System.NumericsX.OpenAL
{
    /// <summary>
    /// Provides a base for ApiContext so that it can register dll intercepts.
    /// </summary>
    internal static class ALLoader
    {
        static readonly OpenALLibraryNameContainer ALLibraryNameContainer = new();

        static bool RegisteredResolver = false;

        static ALLoader()
            => RegisterDllResolver();

        internal static void RegisterDllResolver()
        {
            if (RegisteredResolver == false)
            {
                NativeLibrary.SetDllImportResolver(typeof(ALLoader).Assembly, ImportResolver);
                RegisteredResolver = true;
            }
        }

        static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName == AL.Lib || libraryName == ALC.Lib)
            {
                var libName = ALLibraryNameContainer.GetLibraryName();

                if (!NativeLibrary.TryLoad(libName, assembly, searchPath, out IntPtr libHandle))
                    throw new DllNotFoundException($"Could not load the dll '{libName}' (this load is intercepted, specified in DllImport as '{libraryName}').");

                return libHandle;
            }
            else return NativeLibrary.Load(libraryName, assembly, searchPath);
        }
    }
}
