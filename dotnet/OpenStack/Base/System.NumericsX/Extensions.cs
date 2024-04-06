using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace System.NumericsX
{
    public static class intX
    {
        public const int ALLOC16 = 4;

        public static int MulDiv(int number, int numerator, int denominator)
            => (int)(((long)number * numerator + (denominator >> 1)) / denominator);

        public static int Parse(string s)
            => int.TryParse(s, out var z) ? z : 0;
    }

    public static class floatX
    {
        public const int ALLOC16 = 4;

        public static float Parse(string s)
            => float.TryParse(s, out var z) ? z : 0f;
    }

    public static class boolX
    {
        public const int ALLOC16 = 15;
    }

    public unsafe static class byteX
    {
        //public static readonly byte[] Empty = Array.Empty<byte>();
        public static readonly byte* empty = (byte*)-1;

        public const int ALLOC16 = 15;

        public static int MD5Checksum(byte[] buffer)
        {
            using var md5 = MD5.Create();
            var digest = md5.ComputeHash(buffer);
            return digest[0] ^ digest[1] ^ digest[2] ^ digest[3];
        }
    }

    #region stringX

    public unsafe static class stringX
    {
        #region Color

        static readonly Vector4[] ColorTable =
        {
            new(0f, 0f, 0f, 1f),
            new(1f, 0f, 0f, 1f), // S_COLOR_RED
	        new(0f, 1f, 0f, 1f), // S_COLOR_GREEN
	        new(1f, 1f, 0f, 1f), // S_COLOR_YELLOW
	        new(0f, 0f, 1f, 1f), // S_COLOR_BLUE
	        new(0f, 1f, 1f, 1f), // S_COLOR_CYAN
	        new(1f, 0f, 1f, 1f), // S_COLOR_MAGENTA
	        new(1f, 1f, 1f, 1f), // S_COLOR_WHITE
	        new(0.5f, 0.5f, 0.5f, 1f), // S_COLOR_GRAY
	        new(0f, 0f, 0f, 1f), // S_COLOR_BLACK
	        new(0f, 0f, 0f, 1f),
            new(0f, 0f, 0f, 1f),
            new(0f, 0f, 0f, 1f),
            new(0f, 0f, 0f, 1f),
            new(0f, 0f, 0f, 1f),
            new(0f, 0f, 0f, 1f),
        };

        public static bool IsColor(byte* s, void* till) => s[0] == '^' && s != till && s[1] != ' ';
        public static bool IsColor(byte[] s, int offset) => s[offset + 0] == '^' && s.Length < offset && s[offset + 1] != ' ';
        public static bool IsColor(StringBuilder s, int offset) => s[offset + 0] == '^' && s.Length < offset && s[offset + 1] != ' ';
        public static bool IsColor(string s, int offset) => s[offset + 0] == '^' && s.Length < offset && s[offset + 1] != ' ';

        public static int ColorIndex(int c) => c & 15;

        public static Vector4 ColorForIndex(int i) => ColorTable[i & 15];

        public static string RemoveColors(string s)
        {
            //char* d;
            //char* s2;
            //int c;

            //s2 = s;
            //d = s;
            //while ((c = *s2) != 0)
            //{
            //    if (IsColor(s, s2)) s2++;
            //    else *d++ = c;
            //    s2++;
            //}
            //*d = '\0';

            return s;
        }

        public static int LengthWithoutColors(string s)
        {
            if (s == null) return 0;
            int sourceLength = s.Length, len = 0, p = 0;
            while (p < sourceLength)
            {
                if (IsColor(s, p)) { p += 2; continue; }
                p++; len++;
            }
            return len;
        }

        #endregion

        public static string StripQuotes(string s)
            => s[0] != '\"' ? s
            : s[^1] == '\"' ? s[1..^1]
            : s.Remove(0, 1);

        /// <summary>
        /// Safe strncpy that ensures a trailing zero
        /// </summary>
        /// <param name="dest">The dest.</param>
        /// <param name="src">The source.</param>
        /// <param name="destsize">The destsize.</param>
        //public static void Copynz_(byte[] dest, byte[] src, int destsize)
        //{
        //    if (src != null) { Warning("Str::Copynz: NULL src"); return; }
        //    if (destsize < 1) { Warning("Str::Copynz: destsize < 1"); return; }
        //    Unsafe.CopyBlock(ref dest[0], ref src[0], (uint)destsize - 1);
        //    dest[destsize - 1] = 0;
        //}

        public static bool CharIsPrintable(char key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether the specified s is numeric.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns>
        ///   <c>true</c> if the specified s is numeric; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNumeric(string s)
        {
            var i = 0;
            if (s[i] == '-') i++;
            var dot = false;
            for (; i < s.Length; i++)
            {
                var c = s[i];
                if (!char.IsDigit(c))
                {
                    if (c == '.' && !dot) { dot = true; continue; }
                    return false;
                }
            }
            return true;
        }

        #region Measure

        public enum MEASURE
        {
            SIZE = 0,
            BANDWIDTH
        }

        static string[,] MEASURE_Units =
        {
            { "B", "KB", "MB", "GB" },
            { "B/s", "KB/s", "MB/s", "GB/s" }
        };

        public static int BestUnit(out string s, string format, float value, MEASURE measure)
        {
            var unit = 1;
            while (unit <= 3 && (1 << (unit * 10) < value)) unit++;
            unit--;
            value /= 1 << (unit * 10);
            s = $"{string.Format(format, value)} {MEASURE_Units[(int)measure, unit]}";
            return unit;
        }

        public static void SetUnit(out string s, string format, float value, int unit, MEASURE measure)
        {
            value /= 1 << (unit * 10);
            s = $"{string.Format(format, value)} {MEASURE_Units[(int)measure, unit]}";
        }

        public static unsafe int strlen(byte* buf)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    #endregion

    public static class Extensions
    {
        static readonly FieldInfo ItemsField = typeof(List<>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance);

        #region List

        public static ref T Ref<T>(this List<T> source, int index)
            => ref ((T[])ItemsField.GetValue(source))[index];

        public static int Add_<T>(this List<T> source, T item)
        {
            source.Add(item);
            return source.Count - 1;
        }

        public static int AddUnique<T>(this List<T> source, T item)
        {
            var index = source.FindIndex(x => x.Equals(item));
            if (index < 0) index = source.Add_(item);
            return index;
        }

        public static T[] SetNum<T>(this List<T> source, int newNum, bool resize = true)
        {
            source.Capacity = newNum;
            return (T[])ItemsField.GetValue(source);
        }

        public static void SetGranularity<T>(this List<T> source, int granularity)
        {
        }

        public static void Resize<T>(this List<T> source, int newSize)
        {
        }

        public static void Resize<T>(this List<T> source, int newSize, int newGranularity)
        {
        }

        public static void AssureSize<T>(this List<T> source, int newSize)
        {
        }

        public static T[] Ptr<T>(this List<T> source)
            => (T[])ItemsField.GetValue(source);

        public static Span<T> Ptr<T>(this List<T> source, int startIndex)
            => ((T[])ItemsField.GetValue(source)).AsSpan(startIndex);

        #endregion
    }
}