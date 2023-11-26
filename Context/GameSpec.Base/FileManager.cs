using GameSpec.Formats;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.XPath;
using static Microsoft.Win32.Registry;

namespace GameSpec
{
    /// <summary>
    /// FileManager
    /// </summary>
    public class FileManager
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

        #region Parse File-Manager

        public virtual FileManager ParseFileManager(JsonElement elem)
        {
            // applications
            if (elem.TryGetProperty("application", out var z))
                foreach (var prop in z.EnumerateObject())
                    if (!Paths.ContainsKey(prop.Name))
                        AddApplication(prop);
            // direct
            if (elem.TryGetProperty("direct", out z))
                foreach (var prop in z.EnumerateObject())
                    if (prop.Value.TryGetProperty("path", out z))
                        foreach (var path in z.GetStringOrArray())
                            AddPath(prop, path, false);
            // ignores
            if (elem.TryGetProperty("ignores", out z))
                foreach (var prop in z.EnumerateObject())
                    if (prop.Value.TryGetProperty("path", out z))
                        foreach (var path in z.GetStringOrArray())
                            AddIgnore(prop, path);
            // filters
            if (elem.TryGetProperty("filters", out z))
                foreach (var prop in z.EnumerateObject())
                    foreach (var filter in prop.Value.EnumerateObject())
                        AddFilter(prop, filter.Name, filter.Value);
            return this;
        }

        protected void AddApplication(JsonProperty prop)
        {
            const string GAMESPATH = "Games";
            // get locale games
            var platformOS = FamilyPlatform.PlatformOS;
            var gameRoots = DriveInfo.GetDrives().Select(x => Path.Combine(x.Name, GAMESPATH)).ToList();
            if (platformOS == FamilyPlatform.OS.Android) gameRoots.Add(Path.Combine("/sdcard", GAMESPATH));
            var games = gameRoots.Where(Directory.Exists).SelectMany(Directory.GetDirectories).ToDictionary(Path.GetFileName, x => x);
            //
            if (platformOS == FamilyPlatform.OS.Windows && prop.Value.TryGetProperty("reg", out var z))
                foreach (var key in z.GetStringOrArray())
                    if (!Paths.ContainsKey(prop.Name) && TryGetPathByRegistryKey(key, prop.Value.TryGetProperty(key, out z) ? z : (JsonElement?)null, out var path))
                        AddPath(prop, path);
            if (prop.Value.TryGetProperty("key", out z))
                foreach (var key in z.GetStringOrArray())
                    if (!Paths.ContainsKey(prop.Name) && StoreManager.TryGetPathByKey(key, out var path))
                        AddPath(prop, path);
            if (prop.Value.TryGetProperty("dir", out z))
                foreach (var key in z.GetStringOrArray())
                    if (!Paths.ContainsKey(prop.Name) && games.TryGetValue(key, out var path))
                        AddPath(prop, path);
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

        protected void AddIgnore(JsonProperty prop, string path)
        {
            if (!Ignores.TryGetValue(prop.Name, out var z2)) Ignores.Add(prop.Name, z2 = new HashSet<string>());
            z2.Add(path);
        }

        protected void AddPath(JsonProperty prop, string path, bool usePath = true)
        {
            if (path == null || !Directory.Exists(path = GetPathWithSpecialFolders(path))) return;
            path = Path.GetFullPath(path);
            var paths = usePath && prop.Value.TryGetProperty("path", out var z) ? z.GetStringOrArray(x => Path.Combine(path, x)) : new[] { path };
            foreach (var p in paths)
            {
                if (!Directory.Exists(p)) continue;
                if (!Paths.TryGetValue(prop.Name, out var z2)) Paths.Add(prop.Name, z2 = new HashSet<string>());
                z2.Add(p);
            }
        }

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

        protected static string GetPathWithSpecialFolders(string path, string rootPath = null) =>
            path.StartsWith("%Path%", StringComparison.OrdinalIgnoreCase) ? $"{rootPath}{path[6..]}"
            : path.StartsWith("%AppPath%", StringComparison.OrdinalIgnoreCase) ? $"{FamilyManager.ApplicationPath}{path[9..]}"
            : path.StartsWith("%AppData%", StringComparison.OrdinalIgnoreCase) ? $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{path[9..]}"
            : path.StartsWith("%LocalAppData%", StringComparison.OrdinalIgnoreCase) ? $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{path[14..]}"
            : path;

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

