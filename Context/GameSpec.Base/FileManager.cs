using GameSpec.Formats;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.XPath;
using static GameSpec.Util;
using static Microsoft.Win32.Registry;

namespace GameSpec
{
    /// <summary>
    /// FileManager
    /// </summary>
    public class FileManager
    {
        const string GAMESPATH = "Games";

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
        public readonly IDictionary<string, HashSet<string>> Paths = new Dictionary<string, HashSet<string>>();

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
                        foreach (var path in z.GetStringOrArray())
                            AddPath(prop.Name, prop.Value, path, false);
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
                foreach (var key in y.GetStringOrArray())
                    if (!Paths.ContainsKey(id) && TryGetPathByRegistryKey(key, elem.TryGetProperty(key, out y) ? y : (JsonElement?)null, out z))
                        AddPath(id, elem, z);
            if (elem.TryGetProperty("key", out y))
                foreach (var key in y.GetStringOrArray())
                    if (!Paths.ContainsKey(id) && (z = StoreManager.GetPathByKey(key)) != null)
                        AddPath(id, elem, z);
            if (elem.TryGetProperty("dir", out y))
                foreach (var key in y.GetStringOrArray())
                    if (!Paths.ContainsKey(id) && LocalGames.TryGetValue(key, out var path))
                        AddPath(id, elem, path);
        }

        protected void AddFilter(string id, JsonElement elem)
        {
            if (!Filters.TryGetValue(id, out var z2)) Filters.Add(id, z2 = new Dictionary<string, string>());
            foreach (var filter in elem.EnumerateObject())
            {
                var value = filter.Value.ValueKind switch
                {
                    JsonValueKind.String => filter.Value.GetString(),
                    _ => throw new ArgumentOutOfRangeException(),
                };
                z2.Add(filter.Name, value);
            }
        }

        protected void AddIgnore(string id, string[] paths)
        {
            if (!Ignores.TryGetValue(id, out var z2)) Ignores.Add(id, z2 = new HashSet<string>());
            foreach (var v in paths) z2.Add(v);
        }

        protected void AddPath(string id, JsonElement elem, string path, bool usePath = true)
        {
            if (path == null || !Directory.Exists(path = GetPathWithSpecialFolders(path))) return;
            path = Path.GetFullPath(path);
            var paths = usePath && elem.TryGetProperty("path", out var z) ? z.GetStringOrArray(x => Path.Combine(path, x)) : new[] { path };
            foreach (var p in paths)
            {
                if (!Directory.Exists(p)) continue;
                if (!Paths.TryGetValue(id, out var z2)) Paths.Add(id, z2 = new HashSet<string>());
                z2.Add(p);
            }
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

        protected static bool TryGetPathByRegistryKey(string key, JsonElement? keyElem, out string path)
        {
            path = FindRegistryPath(new[] { $@"Wow6432Node\{key}", key });
            if (keyElem == null) return !string.IsNullOrEmpty(path);
            if (keyElem.Value.TryGetProperty("path", out var path2)) { path = Path.GetFullPath(GetPathWithSpecialFolders(path2.GetString(), path)); return !string.IsNullOrEmpty(path); }
            else if (keyElem.Value.TryGetProperty("xml", out var xml)
                && keyElem.Value.TryGetProperty("xmlPath", out var xmlPath)
                && TryGetSingleFileValue(GetPathWithSpecialFolders(xml.GetString(), path), "xml", xmlPath.GetString(), out path))
                return !string.IsNullOrEmpty(path);
            return false;
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

