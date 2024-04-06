using System.Globalization;

namespace System
{
    public static class ConvertX
    {
        public static bool ToBoolean(string value) { bool.TryParse(value, out var b); return b; }
        public static double ToDouble(string value) { double.TryParse(value, out var d); return d; }
        public static TimeSpan ToTimeSpan(string value) { TimeSpan.TryParse(value, out var t); return t; }
        public static int ToInt32(string value) { int i; if (value.StartsWith("0x")) int.TryParse(value[2..], NumberStyles.HexNumber, null, out i); else int.TryParse(value, out i); return i; }
    }
}