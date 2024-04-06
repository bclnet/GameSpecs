using GameX.Formats;
using GameX.Platforms;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using static GameX.Util;
using static Microsoft.Win32.Registry;

namespace GameX
{
    #region FileManager

    /// <summary>
    /// FileManager
    /// </summary>
    public class FileManager
    {
        const string GAMESPATH = "Games";

        public class PathItem
        {
            public string Root;
            public string Type;
            public string[] Paths;

            public PathItem(string root, JsonElement elem)
            {
                Root = root;
                Type = elem.TryGetProperty("type", out var z) ? z.GetString() : null;
                Paths = elem.TryGetProperty("path", out z) ? _listV(z) : new string[0];
            }

            public void Add(string root, JsonElement elem)
                => throw new NotSupportedException();
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
        public bool HasPaths => Paths.Count != 0;

        /// <summary>
        /// The locations
        /// </summary>
        public readonly IDictionary<string, PathItem> Paths = new Dictionary<string, PathItem>();

        /// <summary>
        /// The ignores
        /// </summary>
        public readonly IDictionary<string, HashSet<string>> Ignores = new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// The filters
        /// </summary>
        public readonly IDictionary<string, IDictionary<string, string>> Filters = new Dictionary<string, IDictionary<string, string>>();

        public static readonly IDictionary<string, string> LocalGames;

        static FileManager()
        {
            // get locale games
            var gameRoots = DriveInfo.GetDrives().Select(x => Path.Combine(x.Name, GAMESPATH)).ToList();
            if (Platform.PlatformOS == Platform.OS.Android) gameRoots.Add(Path.Combine("/sdcard", GAMESPATH));
            LocalGames = gameRoots.Where(Directory.Exists).SelectMany(Directory.GetDirectories).ToDictionary(Path.GetFileName, x => x);
        }

        internal FileManager() { }
        public FileManager(JsonElement elem)
        {
            // applications
            if (elem.TryGetProperty("application", out var z))
                foreach (var prop in z.EnumerateObject())
                    if (!Paths.ContainsKey(prop.Name))
                        AddApplication(prop.Name, prop.Value);
            // direct
            if (elem.TryGetProperty("direct", out z))
                foreach (var prop in z.EnumerateObject())
                    if (prop.Value.TryGetProperty("path", out z))
                        foreach (var path in _listV(z))
                            AddPath(prop.Name, prop.Value, path);
            // ignores
            if (elem.TryGetProperty("ignores", out z))
                foreach (var prop in z.EnumerateObject())
                    AddIgnore(prop.Name, _list(prop.Value, "path"));
            // filters
            if (elem.TryGetProperty("filters", out z))
                foreach (var prop in z.EnumerateObject())
                    AddFilter(prop.Name, prop.Value);
        }

        /// <summary>
        /// Merges the family.
        /// </summary>
        /// <param name="source">The source.</param>
        public void Merge(FileManager source)
        {
            foreach (var s in source.Paths) Paths.Add(s.Key, s.Value);
            foreach (var s in source.Ignores) Ignores.Add(s.Key, s.Value);
            foreach (var s in source.Filters) Filters.Add(s.Key, s.Value);
        }

        #region Parse File-Manager

        protected void AddApplication(string id, JsonElement elem)
        {
            string z;
            if (Platform.PlatformOS == Platform.OS.Windows && elem.TryGetProperty("reg", out var y))
                foreach (var k in _listV(y))
                    if (!Paths.ContainsKey(id) && (z = GetPathByRegistryKey(k, elem.TryGetProperty(k, out y) ? y : (JsonElement?)null)) != null)
                        AddPath(id, elem, z);
            if (elem.TryGetProperty("key", out y))
                foreach (var k in _listV(y))
                    if (!Paths.ContainsKey(id) && (z = StoreManager.GetPathByKey(k)) != null)
                        AddPath(id, elem, z);
            if (elem.TryGetProperty("dir", out y))
                foreach (var k in _listV(y))
                    if (!Paths.ContainsKey(id) && LocalGames.TryGetValue(k, out var path))
                        AddPath(id, elem, path);
        }

        protected void AddFilter(string id, JsonElement elem)
        {
            if (!Filters.TryGetValue(id, out var z2)) Filters.Add(id, z2 = new Dictionary<string, string>());
            foreach (var z in elem.EnumerateObject())
                z2.Add(z.Name, _valueV(z.Value).ToString());
        }

        protected void AddIgnore(string id, string[] paths)
        {
            if (!Ignores.TryGetValue(id, out var z2)) Ignores.Add(id, z2 = new HashSet<string>());
            foreach (var v in paths) z2.Add(v);
        }

        protected void AddPath(string id, JsonElement elem, string path)
        {
            if (path == null) throw new ArgumentOutOfRangeException(nameof(path));
            path = Path.GetFullPath(GetPathWithSpecialFolders(path));
            if (!Directory.Exists(path) && !File.Exists(path)) return;
            if (!Paths.TryGetValue(id, out var z2)) Paths.Add(id, new PathItem(path, elem));
            else z2.Add(path, elem);
        }

        protected static string GetPathWithSpecialFolders(string path, string rootPath = null) =>
            path.StartsWith("%Path%", StringComparison.OrdinalIgnoreCase) ? $"{rootPath}{path[6..]}"
            : path.StartsWith("%AppPath%", StringComparison.OrdinalIgnoreCase) ? $"{FamilyManager.ApplicationPath}{path[9..]}"
            : path.StartsWith("%AppData%", StringComparison.OrdinalIgnoreCase) ? $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{path[9..]}"
            : path.StartsWith("%LocalAppData%", StringComparison.OrdinalIgnoreCase) ? $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{path[14..]}"
            : path;

        /// <summary>
        /// Gets the executable path.
        /// </summary>
        /// <param name="name">Name of the sub.</param>
        /// <returns></returns>
        protected static string FindRegistryPath(string[] paths)
        {
            var localMachine64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            var currentUser64 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            foreach (var p in paths)
                try
                {
                    var keyPath = p.Replace('/', '\\');
                    var key = new Func<RegistryKey>[] {
                        () => localMachine64.OpenSubKey($"SOFTWARE\\{keyPath}"),
                        () => currentUser64.OpenSubKey($"SOFTWARE\\{keyPath}"),
                        () => ClassesRoot.OpenSubKey($"VirtualStore\\MACHINE\\SOFTWARE\\{keyPath}") }
                        .Select(x => x()).FirstOrDefault(x => x != null);
                    if (key == null) continue;
                    // search directories
                    var path = new[] { "Path", "Install Dir", "InstallDir", "InstallLocation" }
                        .Select(x => key.GetValue(x) as string)
                        .FirstOrDefault(x => !string.IsNullOrEmpty(x) && Directory.Exists(x));
                    if (path == null)
                    {
                        // search files
                        path = new[] { "Installed Path", "ExePath", "Exe" }
                            .Select(x => key.GetValue(x) as string)
                            .FirstOrDefault(x => !string.IsNullOrEmpty(x) && File.Exists(x));
                        if (path != null) path = Path.GetDirectoryName(path);
                    }
                    if (path != null && Directory.Exists(path)) return path;
                }
                catch { return null; }
            return null;
        }

        protected static string GetPathByRegistryKey(string key, JsonElement? elem)
        {
            var path = FindRegistryPath(new[] { $@"Wow6432Node\{key}", key });
            if (elem == null) return path;
            else if (elem.Value.TryGetProperty("path", out var z)) return Path.GetFullPath(GetPathWithSpecialFolders(z.GetString(), path));
            else if (elem.Value.TryGetProperty("xml", out z) && elem.Value.TryGetProperty("xmlPath", out var y))
                return GetSingleFileValue(GetPathWithSpecialFolders(z.GetString(), path), "xml", y.GetString());
            else return null;
        }

        protected static string GetSingleFileValue(string path, string ext, string select)
        {
            if (!File.Exists(path)) return null;
            var content = File.ReadAllText(path);
            var value = ext switch
            {
                "xml" => XDocument.Parse(content).XPathSelectElement(select)?.Value,
                _ => throw new ArgumentOutOfRangeException(nameof(ext)),
            };
            return value != null ? Path.GetDirectoryName(value) : null;
        }

        #endregion
    }

    #endregion

    #region IFileSystem

    /// <summary>
    /// IFileSystem
    /// </summary>
    public interface IFileSystem
    {
        IEnumerable<string> Glob(string path, string searchPattern);
        bool FileExists(string path);
        (string path, long length) FileInfo(string path);
        BinaryReader OpenReader(string path);
        BinaryWriter OpenWriter(string path);
    }

    #endregion

    #region StandardFileSystem

    /// <summary>
    /// StandardFileSystem
    /// </summary>
    internal class StandardFileSystem : IFileSystem
    {
        readonly string Root;
        readonly int Skip;
        public StandardFileSystem(string root) { Root = root; Skip = Root.Length + 1; }
        public IEnumerable<string> Glob(string path, string searchPattern)
        {
            var matcher = new Matcher();
            matcher.AddIncludePatterns(new[] { string.IsNullOrEmpty(searchPattern) ? "**/*" : searchPattern });
            return matcher.GetResultsInFullPath(Path.Combine(Root, path)).Select(x => x[Skip..]);
        }
        public bool FileExists(string path) => File.Exists(Path.Combine(Root, path));
        public (string path, long length) FileInfo(string path) => File.Exists(path = Path.Combine(Root, path)) ? (path[Skip..], new FileInfo(Path.Combine(Root, path)).Length) : (null, 0);
        public BinaryReader OpenReader(string path) => new BinaryReader(File.Open(Path.Combine(Root, path), FileMode.Open, FileAccess.Read, FileShare.Read));
        public BinaryWriter OpenWriter(string path) => new BinaryWriter(File.Open(Path.Combine(Root, path), FileMode.Open, FileAccess.Write, FileShare.Write));

        public static Func<string, bool> CreateMatcher(string searchPattern)
        {
            if (string.IsNullOrEmpty(searchPattern)) return x => true;
            var wildcardCount = searchPattern.Count(x => x.Equals('*'));
            if (wildcardCount <= 0) return x => x.Equals(searchPattern, StringComparison.CurrentCultureIgnoreCase);
            else if (wildcardCount == 1)
            {
                var newPattern = searchPattern.Replace("*", "");
                if (searchPattern.StartsWith("*")) return x => x.EndsWith(newPattern, StringComparison.CurrentCultureIgnoreCase);
                else if (searchPattern.EndsWith("*")) return x => x.StartsWith(newPattern, StringComparison.CurrentCultureIgnoreCase);
            }
            var regexPattern = string.Concat("^", Regex.Escape(searchPattern).Replace("\\*", ".*"), "$");
            return x =>
            {
                try { return Regex.IsMatch(x, regexPattern); }
                catch { return false; }
            };
        }
    }

    #endregion

    #region ZipFileSystem

    /// <summary>
    /// ZipFileSystem
    /// </summary>
    internal class ZipFileSystem : IFileSystem
    {
        readonly ZipArchive Pak;
        readonly string Root;
        public ZipFileSystem(string root, string path)
        {
            Pak = ZipFile.Open(root, ZipArchiveMode.Read);
            Root = string.IsNullOrEmpty(path) ? string.Empty : $"{path}{Path.AltDirectorySeparatorChar}";
        }
        public IEnumerable<string> Glob(string path, string searchPattern)
        {
            var root = Path.Combine(Root, path);
            var skip = root.Length;
            var matcher = StandardFileSystem.CreateMatcher(searchPattern);
            return Pak.Entries.Where(x =>
            {
                var fn = x.FullName;
                return fn.Length > skip && fn.StartsWith(root) && matcher(fn[skip..]);
            }).Select(x => x.FullName[skip..]);
        }
        public bool FileExists(string path) => Pak.GetEntry(Path.Combine(Root, path)) != null;
        public (string path, long length) FileInfo(string path) { var e = Pak.GetEntry(Path.Combine(Root, path)); return e != null ? (e.Name, e.Length) : (null, 0); }
        public BinaryReader OpenReader(string path) => new BinaryReader(Pak.GetEntry(Path.Combine(Root, path)).Open());
        public BinaryWriter OpenWriter(string path) => throw new NotSupportedException();
    }

    //public static Func<string, bool> CreateMatcher(string root, string searchPattern)
    //{
    //    if (string.IsNullOrEmpty(root)) return CreateMatcher(searchPattern);
    //    var matcher = CreateMatcher(searchPattern);
    //    var skip = root.Length;
    //    return x => x.StartsWith(root) && matcher(x[skip..]);
    //}

    #endregion

    #region ZipIsoFileSystem

    /// <summary>
    /// ZipIsoFileSystem
    /// </summary>
    internal class ZipIsoFileSystem : IFileSystem
    {
        readonly ZipArchive Pak;
        readonly string Path;
        public ZipIsoFileSystem(string root, string path)
        {
            Pak = ZipFile.Open(root, ZipArchiveMode.Read);
            Path = path;
        }
        public IEnumerable<string> Glob(string path, string searchPattern)
        {
            var matcher = StandardFileSystem.CreateMatcher(searchPattern);
            return Pak.Entries.Where(x => matcher(x.Name)).Select(x => x.Name);
        }
        public bool FileExists(string path) => Pak.GetEntry(path) != null;
        public (string path, long length) FileInfo(string path) { var e = Pak.GetEntry(path); return e != null ? (e.Name, e.Length) : (null, 0); }
        public BinaryReader OpenReader(string path) => new BinaryReader(Pak.GetEntry(path).Open());
        public BinaryWriter OpenWriter(string path) => throw new NotSupportedException();
    }

    #endregion

    #region HostFileSystem

    /// <summary>
    /// HostFileSystem
    /// </summary>
    internal class HostFileSystem : IFileSystem
    {
        public HostFileSystem(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            var pathOrPattern = uri.LocalPath;
            var searchPattern = Path.GetFileName(pathOrPattern);
            var path = Path.GetDirectoryName(pathOrPattern);
            // file
            if (!string.IsNullOrEmpty(searchPattern)) throw new ArgumentOutOfRangeException(nameof(pathOrPattern), pathOrPattern); //: Web single file access to supported.

            //options = PakOption.Stream;
            //searchPattern = Path.GetFileName(path);
            //path = Path.GetDirectoryName(path);
            //if (path.Contains('*')) throw new NotSupportedException("Web wildcard folder access");
            //host = new UriBuilder(uri) { Path = $"{path}/", Fragment = null }.Uri;
            //if (searchPattern.Contains('*'))
            //{
            //    var set = new HttpHost(host).GetSetAsync().Result ?? throw new NotSupportedException(".set not found. Web wildcard access");
            //    var pattern = $"^{Regex.Escape(searchPattern.Replace('*', '%')).Replace("_", ".").Replace("%", ".*")}$";
            //    return set.Where(x => Regex.IsMatch(x, pattern)).ToArray();
            //}
            //return new[] { searchPattern };
        }
        public IEnumerable<string> Glob(string path, string searchPattern)
        {
            var matcher = new Matcher();
            matcher.AddIncludePatterns(new[] { searchPattern });
            return matcher.GetResultsInFullPath(searchPattern);
        }
        public bool FileExists(string path) => File.Exists(path);
        public (string path, long length) FileInfo(string path) => File.Exists(path) ? (path, 0) : (null, 0);
        public BinaryReader OpenReader(string path) => null;
        public BinaryWriter OpenWriter(string path) => null;
    }

    #endregion
}