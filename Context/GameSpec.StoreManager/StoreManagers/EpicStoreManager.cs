using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace GameSpec.StoreManagers
{
    /// <summary>
    /// EpicStoreManager
    /// </summary>
    internal static class EpicStoreManager
    {
        static Dictionary<string, string> AppPaths = new Dictionary<string, string>();

        public static bool TryGetPathByKey(string key, out string path) => AppPaths.TryGetValue(key, out path);

        static EpicStoreManager()
        {
            var root = GetPath();
            if (root == null) return;
            var dbPath = Path.Combine(root, "Manifests");
            foreach (var s in Directory.EnumerateFiles(dbPath).Where(s => s.EndsWith(".item")))
                AppPaths.Add(Path.GetFileNameWithoutExtension(s), JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(s)).GetProperty("InstallLocation").GetString());
        }

        static string GetPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                return new[] { @"Epic\EpicGamesLauncher" }
                    .Select(path => Path.Join(home, path, "Data"))
                    .FirstOrDefault(Directory.Exists);
            }
            else if (RuntimeInformation.OSDescription.StartsWith("android-")) return null;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return new[] { "Epic/EpicGamesLauncher" }
                    .Select(path => Path.Join(home, path, "Data"))
                    .FirstOrDefault(Directory.Exists);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var home = "/Users/Shared";
                return new[] { "Epic/EpicGamesLauncher" }
                    .Select(path => Path.Join(home, path, "Data"))
                    .FirstOrDefault(Directory.Exists);
            }
            throw new PlatformNotSupportedException();
        }
    }
}

