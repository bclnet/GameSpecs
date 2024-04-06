using System.Runtime.InteropServices;

namespace OpenStack.Graphics.Algorithms
{
    public static class HalfPrecConverter
    {
        [StructLayout(LayoutKind.Explicit)]
        struct Union
        {
            [FieldOffset(0)] public float Single;
            [FieldOffset(0)] public int Int32;
            [FieldOffset(0)] public uint UInt32;
        }

        public static ushort ToShort(float value)
            => ToSingle(new Union { Single = value }.Int32);

        static ushort ToSingle(int value)
        {
            var s = (value >> 16) & 0x00008000;
            var e = ((value >> 23) & 0x000000ff) - (127 - 15);
            var m = value & 0x007fffff;

            if (e <= 0)
            {
                if (e < -10) return (ushort)s;
                m |= 0x00800000;
                int t = 14 - e, a = (1 << (t - 1)) - 1, b = (m >> t) & 1;
                m = (m + a + b) >> t;
                return (ushort)(s | m);
            }
            else if (e == 0xff - (127 - 15))
            {
                if (m == 0) return (ushort)(s | 0x7c00);
                else { m >>= 13; return (ushort)(s | 0x7c00 | m | (m == 0 ? 1 : 0)); }
            }
            else
            {
                m = m + 0x00000fff + ((m >> 13) & 1);
                if ((m & 0x00800000) != 0) { m = 0; e += 1; }
                if (e > 30) return (ushort)(s | 0x7c00);
                return (ushort)(s | (e << 10) | (m >> 13));
            }
        }

        public static float ToSingle(ushort value)
        {
            uint r;
            var e = 0xfffffff2;
            var m = (uint)(value & 1023);
            if ((value & -33792) == 0)
            {
                if (m != 0)
                {
                    while ((m & 1024) == 0) { e--; m <<= 1; }
                    m &= 0xfffffbff;
                    r = (((uint)value & 0x8000) << 16) | ((e + 127) << 23) | (m << 13);
                }
                else r = (uint)((value & 0x8000) << 16);
            }
            else r = (((uint)value & 0x8000) << 16) | (((((uint)value >> 10) & 0x1f) - 15 + 127) << 23) | (m << 13);
            return new Union { UInt32 = r }.Single;
        }
    }
}
