using GameX.Formats;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
namespace GameX
{
    /// <summary>
    /// FileManager
    /// </summary>
    public class FileManager
    {
        //internal static PakFile CreatePakFile(FamilyGame game, object value, IFileSystem fileSystem, PakOption options, bool throwOnError)
        //{
        //    var family = game.Family;
        //    return WithPlatformGraphic(value switch
        //    {
        //        string path when game.PakFileType != null => (PakFile)Activator.CreateInstance(game.PakFileType, game, path, null),
        //        //string path when (options & PakOption.Stream) != 0 => new StreamPakFile(family.FileManager.HostFactory, game, path, fileSystem),
        //        string[] paths when (options & PakOption.Paths) != 0 && game.PakFileType != null => (PakFile)Activator.CreateInstance(game.PakFileType, game, paths),
        //        string[] paths when paths.Length == 1 => CreatePakFile(game, paths[0], fileSystem, options, throwOnError),
        //        string[] paths when paths.Length > 1 => new MultiPakFile(game, "Many", paths.Select(path => CreatePakFile(game, path, fileSystem, options, throwOnError)).ToArray()),
        //        string[] paths when paths.Length == 0 => null,
        //        null => null,
        //        _ => throw new ArgumentOutOfRangeException(nameof(value), $"{value}"),
        //    });
        //}

        /// <summary>
        /// Parses the resource.
        /// </summary>
        /// <param name="family">The estate.</param>
        /// <param name="uri">The URI.</param>
        /// <param name="throwOnError">Throws on error.</param>
        /// <returns></returns>
        public virtual Resource ParseResource(Family family, Uri uri, bool throwOnError = true)
        {
            if (uri == null || string.IsNullOrEmpty(uri.Fragment)) return new Resource { Game = new FamilyGame() };
            var game = family.GetGame(uri.Fragment[1..]);
            var fileSystem = string.IsNullOrEmpty(uri.Host) ? new HostSystem(uri) : game.CreateFileSystem() ?? DefaultSystem;
            var paths =
                // game-scheme
                string.Equals(uri.Scheme, "game", StringComparison.OrdinalIgnoreCase) ? FindGameFilePaths(family, fileSystem, r.Game, uri.LocalPath[1..]) ?? (throwOnError ? throw new ArgumentOutOfRangeException(nameof(r.Game), $"{game.Id}: unable to locate game resources") : Array.Empty<string>())
                // file-scheme
                : uri.IsFile ? GetLocalFilePaths(uri.LocalPath, out r.Options) ?? (throwOnError ? throw new InvalidOperationException($"{game.Id}: unable to locate file resources") : Array.Empty<string>())
                // network-scheme
                : GetHttpFilePaths(uri, out r.Host, out r.Options) ?? (throwOnError ? throw new InvalidOperationException($"{game.Id}: unable to locate network resources") : Array.Empty<string>());
            r.Paths = Ignores.TryGetValue(game.Id, out var ignores) ? paths.Where(file => !ignores.Contains(Path.GetFileName(file))).ToArray() : paths;
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
        internal string[] FindGameFilePaths(Family family, IFileSystem fileSystem, FamilyGame game, string pathOrPattern)
        {
            if (game == null) return null;
            // root folder
            if (string.IsNullOrEmpty(pathOrPattern)) return Paths.TryGetValue(game.Id, out var z) ? z.ToArray() : null;
            // search folder
            var searchPattern = Path.GetFileName(pathOrPattern);
            if (string.IsNullOrEmpty(searchPattern)) throw new ArgumentOutOfRangeException(nameof(pathOrPattern), pathOrPattern);
            var r = Paths.TryGetValue(game.Id, out var paths)
                ? ExpandGameFilePaths(game, fileSystem, paths, pathOrPattern).ToArray()
                : null;
            return r;
        }

        static IEnumerable<string> ExpandGameFilePaths(FamilyGame game, IFileSystem fileSystem, HashSet<string> paths, string pathOrPattern)
        {
            foreach (var gamePath in game.Paths ?? new[] { "." })
                foreach (var path in ExpandAndSearchPaths(fileSystem, paths, gamePath, pathOrPattern))
                    yield return path;
        }

        static IEnumerable<string> ExpandAndSearchPaths(IFileSystem fileSystem, HashSet<string> paths, string gamePath, string pathOrPattern)
        {
            // expand
            int expandStartIdx, expandMidIdx, expandEndIdx;
            if ((expandStartIdx = pathOrPattern.IndexOf('(')) != -1 &&
                (expandMidIdx = pathOrPattern.IndexOf(':', expandStartIdx)) != -1 &&
                (expandEndIdx = pathOrPattern.IndexOf(')', expandMidIdx)) != -1 &&
                expandStartIdx < expandEndIdx)
            {
                foreach (var expand in pathOrPattern.Substring(expandStartIdx + 1, expandEndIdx - expandStartIdx - 1).Split(':'))
                    foreach (var found in ExpandAndSearchPaths(fileSystem, paths, gamePath, pathOrPattern.Remove(expandStartIdx, expandEndIdx - expandStartIdx + 1).Insert(expandStartIdx, expand)))
                        yield return found;
                yield break;
            }
            foreach (var path in paths)
            {
                var searchPath = gamePath != "." ? Path.Combine(path, gamePath) : path;
                // folder
                var searchPattern = Path.GetDirectoryName(pathOrPattern);
                if (searchPattern.Contains('*'))
                {
                    foreach (var directory in fileSystem.GetDirectories(searchPath, searchPattern, searchPattern.Contains("**")))
                        foreach (var found in ExpandAndSearchPaths(fileSystem, new HashSet<string> { directory }, ".", Path.GetFileName(pathOrPattern)))
                            yield return found;
                    pathOrPattern = Path.GetFileName(pathOrPattern);
                }
                // file
                if (!pathOrPattern.Contains('*')) yield return Path.Combine(searchPath, pathOrPattern);
                else foreach (var file in fileSystem.GetFiles(searchPath, pathOrPattern)) yield return file;
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
    }
}

