using SQLitePCL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using static SQLitePCL.raw;

namespace GameX.Store
{
    /// <summary>
    /// Store_Gog
    /// </summary>
    internal static class Store_Gog
    {
        internal static Dictionary<string, string> GogPaths = new Dictionary<string, string>();

        static Store_Gog()
        {
            SetProvider(new SQLite3Provider_e_sqlite3());

            // get dbPath
            var root = GetPath();
            if (root == null) return;
            var dbPath = Path.Combine(root, "galaxy-2.0.db");
            if (!File.Exists(dbPath)) return;

            // query games
            if (sqlite3_open(dbPath, out var conn) != SQLITE_OK ||
                sqlite3_prepare_v2(conn, "SELECT productId, installationPath FROM InstalledBaseProducts", out var stmt) != SQLITE_OK) return;
            var read = true;
            while (read)
                switch (sqlite3_step(stmt))
                {
                    case SQLITE_ROW:
                        // add appPath if exists
                        var appId = sqlite3_column_int(stmt, 0).ToString();
                        var appPath = sqlite3_column_text(stmt, 1).utf8_to_string();
                        if (Directory.Exists(appPath)) GogPaths.Add(appId, appPath);
                        break;
                    case SQLITE_DONE: read = false; break;
                }
            sqlite3_finalize(stmt);
        }

        static string GetPath()
        {
            IEnumerable<string> paths;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // windows paths
                var home = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                var search = new[] { @"GOG.com\Galaxy" };
                paths = search.Select(path => Path.Join(home, path, "storage"));
            }
            else if (RuntimeInformation.OSDescription.StartsWith("android-")) return null;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // linux paths
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var search = new[] { "??" };
                paths = search.Select(path => Path.Join(home, path, "Storage"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // mac paths
                var home = "/Users/Shared";
                var search = new[] { "GOG.com/Galaxy" };
                paths = search.Select(path => Path.Join(home, path, "Storage"));

            }
            else throw new PlatformNotSupportedException();
            return paths.FirstOrDefault(Directory.Exists);
        }
    }
}
