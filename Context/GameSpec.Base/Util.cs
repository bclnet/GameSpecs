using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace GameSpec
{
    public static class Util
    {
        // value
        public static T _value<T>(JsonElement elem, string key, Func<JsonElement, T> func, T default_ = default)
            => elem.TryGetProperty(key, out var z) ? func(z) : default_;
        public static string _value(JsonElement elem, string key, string default_ = default)
            => elem.TryGetProperty(key, out var z) ? z.GetString() : default_;
        public static bool _valueBool(JsonElement elem, string key, bool default_ = default)
            => elem.TryGetProperty(key, out var z) ? z.GetBoolean() : default_;

        // list
        public static T[] _list<T>(JsonElement elem, string key, Func<string, T> func, T[] default_ = default)
            => elem.TryGetProperty(key, out var z) ? z.GetStringOrArray(func) : default_;
        public static string[] _list(JsonElement elem, string key, string[] default_ = default)
            => elem.TryGetProperty(key, out var z) ? z.GetStringOrArray(x => x) : default_;

        // method
        public static TResult _method<TResult>(JsonElement elem, string key, Func<JsonElement, TResult> func, TResult default_ = default)
            => elem.TryGetProperty(key, out var z) ? func(z) : default_;

        // related
        public static Dictionary<string, T> _related<T>(JsonElement elem, string key, Func<string, JsonElement, T> func)
            => elem.TryGetProperty(key, out var z) ? z.EnumerateObject().ToDictionary(x => x.Name, x => func(x.Name, x.Value)) : new Dictionary<string, T>();
        public static Dictionary<string, T> _relatedTrim<T>(JsonElement elem, string key, Func<string, JsonElement, T> func)
            => elem.TryGetProperty(key, out var z) ? z.EnumerateObject().ToDictionary(x => x.Name, x => func(x.Name, x.Value)).Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value) : new Dictionary<string, T>();
        
    }
}