using GameSpec.Base.FileManagers;
using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using static GameSpec.Resource;

namespace GameSpec
{
    /// <summary>
    /// FileManager
    /// </summary>
    public abstract class FileManager
    {
        public static readonly IFileSystem DefaultSystem = new StandardSystem();

        /// <summary>
        /// IFileSystem
        /// </summary>
        public interface IFileSystem
        {
            string[] GetDirectories(string path, string searchPattern);
            string[] GetFiles(string path, string searchPattern);
        }

        /// <summary>
        /// StandardSystem
        /// </summary>
        class StandardSystem : IFileSystem
        {
            public string[] GetDirectories(string path, string searchPattern) => Directory.GetDirectories(path, searchPattern);
            public string[] GetFiles(string path, string searchPattern) => Directory.GetFiles(path, searchPattern);
        }

        /// <summary>
        /// Gets the host factory.
        /// </summary>
        /// <value>
        /// The host factory.
        /// </value>
        public virtual Func<Uri, string, AbstractHost> HostFactory { get; } = HttpHost.Factory;

        /// <summary>
        /// Gets a value indicating whether this instance has locations.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is data present; otherwise, <c>false</c>.
        /// </value>
        public bool HasPaths
            => Paths.Count != 0;

        /// <summary>
        /// The locations
        /// </summary>
        public IDictionary<string, HashSet<string>> Paths = new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// The ignores
        /// </summary>
        public IDictionary<string, HashSet<string>> Ignores = new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// The filters
        /// </summary>
        public IDictionary<string, IDictionary<string, string>> Filters = new Dictionary<string, IDictionary<string, string>>();

        #region Parse Resource

        /// <summary>
        /// Parses the resource.
        /// </summary>
        /// <param name="family">The estate.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public virtual Resource ParseResource(Family family, Uri uri, bool throwOnError = true)
        {
            if (uri == null) return new Resource { Game = string.Empty };
            var fragment = uri.Fragment?[(uri.Fragment.Length != 0 ? 1 : 0)..];
            var (gameId, game) = family.GetGame(fragment);
            var fileSystem = game.CreateFileSystem();
            var r = new Resource { Game = gameId };
            // game-scheme
            if (string.Equals(uri.Scheme, "game", StringComparison.OrdinalIgnoreCase)) r.Paths = FindGameFilePaths(family, fileSystem, r.Game, uri.LocalPath[1..]) ?? (throwOnError ? throw new ArgumentOutOfRangeException(nameof(r.Game), $"{gameId}: unable to locate game resources") : Array.Empty<string>());
            // file-scheme
            else if (uri.IsFile) r.Paths = GetLocalFilePaths(uri.LocalPath, out r.Options) ?? (throwOnError ? throw new InvalidOperationException($"{gameId}: unable to locate file resources") : Array.Empty<string>());
            // network-scheme
            else r.Paths = GetHttpFilePaths(uri, out r.Host, out r.Options) ?? (throwOnError ? throw new InvalidOperationException($"{gameId}: unable to locate network resources") : Array.Empty<string>());
            return r;
        }

        /// <summary>
        /// Gets the game file paths.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="game">The game.</param>
        /// <param name="pathOrPattern">The path or pattern.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">pathOrPattern</exception>
        /// <exception cref="ArgumentOutOfRangeException">pathOrPattern</exception>
        public string[] FindGameFilePaths(Family family, IFileSystem fileSystem, string game, string pathOrPattern)
        {
            fileSystem ??= DefaultSystem;
            var (_, familyGame) = family.GetGame(game);
            if (familyGame == null) return null;
            // root folder
            if (string.IsNullOrEmpty(pathOrPattern)) return Paths.TryGetValue(game, out var z) ? z.ToArray() : null;
            // search folder
            var searchPattern = Path.GetFileName(pathOrPattern);
            if (string.IsNullOrEmpty(searchPattern)) throw new ArgumentOutOfRangeException(nameof(pathOrPattern), pathOrPattern);
            return Paths.TryGetValue(game, out var paths)
                ? ExpandGameFilePaths(familyGame, fileSystem, Ignores.TryGetValue(game, out var ignores) ? ignores : null, paths, pathOrPattern).ToArray()
                : null;
        }

        static IEnumerable<string> ExpandGameFilePaths(FamilyGame game, IFileSystem fileSystem, HashSet<string> ignore, HashSet<string> paths, string pathOrPattern)
        {
            foreach (var gamePath in game.Paths ?? new[] { "." })
                foreach (var path in ExpandAndSearchPaths(fileSystem, ignore, paths, gamePath, pathOrPattern))
                    yield return path;
        }

        static IEnumerable<string> ExpandAndSearchPaths(IFileSystem fileSystem, HashSet<string> ignore, HashSet<string> paths, string gamePath, string pathOrPattern)
        {
            // expand
            int expandStartIdx, expandMidIdx, expandEndIdx;
            if ((expandStartIdx = pathOrPattern.IndexOf('(')) != -1 &&
                (expandMidIdx = pathOrPattern.IndexOf(':', expandStartIdx)) != -1 &&
                (expandEndIdx = pathOrPattern.IndexOf(')', expandMidIdx)) != -1 &&
                expandStartIdx < expandEndIdx)
            {
                foreach (var expand in pathOrPattern.Substring(expandStartIdx + 1, expandEndIdx - expandStartIdx - 1).Split(':'))
                    foreach (var found in ExpandAndSearchPaths(fileSystem, ignore, paths, gamePath, pathOrPattern.Remove(expandStartIdx, expandEndIdx - expandStartIdx + 1).Insert(expandStartIdx, expand)))
                        yield return found;
                yield break;
            }
            foreach (var path in paths)
            {
                // folder
                var searchPattern = Path.GetDirectoryName(pathOrPattern);
                if (searchPattern.IndexOf('*') != -1)
                {
                    foreach (var directory in fileSystem.GetDirectories(path, searchPattern))
                        foreach (var found in ExpandAndSearchPaths(fileSystem, ignore, new HashSet<string> { directory }, gamePath, Path.GetFileName(pathOrPattern)))
                            yield return found;
                    yield break;
                }
                // file
                var searchIdx = pathOrPattern.IndexOf('*');
                if (searchIdx == -1) yield return Path.Combine(path, pathOrPattern);
                else foreach (var file in fileSystem.GetFiles(path, pathOrPattern)) if (ignore == null || !ignore.Contains(Path.GetFileName(file))) yield return file;
            }
        }

        /// <summary>
        /// Gets the local file paths.
        /// </summary>
        /// <param name="pathOrPattern">The path or pattern.</param>
        /// <param name="streamPak">if set to <c>true</c> [file pak].</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">pathOrPattern</exception>
        string[] GetLocalFilePaths(string pathOrPattern, out PakOption options)
        {
            if (pathOrPattern == null) throw new ArgumentNullException(nameof(pathOrPattern));
            var searchPattern = Path.GetFileName(pathOrPattern);
            var path = Path.GetDirectoryName(pathOrPattern);
            // file
            if (!string.IsNullOrEmpty(searchPattern))
            {
                options = default;
                return searchPattern.Contains('*')
                    ? Directory.GetFiles(path, searchPattern)
                    : File.Exists(pathOrPattern) ? new[] { pathOrPattern } : null;
            }
            // folder
            options = PakOption.Stream;
            searchPattern = Path.GetFileName(path);
            path = Path.GetDirectoryName(path);
            return pathOrPattern.Contains('*')
                ? Directory.GetDirectories(path, searchPattern)
                : Directory.Exists(pathOrPattern) ? new[] { pathOrPattern } : null;
        }

        /// <summary>
        /// Gets the host file paths.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="host">The host.</param>
        /// <param name="options">if set to <c>true</c> [file pak].</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">uri</exception>
        /// <exception cref="ArgumentOutOfRangeException">pathOrPattern</exception>
        /// <exception cref="NotSupportedException">Web wildcard access to supported</exception>
        string[] GetHttpFilePaths(Uri uri, out Uri host, out PakOption options)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            var pathOrPattern = uri.LocalPath;
            var searchPattern = Path.GetFileName(pathOrPattern);
            var path = Path.GetDirectoryName(pathOrPattern);
            // file
            if (!string.IsNullOrEmpty(searchPattern)) throw new ArgumentOutOfRangeException(nameof(pathOrPattern), pathOrPattern); //: Web single file access to supported.
            // folder
            options = PakOption.Stream;
            searchPattern = Path.GetFileName(path);
            path = Path.GetDirectoryName(path);
            if (path.Contains('*')) throw new NotSupportedException("Web wildcard folder access");
            host = new UriBuilder(uri) { Path = $"{path}/", Fragment = null }.Uri;
            if (searchPattern.Contains('*'))
            {
                var set = new HttpHost(host).GetSetAsync().Result ?? throw new NotSupportedException(".set not found. Web wildcard access");
                var pattern = $"^{Regex.Escape(searchPattern.Replace('*', '%')).Replace("_", ".").Replace("%", ".*")}$";
                return set.Where(x => Regex.IsMatch(x, pattern)).ToArray();
            }
            return new[] { searchPattern };
        }

        #endregion

        #region Parse File-Manager

        static bool TryGetStorePathByKey(string key, JsonProperty prop, JsonElement? keyElem, out string path)
        {
            var parts = key.Split(':', 2);
            return parts[0] switch
            {
                "Steam" => SteamStoreManager.TryGetPathByKey(parts[1], prop, keyElem, out path),
                "GOG" => GogStoreManager.TryGetPathByKey(parts[1], prop, keyElem, out path),
                "Blizzard" => BlizzardStoreManager.TryGetPathByKey(parts[1], prop, keyElem, out path),
                "Epic" => EpicStoreManager.TryGetPathByKey(parts[1], prop, keyElem, out path),
                _ => throw new ArgumentOutOfRangeException(nameof(key), parts[0]),
            };
        }

        protected void AddPath(JsonProperty prop, string path)
        {
            if (path == null || !Directory.Exists(path = PathWithSpecialFolders(path))) return;
            path = Path.GetFullPath(path);
            var paths = prop.Value.TryGetProperty("path", out var z) ? z.GetStringOrArray(x => Path.Combine(path, x)) : new[] { path };
            foreach (var path2 in paths)
            {
                if (!Directory.Exists(path2)) continue;
                if (!Paths.TryGetValue(prop.Name, out var z2)) Paths.Add(prop.Name, z2 = new HashSet<string>());
                z2.Add(path2.Replace('/', '\\'));
            }
        }

        protected void AddIgnore(JsonProperty prop, string path)
        {
            if (!Ignores.TryGetValue(prop.Name, out var z2)) Ignores.Add(prop.Name, z2 = new HashSet<string>());
            z2.Add(path);
        }

        protected void AddFilter(JsonProperty prop, string name, JsonElement element)
        {
            if (!Filters.TryGetValue(prop.Name, out var z2)) Filters.Add(prop.Name, z2 = new Dictionary<string, string>());
            var value = element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                _ => throw new ArgumentOutOfRangeException(),
            };
            z2.Add(name, value);
        }

        protected static string PathWithSpecialFolders(string path, string rootPath = null) =>
            path.StartsWith("%Path%", StringComparison.OrdinalIgnoreCase) ? $"{rootPath}{path[6..]}"
            : path.StartsWith("%AppPath%", StringComparison.OrdinalIgnoreCase) ? $"{FamilyManager.ApplicationPath}{path[9..]}"
            : path.StartsWith("%AppData%", StringComparison.OrdinalIgnoreCase) ? $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{path[9..]}"
            : path.StartsWith("%LocalAppData%", StringComparison.OrdinalIgnoreCase) ? $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{path[14..]}"
            : path;

        public virtual FileManager ParseFileManager(JsonElement elem)
        {
            AddApplication(elem);
            AddDirect(elem);
            AddIgnores(elem);
            AddFilters(elem);
            return this;
        }

        protected void AddDirect(JsonElement elem)
        {
            if (!elem.TryGetProperty("direct", out var z)) return;
            foreach (var prop in z.EnumerateObject())
                if (prop.Value.TryGetProperty("path", out z))
                    foreach (var path in z.GetStringOrArray()) AddPath(prop, path);
        }

        protected void AddIgnores(JsonElement elem)
        {
            if (!elem.TryGetProperty("ignores", out var z)) return;
            foreach (var prop in z.EnumerateObject())
                if (prop.Value.TryGetProperty("path", out z))
                    foreach (var path in z.GetStringOrArray()) AddIgnore(prop, path);
        }

        protected void AddFilters(JsonElement elem)
        {
            if (!elem.TryGetProperty("filters", out var z)) return;
            foreach (var prop in z.EnumerateObject())
                foreach (var filter in prop.Value.EnumerateObject()) AddFilter(prop, filter.Name, filter.Value);
        }

        protected void AddApplication(JsonElement elem)
        {
            if (!elem.TryGetProperty("application", out var z)) return;
            foreach (var prop in z.EnumerateObject())
                if (!Paths.ContainsKey(prop.Name) && prop.Value.TryGetProperty("key", out z))
                    foreach (var key in z.GetStringOrArray())
                        if (TryGetStorePathByKey(key, prop, prop.Value.TryGetProperty(key, out z) ? z : null, out var path)) AddPath(prop, path);
        }

        protected static bool TryGetSingleFileValue(string path, string ext, string select, out string value)
        {
            value = null;
            if (!File.Exists(path)) return false;
            var content = File.ReadAllText(path);
            value = ext switch
            {
                "xml" => XDocument.Parse(content).XPathSelectElement(select)?.Value,
                _ => throw new ArgumentOutOfRangeException(nameof(ext)),
            };
            if (value != null) value = Path.GetDirectoryName(value);
            return true;
        }

        #endregion
    }
}

