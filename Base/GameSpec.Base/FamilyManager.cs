using GameSpec.FileManagers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using static GameSpec.Resource;

namespace GameSpec
{
    public partial class FamilyManager
    {
        public static readonly string ApplicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly Family Unknown;
        public static readonly PakFile UnknownPakFile;

        //static string[] FamilyKeys;

        public class DefaultOptions
        {
            public string Family { get; set; }
            public string GameId { get; set; }
            public string ForcePath { get; set; }
            public bool ForceOpen { get; set; }
        }

        //public static DefaultOptions AppDefaultOptions;

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
        /// Gets the estates.
        /// </summary>
        /// <value>
        /// The estates.
        /// </value>
        public static IDictionary<string, Family> Families { get; } = new Dictionary<string, Family>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the specified estate.
        /// </summary>
        /// <param name="familyName">Name of the estate.</param>
        /// <param name="throwOnError">Throw on error.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">estateName</exception>
        public static Family GetFamily(string familyName, bool throwOnError = true)
            => Families.TryGetValue(familyName, out var estate) ? estate
            : throwOnError ? throw new ArgumentOutOfRangeException(nameof(familyName), familyName) : (Family)null;

        #region Parse

        internal static FileManager CreateFileManager()
            => FamilyPlatform.GetPlatformType() switch
            {
                FamilyPlatform.PlatformType.Windows => new WindowsFileManager(),
                FamilyPlatform.PlatformType.OSX => new MacOsFileManager(),
                FamilyPlatform.PlatformType.Linux => new LinuxFileManager(),
                FamilyPlatform.PlatformType.Android => new AndroidFileManager(),
                _ => throw new ArgumentOutOfRangeException(nameof(FamilyPlatform.GetPlatformType), FamilyPlatform.GetPlatformType().ToString()),
            };

        /// <summary>
        /// Parses the estate.
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
                return family;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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
                Key = elem.TryGetProperty("key", out z) ? TryParseKey(z.GetString(), out var z2) ? z2 : throw new ArgumentOutOfRangeException("key", z.GetString()) : null,
                FileSystemType = elem.TryGetProperty("fileSystemType", out z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("fileSystemType", z.GetString()) : family.FileSystemType,
                Editions = elem.TryGetProperty("editions", out z) ? z.EnumerateObject().ToDictionary(x => x.Name, x => ParseGameEdition(x.Name, x.Value), StringComparer.OrdinalIgnoreCase) : null,
                Found = locations.ContainsKey(id),
            };
            if (elem.TryGetProperty("pak", out z))
                familyGame.Paks = z.ValueKind switch
                {
                    JsonValueKind.String => new[] { new Uri(z.GetString()) },
                    JsonValueKind.Array => z.EnumerateArray().Select(y => new Uri(y.GetString())).ToArray(),
                    _ => throw new ArgumentOutOfRangeException("pak", $"{z}"),
                };
            if (elem.TryGetProperty("dat", out z))
                familyGame.Dats = z.ValueKind switch
                {
                    JsonValueKind.String => new[] { new Uri(z.GetString()) },
                    JsonValueKind.Array => z.EnumerateArray().Select(y => new Uri(y.GetString())).ToArray(),
                    _ => throw new ArgumentOutOfRangeException("dat", $"{z}"),
                };
            return familyGame;
        }

        static FamilyGame.Edition ParseGameEdition(string edition, JsonElement elem)
            => new FamilyGame.Edition
            {
                Id = edition,
                Name = (elem.TryGetProperty("name", out var z) ? z.GetString() : null) ?? throw new ArgumentNullException("name"),
                Key = elem.TryGetProperty("key", out z) ? TryParseKey(z.GetString(), out var z2) ? z2 : throw new ArgumentOutOfRangeException("key", z.GetString()) : null,
            };
        #endregion
    }
}