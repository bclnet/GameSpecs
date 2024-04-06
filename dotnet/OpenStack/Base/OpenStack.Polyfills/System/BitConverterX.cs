using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SystemX;
using static System.BitConverter;

namespace System
{
    public static class BitConverterX
    {
        #region Half

        /// <summary>
        /// Returns a half-precision floating point number converted from two bytes at a specified position in a byte array.
        /// </summary>
        /// <param name="value">An array of bytes.</param>
        /// <param name="startIndex">The starting position within <paramref name="value"/>.</param>
        /// <returns>A half-precision floating point number signed integer formed by two bytes beginning at <paramref name="startIndex"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="startIndex"/> equals the length of <paramref name="value"/> minus 1.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startIndex"/> is less than zero or greater than the length of <paramref name="value"/> minus 1.</exception>
        public static Half ToHalf(byte[] value, int startIndex) => Int16BitsToHalf(ToInt16(value, startIndex));

        /// <summary>
        /// Converts a read-only byte span into a half-precision floating-point value.
        /// </summary>
        /// <param name="value">A read-only span containing the bytes to convert.</param>
        /// <returns>A half-precision floating-point value representing the converted bytes.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The length of <paramref name="value"/> is less than 2.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Half ToHalf(ReadOnlySpan<byte> value)
        {
            if (value.Length < sizeof(Half))
                throw new ArgumentOutOfRangeException(nameof(value));
            return Unsafe.ReadUnaligned<Half>(ref MemoryMarshal.GetReference(value));
        }

        /// <summary>
        /// Converts the specified half-precision floating point number to a 16-bit signed integer.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A 16-bit signed integer whose bits are identical to <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe short HalfToInt16Bits(Half value) => (short)value.m_value;

        /// <summary>
        /// Converts the specified 16-bit signed integer to a half-precision floating point number.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>A half-precision floating point number whose bits are identical to <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Half Int16BitsToHalf(short value) => new Half((ushort)(value));

        #endregion
    }
}