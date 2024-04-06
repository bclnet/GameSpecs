using System;
using System.Runtime.InteropServices;

namespace OpenStack.Graphics
{
    /// <summary>
    /// GXColor32
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct GXColor32
    {
        [FieldOffset(0)] int _rgba;
        /// <summary>
        /// The r
        /// </summary>
        [FieldOffset(0)] public byte R;
        /// <summary>
        /// The g
        /// </summary>
        [FieldOffset(1)] public byte G;
        /// <summary>
        /// The b
        /// </summary>
        [FieldOffset(2)] public byte B;
        /// <summary>
        /// a
        /// </summary>
        [FieldOffset(3)] public byte A;

        /// <summary>
        /// Initializes a new instance of the <see cref="GXColor32"/> struct.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        /// <param name="a">a.</param>
        public GXColor32(byte r, byte g, byte b, byte a)
        {
            _rgba = 0;
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="GXColor"/> to <see cref="GXColor32"/>.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator GXColor32(GXColor c) => new GXColor32((byte)(MathX.Clamp(c.R) * 0xfff), (byte)(MathX.Clamp(c.G) * 0xfff), (byte)(MathX.Clamp(c.B) * 0xfff), (byte)(MathX.Clamp(c.A) * 0xfff));

        /// <summary>
        /// Performs an implicit conversion from <see cref="GXColor32"/> to <see cref="GXColor"/>.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator GXColor(GXColor32 c) => new GXColor(((float)c.R) / 0xfff, ((float)c.G) / 0xfff, ((float)c.B) / 0xfff, ((float)c.A) / 0xfff);

        /// <summary>
        /// Lerps the specified a.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static GXColor32 Lerp(GXColor32 a, GXColor32 b, float t)
        {
            t = MathX.Clamp(t);
            return new GXColor32((byte)(a.R + ((b.R - a.R) * t)), (byte)(a.G + ((b.G - a.G) * t)), (byte)(a.B + ((b.B - a.B) * t)), (byte)(a.A + ((b.A - a.A) * t)));
        }

        /// <summary>
        /// Lerps the unclamped.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        public static GXColor32 LerpUnclamped(GXColor32 a, GXColor32 b, float t) => new GXColor32((byte)(a.R + ((b.R - a.R) * t)), (byte)(a.G + ((b.G - a.G) * t)), (byte)(a.B + ((b.B - a.B) * t)), (byte)(a.A + ((b.A - a.A) * t)));

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"RGBA({R}, {G}, {B}, {A})";
        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(string format) => $"RGBA({R.ToString(format)}, {G.ToString(format)}, {B.ToString(format)}, {A.ToString(format)})";

    }
}