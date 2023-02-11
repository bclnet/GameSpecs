using GameSpec.Formats;
using Microsoft.Win32;
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
using static Microsoft.Win32.Registry;

namespace GameSpec
{
    /// <summary>
    /// FileManager
    /// </summary>
    public class FileManager
    {
        public static readonly IFileSystem DefaultSystem = new StandardSystem();
        public string platformName;

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

        public FileManager(string platformName) => this.platformName = platformName;

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
            if (uri == null) return new Resource { Game = new FamilyGame() };
            var fragment = uri.Fragment?[(uri.Fragment.Length != 0 ? 1 : 0)..];
            var game = family.GetGame(fragment);
            var fileSystem = game.CreateFileSystem();
            var r = new Resource { Game = game };
            // game-scheme
            if (string.Equals(uri.Scheme, "game", StringComparison.OrdinalIgnoreCase)) r.Paths = FindGameFilePaths(family, fileSystem, r.Game, uri.LocalPath[1..]) ?? (throwOnError ? throw new ArgumentOutOfRangeException(nameof(r.Game), $"{game.Id}: unable to locate game resources") : Array.Empty<string>());
            // file-scheme
            else if (uri.IsFile) r.Paths = GetLocalFilePaths(uri.LocalPath, out r.Options) ?? (throwOnError ? throw new InvalidOperationException($"{game.Id}: unable to locate file resources") : Array.Empty<string>());
            // network-scheme
            else r.Paths = GetHttpFilePaths(uri, out r.Host, out r.Options) ?? (throwOnError ? throw new InvalidOperationException($"{game.Id}: unable to locate network resources") : Array.Empty<string>());
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
        public string[] FindGameFilePaths(Family family, IFileSystem fileSystem, FamilyGame game, string pathOrPattern)
        {
            fileSystem ??= DefaultSystem;
            if (game == null) return null;
            // root folder
            if (string.IsNullOrEmpty(pathOrPattern)) return Paths.TryGetValue(game.Id, out var z) ? z.ToArray() : null;
            // search folder
            var searchPattern = Path.GetFileName(pathOrPattern);
            if (string.IsNullOrEmpty(searchPattern)) throw new ArgumentOutOfRangeException(nameof(pathOrPattern), pathOrPattern);
            return Paths.TryGetValue(game.Id, out var paths)
                ? ExpandGameFilePaths(game, fileSystem, Ignores.TryGetValue(game.Id, out var ignores) ? ignores : null, paths, pathOrPattern).ToArray()
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
                var searchPath = gamePath != "." ? Path.Combine(path, gamePath) : path;
                if (pathOrPattern.IndexOf('*') == -1) yield return Path.Combine(searchPath, pathOrPattern);
                else foreach (var file in fileSystem.GetFiles(searchPath, pathOrPattern)) if (ignore == null || !ignore.Contains(Path.GetFileName(file))) yield return file;
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

        protected void AddPath(JsonProperty prop, string path)
        {
            if (path == null || !Directory.Exists(path = PathWithSpecialFolders(path))) return;
            path = Path.GetFullPath(path);
            var paths = prop.Value.TryGetProperty("path", out var z) ? z.GetStringOrArray(x => Path.Combine(path, x)) : new[] { path };
            foreach (var path2 in paths)
            {
                if (!Directory.Exists(path2)) continue;
                if (!Paths.TryGetValue(prop.Name, out var z2)) Paths.Add(prop.Name, z2 = new HashSet<string>());
                z2.Add(path2); //.Replace('/', '\\'));
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

        protected void AddApplicationByRegistry(JsonElement elem)
        {
            if (!elem.TryGetProperty("application", out var z)) return;
            foreach (var prop in z.EnumerateObject())
                if (!Paths.ContainsKey(prop.Name) && prop.Value.TryGetProperty("reg", out z))
                    foreach (var reg in z.GetStringOrArray())
                        if (!Paths.ContainsKey(prop.Name) && TryGetRegistryByKey(reg, prop, prop.Value.TryGetProperty(reg, out z) ? z : null, out var path)) AddPath(prop, path);
        }

        protected static bool TryGetRegistryByKey(string key, JsonProperty prop, JsonElement? keyElem, out string path)
        {
            path = GetRegistryExePath(new[] { $@"Wow6432Node\{key}", key });
            if (keyElem == null) return !string.IsNullOrEmpty(path);
            if (keyElem.Value.TryGetProperty("path", out var path2)) { path = Path.GetFullPath(PathWithSpecialFolders(path2.GetString(), path)); return !string.IsNullOrEmpty(path); }
            else if (keyElem.Value.TryGetProperty("xml", out var xml)
                && keyElem.Value.TryGetProperty("xmlPath", out var xmlPath)
                && TryGetSingleFileValue(PathWithSpecialFolders(xml.GetString(), path), "xml", xmlPath.GetString(), out path))
                return !string.IsNullOrEmpty(path);
            return false;
        }

        /// <summary>
        /// Gets the executable path.
        /// </summary>
        /// <param name="name">Name of the sub.</param>
        /// <returns></returns>
        protected static string GetRegistryExePath(string[] paths)
        {
            var localMachine64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            var currentUser64 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            foreach (var path in paths)
                try
                {
                    var key = path.Replace('/', '\\');
                    var foundKey = new Func<RegistryKey>[] {
                        () => localMachine64.OpenSubKey($"SOFTWARE\\{key}"),
                        () => currentUser64.OpenSubKey($"SOFTWARE\\{key}"),
                        () => ClassesRoot.OpenSubKey($"VirtualStore\\MACHINE\\SOFTWARE\\{key}") }
                        .Select(x => x()).FirstOrDefault(x => x != null);
                    if (foundKey == null) continue;
                    var foundPath = new[] { "Path", "Install Dir", "InstallDir", "InstallLocation" }
                        .Select(x => foundKey.GetValue(x) as string)
                        .FirstOrDefault(x => !string.IsNullOrEmpty(x) || Directory.Exists(x));
                    if (foundPath == null)
                    {
                        foundPath = new[] { "Installed Path", "ExePath", "Exe" }
                            .Select(x => foundKey.GetValue(x) as string)
                            .FirstOrDefault(x => !string.IsNullOrEmpty(x) || File.Exists(x));
                        if (foundPath != null) foundPath = Path.GetDirectoryName(foundPath);
                    }
                    if (foundPath != null && Directory.Exists(foundPath)) return foundPath;
                }
                catch { return null; }
            return null;
        }

        protected static string PathWithSpecialFolders(string path, string rootPath = null) =>
            path.StartsWith("%Path%", StringComparison.OrdinalIgnoreCase) ? $"{rootPath}{path[6..]}"
            : path.StartsWith("%AppPath%", StringComparison.OrdinalIgnoreCase) ? $"{FamilyManager.ApplicationPath}{path[9..]}"
            : path.StartsWith("%AppData%", StringComparison.OrdinalIgnoreCase) ? $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{path[9..]}"
            : path.StartsWith("%LocalAppData%", StringComparison.OrdinalIgnoreCase) ? $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{path[14..]}"
            : path;

        public virtual FileManager ParseFileManager(JsonElement elem)
        {
            AddApplicationByRegistry(elem);
            AddApplication(elem);
            AddDirect(elem);
            AddIgnores(elem);
            AddFilters(elem);
            if (!elem.TryGetProperty(platformName, out var z)) return this;
            elem = z;
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
                        if (!Paths.ContainsKey(prop.Name) && StoreManager.TryGetPathByKey(key, prop, prop.Value.TryGetProperty(key, out z) ? z : null, out var path)) AddPath(prop, path);
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

