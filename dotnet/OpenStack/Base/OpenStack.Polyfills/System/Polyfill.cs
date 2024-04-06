using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace System
{
    public static class Polyfill
    {
        #region Convert Color

        public static uint FromBGR555(this ushort bgr555, bool addAlpha = true)
        {
            var a = addAlpha ? (byte)0xFF : (byte)0;
            var r = (byte)Math.Min(((bgr555 & 0x7C00) >> 10) * 8, byte.MaxValue);
            var g = (byte)Math.Min(((bgr555 & 0x03E0) >> 5) * 8, byte.MaxValue);
            var b = (byte)Math.Min(((bgr555 & 0x001F) >> 0) * 8, byte.MaxValue);
            var color =
                ((uint)(a << 24) & 0xFF000000) |
                ((uint)(r << 16) & 0x00FF0000) |
                ((uint)(g << 8) & 0x0000FF00) |
                ((uint)(b << 0) & 0x000000FF);
            return color;
        }

        #endregion

        #region Sequence

        //public static T Last<T>(this T[] source) => source[^1];
        //public static T Last<T>(this List<T> source) => source[^1];

        /// <summary>
        /// Calculates the minimum and maximum values of an array.
        /// </summary>
        public static void GetExtrema(this float[] source, out float min, out float max)
        {
            min = float.MaxValue; max = float.MinValue;
            foreach (var element in source) { min = Math.Min(min, element); max = Math.Max(max, element); }
        }
        /// <summary>
        /// Calculates the minimum and maximum values of a 2D array.
        /// </summary>
        public static void GetExtrema(this float[,] source, out float min, out float max)
        {
            min = float.MaxValue; max = float.MinValue;
            foreach (var element in source) { min = Math.Min(min, element); max = Math.Max(max, element); }
        }
        /// <summary>
        /// Calculates the minimum and maximum values of a 3D array.
        /// </summary>
        public static void GetExtrema(this float[,,] source, out float min, out float max)
        {
            min = float.MaxValue; max = float.MinValue;
            foreach (var element in source) { min = Math.Min(min, element); max = Math.Max(max, element); }
        }

        #endregion

        public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var memInfo = enumVal.GetType().GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return attributes.Length > 0 ? (T)attributes[0] : null;
        }

        /// <summary>
        /// Returns a list of flags for enum
        /// </summary>
        public static List<Enum> GetFlags(this Enum e)
            => Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag).ToList();

        /// <summary>
        /// Returns the # of bits set in a Flags enum
        /// </summary>
        /// <param name="enumVal">The enum uint value</param>
        public static int EnumNumFlags(uint enumVal)
        {
            var cnt = 0;
            while (enumVal != 0) { enumVal &= enumVal - 1; cnt++; } // remove the next set bit
            return cnt;
        }

        /// <summary>
        /// Returns TRUE if this flags enum has multiple flags set
        /// </summary>
        /// <param name="enumVal">The enum uint value</param>
        public static bool EnumHasMultiple(uint enumVal)
            => (enumVal & (enumVal - 1)) != 0;

        public static string GetEnumDescription(this Type source, string value)
        {
            var name = Enum.GetNames(source).FirstOrDefault(f => f.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (name == null) return string.Empty;
            var field = source.GetField(name);
            var attributes = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static bool Equals(this string source, byte[] bytes)
        {
            if (bytes.Length != source.Length) return false;
            for (var i = 0; i < bytes.Length; i++) if (bytes[i] != source[i]) return false;
            return true;
        }

        static readonly MethodInfo Enumerable_CastMethod = typeof(Enumerable).GetMethod("Cast");
        static readonly MethodInfo Enumerable_ToArrayMethod = typeof(Enumerable).GetMethod("ToArray");

        /// <summary>
        /// Casts to array.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static object CastToArray(this IEnumerable source, Type type)
            => Enumerable_ToArrayMethod.MakeGenericMethod(type).Invoke(null, new[] { Enumerable_CastMethod.MakeGenericMethod(type).Invoke(null, new[] { source }) });

        public static string Reverse(this string s)
        {
            var charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}