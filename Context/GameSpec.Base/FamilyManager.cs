﻿using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using static GameSpec.FamilyGame;
using static GameSpec.Resource;

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
            UnknownPakFile = Unknown.OpenPakFile(null);
        }

        /// <summary>
        /// Gets the families.
        /// </summary>
        /// <value>
        /// The family.
        /// </value>
        public static IDictionary<string, Family> Families { get; } = new Dictionary<string, Family>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the specified family.
        /// </summary>
        /// <param name="familyName">Name of the family.</param>
        /// <param name="throwOnError">Throw on error.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">estateName</exception>
        public static Family GetFamily(string familyName, bool throwOnError = true)
            => Families.TryGetValue(familyName, out var estate) ? estate
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
        /// <param name="value">The value.</param>
        /// <param name="index">The index.</param>
        /// <param name="host">The host.</param>
        /// <param name="options">The options.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        internal static PakFile CreatePakFile(Family source, FamilyGame game, object value, int index, Uri host, PakOption options, bool throwOnError)
            => WithPlatformGraphic(value switch
            {
                string path when index == 0 && source.PakFileType != null => (PakFile)Activator.CreateInstance(source.PakFileType, source, game, path, null),
                string path when index == 1 && source.Pak2FileType != null => (PakFile)Activator.CreateInstance(source.Pak2FileType, source, game, path, null),
                string path when (options & PakOption.Stream) != 0 => new StreamPakFile(source.FileManager.HostFactory, source, game, path, host),
                string[] paths when (options & PakOption.Paths) != 0 && index == 0 && source.PakFileType != null => (PakFile)Activator.CreateInstance(source.PakFileType, source, game, paths),
                string[] paths when (options & PakOption.Paths) != 0 && index == 1 && source.Pak2FileType != null => (PakFile)Activator.CreateInstance(source.Pak2FileType, source, game, paths),
                string[] paths when paths.Length == 1 => CreatePakFile(source, game, paths[0], index, host, options, throwOnError),
                string[] paths when paths.Length > 1 => new MultiPakFile(source, game, "Many", paths.Select(path => CreatePakFile(source, game, path, index, host, options, throwOnError)).ToArray()),
                string[] paths when paths.Length == 0 => null,
                null => null,
                _ => throw new ArgumentOutOfRangeException(nameof(value), $"{value}"),
            });

        #endregion

        #region Parse

        /// <summary>
        /// Create family manager.
        /// </summary>
        internal static FileManager CreateFileManager() => new();

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
                using var doc = JsonDocument.Parse(json, options);
                var elem = doc.RootElement;
                if (elem.TryGetProperty("fileManager", out var z)) fileManager.ParseFileManager(z);
                var locations = fileManager.Paths;
                var familyType = elem.TryGetProperty("familyType", out z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("familyType", z.GetString()) : null;
                var family = familyType != null ? (Family)Activator.CreateInstance(familyType) : new Family();
                family.Id = (elem.TryGetProperty("id", out z) ? z.GetString() : null) ?? throw new ArgumentNullException("id");
                family.Name = (elem.TryGetProperty("name", out z) ? z.GetString() : null) ?? throw new ArgumentNullException("name");
                family.Engine = elem.TryGetProperty("engine", out z) ? z.GetString() : null;
                family.Studio = (elem.TryGetProperty("studio", out z) ? z.GetString() : null) ?? string.Empty;
                family.Description = elem.TryGetProperty("description", out z) ? z.GetString() : string.Empty;
                family.PakFileType = elem.TryGetProperty("pakFileType", out z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("pakFileType", z.GetString()) : null;
                family.PakOptions = elem.TryGetProperty("pakOptions", out z) ? Enum.TryParse<PakOption>(z.GetString(), true, out var z1) ? z1 : throw new ArgumentOutOfRangeException("pakOptions", z.GetString()) : 0;
                family.Pak2FileType = elem.TryGetProperty("pak2FileType", out z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("pak2FileType", z.GetString()) : null;
                family.Pak2Options = elem.TryGetProperty("pak2Options", out z) ? Enum.TryParse<PakOption>(z.GetString(), true, out var z2) ? z2 : throw new ArgumentOutOfRangeException("pak2Options", z.GetString()) : 0;
                family.FileSystemType = elem.TryGetProperty("fileSystemType", out z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("fileSystemType", z.GetString()) : null;
                family.FileManager = fileManager;
                family.Games = elem.TryGetProperty("games", out z) ? z.EnumerateObject().ToDictionary(x => x.Name, x => ParseGame(family, locations, x.Name, x.Value), StringComparer.OrdinalIgnoreCase) : throw new ArgumentNullException("games");
                family.OtherGames = elem.TryGetProperty("other-games", out z) ? z.EnumerateObject().ToDictionary(x => x.Name, x => ParseOtherGame(family, x.Name, x.Value), StringComparer.OrdinalIgnoreCase) : null;
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

        static FamilyGame ParseGame(Family family, IDictionary<string, HashSet<string>> locations, string id, JsonElement elem)
        {
            var familyGame = new FamilyGame
            {
                Id = id,
                Name = (elem.TryGetProperty("name", out var z) ? z.GetString() : null) ?? throw new ArgumentNullException("name"),
                Engine = elem.TryGetProperty("engine", out z) ? z.GetString() : family.Engine,
                Paks = elem.TryGetProperty("pak", out z) ? z.GetStringOrArray(x => new Uri(x)) : null,
                Dats = elem.TryGetProperty("dat", out z) ? z.GetStringOrArray(x => new Uri(x)) : null,
                Paths = elem.TryGetProperty("path", out z) ? z.GetStringOrArray() : null,
                Key = elem.TryGetProperty("key", out z) ? TryParseKey(z.GetString(), out var z2) ? z2 : throw new ArgumentOutOfRangeException("key", z.GetString()) : null,
                FileSystemType = elem.TryGetProperty("fileSystemType", out z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("fileSystemType", z.GetString()) : family.FileSystemType,
                Editions = elem.TryGetProperty("editions", out z) ? z.EnumerateObject().ToDictionary(x => x.Name, x => ParseGameEdition(x.Name, x.Value), StringComparer.OrdinalIgnoreCase) : null,
                Dlc = elem.TryGetProperty("dlc", out z) ? z.EnumerateObject().ToDictionary(x => x.Name, x => ParseGameDownloadableContent(x.Name, x.Value), StringComparer.OrdinalIgnoreCase) : null,
                Locales = elem.TryGetProperty("locals", out z) ? z.EnumerateObject().ToDictionary(x => x.Name, x => ParseGameLocal(x.Name, x.Value), StringComparer.OrdinalIgnoreCase) : null,
                Found = locations.ContainsKey(id),
            };
            return familyGame;
        }

        static FamilyGame ParseOtherGame(Family family, string id, JsonElement elem)
        {
            var familyGame = new FamilyGame
            {
                Id = id,
                Name = (elem.TryGetProperty("name", out var z) ? z.GetString() : null) ?? throw new ArgumentNullException("name"),
                Engine = elem.TryGetProperty("engine", out z) ? z.GetString() : family.Engine,
            };
            return familyGame;
        }

        static Edition ParseGameEdition(string edition, JsonElement elem)
            => new()
            {
                Id = edition,
                Name = (elem.TryGetProperty("name", out var z) ? z.GetString() : null) ?? throw new ArgumentNullException("name"),
                Key = elem.TryGetProperty("key", out z) ? TryParseKey(z.GetString(), out var z2) ? z2 : throw new ArgumentOutOfRangeException("key", z.GetString()) : null,
            };

        static DownloadableContent ParseGameDownloadableContent(string edition, JsonElement elem)
            => new()
            {
                Id = edition,
                Name = (elem.TryGetProperty("name", out var z) ? z.GetString() : null) ?? throw new ArgumentNullException("name"),
            };

        static Locale ParseGameLocal(string edition, JsonElement elem)
            => new()
            {
                Id = edition,
                Name = (elem.TryGetProperty("name", out var z) ? z.GetString() : null) ?? throw new ArgumentNullException("name"),
            };

        #endregion
    }
}