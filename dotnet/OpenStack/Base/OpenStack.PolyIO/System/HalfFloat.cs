using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System
{
    /// <summary>
    /// The name HalfFloat is derived from half-precision floating-point number.
    /// It occupies only 16 bits, which are split into 1 Sign bit, 5 Exponent bits and 10 Mantissa bits.
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public unsafe struct HalfFloat : ISerializable, IComparable<HalfFloat>, IFormattable, IEquatable<HalfFloat>
    {
        /// <summary>The size in bytes for an instance of the HalfFloat struct.</summary>
        public const int SizeInBytes = 2;
        /// <summary>Smallest positive half</summary>
        public const float MinValue = 5.96046448e-08f;
        /// <summary>Smallest positive normalized half</summary>
        public const float MinNormalizedValue = 6.10351562e-05f;
        /// <summary>Largest positive half</summary>
        public const float MaxValue = 65504.0f;
        /// <summary>Smallest positive e for which half (1.0 + e) != half (1.0)</summary>
        public const float Epsilon = 0.00097656f;
        const int MaxUlps = 1;
        public ushort bits;

        /// <summary>Returns true if the HalfFloat is zero.</summary>
        public bool IsZero => (bits == 0) || (bits == 0x8000);

        /// <summary>Returns true if the HalfFloat represents Not A Number (NaN)</summary>
        public bool IsNaN => ((bits & 0x7C00) == 0x7C00) && (bits & 0x03FF) != 0x0000;

        /// <summary>Returns true if the HalfFloat represents positive infinity.</summary>
        public bool IsPositiveInfinity => bits == 31744;

        /// <summary>Returns true if the HalfFloat represents negative infinity.</summary>
        public bool IsNegativeInfinity => bits == 64512;

        /// <summary>
        /// The new HalfFloat instance will convert the parameter into 16-bit half-precision floating-point.
        /// </summary>
        /// <param name="f">32-bit single-precision floating-point number.</param>
        public HalfFloat(float f) : this() => bits = SingleToHalf(*(int*)&f);

        /// <summary>
        /// The new HalfFloat instance will convert the parameter into 16-bit half-precision floating-point.
        /// </summary>
        /// <param name="f">32-bit single-precision floating-point number.</param>
        /// <param name="throwOnError">Enable checks that will throw if the conversion result is not meaningful.</param>
        public HalfFloat(float f, bool throwOnError) : this(f)
        {
            if (!throwOnError) return;
            // handle cases that cause overflow rather than silently ignoring it
            if (f > MaxValue) throw new ArithmeticException("HalfFloat: Positive maximum value exceeded.");
            else if (f < -MaxValue) throw new ArithmeticException("HalfFloat: Negative minimum value exceeded.");
            // handle cases that make no sense
            else if (float.IsNaN(f)) throw new ArithmeticException("HalfFloat: Input is not a number (NaN).");
            else if (float.IsPositiveInfinity(f)) throw new ArithmeticException("HalfFloat: Input is positive infinity.");
            else if (float.IsNegativeInfinity(f)) throw new ArithmeticException("HalfFloat: Input is negative infinity.");
        }

        /// <summary>
        /// The new HalfFloat instance will convert the parameter into 16-bit half-precision floating-point.
        /// </summary>
        /// <param name="d">64-bit double-precision floating-point number.</param>
        public HalfFloat(double d) : this((float)d) { }

        /// <summary>
        /// The new HalfFloat instance will convert the parameter into 16-bit half-precision floating-point.
        /// </summary>
        /// <param name="d">64-bit double-precision floating-point number.</param>
        /// <param name="throwOnError">Enable checks that will throw if the conversion result is not meaningful.</param>
        public HalfFloat(double d, bool throwOnError) : this((float)d, throwOnError) { }

        ushort SingleToHalf(int si32)
        {
            // Our floating point number, F, is represented by the bit pattern in integer i.
            // Disassemble that bit pattern into the sign, S, the exponent, E, and the significand, M.
            // Shift S into the position where it will go in in the resulting half number.
            // Adjust E, accounting for the different exponent bias of float and half (127 versus 15).
            int sign = (si32 >> 16) & 0x00008000, exponent = ((si32 >> 23) & 0x000000ff) - (127 - 15), mantissa = si32 & 0x007fffff;

            // Now reassemble S, E and M into a half:
            if (exponent <= 0)
            {
                // E is less than -10. The absolute value of F is less than HalfFloat.MinValue (F may be a small normalized float, a denormalized float or a zero).
                // We convert F to a half zero with the same sign as F.
                if (exponent < -10) return (ushort)sign;

                // E is between -10 and 0. F is a normalized float whose magnitude is less than HalfFloat.MinNormalizedValue.
                // We convert F to a denormalized half.
                // Add an explicit leading 1 to the significand.
                mantissa |= 0x00800000;

                // Round to M to the nearest (10+E)-bit value (with E between -10 and 0); in case of a tie, round to the nearest even value.
                // Rounding may cause the significand to overflow and make our number normalized. Because of the way a half's bits are laid out, we don't have to treat this case separately; the code below will handle it correctly.
                int t = 14 - exponent, a = (1 << (t - 1)) - 1, b = (mantissa >> t) & 1;
                mantissa = (mantissa + a + b) >> t;
                // Assemble the half from S, E (==zero) and M.
                return (ushort)(sign | mantissa);
            }
            else if (exponent == 0xff - (127 - 15))
            {
                // F is an infinity; convert F to a half infinity with the same sign as F.
                if (mantissa == 0) return (ushort)(sign | 0x7c00);
                else
                {
                    // F is a NAN; we produce a half NAN that preserves the sign bit and the 10 leftmost bits of the significand of F, with one exception: If the 10 leftmost bits are all zero, the NAN would turn 
                    // into an infinity, so we have to set at least one bit in the significand.
                    mantissa >>= 13;
                    return (ushort)(sign | 0x7c00 | mantissa | ((mantissa == 0) ? 1 : 0));
                }
            }
            else
            {
                // E is greater than zero.  F is a normalized float. We try to convert F to a normalized half.
                // Round to M to the nearest 10-bit value. In case of a tie, round to the nearest even value.
                mantissa = mantissa + 0x00000fff + ((mantissa >> 13) & 1);
                if ((mantissa & 0x00800000) == 1) { mantissa = 0; exponent += 1; } // overflow in significand, and adjust exponent
                // exponent overflow
                if (exponent > 30) throw new ArithmeticException("HalfFloat: Hardware floating-point overflow.");
                // Assemble the half from S, E and M.
                return (ushort)(sign | (exponent << 10) | (mantissa >> 13));
            }
        }

        /// <summary>Converts the 16-bit half to 32-bit floating-point.</summary>
        /// <returns>A single-precision floating-point number.</returns>
        public float ToSingle() { var i = HalfToFloat(bits); return *(float*)&i; }

        int HalfToFloat(ushort ui16)
        {
            int sign = (ui16 >> 15) & 0x00000001, exponent = (ui16 >> 10) & 0x0000001f, mantissa = ui16 & 0x000003ff;

            if (exponent == 0)
            {
                // Plus or minus zero
                if (mantissa == 0) return sign << 31;
                else
                {
                    // Denormalized number -- renormalize it
                    while ((mantissa & 0x00000400) == 0) { mantissa <<= 1; exponent -= 1; }
                    exponent += 1; mantissa &= ~0x00000400;
                }
            }
            else if (exponent == 31)
            {
                // Positive or negative infinity
                if (mantissa == 0) return (sign << 31) | 0x7f800000;
                // Nan -- preserve sign and significand bits
                else return (sign << 31) | 0x7f800000 | (mantissa << 13);
            }

            // Normalized number
            exponent += (127 - 15);
            mantissa <<= 13;

            // Assemble S, E and M.
            return (sign << 31) | (exponent << 23) | mantissa;
        }

        #region Conversions

        /// <summary>
        /// Converts a System.Single to a System.HalfFloat.
        /// </summary>
        /// <param name="f">The value to convert.
        /// A <see cref="System.Single"/>
        /// </param>
        /// <returns>The result of the conversion.
        /// A <see cref="HalfFloat"/>
        /// </returns>
        public static explicit operator HalfFloat(float f) => new HalfFloat(f);

        /// <summary>
        /// Converts a System.Double to a System.HalfFloat.
        /// </summary>
        /// <param name="d">The value to convert.
        /// A <see cref="System.Double"/>
        /// </param>
        /// <returns>The result of the conversion.
        /// A <see cref="HalfFloat"/>
        /// </returns>
        public static explicit operator HalfFloat(double d) => new HalfFloat(d);

        /// <summary>
        /// Converts a System.HalfFloat to a System.Single.
        /// </summary>
        /// <param name="h">The value to convert.
        /// A <see cref="HalfFloat"/>
        /// </param>
        /// <returns>The result of the conversion.
        /// A <see cref="System.Single"/>
        /// </returns>
        public static implicit operator float(HalfFloat h) => h.ToSingle();

        /// <summary>
        /// Converts a System.HalfFloat to a System.Double.
        /// </summary>
        /// <param name="h">The value to convert.
        /// A <see cref="HalfFloat"/>
        /// </param>
        /// <returns>The result of the conversion.
        /// A <see cref="System.Double"/>
        /// </returns>
        public static implicit operator double(HalfFloat h) => (double)h.ToSingle();

        #endregion

        /// <summary>Constructor used by ISerializable to deserialize the object.</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public HalfFloat(SerializationInfo info, StreamingContext context) => bits = (ushort)info.GetValue("bits", typeof(ushort));

        /// <summary>Used by ISerialize to serialize the object.</summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context) => info.AddValue("bits", bits);

        /// <summary>Updates the HalfFloat by reading from a Stream.</summary>
        /// <param name="r">A BinaryReader instance associated with an open Stream.</param>
        public void FromBinaryStream(BinaryReader r) => bits = r.ReadUInt16();

        /// <summary>Writes the HalfFloat into a Stream.</summary>
        /// <param name="w">A BinaryWriter instance associated with an open Stream.</param>
        public void ToBinaryStream(BinaryWriter w) => w.Write(bits);

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified System.HalfFloat value.
        /// </summary>
        /// <param name="other">System.HalfFloat object to compare to this instance..</param>
        /// <returns>True, if other is equal to this instance; false otherwise.</returns>
        public bool Equals(HalfFloat other)
        {
            short aInt, bInt;
            unchecked { aInt = (short)other.bits; }
            unchecked { bInt = (short)bits; }

            // Make aInt lexicographically ordered as a twos-complement int
            if (aInt < 0) aInt = (short)(0x8000 - aInt);
            // Make bInt lexicographically ordered as a twos-complement int
            if (bInt < 0) bInt = (short)(0x8000 - bInt);
            var intDiff = Math.Abs((short)(aInt - bInt));
            return intDiff <= MaxUlps;
        }

        /// <summary>
        /// Compares this instance to a specified half-precision floating-point number
        /// and returns an integer that indicates whether the value of this instance
        /// is less than, equal to, or greater than the value of the specified half-precision
        /// floating-point number. 
        /// </summary>
        /// <param name="other">A half-precision floating-point number to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value. If the number is:
        /// <para>Less than zero, then this instance is less than other, or this instance is not a number
        /// (System.HalfFloat.NaN) and other is a number.</para>
        /// <para>Zero: this instance is equal to value, or both this instance and other
        /// are not a number (System.HalfFloat.NaN), System.HalfFloat.PositiveInfinity, or
        /// System.HalfFloat.NegativeInfinity.</para>
        /// <para>Greater than zero: this instance is greater than othrs, or this instance is a number
        /// and other is not a number (System.HalfFloat.NaN).</para>
        /// </returns>
        public int CompareTo(HalfFloat other) => ((float)this).CompareTo((float)other);

        /// <summary>Converts this HalfFloat into a human-legible string representation.</summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() => ToSingle().ToString();

        /// <summary>Converts this HalfFloat into a human-legible string representation.</summary>
        /// <param name="format">Formatting for the output string.</param>
        /// <param name="formatProvider">Culture-specific formatting information.</param>
        /// <returns>The string representation of this instance.</returns>
        public string ToString(string format, IFormatProvider formatProvider) => ToSingle().ToString(format, formatProvider);

        /// <summary>Converts the string representation of a number to a half-precision floating-point equivalent.</summary>
        /// <param name="s">String representation of the number to convert.</param>
        /// <returns>A new HalfFloat instance.</returns>
        public static HalfFloat Parse(string s) => (HalfFloat)float.Parse(s);

        /// <summary>Converts the string representation of a number to a half-precision floating-point equivalent.</summary>
        /// <param name="s">String representation of the number to convert.</param>
        /// <param name="style">Specifies the format of s.</param>
        /// <param name="provider">Culture-specific formatting information.</param>
        /// <returns>A new HalfFloat instance.</returns>
        public static HalfFloat Parse(string s, System.Globalization.NumberStyles style, IFormatProvider provider) => (HalfFloat)float.Parse(s, style, provider);

        /// <summary>Converts the string representation of a number to a half-precision floating-point equivalent. Returns success.</summary>
        /// <param name="s">String representation of the number to convert.</param>
        /// <param name="result">The HalfFloat instance to write to.</param>
        /// <returns>Success.</returns>
        public static bool TryParse(string s, out HalfFloat result)
        {
            var b = float.TryParse(s, out var f);
            result = (HalfFloat)f;
            return b;
        }

        /// <summary>Converts the string representation of a number to a half-precision floating-point equivalent. Returns success.</summary>
        /// <param name="s">string representation of the number to convert.</param>
        /// <param name="style">specifies the format of s.</param>
        /// <param name="provider">Culture-specific formatting information.</param>
        /// <param name="result">The HalfFloat instance to write to.</param>
        /// <returns>Success.</returns>
        public static bool TryParse(string s, System.Globalization.NumberStyles style, IFormatProvider provider, out HalfFloat result)
        {
            var b = float.TryParse(s, style, provider, out var f);
            result = (HalfFloat)f;
            return b;
        }

        /// <summary>Returns the HalfFloat as an array of bytes.</summary>
        /// <param name="h">The HalfFloat to convert.</param>
        /// <returns>The input as byte array.</returns>
        public static byte[] GetBytes(HalfFloat h) => BitConverter.GetBytes(h.bits);

        /// <summary>Converts an array of bytes into HalfFloat.</summary>
        /// <param name="value">A HalfFloat in it's byte[] representation.</param>
        /// <param name="startIndex">The starting position within value.</param>
        /// <returns>A new HalfFloat instance.</returns>
        public static HalfFloat FromBytes(byte[] value, int startIndex) => new HalfFloat { bits = BitConverter.ToUInt16(value, startIndex) };
    }
}