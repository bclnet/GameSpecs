using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using static GameSpec.FamilyGame;

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
            Pak,
            TopDir,
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

        /// <summary>
        /// Default Options for Applications.
        /// </summary>
        public class DefaultOptions
        {
            public string Family { get; set; }
            public string GameId { get; set; }
            public string ForcePath { get; set; }
            public bool ForceOpen { get; set; }
        }

        static FamilyManager()
        {
            Family.Bootstrap();
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var id in FamilyKeys)
                using (var r = new StreamReader(assembly.GetManifestResourceStream($"GameSpec.Base.Families.{id}Family.json")))
                {
                    var family = ParseFamily(r.ReadToEnd());
                    if (family != null) Families.Add(family.Id, family);
                }
            Unknown = GetFamily("Unknown");
            UnknownPakFile = Unknown.OpenPakFile(new Uri("game:/#App"), throwOnError: false);
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
        /// <param name="familyName">Name of the family.</param>
        /// <param name="throwOnError">Throw on error.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">estateName</exception>
        public static Family GetFamily(string familyName, bool throwOnError = true)
            => Families.TryGetValue(familyName, out var family) ? family
            : throwOnError ? throw new ArgumentOutOfRangeException(nameof(familyName), familyName) : (Family)null;

        #region Pak

        /// <summary>
        /// Adds the platform graphic.
        /// </summary>
        /// <param name="pakFile">The pak file.</param>
        /// <returns></returns>
        static PakFile WithPlatformGraphic(PakFile pakFile)
        {
            if (pakFile != null) pakFile.Graphic = FamilyPlatform.GraphicFactory?.Invoke(pakFile);
            return pakFile;
        }

        /// <summary>
        /// Create pak file.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="fileSystem">The fileSystem.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        internal static PakFile CreatePakFile(FamilyGame game, IFileSystem fileSystem, string searchPattern, bool throwOnError)
        {
            if (fileSystem is HostFileSystem k) throw new NotImplementedException($"{k}"); //return new StreamPakFile(family.FileManager.HostFactory, game, path, fileSystem),
            searchPattern = game.CreateSearchPatterns(searchPattern) ?? (throwOnError ? throw new InvalidOperationException($"{game.Id} missing PakExts") : (string)null);
            if (searchPattern == null) return null;
            var pakFiles = new List<PakFile>();
            var fileManager = game.Family.FileManager;
            foreach (var p in fileManager.GetGamePaths(game, fileSystem, searchPattern, throwOnError))
                switch (game.SearchBy)
                {
                    case SearchBy.Pak:
                        foreach (var path in p.paths)
                            if (game.IsPakFile(path)) pakFiles.Add(game.CreatePakFile(fileSystem, (object)path));
                        break;
                    case SearchBy.AllDir:
                    case SearchBy.TopDir:
                        pakFiles.Add(game.CreatePakFile(fileSystem, p));
                        break;
                    default: throw new ArgumentOutOfRangeException(nameof(game.SearchBy), $"{game.SearchBy}");
                }
            return WithPlatformGraphic(game.CreatePakFile(fileSystem, pakFiles));
        }

        #endregion

        #region Parse

        /// <summary>
        /// Create family manager.
        /// </summary>
        internal static FileManager CreateFileManager() => new FileManager();

        /// <summary>
        /// Parses the family.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">pakFileType</exception>
        /// <exception cref="ArgumentNullException">games</exception>
        public static Family ParseFamily(string json)
        {
            if (string.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));
            var fileManager = CreateFileManager();
            var options = new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip };
            try
            {
                FamilyGame dgame = null;
                using var doc = JsonDocument.Parse(json, options);
                var elem = doc.RootElement;
                if (elem.TryGetProperty("fileManager", out var z)) fileManager.ParseFileManager(z);
                var paths = fileManager.Paths;
                var familyType = elem.TryGetProperty("familyType", out z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("familyType", $"Unknown type: {z}") : null;
                var family = familyType != null ? (Family)Activator.CreateInstance(familyType) : new Family();
                family.Id = (elem.TryGetProperty("id", out z) ? z.GetString() : null) ?? throw new ArgumentNullException("id");
                family.Name = (elem.TryGetProperty("name", out z) ? z.GetString() : null) ?? throw new ArgumentNullException("name");
                family.Studio = (elem.TryGetProperty("studio", out z) ? z.GetString() : null) ?? string.Empty;
                family.Description = elem.TryGetProperty("description", out z) ? z.GetString() : string.Empty;
                family.FileManager = fileManager;
                family.Engines = elem.TryGetProperty("engines", out z) ? z.EnumerateObject().ToDictionary(x => x.Name, x => ParseEngine(family, x.Name, x.Value), StringComparer.OrdinalIgnoreCase) : null;
                family.Games = elem.TryGetProperty("games", out z) ? z.EnumerateObject().Select(x => (x.Name, Value: ParseGame(ref dgame, family, paths, x.Name, x.Value))).Where(x => x.Value != null).ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase) : throw new ArgumentNullException("games");
                family.Apps = elem.TryGetProperty("apps", out z) ? z.EnumerateObject().ToDictionary(x => x.Name, x => ParseApp(family, x.Name, x.Value), StringComparer.OrdinalIgnoreCase) : null;
                return family;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        static bool TryParseKey(string str, out object value)
        {
            if (string.IsNullOrEmpty(str)) { value = null; return false; }
            if (str.StartsWith("aes:", StringComparison.OrdinalIgnoreCase))
            {
                var keyStr = str[4..];
                var key = keyStr.StartsWith("/")
                    ? Enumerable.Range(0, keyStr.Length >> 2).Select(x => byte.Parse(keyStr.Substring((x << 2) + 2, 2), NumberStyles.HexNumber)).ToArray()
                    : Enumerable.Range(0, keyStr.Length >> 1).Select(x => byte.Parse(keyStr.Substring(x << 1, 2), NumberStyles.HexNumber)).ToArray();
                value = new Family.ByteKey { Key = key };
            }
            else if (str.StartsWith("txt:", StringComparison.OrdinalIgnoreCase))
            {
                var keyStr = str[4..];
                value = new Family.ByteKey { Key = Encoding.ASCII.GetBytes(keyStr) };
            }
            else throw new ArgumentOutOfRangeException(nameof(str), str);
            return true;
        }

        static FamilyEngine ParseEngine(Family family, string id, JsonElement elem)
        {
            var engineType = elem.TryGetProperty("engineType", out var z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("engineType", $"Unknown type: {z}") : null;
            var engine = engineType != null ? (FamilyEngine)Activator.CreateInstance(engineType) : new FamilyEngine();
            engine.Family = family;
            engine.Id = id;
            engine.Name = (elem.TryGetProperty("name", out z) ? z.GetString() : null) ?? throw new ArgumentNullException("name");
            return engine;
        }

        static FamilyGame ParseGame(ref FamilyGame dgame, Family family, IDictionary<string, HashSet<string>> locations, string id, JsonElement elem)
        {
            var gameType = elem.TryGetProperty("gameType", out var z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("gameType", $"Unknown type: {z}") : dgame?.GameType;
            var game = gameType != null ? (FamilyGame)Activator.CreateInstance(gameType) : new FamilyGame();
            game.GameType = gameType;
            game.Family = family;
            game.Id = id;
            game.Ignore = elem.TryGetProperty("n/a", out z) ? z.GetBoolean() : dgame != null ? dgame.Ignore : false;
            game.Name = elem.TryGetProperty("name", out z) ? z.GetString() : null;
            game.Engine = elem.TryGetProperty("engine", out z) ? z.GetString() : dgame?.Engine;
            game.SearchBy = elem.TryGetProperty("searchBy", out z) ? Enum.TryParse<SearchBy>(z.GetString(), true, out var zS) ? zS : throw new ArgumentOutOfRangeException("searchBy", $"Unknown option: {z}") : dgame != null ? dgame.SearchBy : default;
            game.Option = elem.TryGetProperty("option", out z) ? Enum.TryParse<GameOption>(z.GetString(), true, out var zT) ? zT : throw new ArgumentOutOfRangeException("option", $"Unknown option: {z}") : dgame != null ? dgame.Option : default;
            game.PakFileType = elem.TryGetProperty("pakFileType", out z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("pakFileType", $"Unknown type: {z}") : dgame?.PakFileType;
            game.PakExts = elem.TryGetProperty("pakExt", out z) ? z.GetStringOrArray(x => x) : dgame?.PakExts;
            game.Paks = elem.TryGetProperty("pak", out z) ? z.GetStringOrArray(x => new Uri(x)) : dgame?.Paks;
            game.Dats = elem.TryGetProperty("dat", out z) ? z.GetStringOrArray(x => new Uri(x)) : dgame?.Dats;
            game.Paths = elem.TryGetProperty("path", out z) ? z.GetStringOrArray() : dgame?.Paths;
            game.Key = elem.TryGetProperty("key", out z) ? TryParseKey(z.GetString(), out var zO) ? zO : throw new ArgumentOutOfRangeException("key", z.GetString()) : null;
            game.FileSystemType = elem.TryGetProperty("fileSystemType", out z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("fileSystemType", $"Unknown type: {z}") : dgame?.FileSystemType;
            game.Editions = elem.TryGetProperty("editions", out z) ? z.EnumerateObject().ToDictionary(x => x.Name, x => ParseGameEdition(x.Name, x.Value), StringComparer.OrdinalIgnoreCase) : null;
            game.Dlc = elem.TryGetProperty("dlc", out z) ? z.EnumerateObject().ToDictionary(x => x.Name, x => ParseGameDownloadableContent(x.Name, x.Value), StringComparer.OrdinalIgnoreCase) : null;
            game.Locales = elem.TryGetProperty("locals", out z) ? z.EnumerateObject().ToDictionary(x => x.Name, x => ParseGameLocal(x.Name, x.Value), StringComparer.OrdinalIgnoreCase) : null;
            game.Found = locations.ContainsKey(id);
            if (id.StartsWith("*")) { dgame = game; game = null; }
            return game;
        }

        static Edition ParseGameEdition(string edition, JsonElement elem) => new Edition
        {
            Id = edition,
            Name = (elem.TryGetProperty("name", out var z) ? z.GetString() : null) ?? throw new ArgumentNullException("name"),
            Key = elem.TryGetProperty("key", out z) ? TryParseKey(z.GetString(), out var z2) ? z2 : throw new ArgumentOutOfRangeException("key", z.GetString()) : null,
        };

        static DownloadableContent ParseGameDownloadableContent(string edition, JsonElement elem) => new DownloadableContent
        {
            Id = edition,
            Name = (elem.TryGetProperty("name", out var z) ? z.GetString() : null) ?? throw new ArgumentNullException("name"),
        };

        static Locale ParseGameLocal(string edition, JsonElement elem) => new Locale
        {
            Id = edition,
            Name = (elem.TryGetProperty("name", out var z) ? z.GetString() : null) ?? throw new ArgumentNullException("name"),
        };

        static FamilyApp ParseApp(Family family, string id, JsonElement elem)
        {
            var familyAppType = elem.TryGetProperty("familyAppType", out var z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("familyAppType", z.GetString()) : null;
            var familyApp = familyAppType != null ? (FamilyApp)Activator.CreateInstance(familyAppType) : throw new ArgumentOutOfRangeException("familyAppType", familyAppType.ToString());
            familyApp.Family = family;
            familyApp.Id = id;
            familyApp.Name = (elem.TryGetProperty("name", out z) ? z.GetString() : null) ?? throw new ArgumentNullException("name");
            familyApp.ExplorerType = elem.TryGetProperty("explorerAppType", out z) ? Type.GetType(z.GetString(), false) : null;
            familyApp.Explorer2Type = elem.TryGetProperty("explorer2AppType", out z) ? Type.GetType(z.GetString(), false) : null;
            return familyApp;
        }

        #endregion
    }
}