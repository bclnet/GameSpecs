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

        public static bool TryGetPathByKey(string key, JsonProperty prop, JsonElement? keyElem, out string path)
            => AppPaths.TryGetValue(key, out path);
        
        static EpicStoreManager()
        {
            var root = GetPath();
            if (root == null) return;
        }

        static string GetPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam") ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\Valve\Steam");
                if (key != null && key.GetValue("SteamPath") is string steamPath) return steamPath;
            }
            else if (RuntimeInformation.OSDescription.StartsWith("android-")) return null;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return new[] { "?Epic?" }
                    .Select(path => Path.Join(home, path, "appcache"))
                    .FirstOrDefault(Directory.Exists);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var home = "/Users/Shared";
                return new[] { "UnrealEngine/Launcher" }
                    .Select(path => Path.Join(home, path, "VaultCache"))
                    .FirstOrDefault(Directory.Exists);
            }
            throw new PlatformNotSupportedException();
        }
    }
}

