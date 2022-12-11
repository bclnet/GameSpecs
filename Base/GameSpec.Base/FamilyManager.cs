using GameSpec.FileManagers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using static GameSpec.Resource;

namespace GameSpec
{
    public class FamilyManager
    {
        public static readonly string ApplicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly Family Unknown;
        public static readonly PakFile UnknownPakFile;

        //static string[] FamilyKeys = new[] { "AC", "Arkane", "Aurora", "Blizzard", "Cry", "Cyanide", "Hpl", "IW", "Lith", "Origin", "Red", "Rsi", "Tes", "Unity", "Unknown", "Unreal", "Valve" };
        static string[] FamilyKeys = new[] { "IW", "Unknown" };

        public class DefaultOptions
        {
            public string Family { get; set; }
            public string GameId { get; set; }
            public string ForcePath { get; set; }
            public bool ForceOpen { get; set; }
        }

        /* Sample: Data
         * OK - Family = "AC", GameId = "AC", ForcePath = "TabooTable/0E00001E.taboo", ForceOpen = true,
         */

        /* Sample: Texture
         * BAD - Family = "AC", GameId = "AC", ForcePath = "Texture060043BE", ForceOpen = true,
         * BAD - Family = "Cry", GameId = "Hunt", ForcePath = "Data/Textures/asteroids/asteroid_dmg_brown_organic_01_ddn.dds", ForceOpen = true,
         * BAD - Family = "Rsi", GameId = "StarCitizen", ForcePath = "Data/Textures/references/color.dds", ForceOpen = true,
         * BAD - Family = "Rsi", GameId = "StarCitizen", ForcePath = "Data/Textures/asteroids/asteroid_dmg_brown_organic_01_ddn.dds", ForceOpen = true,
         * BAD - Family = "Tes", GameId = "Morrowind", ForcePath = "Data/Textures/asteroids/asteroid_dmg_brown_organic_01_ddn.dds", ForceOpen = true,
         * BAD - Family = "Tes", GameId = "StarCitizen", ForcePath = "Data/Textures/asteroids/asteroid_dmg_brown_organic_01_ddn.dds", ForceOpen = true,
         * OK - Family = "Valve", GameId = "Dota2", ForcePath = "materials/console_background_color_psd_b9e26a4.vtex_c", ForceOpen = true,
         */

        public static DefaultOptions AppDefaultOptions = new DefaultOptions
        {
            Family = "IW",

            // Call of Duty 2 - IWD
            //GameId = "COD2", ForceOpen = true,
            //ForcePath = "iw_08.iwd/images/155_cannon.iwi",

            // Call of Duty 3 - XBOX only
            //GameId = "COD3", ForceOpen = true,

            // Call of Duty 4: Modern Warfare - IWD, FF
            //GameId = "COD4", ForceOpen = true,
            //ForcePath = "mp_farm.ff/images/155_cannon.iwi",

            // Call of Duty: World at War - IWD, FF
            //GameId = "COD:WaW", ForceOpen = true,

            // Call of Duty: Modern Warfare 2
            //GameId = "MW2", ForceOpen = true,

            // Call of Duty: Black Ops - IWD, FF
            //GameId = "COD:BO", ForceOpen = true,

            // Call of Duty: Call of Duty: Modern Warfare 3
            //GameId = "MW3", ForceOpen = true,

            // Call of Duty: Black Ops 2 - FF
            //GameId = "COD:BO2", ForceOpen = true,

            // Call of Duty: Advanced Warfare
            //GameId = "COD:AW", ForceOpen = true,

            // Call of Duty: Black Ops III - XPAC,FF
            //GameId = "COD:BO3", ForceOpen = true,

            // Call of Duty: Modern Warfare 3
            //GameId = "MW3", ForceOpen = true,

            // Call of Duty: WWII
            //GameId = "WWII", ForceOpen = true,
        };

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
                family.Studio = (elem.TryGetProperty("studio", out z) ? z.GetString() : null) ?? string.Empty;
                family.Description = elem.TryGetProperty("description", out z) ? z.GetString() : string.Empty;
                family.PakFileType = elem.TryGetProperty("pakFileType", out z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("pakFileType", z.GetString()) : null;
                family.PakOptions = elem.TryGetProperty("pakOptions", out z) ? Enum.TryParse<PakOption>(z.GetString(), true, out var z1) ? z1 : throw new ArgumentOutOfRangeException("pakOptions", z.GetString()) : 0;
                family.Pak2FileType = elem.TryGetProperty("pak2FileType", out z) ? Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException("pak2FileType", z.GetString()) : null;
                family.Pak2Options = elem.TryGetProperty("pak2Options", out z) ? Enum.TryParse<PakOption>(z.GetString(), true, out var z2) ? z2 : throw new ArgumentOutOfRangeException("pak2Options", z.GetString()) : 0;
                family.Games = elem.TryGetProperty("games", out z) ? z.EnumerateObject().ToDictionary(x => x.Name, x => ParseGame(locations, x.Name, x.Value), StringComparer.OrdinalIgnoreCase) : throw new ArgumentNullException("games");
                family.FileManager = fileManager;
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
                value = new Family.AesKey { Key = key };
            }
            else throw new ArgumentOutOfRangeException(nameof(str), str);
            return true;
        }

        static FamilyGame ParseGame(IDictionary<string, HashSet<string>> locations, string game, JsonElement elem)
        {
            var familyGame = new FamilyGame
            {
                Game = game,
                Name = (elem.TryGetProperty("name", out var z) ? z.GetString() : null) ?? throw new ArgumentNullException("name"),
                Key = elem.TryGetProperty("key", out z) ? TryParseKey(z.GetString(), out var z2) ? z2 : throw new ArgumentOutOfRangeException("key", z.GetString()) : null,
                Found = locations.ContainsKey(game),
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

        #endregion
    }
}