using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace GameSpec
{
    /// <summary>
    /// FamilyManager
    /// </summary>
    public partial class FamilyManager
    {
        public static readonly string ApplicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly Family Unknown;
        public static readonly PakFile UnknownPakFile;

        /// <summary>
        /// Search by.
        /// </summary>
        public enum SearchBy
        {
            None,
            Pak,
            TopDir,
            TwoDir,
            AllDir,
        }

        /// <summary>
        /// Game options.
        /// </summary>
        [Flags]
        public enum GameOption
        {
            //Paths = 0x1,
            //Stream = 0x2,
        }

        static readonly Func<string, Stream> GetManifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream;
        static string FamilyLoader(string path)
        {
            using var r = new StreamReader(GetManifestResourceStream($"GameSpec.Base.Families.{path}"));
            return r.ReadToEnd();
        }

        static FamilyManager()
        {
            Family.Bootstrap();
            foreach (var id in FamilyKeys)
            {
                var family = CreateFamily($"{id}Family.json", FamilyLoader);
                Families.Add(family.Id, family);
            }
            Unknown = GetFamily("Unknown");
            UnknownPakFile = Unknown.OpenPakFile(new Uri("game:/#APP"), throwOnError: false);
        }

        /// <summary>
        /// Gets the families.
        /// </summary>
        /// <value>
        /// The family.
        /// </value>
        public static readonly IDictionary<string, Family> Families = new Dictionary<string, Family>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the specified family.
        /// </summary>
        /// <param name="id">Name of the family.</param>
        /// <param name="throwOnError">Throw on error.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">estateName</exception>
        public static Family GetFamily(string id, bool throwOnError = true)
            => Families.TryGetValue(id, out var family) ? family
            : throwOnError ? throw new ArgumentOutOfRangeException(nameof(id), id) : (Family)default;

        #region Parse

        /// <summary>
        /// Parse Key.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static object ParseKey(string str)
        {
            if (string.IsNullOrEmpty(str)) { return null; }
            else if (str.StartsWith("hex:", StringComparison.OrdinalIgnoreCase))
            {
                var keyStr = str[4..];
                return keyStr.StartsWith("/")
                    ? Enumerable.Range(0, keyStr.Length >> 2).Select(x => byte.Parse(keyStr.Substring((x << 2) + 2, 2), NumberStyles.HexNumber)).ToArray()
                    : Enumerable.Range(0, keyStr.Length >> 1).Select(x => byte.Parse(keyStr.Substring(x << 1, 2), NumberStyles.HexNumber)).ToArray();
            }
            else if (str.StartsWith("txt:", StringComparison.OrdinalIgnoreCase))
            {
                var keyStr = str[4..];
                return Encoding.ASCII.GetBytes(keyStr);
            }
            else throw new ArgumentOutOfRangeException(nameof(str), str);
        }

        /// <summary>
        /// Create family.
        /// </summary>
        /// <param name="any"></param>
        /// <param name="loader"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static Family CreateFamily(string any, Func<string, string> loader = null)
        {
            var json = loader != null ? loader(any) : any;
            if (string.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));
            using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
            var elem = doc.RootElement;
            var familyType = elem.TryGetProperty("familyType", out var z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("familyType", $"Unknown type: {z}") : default;
            var family = familyType != null ? (Family)Activator.CreateInstance(familyType, elem) : new Family(elem);
            if (family.Specs != null)
                foreach (var spec in family.Specs)
                    family.Merge(CreateFamily(spec, loader));
            return family;
        }

        /// <summary>
        /// Create family engine.
        /// </summary>
        /// <param name="family"></param>
        /// <param name="id"></param>
        /// <param name="elem"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static FamilyEngine CreateFamilyEngine(Family family, string id, JsonElement elem)
        {
            var engineType = elem.TryGetProperty("engineType", out var z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("engineType", $"Unknown type: {z}") : default;
            return engineType != null ? (FamilyEngine)Activator.CreateInstance(engineType, family, id, elem) : new FamilyEngine(family, id, elem);
        }

        /// <summary>
        /// Create family engine.
        /// </summary>
        /// <param name="family"></param>
        /// <param name="id"></param>
        /// <param name="elem"></param>
        /// <param name="dgame"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static FamilyGame CreateFamilyGame(Family family, string id, JsonElement elem, ref FamilyGame dgame, IDictionary<string, HashSet<string>> paths)
        {
            var gameType = elem.TryGetProperty("gameType", out var z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("gameType", $"Unknown type: {z}") : dgame?.GameType;
            var game = gameType != null ? (FamilyGame)Activator.CreateInstance(gameType, family, id, elem, dgame) : new FamilyGame(family, id, elem, dgame);
            game.GameType = gameType;
            game.Found = paths != null && paths.ContainsKey(id);
            if (id.StartsWith("*")) { dgame = game; return null; }
            return game;
        }

        /// <summary>
        /// Create family app.
        /// </summary>
        /// <param name="family"></param>
        /// <param name="id"></param>
        /// <param name="elem"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static FamilyApp CreateFamilyApp(Family family, string id, JsonElement elem)
        {
            var appType = elem.TryGetProperty("appType", out var z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("appType", $"Unknown type: {z}") : default;
            return appType != null ? (FamilyApp)Activator.CreateInstance(appType, family, id, elem) : new FamilyApp(family, id, elem);
        }

        /// <summary>
        /// Create filemanager.
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        internal static FileManager CreateFileManager(JsonElement elem) => new FileManager(elem);

        #endregion
    }
}