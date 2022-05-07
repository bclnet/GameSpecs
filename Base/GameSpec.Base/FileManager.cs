using GameSpec.Formats;
using System;
using System.Collections.Generic;
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
            var game = family.GetGame(fragment);
            var r = new Resource { Game = game.id };
            // game-scheme
            if (string.Equals(uri.Scheme, "game", StringComparison.OrdinalIgnoreCase)) r.Paths = FindGameFilePaths(r.Game, uri.LocalPath[1..]) ?? (throwOnError ? throw new ArgumentOutOfRangeException(nameof(r.Game), $"{game.id}: unable to locate game resources") : Array.Empty<string>());
            // file-scheme
            else if (uri.IsFile) r.Paths = GetLocalFilePaths(uri.LocalPath, out r.Options) ?? (throwOnError ? throw new InvalidOperationException($"{game.id}: unable to locate file resources") : Array.Empty<string>());
            // network-scheme
            else r.Paths = GetHttpFilePaths(uri, out r.Host, out r.Options) ?? (throwOnError ? throw new InvalidOperationException($"{game.id}: unable to locate network resources") : Array.Empty<string>());
            return r;
        }

        /// <summary>
        /// Gets the game file paths.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="pathOrPattern">The path or pattern.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">pathOrPattern</exception>
        /// <exception cref="ArgumentOutOfRangeException">pathOrPattern</exception>
        public string[] FindGameFilePaths(string game, string pathOrPattern)
        {
            if (pathOrPattern == null) throw new ArgumentNullException(nameof(pathOrPattern));
            var searchPattern = Path.GetFileName(pathOrPattern);
            // folder
            if (string.IsNullOrEmpty(searchPattern)) throw new ArgumentOutOfRangeException(nameof(pathOrPattern), pathOrPattern);
            // file
            return Paths.TryGetValue(game, out var paths)
                ? ExpandAndSearchPaths(Ignores.TryGetValue(game, out var ignores) ? ignores : null, paths, pathOrPattern).ToArray()
                : null;
        }

        static IEnumerable<string> ExpandAndSearchPaths(HashSet<string> ignore, HashSet<string> paths, string pathOrPattern)
        {
            // expand
            int expandStartIdx, expandMidIdx, expandEndIdx;
            if ((expandStartIdx = pathOrPattern.IndexOf('(')) != -1 &&
                (expandMidIdx = pathOrPattern.IndexOf(':', expandStartIdx)) != -1 &&
                (expandEndIdx = pathOrPattern.IndexOf(')', expandMidIdx)) != -1 &&
                expandStartIdx < expandEndIdx)
            {
                foreach (var expand in pathOrPattern.Substring(expandStartIdx + 1, expandEndIdx - expandStartIdx - 1).Split(':'))
                    foreach (var found in ExpandAndSearchPaths(ignore, paths, pathOrPattern.Remove(expandStartIdx, expandEndIdx - expandStartIdx + 1).Insert(expandStartIdx, expand)))
                        yield return found;
                yield break;
            }
            foreach (var path in paths)
            {
                // folder
                var searchPattern = Path.GetDirectoryName(pathOrPattern);
                if (searchPattern.IndexOf('*') != -1)
                {
                    foreach (var directory in Directory.GetDirectories(path, searchPattern))
                        foreach (var found in ExpandAndSearchPaths(ignore, new HashSet<string> { directory }, Path.GetFileName(pathOrPattern)))
                            yield return found;
                    yield break;
                }
                // file
                var searchIdx = pathOrPattern.IndexOf('*');
                if (searchIdx == -1) yield return Path.Combine(path, pathOrPattern);
                else foreach (var file in Directory.GetFiles(path, pathOrPattern)) if (ignore == null || !ignore.Contains(Path.GetFileName(file))) yield return file;
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

        protected void AddPath(JsonProperty prop, string path)
        {
            if (path == null || !Directory.Exists(path = PathWithSpecialFolders(path))) return;
            path = Path.GetFullPath(path);
            var paths = prop.Value.TryGetProperty("path", out var z) ? z.ValueKind switch
            {
                JsonValueKind.String => new[] { Path.Combine(path, z.GetString()) },
                JsonValueKind.Array => z.EnumerateArray().Select(y => Path.Combine(path, y.GetString())),
                _ => throw new ArgumentOutOfRangeException(),
            } : new[] { path };
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
            : path.StartsWith("%AppData%", StringComparison.OrdinalIgnoreCase) ? $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{path[9..]}"
            : path.StartsWith("%LocalAppData%", StringComparison.OrdinalIgnoreCase) ? $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{path[14..]}"
            : path;

        public virtual FileManager ParseFileManager(JsonElement elem)
        {
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
                {
                    var paths = z.ValueKind switch
                    {
                        JsonValueKind.String => new[] { z.GetString() },
                        JsonValueKind.Array => z.EnumerateArray().Select(y => y.GetString()),
                        _ => throw new ArgumentOutOfRangeException(),
                    };
                    foreach (var path in paths) AddPath(prop, path);
                }
        }

        protected void AddIgnores(JsonElement elem)
        {
            if (!elem.TryGetProperty("ignores", out var z)) return;
            foreach (var prop in z.EnumerateObject())
                if (prop.Value.TryGetProperty("path", out z))
                {
                    var paths = z.ValueKind switch
                    {
                        JsonValueKind.String => new[] { z.GetString() },
                        JsonValueKind.Array => z.EnumerateArray().Select(y => y.GetString()),
                        _ => throw new ArgumentOutOfRangeException(),
                    };
                    foreach (var path in paths) AddIgnore(prop, path);
                }
        }

        protected void AddFilters(JsonElement elem)
        {
            if (!elem.TryGetProperty("filters", out var z)) return;
            foreach (var prop in z.EnumerateObject())
                foreach (var filter in prop.Value.EnumerateObject()) AddFilter(prop, filter.Name, filter.Value);
        }

        #endregion
    }
}

