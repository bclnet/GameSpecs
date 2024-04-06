using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace GameX
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
        public static object _valueV(JsonElement elem)
            => elem.ValueKind switch
            {
                JsonValueKind.Number => elem.GetInt32(),
                JsonValueKind.String => elem.GetString(),
                JsonValueKind.Array => elem.EnumerateArray().Select(y => y.GetString()).ToArray(),
                _ => throw new ArgumentOutOfRangeException($"{elem}"),
            };

        // list
        public static T[] _list<T>(JsonElement elem, string key, Func<string, T> func, T[] default_ = default)
            => elem.TryGetProperty(key, out var z) ? _listV(z, func) : default_;
        public static string[] _list(JsonElement elem, string key, string[] default_ = default)
            => elem.TryGetProperty(key, out var z) ? _listV(z, x => x) : default_;
        public static T[] _listV<T>(JsonElement elem, Func<string, T> func)
            => elem.ValueKind switch
            {
                JsonValueKind.Number => new[] { func(elem.GetInt32().ToString()) },
                JsonValueKind.String => new[] { func(elem.GetString()) },
                JsonValueKind.Array => elem.EnumerateArray().Select(y => func(y.GetString())).ToArray(),
                _ => throw new ArgumentOutOfRangeException($"{elem}"),
            };
        public static string[] _listV(JsonElement elem)
            => elem.ValueKind switch
            {
                JsonValueKind.Number => new[] { elem.GetInt32().ToString() },
                JsonValueKind.String => new[] { elem.GetString() },
                JsonValueKind.Array => elem.EnumerateArray().Select(y => y.GetString()).ToArray(),
                _ => throw new ArgumentOutOfRangeException($"{elem}"),
            };

        // method
        public static TResult _method<TResult>(JsonElement elem, string key, Func<JsonElement, TResult> func, TResult default_ = default)
            => elem.TryGetProperty(key, out var z) ? func(z) : default_;

        // related
        public static Dictionary<string, T> _related<T>(JsonElement elem, string key, Func<string, JsonElement, T> func)
            => elem.TryGetProperty(key, out var z) ? z.EnumerateObject().ToDictionary(x => x.Name, x => func(x.Name, x.Value)) : new Dictionary<string, T>();

        public static Dictionary<string, T> _related<T>(JsonElement elem, string key, Func<JsonElement, string> keyFunc, Func<JsonElement, T> valueFunc)
            => elem.TryGetProperty(key, out var z) ? z.EnumerateArray().ToDictionary(x => keyFunc(x), x => valueFunc(x)) : new Dictionary<string, T>();

        public static Dictionary<string, T> _dictTrim<T>(Dictionary<string, T> source)
            => source.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value);

        static Random _random;
        public static int _randomValue(int low, int high)
        {
            _random ??= new Random();
            return _random.Next(low, high + 1);
        }

        // _guessExtension
        public static string _guessExtension(byte[] buf, bool fast = true)
            => buf.Length < 4 ? string.Empty
            : fast ? BitConverter.ToUInt32(buf, 0) switch
            {
                0x75B22630 => ".asf",
                _ => $".{Encoding.ASCII.GetString(buf.AsSpan(0, 3)).ToLowerInvariant()}",
            }
            : BitConverter.ToUInt32(buf, 0) switch
            {
                0x000001D8 => ".motlist",
                0x00424956 => ".vib",
                0x00444957 => ".wid",
                0x00444F4C => ".lod",
                0x00444252 => ".rbd",
                0x004C4452 => ".rdl",
                0x00424650 => ".pfb",
                0x00464453 => ".mmtr",
                0x0046444D => ".mdf2",
                0x004C4F46 => ".fol",
                0x004E4353 => ".scn",
                0x004F4C43 => ".clo",
                0x00504D4C => ".lmp",
                0x00535353 => ".sss",
                0x00534549 => ".ies",
                0x00530040 => ".wel",
                0x00584554 => ".tex",
                0x00525355 => ".user",
                0x005A5352 => ".wcc",
                0x04034B50 => ".zip",
                0x4D534C43 => ".clsm",
                0x54414D2E => ".mat",
                0x54464453 => ".sdft",
                0x44424453 => ".sdbd",
                0x52554653 => ".sfur",
                0x464E4946 => ".finf",
                0x4D455241 => ".arem",
                0x21545353 => ".sst",
                0x204D4252 => ".rbm",
                0x4D534648 => ".hfsm",
                0x59444F42 => ".rdd",
                0x20464544 => ".def",
                0x4252504E => ".nprb",
                0x44484B42 => ".bnk",
                0x75B22630 => ".mov",
                0x4853454D => ".mesh",
                0x4B504B41 => ".pck",
                0x50534552 => ".spmdl",
                0x54564842 => ".fsmv2",
                0x4C4F4352 => ".rcol",
                0x5556532E => ".uvs",
                0x4C494643 => ".cfil",
                0x54504E47 => ".gnpt",
                0x54414D43 => ".cmat",
                0x44545254 => ".trtd",
                0x50494C43 => ".clip",
                0x564D4552 => ".mov",
                0x414D4941 => ".aimapattr",
                0x504D4941 => ".aimp",
                0x72786665 => ".efx",
                0x736C6375 => ".ucls",
                0x54435846 => ".fxct",
                0x58455452 => ".rtex",
                0x4F464246 => ".oft",
                0x4C4F434D => ".mcol",
                0x46454443 => ".cdef",
                0x504F5350 => ".psop",
                0x454D414D => ".mame",
                0x43414D4D => ".mameac",
                0x544C5346 => ".fslt",
                0x64637273 => ".srcd",
                0x68637273 => ".asrc",
                0x4F525541 => ".auto",
                0x7261666C => ".lfar",
                0x52524554 => ".terr",
                0x736E636A => ".jcns",
                0x6C626C74 => ".tmlbld",
                0x54455343 => ".cset",
                0x726D6565 => ".eemr",
                0x434C4244 => ".dblc",
                0x384D5453 => ".stmesh",
                0x32736674 => ".tmlfsm2",
                0x45555141 => ".aque",
                0x46554247 => ".gbuf",
                0x4F4C4347 => ".gclo",
                0x44525453 => ".srtd",
                0x544C4946 => ".filt",
                _ => (buf.Length < 8 ? 0U : BitConverter.ToUInt32(buf, 4)) switch
                {
                    0x00766544 => ".dev",
                    0x6E616863 => ".chain",
                    0x6E6C6B73 => ".fbxskel",
                    0x47534D47 => ".msg",
                    0x52495547 => ".gui",
                    0x47464347 => ".gcfg",
                    0x72617675 => ".uvar",
                    0x544E4649 => ".ifnt",
                    0x20746F6D => ".mot",
                    0x70797466 => ".mov",
                    0x6D61636D => ".mcam",
                    0x6572746D => ".mtre",
                    0x6D73666D => ".mfsm",
                    0x74736C6D => ".motlist",
                    0x6B6E626D => ".motbank",
                    0x3273666D => ".motfsm2",
                    0x74736C63 => ".mcamlist",
                    0x70616D6A => ".jmap",
                    0x736E636A => ".jcns",
                    0x4E414554 => ".tean",
                    0x61646B69 => ".ikda",
                    0x736C6B69 => ".ikls",
                    0x72746B69 => ".iktr",
                    0x326C6B69 => ".ikl2",
                    0x72686366 => ".fchr",
                    0x544C5346 => ".fslt",
                    0x6B6E6263 => ".cbnk",
                    0x30474154 => ".havokcl",
                    0x52504347 => ".gcpr",
                    0x74646366 => ".fcmndatals",
                    0x67646C6A => ".jointlodgroup",
                    0x444E5347 => ".gsnd",
                    0x59545347 => ".gsty",
                    0x3267656C => ".leg2",
                    _ => $".{Encoding.ASCII.GetString(buf.AsSpan(0, 3)).ToLowerInvariant()}",
                },
            };
    }
}