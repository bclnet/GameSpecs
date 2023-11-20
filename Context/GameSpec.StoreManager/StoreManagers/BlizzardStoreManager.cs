using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace GameSpec.StoreManagers
{
    /// <summary>
    /// BlizzardStoreManager
    /// </summary>
    internal static class BlizzardStoreManager
    {
        static Dictionary<string, string> AppPaths = new Dictionary<string, string>();

        public static bool TryGetPathByKey(string key, JsonProperty prop, JsonElement? keyElem, out string path)
            => AppPaths.TryGetValue(key, out path);

        static BlizzardStoreManager()
        {
            var root = GetPath();
            if (root == null) return;
            var dbPath = Path.Combine(root, "product.db");
            if (!File.Exists(dbPath)) return;
            using var s = File.OpenRead(dbPath);
            Database data;
            try
            {
                data = Database.Parser.ParseFrom(s);
            }
            catch (InvalidProtocolBufferException)
            {
                data = new Database { ProductInstall = { ProductInstall.Parser.ParseFrom(s) } };
            }
            foreach (var app in data.ProductInstall)
            {
                var appPath = app.Settings.InstallPath;
                if (Directory.Exists(appPath)) AppPaths.Add(app.Uid, appPath);
            }
        }

        static string GetPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Battle.net", "Agent");
            else if (RuntimeInformation.OSDescription.StartsWith("android-")) return null;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return new[] { ".steam", ".steam/steam", ".steam/root", ".local/share/Steam" }
                    .Select(path => Path.Join(home, path, "appcache"))
                    .FirstOrDefault(Directory.Exists);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var home = "/Users/Shared";
                return new[] { "Battle.net/Agent" }
                    .Select(path => Path.Join(home, path, "data"))
                    .FirstOrDefault(Directory.Exists);
            }
            throw new PlatformNotSupportedException();
        }
    }
}

