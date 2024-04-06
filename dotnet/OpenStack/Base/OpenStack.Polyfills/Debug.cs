using System;

namespace OpenStack
{
    /// <summary>
    /// Debug
    /// </summary>
    public class Debug
    {
        public static Action<bool> AssertFunc;
        public static Action<string> LogFunc;
        public static Action<string, object[]> LogFormatFunc;
        public static void Assert(bool condition, string message = null) { } // => AssertFunc(condition);
        public static void Log(string format = null) => LogFunc(format);
        public static void LogFormat(string format, params object[] args) => LogFormatFunc(format, args);
    }
}