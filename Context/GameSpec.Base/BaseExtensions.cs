using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace GameSpec
{
    public static class BaseExtensions
    {
        public static IList<string> GetStringOrArray(this JsonElement source)
            => source.ValueKind switch
            {
                JsonValueKind.Number => new[] { source.GetInt32().ToString() },
                JsonValueKind.String => new[] { source.GetString() },
                JsonValueKind.Array => source.EnumerateArray().Select(y => y.GetString()).ToArray(),
                _ => throw new ArgumentOutOfRangeException($"{source}"),
            };

        public static IList<T> GetStringOrArray<T>(this JsonElement source, Func<string, T> func)
            => source.ValueKind switch
            {
                JsonValueKind.Number => new[] { func(source.GetInt32().ToString()) },
                JsonValueKind.String => new[] { func(source.GetString()) },
                JsonValueKind.Array => source.EnumerateArray().Select(y => func(y.GetString())).ToArray(),
                _ => throw new ArgumentOutOfRangeException($"{source}"),
            };
    }
}
