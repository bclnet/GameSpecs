using System.Runtime.InteropServices;
using System.Threading;

namespace System
{
    public interface INativeFile
    {
        void Read(IntPtr ptr, IntPtr buffer, int length);
        void Write(IntPtr ptr, IntPtr buffer, int length);
    }

    public static class NativeFile
    {
        public static Func<bool> IsUnix => () => false; //: PlatformStats.Unix
        static readonly INativeFile _nativeFile = IsUnix() ? new NativeFileUnix() : (INativeFile)new NativeFileWin32();
        public static void Read(IntPtr ptr, IntPtr buffer, int length) => _nativeFile.Read(ptr, buffer, length);
        public static void Write(IntPtr ptr, IntPtr buffer, int length) => _nativeFile.Write(ptr, buffer, length);
    }

    unsafe class NativeFileWin32 : INativeFile
    {
        //[DllImport("kernel32")] static extern int _lread(IntPtr hFile, void* lpBuffer, int wBytes);
        [DllImport("kernel32")] static extern bool ReadFile(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead, NativeOverlapped* lpOverlapped);
        [DllImport("kernel32")] static extern bool WriteFile(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToRead, ref uint lpNumberOfBytesRead, NativeOverlapped* lpOverlapped);

        public void Read(IntPtr ptr, IntPtr buffer, int length)
        {
            //_lread(ptr, buffer, length);
            var lpNumberOfBytesRead = 0U;
            ReadFile(ptr, buffer, (uint)length, ref lpNumberOfBytesRead, null);
        }

        public void Write(IntPtr ptr, IntPtr buffer, int length)
        {
            //_lread(ptr, buffer, length);
            var lpNumberOfBytesRead = 0U;
            WriteFile(ptr, buffer, (uint)length, ref lpNumberOfBytesRead, null);
        }
    }

    unsafe class NativeFileUnix : INativeFile
    {
        [DllImport("libc")] static extern int read(IntPtr ptr, IntPtr buffer, int length);
        [DllImport("libc")] static extern int write(IntPtr ptr, IntPtr buffer, int length);

        public void Read(IntPtr ptr, IntPtr buffer, int length) => read(ptr, buffer, length);
        public void Write(IntPtr ptr, IntPtr buffer, int length) => write(ptr, buffer, length);
    }
}