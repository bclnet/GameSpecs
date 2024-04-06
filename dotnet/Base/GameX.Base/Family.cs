using GameX.Meta;
using GameX.Platforms;
using GameX.Unknown;
using OpenStack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using static GameX.FamilyManager;
using static GameX.FileManager;
using static GameX.Util;

namespace GameX
{
    #region FamilyManager

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
            Default,
            Pak,
            TopDir,
            TwoDir,
            DirDown,
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
            using var r = new StreamReader(GetManifestResourceStream($"GameX.Base.Specs.{path}"));
            return r.ReadToEnd();
        }

        static FamilyManager()
        {
            Family.Bootstrap();
            var loadSamples = true;
            foreach (var id in FamilyKeys)
            {
                var family = CreateFamily($"{id}Family.json", FamilyLoader, loadSamples);
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
        /// Create Key.
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static object CreateKey(JsonElement elem)
        {
            var str = elem.ToString();
            if (string.IsNullOrEmpty(str)) { return null; }
            else if (str.StartsWith("b64:", StringComparison.OrdinalIgnoreCase)) return Convert.FromBase64String(str[4..]);
            else if (str.StartsWith("hex:", StringComparison.OrdinalIgnoreCase))
            {
                var keyStr = str[4..];
                return keyStr.StartsWith("/")
                    ? Enumerable.Range(0, keyStr.Length >> 2).Select(x => byte.Parse(keyStr.Substring((x << 2) + 2, 2), NumberStyles.HexNumber)).ToArray()
                    : Enumerable.Range(0, keyStr.Length >> 1).Select(x => byte.Parse(keyStr.Substring(x << 1, 2), NumberStyles.HexNumber)).ToArray();
            }
            else if (str.StartsWith("txt:", StringComparison.OrdinalIgnoreCase)) return str[4..];
            else throw new ArgumentOutOfRangeException(nameof(str), str);
        }

        /// <summary>
        /// Create Detector.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="id"></param>
        /// <param name="elem"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static Detector CreateDetector(FamilyGame game, string id, JsonElement elem)
        {
            var detectorType = _value(elem, "detectorType", z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("detectorType", $"Unknown type: {z}"));
            return detectorType != null ? (Detector)Activator.CreateInstance(detectorType, game, id, elem) : new Detector(game, id, elem);
        }

        /// <summary>
        /// Create family sample.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="loader"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static FamilySample CreateFamilySample(string path, Func<string, string> loader)
        {
            var json = loader(path);
            if (string.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));
            using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
            var elem = doc.RootElement;
            return new FamilySample(elem);
        }

        /// <summary>
        /// Create family.
        /// </summary>
        /// <param name="any"></param>
        /// <param name="loader"></param>
        /// <param name="loadSamples"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal static Family CreateFamily(string any, Func<string, string> loader = null, bool loadSamples = false)
        {
            var json = loader != null ? loader(any) : any;
            if (string.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));
            using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
            var elem = doc.RootElement;
            var familyType = _value(elem, "familyType", z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("familyType", $"Unknown type: {z}"));
            var family = familyType != null ? (Family)Activator.CreateInstance(familyType, elem) : new Family(elem);
            if (family.SpecSamples != null && loadSamples)
                foreach (var sample in family.SpecSamples)
                    family.MergeSample(CreateFamilySample(sample, loader));
            if (family.Specs != null)
                foreach (var spec in family.Specs)
                    family.Merge(CreateFamily(spec, loader, loadSamples));
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
            var engineType = _value(elem, "engineType", z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("engineType", $"Unknown type: {z}"));
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
        internal static FamilyGame CreateFamilyGame(Family family, string id, JsonElement elem, ref FamilyGame dgame, IDictionary<string, PathItem> paths)
        {
            var gameType = _value(elem, "gameType", z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("gameType", $"Unknown type: {z}"), dgame.GameType);
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
            var appType = _value(elem, "appType", z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("appType", $"Unknown type: {z}"));
            return appType != null ? (FamilyApp)Activator.CreateInstance(appType, family, id, elem) : new FamilyApp(family, id, elem);
        }

        /// <summary>
        /// Create filemanager.
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        internal static FileManager CreateFileManager(JsonElement elem)
            => new FileManager(elem);

        /// <summary>
        /// Creates the file system.
        /// </summary>
        /// <param name="fileSystemType">The fileSystemType.</param>
        /// <param name="root">The root.</param>
        /// <param name="host">The host.</param>
        /// <returns></returns>
        internal static IFileSystem CreateFileSystem(Type fileSystemType, PathItem path, Uri host = null)
            => host != null ? new HostFileSystem(host)
            : fileSystemType != null ? (IFileSystem)Activator.CreateInstance(fileSystemType, path)
            : path.Type switch
            {
                null => new StandardFileSystem(Path.Combine(path.Root, path.Paths.SingleOrDefault() ?? string.Empty)),
                "zip" => new ZipFileSystem(path.Root, path.Paths.SingleOrDefault()),
                "zip:iso" => new ZipIsoFileSystem(path.Root, path.Paths.SingleOrDefault()),
                _ => throw new ArgumentOutOfRangeException(nameof(path.Type), $"Unknown {path.Type}")
            };

        #endregion
    }

    #endregion

    #region Detector

    /// <summary>
    /// Detector
    /// </summary>
    public class Detector
    {
        protected ConcurrentDictionary<string, object> Cache = new ConcurrentDictionary<string, object>();
        protected Dictionary<string, Dictionary<string, object>> Hashs;

        /// <summary>
        /// The identifier
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the Game.
        /// </summary>
        public FamilyGame Game { get; set; }
        /// <summary>
        /// The Type
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// The Seed
        /// </summary>
        public object Seed { get; set; }

        /// <summary>
        /// Detector
        /// </summary>
        /// <param name="id"></param>
        /// <param name="game"></param>
        /// <param name="elem"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Detector(FamilyGame game, string id, JsonElement elem)
        {
            Id = id;
            Game = game;
            ParseElem(game, elem);
        }

        public virtual void ParseElem(FamilyGame game, JsonElement elem)
        {
            Type = _value(elem, "type") ?? "md5";
            Seed = _method(elem, "seed", CreateKey);
            Hashs = _related(elem, "hashs", k => k.GetProperty("hash").GetString(), v => ParseHash(game, v));
        }

        public virtual Dictionary<string, object> ParseHash(FamilyGame game, JsonElement elem)
            => elem.EnumerateObject().ToDictionary(x => x.Name, x => x.Name switch
            {
                null => default,
                "edition" => game.Editions != null && game.Editions.TryGetValue(x.Value.GetString(), out var a) ? a : (object)x.Value.GetString(),
                "locale" => game.Locales != null && game.Locales.TryGetValue(x.Value.GetString(), out var a) ? a : (object)x.Value.GetString(),
                _ => _valueV(x.Value)
            });

        public unsafe virtual string GetHash(BinaryReader r)
        {
            switch (Type)
            {
                case "crc":
                    {
                        // create table
                        var seed = 0xEDB88320U;
                        var table = stackalloc uint[256];
                        uint j, n;
                        for (var i = 0U; i < 256; i++)
                        {
                            n = i;
                            for (j = 0; j < 8; j++) n = (n & 1) != 0 ? (n >> 1) ^ seed : n >> 1;
                            table[i] = n;
                        }

                        // generate crc
                        var crc = 0xFFFFFFFFU;
                        var len = r.BaseStream.Length;
                        for (var i = 0U; i < len; i++)
                            crc = (crc >> 8) ^ table[(crc ^ r.ReadByte()) & 0xFF];
                        crc ^= 0xFFFFFFFF;
                        return $"{crc:x}";
                    }
                case "md5":
                    {
                        using var md5 = System.Security.Cryptography.MD5.Create();
                        var data = r.ReadBytes(1024 * 1024);
                        var h = md5.ComputeHash(data, 0, data.Length);
                        return $"{h[0]:x2}{h[1]:x2}{h[2]:x2}{h[3]:x2}{h[4]:x2}{h[5]:x2}{h[6]:x2}{h[7]:x2}{h[8]:x2}{h[9]:x2}{h[10]:x2}{h[11]:x2}{h[12]:x2}{h[13]:x2}{h[14]:x2}{h[15]:x2}";
                    }
                default: throw new ArgumentOutOfRangeException(nameof(Type), $"Unknown Type {Type}");
            }
        }

        public T Get<T>(string key, object value, Func<T, T> func) where T : class => Cache.GetOrAdd(key, (k, v) =>
        {
            var s = Detect<T>(k, v);
            return s == null || func == null ? s : func(s);
        }, value) as T;

        public virtual T Detect<T>(string key, object value) where T : class
        {
            switch (value)
            {
                case null: throw new ArgumentNullException(nameof(value));
                case BinaryReader r:
                    {
                        r.BaseStream.Position = 0;
                        var hash = GetHash(r);
                        r.BaseStream.Position = 0;
                        return hash != null && Hashs.TryGetValue(hash, out var z) ? z as T : default;
                    }
                default: throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }

    #endregion

    #region Resource

    /// <summary>
    /// Resource
    /// </summary>
    public struct Resource
    {
        /// <summary>
        /// The filesystem.
        /// </summary>
        public IFileSystem FileSystem;
        /// <summary>
        /// The game.
        /// </summary>
        public FamilyGame Game;
        /// <summary>
        /// The game edition.
        /// </summary>
        public FamilyGame.Edition Edition;
        /// <summary>
        /// The search pattern.
        /// </summary>
        public string SearchPattern;
    }

    #endregion

    #region Family

    /// <summary>
    /// Family
    /// </summary>
    public class Family
    {
        /// <summary>
        /// An empty family.
        /// </summary>
        public static readonly Family Empty = new UnknownFamily
        {
            Id = string.Empty,
            Name = "Empty",
            Games = new Dictionary<string, FamilyGame>(),
            FileManager = new FileManager(),
        };

        /// <summary>
        /// Gets the file filters.
        /// </summary>
        /// <value>
        /// The file filters.
        /// </value>
        //public IDictionary<string, IDictionary<string, string>> Filters => FileManager.Filters;

        /// <summary>
        /// Gets or sets the family identifier.
        /// </summary>
        /// <value>
        /// The family identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the family name.
        /// </summary>
        /// <value>
        /// The family name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the family studio.
        /// </summary>
        /// <value>
        /// The family studio.
        /// </value>
        public string Studio { get; set; }

        /// <summary>
        /// Gets or sets the family description.
        /// </summary>
        /// <value>
        /// The family description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the game urls.
        /// </summary>
        public Uri[] Urls { get; set; }

        /// <summary>
        /// Gets the families spec samples.
        /// </summary>
        /// <returns></returns>
        public string[] SpecSamples { get; set; }

        /// <summary>
        /// Gets the families specs.
        /// </summary>
        /// <returns></returns>
        public string[] Specs { get; set; }

        /// <summary>
        /// Gets the family samples.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, List<FamilySample.File>> Samples { get; set; }

        /// <summary>
        /// Gets the family engines.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, FamilyEngine> Engines { get; set; }

        /// <summary>
        /// Gets the family games.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, FamilyGame> Games { get; set; }

        /// <summary>
        /// Gets the family apps.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, FamilyApp> Apps { get; set; }

        /// <summary>
        /// Gets or sets the family file manager.
        /// </summary>
        /// <value>
        /// The file manager.
        /// </value>
        public FileManager FileManager { get; set; }

        static unsafe Family()
        {
            if (Platform.InTestHost && Platform.Startups.Count == 0) Platform.Startups.Add(TestPlatform.Startup);
            foreach (var startup in Platform.Startups) if (startup()) return;
            Platform.PlatformType = Platform.Type.Unknown;
            Platform.GraphicFactory = source => null; // throw new Exception("No GraphicFactory");
            Debug.AssertFunc = x => System.Diagnostics.Debug.Assert(x);
            Debug.LogFunc = a => System.Diagnostics.Debug.Print(a);
            Debug.LogFormatFunc = (a, b) => System.Diagnostics.Debug.Print(a, b);
        }

        /// <summary>
        /// Family
        /// </summary>
        internal Family() { }
        /// <summary>
        /// Family
        /// </summary>
        /// <param name="elem"></param>
        public Family(JsonElement elem)
        {
            try
            {
                Id = _value(elem, "id") ?? throw new ArgumentNullException("id");
                Name = _value(elem, "name");
                Studio = _value(elem, "studio");
                Description = _value(elem, "description");
                Urls = _list(elem, "url", x => new Uri(x));
                SpecSamples = _list(elem, "samples");
                Specs = _list(elem, "specs");
                // file manager
                FileManager = _method(elem, "fileManager", CreateFileManager);
                var paths = FileManager?.Paths;
                // related
                var dgame = new FamilyGame { SearchBy = SearchBy.Default, Paks = new[] { new Uri("game:/") } };
                Samples = new Dictionary<string, List<FamilySample.File>>();
                Engines = _related(elem, "engines", (k, v) => CreateFamilyEngine(this, k, v));
                Games = _dictTrim(_related(elem, "games", (k, v) => CreateFamilyGame(this, k, v, ref dgame, paths)));
                Apps = _related(elem, "apps", (k, v) => CreateFamilyApp(this, k, v));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw;
            }
        }

        /// <summary>
        /// Touches this instance.
        /// </summary>
        public static void Bootstrap() { }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => Name;

        /// <summary>
        /// Merges the family.
        /// </summary>
        /// <param name="source">The source.</param>
        public void Merge(Family source)
        {
            if (source == null) return;
            foreach (var s in source.Engines) Engines.Add(s.Key, s.Value);
            foreach (var s in source.Games) Games.Add(s.Key, s.Value);
            foreach (var s in source.Apps) Apps.Add(s.Key, s.Value);
            if (FileManager != null) FileManager.Merge(source.FileManager);
            else FileManager = source.FileManager;
        }

        /// <summary>
        /// Merges the family sample.
        /// </summary>
        /// <param name="source">The source.</param>
        public void MergeSample(FamilySample source)
        {
            if (source == null) return;
            foreach (var s in source.Samples) Samples.Add(s.Key, s.Value);
        }

        /// <summary>
        /// Gets the specified family game.
        /// </summary>
        /// <param name="id">The game id.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">game</exception>
        public FamilyGame GetGame(string id, out FamilyGame.Edition edition, bool throwOnError = true)
        {
            var ids = id.Split('.', 2);
            var game = Games.TryGetValue(ids[0], out var z1) ? z1
                : (throwOnError ? throw new ArgumentOutOfRangeException(nameof(id), id) : (FamilyGame)default);
            edition = ids.Length > 1 && game.Editions.TryGetValue(ids[1], out var z2) ? z2 : default;
            return game;
        }

        /// <summary>
        /// Parses the family resource uri.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public Resource ParseResource(Uri uri, bool throwOnError = true)
        {
            if (uri == null || string.IsNullOrEmpty(uri.Fragment)) return new Resource { Game = new FamilyGame() };
            var game = GetGame(uri.Fragment[1..], out var edition);
            var searchPattern = uri.IsFile ? null : uri.LocalPath[1..];
            var paths = FileManager.Paths;
            var fileSystemType = game.FileSystemType;
            var fileSystem =
                string.Equals(uri.Scheme, "game", StringComparison.OrdinalIgnoreCase) ? paths.TryGetValue(game.Id, out var z) ? CreateFileSystem(fileSystemType, z) : default
                : uri.IsFile ? !string.IsNullOrEmpty(uri.LocalPath) ? CreateFileSystem(fileSystemType, new PathItem(uri.LocalPath, default)) : default
                : uri.Scheme.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? !string.IsNullOrEmpty(uri.Host) ? CreateFileSystem(fileSystemType, null, uri) : default
                : default;
            if (fileSystem == null)
                if (throwOnError) throw new ArgumentOutOfRangeException(nameof(uri), $"{game.Id}: unable to resources");
                else return default;
            return new Resource
            {
                FileSystem = fileSystem,
                Game = game,
                Edition = edition,
                SearchPattern = searchPattern
            };
        }

        /// <summary>
        /// Opens the family pak file.
        /// </summary>
        /// <param name="res">The res.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public PakFile OpenPakFile(object res, bool throwOnError = true)
        {
            var r = res switch
            {
                Resource s => s,
                string s => ParseResource(new Uri(s)),
                Uri u => ParseResource(u),
                _ => throw new ArgumentOutOfRangeException(nameof(res)),
            };
            return r.Game != null
                ? r.Game.CreatePakFile(r.FileSystem, r.Edition, r.SearchPattern, throwOnError)?.Open()
                : throw new ArgumentNullException(nameof(r.Game));
        }
    }

    #endregion

    #region FamilyApp

    /// <summary>
    /// FamilyApp
    /// </summary>
    public class FamilyApp
    {
        /// <summary>
        /// Gets or sets the family.
        /// </summary>
        public Family Family { get; set; }
        /// <summary>
        /// Gets or sets the game identifier.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the game name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the explorer type.
        /// </summary>
        public Type ExplorerType { get; set; }
        /// <summary>
        /// Gets or sets the explorer2 type.
        /// </summary>
        public Type Explorer2Type { get; set; }

        /// <summary>
        /// FamilyApp
        /// </summary>
        /// <param name="family"></param>
        /// <param name="id"></param>
        /// <param name="elem"></param>
        public FamilyApp(Family family, string id, JsonElement elem)
        {
            Family = family;
            Id = id;
            Name = _value(elem, "name") ?? id;
            ExplorerType = _value(elem, "explorerAppType", z => Type.GetType(z.GetString(), false));
            Explorer2Type = _value(elem, "explorer2AppType", z => Type.GetType(z.GetString(), false));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => Name;

        /// <summary>
        /// Gets or sets the game name.
        /// </summary>
        public virtual Task OpenAsync(Type explorerType, MetaManager manager)
        {
            var explorer = Activator.CreateInstance(explorerType);
            var startupMethod = explorerType.GetMethod("Application_Startup", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new ArgumentOutOfRangeException(nameof(explorerType), "No Application_Startup found");
            startupMethod.Invoke(explorer, new object[] { this, null });
            return Task.CompletedTask;
        }
    }

    #endregion

    #region FamilyEngine

    /// <summary>
    /// FamilyEngine
    /// </summary>
    public class FamilyEngine
    {
        /// <summary>
        /// Gets or sets the family.
        /// </summary>
        public Family Family { get; set; }
        /// <summary>
        /// Gets or sets the game identifier.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the game name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// FamilyEngine
        /// </summary>
        /// <param name="family"></param>
        /// <param name="id"></param>
        /// <param name="elem"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public FamilyEngine(Family family, string id, JsonElement elem)
        {
            Family = family;
            Id = id;
            Name = _value(elem, "name") ?? id;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => Name;
    }

    #endregion

    #region FamilySample

    /// <summary>
    /// FamilySample
    /// </summary>
    public class FamilySample
    {
        public Dictionary<string, List<File>> Samples { get; } = new Dictionary<string, List<File>>();

        /// <summary>
        /// FamilySample
        /// </summary>
        /// <param name="elem"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public FamilySample(JsonElement elem)
        {
            foreach (var s in elem.EnumerateObject())
                Samples.Add(s.Name, s.Value.GetProperty("files").EnumerateArray().Select(x => new File(x)).ToList());
        }

        /// <summary>
        /// The sample file.
        /// </summary>
        public class File
        {
            /// <summary>
            /// The path
            /// </summary>
            public string Path { get; set; }
            /// <summary>
            /// The size
            /// </summary>
            public long Size { get; set; }
            /// <summary>
            /// The type
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// File
            /// </summary>
            /// <param name="elem"></param>
            /// <exception cref="ArgumentNullException"></exception>
            public File(JsonElement elem)
            {
                Path = _value(elem, "path");
                Size = _value(elem, "size", x => x.GetInt64(), 0L);
                Type = _value(elem, "type");
            }

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString() => Path;
        }
    }

    #endregion

    #region FamilyGame

    /// <summary>
    /// FamilyGame
    /// </summary>
    public class FamilyGame
    {
        /// <summary>
        /// An empty family game.
        /// </summary>
        public static readonly FamilyGame Empty = new FamilyGame
        {
            Family = Family.Empty,
            Id = "Empty",
            Name = "Empty",
        };

        /// <summary>
        /// The game edition.
        /// </summary>
        public class Edition
        {
            /// <summary>
            /// The identifier
            /// </summary>
            public string Id { get; set; }
            /// <summary>
            /// The name
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// The key
            /// </summary>
            public object Key { get; set; }
            /// <summary>
            /// The Data
            /// </summary>
            public Dictionary<string, object> Data { get; set; }

            /// <summary>
            /// Edition
            /// </summary>
            /// <param name="id"></param>
            /// <param name="elem"></param>
            /// <exception cref="ArgumentNullException"></exception>
            public Edition(string id, JsonElement elem)
            {
                Id = id;
                Name = _value(elem, "name") ?? id;
                Key = _method(elem, "key", CreateKey);
                Data = Parse(elem);
            }

            public Dictionary<string, object> Parse(JsonElement elem)
                => elem.EnumerateObject().Where(x => x.Name != "name" && x.Name != "key").ToDictionary(x => x.Name, x => _valueV(x.Value));
        }

        /// <summary>
        /// The game DLC.
        /// </summary>
        public class DownloadableContent
        {
            /// <summary>
            /// The identifier
            /// </summary>
            public string Id { get; set; }
            /// <summary>
            /// The name
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// The Path
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// DownloadableContent
            /// </summary>
            /// <param name="id"></param>
            /// <param name="elem"></param>
            /// <exception cref="ArgumentNullException"></exception>
            public DownloadableContent(string id, JsonElement elem)
            {
                Id = id;
                Name = _value(elem, "name") ?? id;
                Path = _value(elem, "path");
            }
        }

        /// <summary>
        /// The game locale.
        /// </summary>
        public class Locale
        {
            /// <summary>
            /// The identifier
            /// </summary>
            public string Id { get; set; }
            /// <summary>
            /// The name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Locale
            /// </summary>
            /// <param name="id"></param>
            /// <param name="elem"></param>
            /// <exception cref="ArgumentNullException"></exception>
            public Locale(string id, JsonElement elem)
            {
                Id = id;
                Name = _value(elem, "name") ?? id;
            }
        }

        /// Gets or sets the game type.
        /// </summary>
        public Type GameType { get; set; }
        /// <summary>
        /// Gets or sets the family.
        /// </summary>
        public Family Family { get; set; }
        /// <summary>
        /// Gets or sets the game identifier.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets if ignored.
        /// </summary>
        public bool Ignore { get; set; }
        /// <summary>
        /// Gets or sets the game name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the game engine.
        /// </summary>
        public string Engine { get; set; }
        /// <summary>
        /// Gets or sets the game resource.
        /// </summary>
        public string Resource { get; set; }
        /// <summary>
        /// Gets or sets the game urls.
        /// </summary>
        public Uri[] Urls { get; set; }
        /// <summary>
        /// Gets or sets the game date.
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Gets or sets the search by.
        /// </summary>
        public SearchBy SearchBy { get; set; }
        /// <summary>
        /// Gets or sets the pak option.
        /// </summary>
        //public GameOption Option { get; set; }
        /// <summary>
        /// Gets or sets the pakFile type.
        /// </summary>
        public Type PakFileType { get; set; }
        /// <summary>
        /// Gets or sets the pak etxs.
        /// </summary>
        public string[] PakExts { get; set; }
        /// <summary>
        /// Gets or sets the paks.
        /// </summary>
        public Uri[] Paks { get; set; }
        /// <summary>
        /// Gets or sets the dats.
        /// </summary>
        public Uri[] Dats { get; set; }
        /// <summary>
        /// Gets or sets the Paths.
        /// </summary>
        public string[] Paths { get; set; }
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public object Key { get; set; }
        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        //public string[] Status { get; set; }
        /// <summary>
        /// Gets or sets the Tags.
        /// </summary>
        public string[] Tags { get; set; }
        /// <summary>
        /// Determines if the game has been found.
        /// </summary>
        public bool Found { get; set; }
        /// <summary>
        /// Gets or sets the type of the file system.
        /// </summary>
        /// <value>
        /// Gets or sets the file system type.
        /// </value>
        public Type FileSystemType { get; set; }
        /// <summary>
        /// Gets or sets the game editions.
        /// </summary>
        public IDictionary<string, Edition> Editions { get; set; }
        /// <summary>
        /// Gets or sets the game dlcs.
        /// </summary>
        public IDictionary<string, DownloadableContent> Dlcs { get; set; }
        /// <summary>
        /// Gets or sets the game locales.
        /// </summary>
        public IDictionary<string, Locale> Locales { get; set; }
        /// <summary>
        /// Gets or sets the detectorss.
        /// </summary>
        public IDictionary<string, Detector> Detectors { get; set; }
        /// <summary>
        /// Gets the displayed game name.
        /// </summary>
        /// <value>
        /// The name of the displayed.
        /// </value>
        public string DisplayedName => $"{Name}{(Found ? " - found" : null)}";

        /// <summary>
        /// FamilyGame
        /// </summary>
        internal FamilyGame() { }
        /// <summary>
        /// FamilyGame
        /// </summary>
        /// <param name="family"></param>
        /// <param name="id"></param>
        /// <param name="elem"></param>
        /// <param name="dgame"></param>
        public FamilyGame(Family family, string id, JsonElement elem, FamilyGame dgame)
        {
            Family = family;
            Id = id;
            Ignore = _valueBool(elem, "n/a", dgame.Ignore);
            Name = _value(elem, "name"); //System.Diagnostics.Debugger.Log(0, null, $"Game: {Name}\n");
            Engine = _value(elem, "engine", dgame.Engine);
            Resource = _value(elem, "resource", dgame.Resource);
            Urls = _list(elem, "url", x => new Uri(x));
            Date = _value(elem, "date", z => DateTime.Parse(z.GetString()));
            //Option = _value(elem, "option", z => Enum.TryParse<GameOption>(z.GetString(), true, out var zT) ? zT : throw new ArgumentOutOfRangeException("option", $"Unknown option: {z}"), dgame.Option);
            Paks = _list(elem, "pak", x => new Uri(x), dgame.Paks);
            Dats = _list(elem, "dat", x => new Uri(x), dgame.Dats);
            Paths = _list(elem, "path", dgame.Paths);
            Key = _method(elem, "key", CreateKey, dgame.Key);
            //Status = _value(elem, "status");
            Tags = _value(elem, "tags", string.Empty).Split(' ');
            // interface
            FileSystemType = _value(elem, "fileSystemType", z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("fileSystemType", $"Unknown type: {z}"), dgame.FileSystemType);
            SearchBy = _value(elem, "searchBy", z => Enum.TryParse<SearchBy>(z.GetString(), true, out var zS) ? zS : throw new ArgumentOutOfRangeException("searchBy", $"Unknown option: {z}"), dgame.SearchBy);
            PakFileType = _value(elem, "pakFileType", z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("pakFileType", $"Unknown type: {z}"), dgame.PakFileType);
            PakExts = _list(elem, "pakExt", dgame.PakExts);
            // related
            Editions = _related(elem, "editions", (k, v) => new Edition(k, v));
            Dlcs = _related(elem, "dlcs", (k, v) => new DownloadableContent(k, v));
            Locales = _related(elem, "locals", (k, v) => new Locale(k, v));
            // detector
            Detectors = _related(elem, "detectors", (k, v) => CreateDetector(this, k, v));
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => Name;

        /// <summary>
        /// Detect
        /// </summary>
        public T Detect<T>(string id, string key, object value, Func<T, T> func = null) where T : class => Detectors.TryGetValue(id, out var z) ? z.Get(key, value, func) : default;

        /// <summary>
        /// Ensures this instance.
        /// </summary>
        public virtual FamilyGame Ensure() => this;

        /// <summary>
        /// Converts the Paks to Application Paks.
        /// </summary>
        public IList<Uri> ToPaks(string edition) => Paks.Select(x => new Uri($"{x}#{Id}")).ToList();

        /// <summary>
        /// Gets a family sample
        /// </summary>
        public FamilySample.File GetSample(string id)
        {
            if (!Family.Samples.TryGetValue(Id, out var samples) || samples.Count == 0) return default;
            var idx = id == "*" ? new Random((int)DateTime.Now.Ticks).Next(samples.Count) : int.Parse(id);
            return samples.Count > idx ? samples[idx] : default;
        }

        #region Pak

        /// <summary>
        /// Adds the platform graphic.
        /// </summary>
        /// <param name="pakFile">The pak file.</param>
        /// <returns></returns>
        static PakFile WithPlatformGraphic(PakFile pakFile)
        {
            if (pakFile != null) pakFile.Graphic = Platform.GraphicFactory?.Invoke(pakFile);
            return pakFile;
        }

        /// <summary>
        /// Create pak file.
        /// </summary>
        /// <param name="fileSystem">The fileSystem.</param>
        /// <param name="edition">The edition.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        internal PakFile CreatePakFile(IFileSystem fileSystem, Edition edition, string searchPattern, bool throwOnError)
        {
            if (fileSystem is HostFileSystem k) throw new NotImplementedException($"{k}"); //return new StreamPakFile(family.FileManager.HostFactory, game, path, fileSystem),
            searchPattern = CreateSearchPatterns(searchPattern);
            var pakFiles = new List<PakFile>();
            var dlcKeys = Dlcs.Where(x => !string.IsNullOrEmpty(x.Value.Path)).Select(x => x.Key).ToArray();
            var slash = '\\';
            foreach (var key in (new string[] { null }).Concat(dlcKeys))
                foreach (var p in FindPaths(fileSystem, edition, key != null ? Dlcs[key] : null, searchPattern))
                    switch (SearchBy)
                    {
                        case SearchBy.Pak:
                            foreach (var path in p.paths)
                                if (IsPakFile(path))
                                    pakFiles.Add(CreatePakFileObj(fileSystem, edition, path));
                            break;
                        default:
                            pakFiles.Add(CreatePakFileObj(fileSystem, edition,
                                SearchBy == SearchBy.DirDown ? (p.root, p.paths.Where(x => x.Contains(slash)).ToArray())
                                : p));
                            break;
                    }
            return WithPlatformGraphic(CreatePakFileObj(fileSystem, edition, pakFiles));
        }

        /// <summary>
        /// Create pak file.
        /// </summary>
        /// <param name="fileSystem">The fileSystem.</param>
        /// <param name="edition">The edition.</param>
        /// <param name="value">The value.</param>
        /// <param name="tag">The tag.</param>
        /// <returns></returns>
        public PakFile CreatePakFileObj(IFileSystem fileSystem, Edition edition, object value, object tag = null) => value switch
        {
            string s => IsPakFile(s)
                ? CreatePakFileType(new PakState(fileSystem, this, edition, s, tag))
                : throw new InvalidOperationException($"{Id} missing {s}"),
            ValueTuple<string, string[]> s => s.Item2.Length == 1 && IsPakFile(s.Item2[0])
                ? CreatePakFileObj(fileSystem, edition, s.Item2[0], tag)
                : new ManyPakFile(
                    CreatePakFileType(new PakState(fileSystem, this, edition, null, tag)),
                    new PakState(fileSystem, this, edition, null, tag),
                    s.Item1.Length > 0 ? s.Item1 : "Many", s.Item2,
                    pathSkip: s.Item1.Length > 0 ? s.Item1.Length + 1 : 0),
            IList<PakFile> s => s.Count == 1
                ? s[0]
                : new MultiPakFile(new PakState(fileSystem, this, edition, null, tag), "Multi", s),
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(value), $"{value}"),
        };

        /// <summary>
        /// Create pak file.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public PakFile CreatePakFileType(PakState state)
            => (PakFile)Activator.CreateInstance(PakFileType ?? throw new InvalidOperationException($"{Id} missing PakFileType"), state);

        /// <summary>
        /// Is pak file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public bool IsPakFile(string path) => PakExts != null && PakExts.Any(x => path.EndsWith(x, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Find the games paths.
        /// </summary>
        /// <param name="fileSystem">The fileSystem.</param>
        /// <param name="edition">The edition.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <returns></returns>
        public IEnumerable<(string root, string[] paths)> FindPaths(IFileSystem fileSystem, Edition edition, DownloadableContent dlc, string searchPattern)
        {
            var gameIgnores = Family.FileManager.Ignores.TryGetValue(Id, out var z) ? z : null;
            var paths = dlc != null ? new[] { "" } : Paths ?? new[] { "" };
            foreach (var path in paths)
            {
                var searchPath = dlc?.Path != null ? Path.Join(path, dlc.Path) : path;
                var fileSearch = fileSystem.FindPaths(searchPath, searchPattern);
                if (gameIgnores != null) fileSearch = fileSearch.Where(x => !gameIgnores.Contains(Path.GetFileName(x)));
                yield return (path, fileSearch.ToArray());
            }
        }

        /// <summary>
        /// Creates the search patterns.
        /// </summary>
        /// <param name="searchPattern">The search pattern.</param>
        /// <returns></returns>
        public string CreateSearchPatterns(string searchPattern)
        {
            if (!string.IsNullOrEmpty(searchPattern)) return searchPattern;
            return SearchBy switch
            {
                SearchBy.Default => "",
                SearchBy.Pak => PakExts == null || PakExts.Length == 0 ? ""
                    : PakExts.Length == 1 ? $"*{PakExts[0]}" : $"(*{string.Join(":*", PakExts)})",
                SearchBy.TopDir => "*",
                SearchBy.TwoDir => "*/*",
                SearchBy.DirDown => "**/*",
                SearchBy.AllDir => "**/*",
                _ => throw new ArgumentOutOfRangeException(nameof(SearchBy), $"{SearchBy}"),
            };
        }

        #endregion
    }

    #endregion
}