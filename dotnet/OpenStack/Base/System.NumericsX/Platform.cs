//#define FRUSTUM_DEBUG
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("OpenStack.XTests")]

namespace System.NumericsX
{
    public unsafe delegate T FloatPtr<T>(float* ptr);
    public unsafe delegate void FloatPtr(float* ptr);

    public unsafe static partial class Platform
    {
        //[MethodImpl(MethodImplOptions.AggressiveInlining)] public static float* _align16(float* value) => (float*)((nint)((byte*)value + 15) & ~15);
        //[MethodImpl(MethodImplOptions.AggressiveInlining)] public static byte* _align16(byte* value) => (byte*)((nint)(value + 15) & ~15);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static void* _alloca16(void* value) => (void*)((nint)((byte*)value + 15) & ~15);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Span<T> _alloca16T<T>(Span<T> value) => throw new NotImplementedException();

        // maxs
        public const int MAX_STRING_CHARS = 1024;
        public const int MAX_EXPRESSION_REGISTERS = 4096;

        // maximum world size
        public const int MAX_WORLD_COORD = 128 * 1024;
        public const int MIN_WORLD_COORD = -128 * 1024;
        public const int MAX_WORLD_SIZE = MAX_WORLD_COORD - MIN_WORLD_COORD;

        public static Action<bool> SetRefreshOnPrint = x => { };
        public static Action<string> Warning = x => Console.WriteLine(x);
        public static Action<string> Error = x => { Console.WriteLine(x); Debug.Assert(false); };
        public static Action<string> FatalError = x => { Console.WriteLine(x); throw new Exception(x); };
        public static Action<string> Printf = x => Console.Write(x);
        public static Func<string, string> LanguageDictGetString = x => x;

        internal static void RotateVector(ref Vector3 v, Vector3 origin, float a, float c, float s)
        {
            if (a != 0)
            {
                var x2 = ((v.x - origin.x) * c) - ((v.y - origin.y) * s) + origin.x;
                var y2 = ((v.x - origin.x) * s) + ((v.y - origin.y) * c) + origin.y;
                v.x = x2;
                v.y = y2;
            }
        }

        public static unsafe string FloatArrayToString(float* array, int length, int precision)
        {
            //static int index = 0;
            //static char str[4][16384];  // in case called by nested functions
            //int i, n;
            //char format[16], *s;

            //// use an array of string so that multiple calls won't collide
            //s = str[index];
            //index = (index + 1) & 3;

            //idStr::snPrintf(format, sizeof(format), "%%.%df", precision);
            //n = idStr::snPrintf(s, sizeof(str[0]), format, array[0]);
            //if (precision > 0)
            //{
            //    while (n > 0 && s[n - 1] == '0') s[--n] = '\0';
            //    while (n > 0 && s[n - 1] == '.') s[--n] = '\0';
            //}
            //idStr::snPrintf(format, sizeof(format), " %%.%df", precision);
            //for (i = 1; i < length; i++)
            //{
            //    n += idStr::snPrintf(s + n, sizeof(str[0]) - n, format, array[i]);
            //    if (precision > 0)
            //    {
            //        while (n > 0 && s[n - 1] == '0') s[--n] = '\0';
            //        while (n > 0 && s[n - 1] == '.') s[--n] = '\0';
            //    }
            //}
            //return s;
            return "STRING";
        }

#if FRUSTUM_DEBUG
        public static Func<int> r_showInteractionScissors_0;
        public static Func<int> r_showInteractionScissors_1;
#endif

        static unsafe void Init()
        {
            Debug.Assert(sizeof(bool) == 1);
            Debug.Assert(sizeof(float) == sizeof(int));
            Debug.Assert(sizeof(Vector3) == 3 * sizeof(float));

            // initialize generic SIMD implementation
            //SIMD.Init();

            // initialize math
            MathX.Init();

            // test idMatX
            //MatrixX.Test();

            // test idPolynomial
            //Polynomial.Test();
        }

        static void ShutDown()
        {
            // shut down the SIMD engine
            //SIMD.Shutdown();
        }

        public static void Swap<T>(ref T a, ref T b) { var c = a; a = b; b = c; }

        #region Endian

        static float FloatSwap(float f)
        {
            var dat = new reinterpret.F2ui { f = f };
            dat.u = BinaryPrimitives.ReverseEndianness(dat.u);
            return dat.f;
        }

        static unsafe void RevBytesSwap(void* bp, int elsize, int elcount)
        {
            byte* p, q;

            p = (byte*)bp;
            if (elsize == 2)
            {
                q = p + 1;
                while (elcount-- != 0)
                {
                    *p ^= *q;
                    *q ^= *p;
                    *p ^= *q;
                    p += 2;
                    q += 2;
                }
                return;
            }
            while (elcount-- != 0)
            {
                q = p + elsize - 1;
                while (p < q)
                {
                    *p ^= *q;
                    *q ^= *p;
                    *p ^= *q;
                    ++p;
                    --q;
                }
                p += elsize >> 1;
            }
        }

        static unsafe void RevBitFieldSwap(void* bp, int elsize)
        {
            int i; byte* p; byte t, v;

            LittleRevBytes(bp, elsize, 1);

            p = (byte*)bp;
            while (elsize-- != 0)
            {
                v = *p;
                t = 0;
                for (i = 7; i != 0; i--)
                {
                    t <<= 1;
                    v >>= 1;
                    t |= (byte)(v & 1);
                }
                *p++ = t;
            }
        }

        // little/big endian conversion
        public static bool IsBigEndian
#if !BIG_ENDIAN
            => false;
#else
	        => true;
#endif
        public static short BigShort(short l)
#if !BIG_ENDIAN
            => BinaryPrimitives.ReverseEndianness(l);
#else
            => l;
#endif
        public static short BigShort(ref short l)
#if !BIG_ENDIAN
            => l = BinaryPrimitives.ReverseEndianness(l);
#else
        { }
#endif
        public static short LittleShort(short l)
#if !BIG_ENDIAN
            => l;
#else
            => BinaryPrimitives.ReverseEndianness(l);
#endif
        public static void LittleShort(ref short l)
#if !BIG_ENDIAN
        { }
#else
            => l = BinaryPrimitives.ReverseEndianness(l);
#endif
        public static ushort LittleUShort(ushort l)
#if !BIG_ENDIAN
            => l;
#else
            => BinaryPrimitives.ReverseEndianness(l);
#endif
        public static void LittleUShort(ref ushort l)
#if !BIG_ENDIAN
        { }
#else
            => l = BinaryPrimitives.ReverseEndianness(l);
#endif
        public static int BigInt(int l)
#if !BIG_ENDIAN
            => BinaryPrimitives.ReverseEndianness(l);
#else
            => l;
#endif
        public static void BigInt(ref int l)
#if !BIG_ENDIAN
            => l = BinaryPrimitives.ReverseEndianness(l);
#else
        { }
#endif
        public static int LittleInt(int l)
#if !BIG_ENDIAN
            => l;
#else
            => BinaryPrimitives.ReverseEndianness(l);
#endif
        public static void LittleInt(ref int l)
#if !BIG_ENDIAN
        { }
#else
            => BinaryPrimitives.ReverseEndianness(l);
#endif

        public static uint LittleUInt(uint l)
#if !BIG_ENDIAN
            => l;
#else
            => BinaryPrimitives.ReverseEndianness(l);
#endif
        public static void LittleUInt(ref uint l)
#if !BIG_ENDIAN
        { }
#else
            => BinaryPrimitives.ReverseEndianness(l);
#endif

        public static float BigFloat(float l)
#if !BIG_ENDIAN
            => FloatSwap(l);
#else
	        => l;
#endif
        public static void BigFloat(ref float l)
#if !BIG_ENDIAN
            => l = FloatSwap(l);
#else
        { }
#endif
        public static float LittleFloat(float l)
#if !BIG_ENDIAN
            => l;
#else
	        => FloatSwap(l);
#endif
        public static void LittleFloat(ref float l)
#if !BIG_ENDIAN
        { }
#else
	        => l = FloatSwap(l);
#endif

        public static unsafe void BigRevBytes(void* bp, int elsize, int elcount)
#if !BIG_ENDIAN
           => RevBytesSwap(bp, elsize, elcount);
#else
            { }
#endif

        public static unsafe void LittleRevBytes(void* bp, int elsize, int elcount)
#if !BIG_ENDIAN
        { }
#else
	        => RevBytesSwap(bp, elsize, elcount);
#endif
        public static unsafe void LittleBitField(void* bp, int elsize)
#if !BIG_ENDIAN
        { }
#else
            => RevBitFieldSwap(bp, elsize);
#endif

        public static void LittleVector3(ref Vector3 l)
        {
#if !BIG_ENDIAN
#else
            l.x = FloatSwap(l.x); l.y = FloatSwap(l.y); l.z = FloatSwap(l.z);
#endif
        }

        #endregion
    }
}