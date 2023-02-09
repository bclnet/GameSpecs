using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace GameSpec.Base.FileManagers
{
    /// <summary>
    /// GogStoreManager
    /// </summary>
    internal static class GogStoreManager
    {
        static Dictionary<string, string> AppPaths = new();

        public static bool TryGetPathByKey(string key, JsonProperty prop, JsonElement? keyElem, out string path)
            => AppPaths.TryGetValue(key, out path);

        static GogStoreManager()
        {
            var root = GetPath();
            if (root == null) return;
        }

        static string GetPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\GOG.com\GalaxyClient\paths") ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\GOG.com\GalaxyClient\paths");
                if (key != null && key.GetValue("client") is string steamPath) return steamPath;
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
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var paths = new[] { ".steam", ".steam/steam", ".steam/root", ".local/share/Steam" };
                return paths
                    .Select(path => Path.Join(home, path))
                    .FirstOrDefault(steamPath => Directory.Exists(Path.Join(steamPath, "appcache")));
            }
            throw new PlatformNotSupportedException();
        }
    }
}

