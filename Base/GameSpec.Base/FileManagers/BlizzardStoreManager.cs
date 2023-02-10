using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System;
using System.Text.Json;
using System.IO;

namespace GameSpec.Base.FileManagers
{
    /// <summary>
    /// BlizzardStoreManager
    /// </summary>
    internal static class BlizzardStoreManager
    {
        static Dictionary<string, string> AppPaths = new();

        public static bool TryGetPathByKey(string key, JsonProperty prop, JsonElement? keyElem, out string path)
            => AppPaths.TryGetValue(key, out path);

        static BlizzardStoreManager()
        {
            var root = GetPath();
            if (root == null) return;
            var dbPath = Path.Combine(root, "product.db");
        }

        static string GetPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam") ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Valve\Steam");
                if (key != null && key.GetValue("SteamPath") is string steamPath) return steamPath;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var paths = new[] { ".steam", ".steam/steam", ".steam/root", ".local/share/Steam" };
                return paths
                    .Select(path => Path.Join(home, path))
                    .FirstOrDefault(steamPath => Directory.Exists(Path.Join(steamPath, "appcache")));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var home = "/Users/Shared";
                var paths = new[] { "Battle.net/Agent" };
                return paths
                    .Select(path => Path.Join(home, path))
                    .FirstOrDefault(steamPath => Directory.Exists(Path.Join(steamPath, "data")));
            }
            throw new PlatformNotSupportedException();
        }
    }
}

