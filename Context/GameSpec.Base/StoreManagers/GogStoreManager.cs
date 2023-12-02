using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using static SQLitePCL.raw;

namespace GameSpec.StoreManagers
{
    /// <summary>
    /// GogStoreManager
    /// </summary>
    internal static class GogStoreManager
    {
        static Dictionary<string, string> AppPaths = new Dictionary<string, string>();

        public static bool TryGetPathByKey(string key, out string path) => AppPaths.TryGetValue(key, out path);

        static GogStoreManager()
        {
            SetProvider(new SQLite3Provider_e_sqlite3());
            var root = GetPath();
            if (root == null) return;
            var dbPath = Path.Combine(root, "galaxy-2.0.db");
            if (!File.Exists(dbPath)) { return; }
            if (sqlite3_open(dbPath, out var conn) != SQLITE_OK ||
                sqlite3_prepare_v2(conn, "SELECT productId, installationPath FROM InstalledBaseProducts", out var stmt) != SQLITE_OK) return;
            var read = true;
            while (read)
                switch (sqlite3_step(stmt))
                {
                    case SQLITE_ROW:
                        var appId = sqlite3_column_int(stmt, 0).ToString();
                        var appPath = sqlite3_column_text(stmt, 1).utf8_to_string();
                        if (Directory.Exists(appPath)) AppPaths.Add(appId, appPath);
                        break;
                    case SQLITE_DONE: read = false; break;
                }
            sqlite3_finalize(stmt);
        }

        static string GetPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                return new[] { @"GOG.com\Galaxy" }
                    .Select(path => Path.Join(home, path, "storage"))
                    .FirstOrDefault(Directory.Exists);
                //var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\GOG.com\GalaxyClient\paths") ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\GOG.com\GalaxyClient\paths");
                //if (key != null && key.GetValue("client") is string steamPath) return steamPath;
            }
            else if (RuntimeInformation.OSDescription.StartsWith("android-")) return null;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return new[] { "?GOG?" }
                    .Select(path => Path.Join(home, path, "storage"))
                    .FirstOrDefault(Directory.Exists);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var home = "/Users/Shared";
                return new[] { "GOG.com/Galaxy" }
                    .Select(path => Path.Join(home, path, "Storage"))
                    .FirstOrDefault(Directory.Exists);
            }
            throw new PlatformNotSupportedException();
        }
    }
}

