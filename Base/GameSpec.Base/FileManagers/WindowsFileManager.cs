using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using static Microsoft.Win32.Registry;

namespace GameSpec.FileManagers
{
    /// <summary>
    /// WindowsFileManager
    /// </summary>
    internal class WindowsFileManager : FileManager
    {
        #region Parse File-Manager

        public override FileManager ParseFileManager(JsonElement elem)
        {
            AddRegistry(elem);
            base.ParseFileManager(elem);
            if (!elem.TryGetProperty("windows", out var z)) return this;
            elem = z;
            AddDirect(elem);
            AddIgnores(elem);
            AddFilters(elem);
            return this;
        }

        protected void AddRegistry(JsonElement elem)
        {
            if (!elem.TryGetProperty("registry", out var z)) return;
            foreach (var prop in z.EnumerateObject())
                if (!Paths.ContainsKey(prop.Name) && prop.Value.TryGetProperty("reg", out z))
                    foreach (var reg in z.GetStringOrArray())
                        if (TryGetRegistryByKey(reg, prop, prop.Value.TryGetProperty(reg, out z) ? z : null, out var path)) AddPath(prop, path);
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

        #endregion
    }
}

