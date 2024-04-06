using System.Diagnostics.CodeAnalysis;

namespace System.NumericsX.OpenAL
{
    /// <summary>
    /// Handle to an OpenAL capture device.
    /// </summary>
    public struct ALCaptureDevice : IEquatable<ALCaptureDevice>
    {
        public static readonly ALCaptureDevice Null = new(IntPtr.Zero);

        public IntPtr Handle;

        public ALCaptureDevice(IntPtr handle)
            => Handle = handle;

        public override bool Equals(object obj)
            => obj is ALCaptureDevice device && Equals(device);

        public bool Equals([AllowNull] ALCaptureDevice other)
            => Handle.Equals(other.Handle);

        public override int GetHashCode()
            => HashCode.Combine(Handle);

        public static bool operator ==(ALCaptureDevice left, ALCaptureDevice right)
            => left.Equals(right);

        public static bool operator !=(ALCaptureDevice left, ALCaptureDevice right)
            => !(left == right);

        public static implicit operator IntPtr(ALCaptureDevice device) => device.Handle;
    }
}
