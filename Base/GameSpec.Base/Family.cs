using GameSpec.Formats;
using OpenStack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static GameSpec.Resource;

namespace GameSpec
{
    /// <summary>
    /// Family
    /// </summary>
    public class Family
    {
        public static readonly Family Empty = new()
        {
            Id = string.Empty,
            Name = "Empty",
            Games = new Dictionary<string, FamilyGame>(),
            FileManager = FamilyManager.CreateFileManager(),
        };

        /// <summary>
        /// ByteKey
        /// </summary>
        public class ByteKey
        {
            public byte[] Key;
        }

        static unsafe Family()
        {
            if (FamilyPlatform.InTestHost && FamilyPlatform.Startups.Count == 0) FamilyPlatform.Startups.Add(TestPlatform.Startup);
            foreach (var startup in FamilyPlatform.Startups) if (startup()) return;
            FamilyPlatform.Platform = FamilyPlatform.PlatformUnknown;
            FamilyPlatform.GraphicFactory = source => null; // throw new Exception("No GraphicFactory");
            Debug.AssertFunc = x => System.Diagnostics.Debug.Assert(x);
            Debug.LogFunc = a => System.Diagnostics.Debug.Print(a);
            Debug.LogFormatFunc = (a, b) => System.Diagnostics.Debug.Print(a, b);
        }

        protected internal Family() { }

        /// <summary>
        /// Touches this instance.
        /// </summary>
        public static void Bootstrap() { }

        /// <summary>
        /// Ensures this instance.
        /// </summary>
        public virtual Family Ensure() => this;

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
        /// Gets the identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public string Id { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The estate name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the engine.
        /// </summary>
        /// <value>
        /// The estate name.
        /// </value>
        public string Engine { get; set; }

        /// <summary>
        /// Gets the studio.
        /// </summary>
        /// <value>
        /// The estate studio.
        /// </value>
        public string Studio { get; set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The estate description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets the type of the pak file.
        /// </summary>
        /// <value>
        /// The type of the pak file.
        /// </value>
        public Type PakFileType { get; set; }

        /// <summary>
        /// Gets or sets the pak multi.
        /// </summary>
        /// <value>
        /// The multi-pak.
        /// </value>
        public PakOption PakOptions { get; set; }

        /// <summary>
        /// Gets the type of the dat file.
        /// </summary>
        /// <value>
        /// The type of the pak file.
        /// </value>
        public Type Pak2FileType { get; set; }

        /// <summary>
        /// Gets or sets the pak multi.
        /// </summary>
        /// <value>
        /// The multi-pak.
        /// </value>
        public PakOption Pak2Options { get; set; }

        /// <summary>
        /// Gets the game.
        /// </summary>
        /// <param name="id">The game id.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">game</exception>
        public FamilyGame GetGame(string id)
            => Games.TryGetValue(id, out var game) ? game : throw new ArgumentOutOfRangeException(nameof(id), id);

        /// <summary>
        /// Gets the estates games.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, FamilyGame> Games { get; set; }

        /// <summary>
        /// Gets the estates file manager.
        /// </summary>
        /// <value>
        /// The file manager.
        /// </value>
        public FileManager FileManager { get; set; }

        /// <summary>
        /// Gets the type of the file system.
        /// </summary>
        /// <value>
        /// The type of the file system.
        /// </value>
        public Type FileSystemType { get; set; }

        /// <summary>
        /// Parses the estates resource.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public Resource ParseResource(Uri uri, bool throwOnError = true)
            => FileManager.ParseResource(this, uri, throwOnError);

        #region Pak

        /// <summary>
        /// Withes the platform graphic.
        /// </summary>
        /// <param name="pakFile">The pak file.</param>
        /// <returns></returns>
        static PakFile WithPlatformGraphic(PakFile pakFile)
        {
            if (pakFile != null) pakFile.Graphic = FamilyPlatform.GraphicFactory?.Invoke(pakFile);
            return pakFile;
        }

        /// <summary>
        /// Pak file factory.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        /// <param name="host">The host.</param>
        /// <param name="options">The options.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        PakFile CreatePakFile(FamilyGame game, object value, int index, Uri host, PakOption options, bool throwOnError)
            => WithPlatformGraphic(value switch
            {
                string path when index == 0 && PakFileType != null => (PakFile)Activator.CreateInstance(PakFileType, this, game, path, null),
                string path when index == 1 && Pak2FileType != null => (PakFile)Activator.CreateInstance(Pak2FileType, this, game, path, null),
                string path when (options & PakOption.Stream) != 0 => new StreamPakFile(FileManager.HostFactory, this, game, path, host),
                string[] paths when (options & PakOption.Paths) != 0 && index == 0 && PakFileType != null => (PakFile)Activator.CreateInstance(PakFileType, this, game, paths),
                string[] paths when (options & PakOption.Paths) != 0 && index == 1 && Pak2FileType != null => (PakFile)Activator.CreateInstance(Pak2FileType, this, game, paths),
                string[] paths when paths.Length == 1 => CreatePakFile(game, paths[0], index, host, options, throwOnError),
                string[] paths when paths.Length > 1 => new MultiPakFile(this, game, "Many", paths.Select(path => CreatePakFile(game, path, index, host, options, throwOnError)).ToArray()),
                string[] paths when paths.Length == 0 => null,
                null => null,
                _ => throw new ArgumentOutOfRangeException(nameof(value), $"{value}"),
            });

        /// <summary>
        /// Opens the estates pak file.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="paths">The file paths.</param>
        /// <param name="index">The index.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public PakFile OpenPakFile(FamilyGame game, string[] paths, int index = 0, bool throwOnError = true)
            => CreatePakFile(
                game ?? throw new ArgumentNullException(nameof(game)), 
                paths?.Length != 0 ? paths : throw new ArgumentOutOfRangeException(nameof(paths)), 
                index, null, PakOptions, throwOnError);

        /// <summary>
        /// Opens the estates pak file.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public PakFile OpenPakFile(Resource resource, int index = 0, bool throwOnError = true)
            => CreatePakFile(
                resource.Game ?? throw new ArgumentNullException(nameof(resource.Game)),
                resource.Paths?.Length != 0 ? resource.Paths : throw new ArgumentOutOfRangeException(nameof(resource.Paths)),
                index, resource.Host, resource.Options, throwOnError);

        /// <summary>
        /// Opens the estates pak file.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="index">The index.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public PakFile OpenPakFile(Uri uri, int index = 0, bool throwOnError = true)
        {
            var resource = FileManager.ParseResource(this, uri);
            return CreatePakFile(
                resource.Game ?? throw new ArgumentNullException(nameof(resource.Game)), 
                resource.Paths?.Length != 0 ? resource.Paths : throw new ArgumentOutOfRangeException(nameof(resource.Paths)),
                index, resource.Host, resource.Options, throwOnError);
        }

        #endregion
    }
}