using GameSpec.Unknown;
using OpenStack;
using System;
using System.Collections.Generic;
using static GameSpec.Resource;

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
            FileManager = FamilyManager.CreateFileManager(),
        };

        /// <summary>
        /// A ByteKey Container.
        /// </summary>
        public class ByteKey
        {
            public byte[] Key;
        }

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
        /// Gets or sets the family engine.
        /// </summary>
        /// <value>
        /// The family engine.
        /// </value>
        public string Engine { get; set; }

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
        /// Gets or sets the pakFile type.
        /// </summary>
        /// <value>
        /// The type of the pak file.
        /// </value>
        public Type PakFileType { get; set; }

        /// <summary>
        /// Gets or sets the pak options.
        /// </summary>
        /// <value>
        /// The pack option.
        /// </value>
        public PakOption PakOptions { get; set; }

        /// <summary>
        /// Gets or sets the pakFile2 type.
        /// </summary>
        /// <value>
        /// The type of the pak file.
        /// </value>
        public Type Pak2FileType { get; set; }

        /// <summary>
        /// Gets or sets the pak2 options.
        /// </summary>
        /// <value>
        /// The pack option.
        /// </value>
        public PakOption Pak2Options { get; set; }

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
        /// Gets the family other games.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, FamilyGame> OtherGames { get; set; }

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

        /// <summary>
        /// Gets or sets the file system type.
        /// </summary>
        /// <value>
        /// The type of the file system.
        /// </value>
        public Type FileSystemType { get; set; }

        /// <summary>
        /// Gets the specified family game.
        /// </summary>
        /// <param name="id">The game id.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">game</exception>
        public FamilyGame GetGame(string id) => Games.TryGetValue(id, out var game) ? game : throw new ArgumentOutOfRangeException(nameof(id), id);

        /// <summary>
        /// Parses the family resource uri.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public Resource ParseResource(Uri uri, bool throwOnError = true) => FileManager.ParseResource(this, uri, throwOnError);

        #region Pak

        /// <summary>
        /// Opens the family pak file.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="paths">The file paths.</param>
        /// <param name="index">The index.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public PakFile OpenPakFile(FamilyGame game, string[] paths, int index = 0, bool throwOnError = true) => FamilyManager.CreatePakFile(
            game ?? throw new ArgumentNullException(nameof(game)),
            throwOnError && (paths == null || paths.Length == 0) ? throw new ArgumentOutOfRangeException(nameof(paths)) : paths,
            index, null, game.PakOptions, throwOnError);

        /// <summary>
        /// Opens the family pak file.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public PakFile OpenPakFile(Resource resource, int index = 0, bool throwOnError = true) => FamilyManager.CreatePakFile(
            resource.Game ?? throw new ArgumentNullException(nameof(resource.Game)),
            throwOnError && (resource.Paths == null || resource.Paths.Length == 0) ? throw new ArgumentOutOfRangeException(nameof(resource.Paths)) : resource.Paths,
            index, resource.Host, resource.Options, throwOnError);

        /// <summary>
        /// Opens the family pak file.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="index">The index.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public PakFile OpenPakFile(Uri uri, int index = 0, bool throwOnError = true)
        {
            var resource = FileManager.ParseResource(this, uri);
            return FamilyManager.CreatePakFile(
                resource.Game ?? throw new ArgumentNullException(nameof(resource.Game)),
                throwOnError && (resource.Paths == null || resource.Paths.Length == 0) ? throw new ArgumentOutOfRangeException(nameof(resource.Paths)) : resource.Paths,
                index, resource.Host, resource.Options, throwOnError);
        }

        #endregion
    }
}