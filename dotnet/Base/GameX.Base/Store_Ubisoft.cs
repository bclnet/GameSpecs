using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace GameX.Store
{
    /// <summary>
    /// Store_Ubisoft
    /// </summary>
    internal static class Store_Ubisoft
    {
        internal static Dictionary<string, string> UbisoftPaths = new Dictionary<string, string>();

        static Store_Ubisoft()
        {
            // get dbPath
            var root = GetPath();
            if (root == null) return;
            var dbPath = Path.Combine(root, "settings.yaml");
            if (!File.Exists(dbPath)) return;
        }

        static string GetPath()
        {
            IEnumerable<string> paths;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // windows paths
                var home = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var search = new[] { "Ubisoft Game Launcher" };
                paths = search.Select(path => Path.Join(home, path));
            }
            else if (RuntimeInformation.OSDescription.StartsWith("android-")) return null;
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // linux paths
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var search = new[] { "??" };
                paths = search.Select(path => Path.Join(home, path));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // mac paths
                var home = "/Users/Shared";
                var search = new[] { "??" };
                paths = search.Select(path => Path.Join(home, path));

            }
            else throw new PlatformNotSupportedException();
            return paths.FirstOrDefault(Directory.Exists);
        }
    }
}
