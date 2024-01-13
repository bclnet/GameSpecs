using GameSpec.Formats;
using GameSpec.Platforms;
using GameSpec.Unknown;
using OpenStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using static GameSpec.FamilyManager;
using static GameSpec.Util;

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
            //try
            //{
            Id = _value(elem, "id") ?? throw new ArgumentNullException("id");
            Name = _value(elem, "name");
            Studio = _value(elem, "studio");
            Description = _value(elem, "description");
            Urls = _list(elem, "url", x => new Uri(x));
            Specs = _list(elem, "specs");
            // file manager
            FileManager = _method(elem, "fileManager", CreateFileManager);
            var paths = FileManager?.Paths;
            // related
            var dgame = new FamilyGame { SearchBy = SearchBy.Pak, Paks = new[] { new Uri("game:/") } };
            Engines = _related(elem, "engines", (k, v) => CreateFamilyEngine(this, k, v));
            Games = _dictTrim(_related(elem, "games", (k, v) => CreateFamilyGame(this, k, v, ref dgame, paths)));
            Apps = _related(elem, "apps", (k, v) => CreateFamilyApp(this, k, v));
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
                string.Equals(uri.Scheme, "game", StringComparison.OrdinalIgnoreCase) ? paths.TryGetValue(game.Id, out var z) ? CreateFileSystem(fileSystemType, z.Single()) : default
                : uri.IsFile ? !string.IsNullOrEmpty(uri.LocalPath) ? CreateFileSystem(fileSystemType, uri.LocalPath) : default
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
                Uri u => ParseResource(u),
                _ => throw new ArgumentOutOfRangeException(nameof(res)),
            };
            return r.Game != null
                ? r.Game.CreatePakFile(r.FileSystem, r.Edition, r.SearchPattern, throwOnError)?.Open()
                : throw new ArgumentNullException(nameof(r.Game));
        }
    }
}