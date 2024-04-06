using System.Linq;
using System.Numerics;
using System.Text;

namespace System.Collections.Generic
{
    public static class KVExtensions
    {
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, TValue defaultValue)
            => source.TryGetValue(key, out var z) ? z : defaultValue;

        public static bool TryGet<T>(this IDictionary<string, object> source, string name, out T value, T defaultValue = default)
        {
            if (!source.TryGetValue(name, out var z)) { value = defaultValue; return false; }
            if (!typeof(T).IsArray) { if (z is T v) { value = v; return true; } }
            else { if (z is Array a) { value = (T)a.CastToArray(typeof(T).GetElementType()); return true; } }
            value = defaultValue; return false;
        }

        public static T Get<T>(this IDictionary<string, object> source, string name, T defaultValue = default)
            => !source.TryGetValue(name, out var z) ? defaultValue
                : !typeof(T).IsArray
                ? z is T v ? v : defaultValue
                : z is Array a ? (T)a.CastToArray(typeof(T).GetElementType()) : defaultValue;

        public static IDictionary<string, object> GetSub(this IDictionary<string, object> source, string name)
            => source.TryGetValue(name, out var z) && z is IDictionary<string, object> v ? v : default;

        //public static T[] GetMap<T>(this IDictionary<string, object> collection, string name, Func<object, T> mapper)
        //    => collection.TryGetValue(name, out var z) && z is Array a ? a.Cast<object>().Select(mapper).ToArray() : default;

        public static int GetInt32(this IDictionary<string, object> source, string name, int defaultValue = default)
            => !source.TryGetValue(name, out var z) ? defaultValue
            : z is int v ? v : Convert.ToInt32(z);

        public static uint GetUInt32(this IDictionary<string, object> source, string name, uint defaultValue = default)
            => !source.TryGetValue(name, out var z) ? defaultValue
            : z is uint v ? v : Convert.ToUInt32(z);

        public static long GetInt64(this IDictionary<string, object> source, string name, long defaultValue = default)
            => !source.TryGetValue(name, out var z) ? defaultValue
            : z is long v ? v : Convert.ToInt64(z);
        public static long[] GetInt64Array(this IDictionary<string, object> source, string name)
            => source.TryGetValue(name, out var z) && z is Array v ? v.Cast<object>().Select(Convert.ToInt64).ToArray() : default;

        public static ulong GetUInt64(this IDictionary<string, object> source, string name, ulong defaultValue = default)
        {
            unchecked { return source.TryGetValue(name, out var value) ? value is int i ? (ulong)i : Convert.ToUInt64(value) : defaultValue; }
        }
        public static ulong[] GetUInt64Array(this IDictionary<string, object> source, string name)
            => source.TryGetValue(name, out var z) && z is Array v ? v.Cast<object>().Select(Convert.ToUInt64).ToArray() : default;

        public static double GetDouble(this IDictionary<string, object> source, string name, double defaultValue = default)
            => source.TryGetValue(name, out var z) ? Convert.ToDouble(z) : defaultValue;

        public static float GetFloat(this IDictionary<string, object> source, string name, float defaultValue = default)
            => (float)GetDouble(source, name, defaultValue);

        public static IDictionary<string, object>[] GetArray(this IDictionary<string, object> source, string name)
            => source.TryGetValue(name, out var z) && z is Array v ? v.Cast<IDictionary<string, object>>().ToArray() : default;

        public static T[] GetArray<T>(this IDictionary<string, object> source, string name, Func<IDictionary<string, object>, T> select)
            => source.TryGetValue(name, out var z) && z is Array v ? v.Cast<IDictionary<string, object>>().Select(select).ToArray() : default;

        public static Vector3 ToVector3(this object[] source) => new Vector3(
            (float)Convert.ToDouble(source[0]),
            (float)Convert.ToDouble(source[1]),
            (float)Convert.ToDouble(source[2]));
        public static Vector3 GetVector3(this IDictionary<string, object> source, string name)
            => source.TryGetValue(name, out var z) && z is object[] v && v.Length == 3 ? new Vector3(
            (float)Convert.ToDouble(v[0]),
            (float)Convert.ToDouble(v[1]),
            (float)Convert.ToDouble(v[2])) : default;

        public static Vector4 ToVector4(this object[] source) => new Vector4(
            (float)Convert.ToDouble(source[0]),
            (float)Convert.ToDouble(source[1]),
            (float)Convert.ToDouble(source[2]),
            (float)Convert.ToDouble(source[3]));
        public static Vector4 GetVector4(this IDictionary<string, object> source, string name)
            => source.TryGetValue(name, out var z) && z is object[] v && v.Length == 4 ? new Vector4(
            (float)Convert.ToDouble(v[0]),
            (float)Convert.ToDouble(v[1]),
            (float)Convert.ToDouble(v[2]),
            (float)Convert.ToDouble(v[3])) : default;

        public static Quaternion ToQuaternion(this object[] source) => new Quaternion(
            (float)Convert.ToDouble(source[0]),
            (float)Convert.ToDouble(source[1]),
            (float)Convert.ToDouble(source[2]),
            (float)Convert.ToDouble(source[3]));
        public static Quaternion GetQuaternion(this IDictionary<string, object> source, string name)
            => source.TryGetValue(name, out var z) && z is object[] v && v.Length == 4 ? new Quaternion(
            (float)Convert.ToDouble(v[0]),
            (float)Convert.ToDouble(v[1]),
            (float)Convert.ToDouble(v[2]),
            (float)Convert.ToDouble(v[3])) : default;

        public static Matrix4x4 ToMatrix4x4(this IDictionary<string, object>[] source)
        {
            throw new ArgumentException();
            //var column1 = source[0].ToVector4();
            //var column2 = source[1].ToVector4();
            //var column3 = source[2].ToVector4();
            //var column4 = source.Length > 3 ? source[3].ToVector4() : new Vector4(0, 0, 0, 1);
            //return new Matrix4x4(column1.X, column2.X, column3.X, column4.X, column1.Y, column2.Y, column3.Y, column4.Y, column1.Z, column2.Z, column3.Z, column4.Z, column1.W, column2.W, column3.W, column4.W);
        }

        public static string Print(IDictionary<string, object> source, int indent = 0)
        {
            var b = new StringBuilder();
            var space = new string(' ', indent * 4);
            foreach (var kvp in source)
                if (kvp.Value is IDictionary<string, object> nestedCollection) { b.AppendLine($"{space}{kvp.Key} = {{"); b.Append(Print(nestedCollection, indent + 1)); b.AppendLine($"{space}}}"); }
                else b.AppendLine($"{space}{kvp.Key} = {kvp.Value}");
            return b.ToString();
        }
    }
}
