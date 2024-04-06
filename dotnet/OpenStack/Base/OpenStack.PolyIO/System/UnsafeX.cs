using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace System
{
    [SuppressUnmanagedCodeSecurity]
    public unsafe static class UnsafeX
    {
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)] extern unsafe static void msvcrt_memcpy(void* dest, void* src, uint count);
        [DllImport("libc.so", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)] extern unsafe static void libc_memcpy(void* dest, void* src, uint count);
        public delegate void MemcpyDelgate(void* dest, void* src, uint count);
        public static MemcpyDelgate Memcpy = Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => msvcrt_memcpy,
            PlatformID.Unix => libc_memcpy,
            _ => Unsafe.CopyBlock,
        };

        public static byte[] MarshalSApply(byte[] source, string map, int count = 1)
        {
            const string StructMap = "cxbhiq"; const int StructMapIdx = 2;
            if (map[0] == '<') return source;
            var s = map.ToCharArray();
            char c;
            int p = 0, cnt = 0, size;
            for (var k = 0; k < count; k++)
                for (var i = 1; i < s.Length; i++)
                {
                    c = s[i];
                    if (char.IsDigit(c)) { cnt = cnt * 10 + c - '0'; continue; }
                    else if (cnt == 0) cnt = 1;
                    size = (int)Math.Pow(2, StructMap.IndexOf(char.ToLower(c)) - StructMapIdx);
                    if (size <= 0) p += cnt;
                    else for (var j = 0; j < cnt; j++) { Array.Reverse(source, p, size); p += size; }
                    cnt = 0;
                }
            return source;
        }

        public class Shape<T>
        {
            static (string map, int sizeOf) GetValue()
                => ((string, int))
                (typeof(T).GetField("Struct", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                ?? throw new Exception($"{typeof(T).Name} needs a Struct field"))
                .GetValue(null);
            public static readonly (string map, int sizeOf) Struct = GetValue();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MarshalT<T>(byte[] bytes) where T : struct
        {
            fixed (byte* _ = bytes) return Marshal.PtrToStructure<T>((IntPtr)_);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MarshalS<T>(Func<int, byte[]> bytesFunc) where T : struct
        {
            var (map, sizeOf) = Shape<T>.Struct;
            var bytes = MarshalSApply(bytesFunc(sizeOf), map);
            //return MemoryMarshal.Cast<byte, T>(bytes)[0];
            fixed (byte* _ = bytes) return Marshal.PtrToStructure<T>((IntPtr)_);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] MarshalTArray<T>(byte[] bytes, int count) where T : struct
        {
            //return MemoryMarshal.Cast<byte, T>(bytes).ToArray();
            var typeOfT = typeof(T);
            var isEnum = typeOfT.IsEnum;
            var result = isEnum ? Array.CreateInstance(typeOfT.GetEnumUnderlyingType(), count) : new T[count];
            var hresult = GCHandle.Alloc(result, GCHandleType.Pinned);
            fixed (byte* _ = bytes) Memcpy((void*)hresult.AddrOfPinnedObject(), _, (uint)bytes.Length);
            hresult.Free();
            return isEnum ? result.Cast<T>().ToArray() : (T[])result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] MarshalSArray<T>(Func<int, byte[]> bytesFunc, int count) where T : struct
        {
            var (map, sizeOf) = Shape<T>.Struct;
            var bytes = MarshalSApply(bytesFunc(sizeOf), map);
            var typeOfT = typeof(T);
            var isEnum = typeOfT.IsEnum;
            var result = isEnum ? Array.CreateInstance(typeOfT.GetEnumUnderlyingType(), count) : new T[count];
            var hresult = GCHandle.Alloc(result, GCHandleType.Pinned);
            fixed (byte* _ = bytes) Memcpy((void*)hresult.AddrOfPinnedObject(), _, (uint)bytes.Length);
            hresult.Free();
            return isEnum ? result.Cast<T>().ToArray() : (T[])result;
        }

        public static byte[] MarshalTArray<T>(T[] values, int count) where T : struct
        {
            throw new NotImplementedException();
        }

        public static string FixedAString(byte* data, int length)
        {
            var i = 0;
            while (data[i] != 0 && length-- > 0) i++;
            if (i == 0) return null;
            var value = new byte[i];
            fixed (byte* p = value) while (--i >= 0) p[i] = data[i];
            return Encoding.ASCII.GetString(value);
        }

        public static T[] FixedTArray<T>(T* data, int length)
        {
            var value = new T[length];
            fixed (T* p = value) for (var i = 0; i < length; i++) p[i] = data[i];
            return value;
        }

        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate int QuickSortComparDelegate(void* a, void* b);
        //[DllImport("msvcrt.dll", EntryPoint = "qsort", SetLastError = false)] public static unsafe extern void QuickSort(void* base0, nint n, nint size, QuickSortComparDelegate compar);
        //[DllImport("msvcrt.dll", EntryPoint = "memmove", SetLastError = false)] public static unsafe extern void MoveBlock(void* destination, void* source, uint byteCount);
        //[DllImport("msvcrt.dll", EntryPoint = "memcpy", SetLastError = false)] public static unsafe extern void CopyBlock(void* destination, void* source, uint byteCount);
        //[DllImport("msvcrt.dll", EntryPoint = "memset", SetLastError = false)] public static unsafe extern void InitBlock(void* destination, int c, uint byteCount);
        //[DllImport("msvcrt.dll", EntryPoint = "memcmp", SetLastError = false)] public static unsafe extern int CompareBlock(void* b1, void* b2, int byteCount);

        //[DllImport("Kernel32")] extern static int _lread(SafeFileHandle hFile, void* lpBuffer, int wBytes);
        //public static void ReadBuffer(this FileStream stream, byte[] buf, int length)
        //{
        //    fixed (byte* pbuf = buf) _lread(stream.SafeFileHandle, pbuf, length);
        //}

        //public static T MarshalT<T>(byte[] bytes, int length = -1)
        //{
        //    var size = Marshal.SizeOf(typeof(T));
        //    if (length > 0 && size > length) Array.Resize(ref bytes, size);
        //    fixed (byte* src = bytes) return Marshal.PtrToStructure<T>(new IntPtr(src));
        //    //return (T)Marshal.PtrToStructure(new IntPtr(src), typeof(T));
        //}

        //public static T MarshalTCopy<T>(byte[] bytes, int offset = 0, int length = -1)
        //{
        //    var r = default(T);
        //    var hr = GCHandle.Alloc(r, GCHandleType.Pinned);
        //    fixed (byte* _ = bytes) Memcpy((void*)hr.AddrOfPinnedObject(), _ + offset, (uint)bytes.Length);
        //    hr.Free();
        //    return r;
        //}

        //public static byte[] MarshalF<T>(T value, int length = -1)
        //{
        //    var size = Marshal.SizeOf(typeof(T));
        //    var bytes = new byte[size];
        //    fixed (byte* _ = bytes) Marshal.StructureToPtr(value, new IntPtr(_), false);
        //    return bytes;
        //}

        //        public static T[] MarshalTArray<T>(FileStream stream, int offset, int length)
        //        {
        //            var dest = new T[length];
        //            var h = GCHandle.Alloc(dest, GCHandleType.Pinned);
        //#if !MONO
        //            NativeFile.Read(stream.SafeFileHandle.DangerousGetHandle() + offset, h.AddrOfPinnedObject(), length);
        //#else
        //            NativeFile.Read(stream.Handle + offset, h.AddrOfPinnedObject(), length);
        //#endif
        //            h.Free();
        //            return dest;
        //        }

        //public static T[] MarshalTArray<T>(byte[] bytes, int offset, int count)
        //{
        //    var typeOfT = typeof(T);
        //    var isEnum = typeOfT.IsEnum;
        //    var result = isEnum ? Array.CreateInstance(typeOfT.GetEnumUnderlyingType(), count) : new T[count];
        //    var hresult = GCHandle.Alloc(result, GCHandleType.Pinned);
        //    fixed (byte* _ = bytes) Memcpy((void*)hresult.AddrOfPinnedObject(), _ + offset, (uint)bytes.Length);
        //    hresult.Free();
        //    return isEnum ? result.Cast<T>().ToArray() : (T[])result;
        //}

        //public static byte[] MarshalTArray<T>(T[] values, int count)
        //{
        //    throw new NotImplementedException();
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static void WriteGenericToPtr<T>(IntPtr dest, T value, int sizeOfT) where T : struct
        //{
        //    var bytePtr = (byte*)dest;

        //    var valueref = __makeref(value);
        //    var valuePtr = (byte*)*((IntPtr*)&valueref);
        //    for (var i = 0; i < sizeOfT; ++i) bytePtr[i] = valuePtr[i];
        //}

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static T ReadGenericFromPtr<T>(IntPtr source, int sizeOfT) where T : struct
        //{
        //    var bytePtr = (byte*)source;

        //    T result = default;
        //    var resultRef = __makeref(result);
        //    var resultPtr = (byte*)*((IntPtr*)&resultRef);

        //    for (var i = 0; i < sizeOfT; ++i) resultPtr[i] = bytePtr[i];

        //    return result;
        //}
    }
}