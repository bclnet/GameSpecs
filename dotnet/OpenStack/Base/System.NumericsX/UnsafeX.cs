using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// https://benbowen.blog/post/fun_with_makeref/
namespace System.NumericsX
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe static class UnsafeX
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate int QuickSortComparDelegate(void* a, void* b);
        [DllImport("msvcrt.dll", EntryPoint = "qsort", SetLastError = false)] public static unsafe extern void QuickSort(void* base0, nint n, nint size, QuickSortComparDelegate compar);
        [DllImport("msvcrt.dll", EntryPoint = "memmove", SetLastError = false)] public static unsafe extern void MoveBlock(void* destination, void* source, uint byteCount);
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", SetLastError = false)] public static unsafe extern void CopyBlock(void* destination, void* source, uint byteCount);
        [DllImport("msvcrt.dll", EntryPoint = "memset", SetLastError = false)] public static unsafe extern void InitBlock(void* destination, int c, uint byteCount);
        [DllImport("msvcrt.dll", EntryPoint = "memcmp", SetLastError = false)] public static unsafe extern int CompareBlock(void* b1, void* b2, int byteCount);

        public static unsafe void InitBlock(float* destination, int c, int byteCount)
        {
            throw new NotImplementedException();
        }

        public static T ReadT<T>(byte[] buffer, int offset = 0)
        {
            throw new NotImplementedException();
        }
        public static T ReadTSize<T>(int sizeOf, byte[] buffer, int offset = 0)
        {
            throw new NotImplementedException();
        }

        public static T[] ReadTArray<T>(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteGenericToPtr<T>(IntPtr dest, T value, int sizeOfT) where T : struct
        {
            var bytePtr = (byte*)dest;

            var valueref = __makeref(value);
            var valuePtr = (byte*)*((IntPtr*)&valueref);
            for (var i = 0; i < sizeOfT; ++i) bytePtr[i] = valuePtr[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadGenericFromPtr<T>(IntPtr source, int sizeOfT) where T : struct
        {
            var bytePtr = (byte*)source;

            T result = default;
            var resultRef = __makeref(result);
            var resultPtr = (byte*)*((IntPtr*)&resultRef);

            for (var i = 0; i < sizeOfT; ++i) resultPtr[i] = bytePtr[i];

            return result;
        }

        public static void ArrayCopy<T>(T[] dst, T[] src, int count)
            => Array.Copy(src, dst, count);
    }
}