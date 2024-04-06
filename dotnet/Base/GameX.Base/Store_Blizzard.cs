using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace GameX.Store
{
    /// <summary>
    /// Store_Blizzard
    /// </summary>
    internal static class Store_Blizzard
    {
        internal static Dictionary<string, string> BlizzardPaths = new Dictionary<string, string>();

        static Store_Blizzard()
        {
            // get dbPath
            var root = GetPath();
            if (root == null) return;
            var dbPath = Path.Combine(root, "product.db");
            if (!File.Exists(dbPath)) return;

            // query games
            Database productDb;
            using var s = File.OpenRead(dbPath);
            try
            {
                productDb = Database.Parser.ParseFrom(s);
            }
            catch (InvalidProtocolBufferException)
            {
                productDb = new Database { ProductInstall = { ProductInstall.Parser.ParseFrom(s) } };
            }
            foreach (var app in productDb.ProductInstall)
            {
                // add appPath if exists
                var appPath = app.Settings.InstallPath;
                if (Directory.Exists(appPath)) BlizzardPaths.Add(app.Uid, appPath);
            }
        }

        static string GetPath()
        {
            IEnumerable<string> paths;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // windows paths
                var home = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                paths = new[] { Path.Combine(home, "Battle.net", "Agent") };
            }
            else if (RuntimeInformation.OSDescription.StartsWith("android-")) return null;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // linux paths
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var search = new[] { ".steam", ".steam/steam", ".steam/root", ".local/share/Steam" };
                paths = search.Select(path => Path.Join(home, path, "appcache"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // mac paths
                var home = "/Users/Shared";
                var search = new[] { "Battle.net/Agent" };
                paths = search.Select(path => Path.Join(home, path, "data"));
            }
            else throw new PlatformNotSupportedException();
            return paths.FirstOrDefault(Directory.Exists);
        }
    }
}

