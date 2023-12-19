using GameSpec.Unknown;
using OpenStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using static GameSpec.FamilyManager;

namespace GameSpec
{
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
        public IDictionary<string, IDictionary<string, string>> FileFilters => FileManager.Filters;

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
        /// Gets the families specs.
        /// </summary>
        /// <returns></returns>
        public string[] Specs { get; set; }

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
            if (FamilyPlatform.InTestHost && FamilyPlatform.Startups.Count == 0) FamilyPlatform.Startups.Add(TestPlatform.Startup);
            foreach (var startup in FamilyPlatform.Startups) if (startup()) return;
            FamilyPlatform.Platform = FamilyPlatform.Type.Unknown;
            FamilyPlatform.GraphicFactory = source => null; // throw new Exception("No GraphicFactory");
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
            //try
            //{
            FamilyGame dgame = null;
            var fileManager = elem.TryGetProperty("fileManager", out var z) ? CreateFileManager(z) : default;
            var paths = fileManager?.Paths;
            Id = (elem.TryGetProperty("id", out z) ? z.GetString() : default) ?? throw new ArgumentNullException("id");
            Name = elem.TryGetProperty("name", out z) ? z.GetString() : default;
            Studio = elem.TryGetProperty("studio", out z) ? z.GetString() : default;
            Description = elem.TryGetProperty("description", out z) ? z.GetString() : default;
            Urls = elem.TryGetProperty("url", out z) ? z.GetStringOrArray(x => new Uri(x)) : default;
            FileManager = fileManager;
            Specs = elem.TryGetProperty("specs", out z) ? z.GetStringOrArray(x => x) : default;
            Engines = elem.TryGetProperty("engines", out z) ? z.EnumerateObject().ToDictionary(x => x.Name, x => CreateFamilyEngine(this, x.Name, x.Value)) : new Dictionary<string, FamilyEngine>();
            Games = elem.TryGetProperty("games", out z) ? z.EnumerateObject().Select(x => (x.Name, Value: CreateFamilyGame(this, x.Name, x.Value, ref dgame, paths))).Where(x => x.Value != null).ToDictionary(x => x.Name, x => x.Value) : new Dictionary<string, FamilyGame>();
            Apps = elem.TryGetProperty("apps", out z) ? z.EnumerateObject().ToDictionary(x => x.Name, x => CreateFamilyApp(this, x.Name, x.Value)) : new Dictionary<string, FamilyApp>();
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    Console.WriteLine(e.StackTrace);
            //    throw;
            //}
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
        /// Gets the specified family game.
        /// </summary>
        /// <param name="id">The game id.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">game</exception>
        public FamilyGame GetGame(string id, bool throwOnError = true) => Games.TryGetValue(id, out var game) ? game : (throwOnError ? throw new ArgumentOutOfRangeException(nameof(id), id) : (FamilyGame)default);

        /// <summary>
        /// Parses the family resource uri.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public Resource ParseResource(Uri uri, bool throwOnError = true)
        {
            if (uri == null || string.IsNullOrEmpty(uri.Fragment)) return new Resource { Game = new FamilyGame() };
            var game = GetGame(uri.Fragment[1..]);
            var searchPattern = uri.IsFile ? null : uri.LocalPath[1..];
            var paths = FileManager.Paths;
            var fileSystem =
                string.Equals(uri.Scheme, "game", StringComparison.OrdinalIgnoreCase) ? paths.TryGetValue(game.Id, out var z) ? game.CreateFileSystem(z.Single()) : default
                : uri.IsFile ? !string.IsNullOrEmpty(uri.LocalPath) ? game.CreateFileSystem(uri.LocalPath) : default
                : uri.Scheme.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? !string.IsNullOrEmpty(uri.Host) ? game.CreateFileSystem(null, uri) : default
                : default;
            if (fileSystem == null)
                if (throwOnError) throw new ArgumentOutOfRangeException(nameof(uri), $"{game.Id}: unable to resources");
                else return default;
            return new Resource { FileSystem = fileSystem, Game = game, SearchPattern = searchPattern };
        }

        #region Pak

        /// <summary>
        /// Opens the family pak file.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="path">The file path.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public PakFile OpenPakFile(FamilyGame game, string path, string searchPattern, bool throwOnError = true) => game != null
            ? game.CreatePakFile(game.CreateFileSystem(path), searchPattern, throwOnError)?.Open()
            : throw new ArgumentNullException(nameof(game));

        /// <summary>
        /// Opens the family pak file.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public PakFile OpenPakFile(Resource resource, bool throwOnError = true) => resource.Game != null
            ? resource.Game.CreatePakFile(resource.FileSystem, resource.SearchPattern, throwOnError)?.Open()
            : throw new ArgumentNullException(nameof(resource.Game));

        /// <summary>
        /// Opens the family pak file.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="index">The index.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public PakFile OpenPakFile(Uri uri, bool throwOnError = true)
        {
            var resource = ParseResource(uri);
            return resource.Game != null
                ? resource.Game.CreatePakFile(resource.FileSystem, resource.SearchPattern, throwOnError)?.Open()
                : throw new ArgumentNullException(nameof(resource.Game));
        }

        #endregion
    }
}